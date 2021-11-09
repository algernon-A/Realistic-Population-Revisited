using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement production changes for extractor buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(IndustrialExtractorAI), nameof(IndustrialExtractorAI.CalculateProductionCapacity))]
    public static class RealisticExtractorProduction
    {
        // Extractor production calculation modes.
        internal enum ProdModes
        {
            popCalcs = 0,
            legacy
        }

        // Array indexes.
        private enum SubServiceIndex
        {
            IndustrialFarming = 0,
            IndustrialForestry,
            IndustrialOil,
            IndustrialOre,
            IndustrialGeneric, // Generic should be unused but including it makes code more consistent.
            NumSubServices
        }

        // Sub-service mapping.
        private static readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre,
            ItemClass.SubService.IndustrialGeneric
        };

        // Default multiplier.
        internal const int DefaultProdMult = 100;

        // Maximum multiplier.
        internal const int MaxProdMult = 200;

        // Arrays for calculation mode and multipliers.
        private static readonly int[] prodModes =
        {
            (int)ProdModes.legacy,
            (int)ProdModes.legacy,
            (int)ProdModes.legacy,
            (int)ProdModes.legacy,
            (int)ProdModes.legacy
        };
        private static readonly int[] prodMults =
        {
            DefaultProdMult,
            DefaultProdMult,
            DefaultProdMult,
            DefaultProdMult,
            DefaultProdMult
        };


        /// <summary>
        /// Harmony Prefix patch to IndustrialExtractorAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>False (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, IndustrialExtractorAI __instance, ItemClass.Level level, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // Array index.
            int arrayIndex = GetIndex(subService);

            // New or old method?
            if (prodModes[arrayIndex] == (int)ProdModes.popCalcs)
            {
                // New settings, based on population.
                float multiplier;
                switch (subService)
                {
                    case ItemClass.SubService.IndustrialFarming:
                    case ItemClass.SubService.IndustrialForestry:
                        multiplier = 1f;
                        break;
                    default:
                        multiplier = 0.625f;
                        break;
                }

                // Get cached workplace count and calculate total workplaces.
                int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);

                float totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];
                // Multiply total workers by multipler and overall multiplier (from settings) to get result.
                __result = (int)((totalWorkers * multiplier * prodMults[arrayIndex]) / 100f);
            }
            else
            {
                // Legacy calcs.
                int[] array = LegacyAIUtils.GetExtractorArray(__instance.m_info);

                // Original method return value.
                __result = Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;
            }


            // Always set at least one.
            if (__result < 1)
            {
                __result = 1;
            }

            // Don't execute base method after this.
            return false;
        }


        /// <summary>
        /// Sets the extractor production mode for all subservices to the specified mode.
        /// </summary>
        internal static int SetProdModes
        {
            set
            {
                for (int i = 0; i < prodModes.Length; ++i)
                {
                    prodModes[i] = value;
                }
            }
        }


        /// <summary>
        /// Sets the extractor production percentage multiplier for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetProdMults
        {
            set
            {
                for (int i = 0; i < prodMults.Length; ++i)
                {
                    prodMults[i] = value;
                }
            }
        }


        /// <summary>
        /// Gets the current extractor production calculation mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Production calculation mode</returns>
        internal static int GetProdMode(ItemClass.SubService subService) => prodModes[GetIndex(subService)];


        /// <summary>
        /// Sets the current extractor production mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        internal static void SetProdMode(ItemClass.SubService subService, int value) => prodModes[GetIndex(subService)] = value;


        /// <summary>
        /// Gets the current extractor production percentage multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Production multiplier</returns>
        internal static int GetProdMult(ItemClass.SubService subService) => prodMults[GetIndex(subService)];


        /// <summary>
        /// Sets the current extractor production percentage multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetProdMult(ItemClass.SubService subService, int value) => prodMults[GetIndex(subService)] = value;


        /// <summary>
        /// Serializes the current extractor production mode settings ready for XML.
        /// </summary>
        /// <returns>New list of extractor production mode entries ready for serialization</returns>
        internal static List<SubServiceMode> SerializeProds()
        {
            List<SubServiceMode> entries = new List<SubServiceMode>();

            for (int i = 0; i < prodModes.Length; ++i)
            {
                entries.Add(new SubServiceMode
                {
                    subService = subServices[i],
                    mode = prodModes[i],
                    multiplier = prodMults[i]
                });
            }

            return entries;
        }


        /// <summary>
        /// Deserializes XML extractor production mode entries.
        /// </summary>
        /// <param name="entries">List of extractor production mode entries to deserialize</param>
        /// <returns>New list of extractor production mode entries ready for serialization</returns>
        internal static void DeserializeProds(List<SubServiceMode> entries)
        {
            foreach (SubServiceMode entry in entries)
            {
                SetProdMode(entry.subService, entry.mode);
                SetProdMult(entry.subService, entry.multiplier);
            }
        }


        /// <summary>
        /// Returns the sub-service array index for the given sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Array index</returns>
        private static int GetIndex(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.IndustrialFarming:
                    return (int)SubServiceIndex.IndustrialFarming;
                case ItemClass.SubService.IndustrialForestry:
                    return (int)SubServiceIndex.IndustrialForestry;
                case ItemClass.SubService.IndustrialOil:
                    return (int)SubServiceIndex.IndustrialOil;
                case ItemClass.SubService.IndustrialOre:
                    return (int)SubServiceIndex.IndustrialOre;
                case ItemClass.SubService.IndustrialGeneric:
                    return (int)SubServiceIndex.IndustrialGeneric;
                default:
                    Logging.Error("invalid subservice ", subService, " passed to RealisticExtractorProduction.GetIndex");
                    return (int)SubServiceIndex.IndustrialFarming;
            }
        }
    }
}
