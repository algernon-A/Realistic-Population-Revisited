using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement production changes for office buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(OfficeBuildingAI), nameof (OfficeBuildingAI.CalculateProductionCapacity))]
    public static class RealisticOfficeProduction
    {
        // Default multiplier.
        internal const int DefaultProdMult = 100;

        // Maximum multiplier.
        internal const int MaxProdMult = 200;

        // Settings per subservice.
        private static int genericOfficeProdMult = DefaultProdMult;
        private static int highTechOfficeProdMult = DefaultProdMult;


        /// <summary>
        /// Harmony Prefix patch to OfficeBuildingAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>False (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, OfficeBuildingAI __instance, ItemClass.Level level)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // Get cached workplace count and calculate total workplaces.
            int[] workplaces = PopData.instance.WorkplaceCache(info, (int)level);
            int totalWorkers = workplaces[0] + workplaces[1] + workplaces[2] + workplaces[3];

            // Using legacy settings?
            if (PopData.instance.ActivePack(info).version == (int)DataVersion.legacy)
            {
                // Legacy settings.
                int[] array = LegacyAIUtils.GetOfficeArray(info, (int)level);

                // Original method return value.
                __result = totalWorkers / array[DataStore.PRODUCTION];
            }
            else
            {
                // Hew settings - multiply total workers by overall multiplier (from settings) to get result; divisor is 1,000 to match original mod 1/10th when at 100% production.
                __result = (totalWorkers * (subService == ItemClass.SubService.OfficeHightech ? highTechOfficeProdMult : genericOfficeProdMult)) / 1000;
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
        /// Gets the current office production multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit mode</returns>
        internal static int GetProdMult(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.OfficeGeneric:
                    return genericOfficeProdMult;
                case ItemClass.SubService.OfficeHightech:
                    return highTechOfficeProdMult;
                default:
                    Logging.Error("invalid subservice ", subService, " passed to office GetProdMult");
                    return 0;
            }
        }


        /// <summary>
        /// Sets the current office production multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit mode</returns>
        internal static void SetProdMult(ItemClass.SubService subService, int value)
        {
            int cleanValue = Mathf.Clamp(0, value, MaxProdMult);

            switch (subService)
            {
                case ItemClass.SubService.OfficeGeneric:
                    genericOfficeProdMult = cleanValue;
                    break;
                case ItemClass.SubService.OfficeHightech:
                    highTechOfficeProdMult = cleanValue;
                    break;
                default:
                    Logging.Error("invalid subservice ", subService, " passed to office SetProdMult");
                    break;
            }
        }


        /// <summary>
        /// Serializes the current office production multiplier settings ready for XML.
        /// </summary>
        /// <returns>New list of office production multiplier entries ready for serialization</returns>
        internal static List<SubServiceValue> SerializeProdMults()
        {
            return new List<SubServiceValue>()
            {
                new SubServiceValue
                {
                    subService = ItemClass.SubService.OfficeGeneric,
                    value = genericOfficeProdMult
                },
                new SubServiceValue
                {
                    subService = ItemClass.SubService.OfficeHightech,
                    value = highTechOfficeProdMult
                }
            };
        }


        /// <summary>
        /// Deserializes XML office production multiplier entries.
        /// </summary>
        /// <param name="entries">List of office production multiplier entries to deserialize</param>
        /// <returns>New list of voffice production multiplier entries ready for serialization</returns>
        internal static void DeserializeProdMults(List<SubServiceValue> entries)
        {
            foreach (SubServiceValue entry in entries)
            {
                SetProdMult(entry.subService, entry.value);
            }
        }
    }
}
