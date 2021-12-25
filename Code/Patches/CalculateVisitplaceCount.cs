using System.Collections.Generic;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement visit count changes for commercial buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI), nameof(CommercialBuildingAI.CalculateVisitplaceCount))]
    public static class RealisticVisitplaceCount
    {
        // Commercial visits modes.
        internal enum ComVisitModes
        {
            popCalcs = 0,
            legacy
        }

        // Array indexes.
        private enum SubServiceIndex
        {
            CommercialLow = 0,
            CommercialHigh,
            CommercialLeisure,
            CommercialTourist,
            CommercialEco,
            NumSubServices
        }

        // Sub-service mapping.
        private static readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.CommercialEco
        };

        // Default multiplier.
        internal const int DefaultVisitMultLow = 100;
        internal const int DefaultVisitMultHigh = 100;

        // Maximum multiplier.
        internal const int MaxVisitMult = 100;

        // Minimum value.
        private const int MinVisitCount = 5;


        // Arrays for calculation mode and multipliers.
        private static readonly int[] comVisitModes =
        {
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs,
            (int)ComVisitModes.popCalcs
        };
        private static readonly int[] comVisitMults =
        {
            DefaultVisitMultLow,
            DefaultVisitMultHigh,
            DefaultVisitMultLow,
            DefaultVisitMultLow,
            DefaultVisitMultLow
        };


        /// <summary>
        /// Harmony Prefix patch to CommercialBuildingAI.CalculateVisitplaceCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <returns>Always false (don't execute base game method after this)</returns>
        public static bool Prefix(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // Array index.
            int arrayIndex = GetIndex(subService);

            // New or old calculations?
            if (comVisitModes[arrayIndex] == (int)ComVisitModes.popCalcs)
            {
                // Get cached workplace count and calculate total workplaces.
                WorkplaceLevels workplaces = PopData.instance.WorkplaceCache(info, (int)level);
                __result = NewVisitCount(subService, level, workplaces.level0 + workplaces.level1 + workplaces.level2 + workplaces.level3);
            }
            else
            {
                // Old settings, based on lot size.
                __result = LegacyVisitCount(info, level);
            }

            // Always set at least our minimum.
            if (__result < MinVisitCount)
            {
                __result = MinVisitCount;
            }

            // Don't execute base method after this.
            return false;
        }


        /// <summary>
        /// Returns the calculated visitplace count according to current settings for the given prefab and workforce total (e.g. for previewing effects of changes to workforces).
        /// </summary>
        /// <param name="prefab">Prefab to check</param>
        /// <param name="workplaces">Number of workplaces to apply</param>
        /// <returns>Calculated visitplaces</returns>
        internal static int PreviewVisitCount(BuildingInfo prefab, int workplaces)
        {
            // Get builidng info.
            ItemClass.SubService subService = prefab.GetSubService();

            // Array index.
            int arrayIndex = GetIndex(subService);

            // New or old calculations?
            if (comVisitModes[arrayIndex] == (int)ComVisitModes.popCalcs)
            {
                // New calcs.
                return NewVisitCount(subService, prefab.GetClassLevel(), workplaces);
            }
            else
            {
                // Old calcs.
                return LegacyVisitCount(prefab, prefab.GetClassLevel());
            }
        }


        /// <summary>
        /// Calculates the visitplace count for the given subservice, level and workforce, using on new volumetric calculations.
        /// </summary>
        /// <param name="subService">Building subservice</param>
        /// <param name="level">Building level </param>
        /// <param name="workplaces">Total workplaces</param>
        /// <returns>Calculated visitplace count</returns>
        internal static int NewVisitCount(ItemClass.SubService subService, ItemClass.Level level, int workplaces)
        {
            float multiplier;

            // New settings, based on population.
            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    switch (level)
                    {
                        case ItemClass.Level.Level1:
                            multiplier = 1.8f;
                            break;
                        case ItemClass.Level.Level2:
                            multiplier = 1.333333333f;
                            break;
                        default:
                            multiplier = 1.1f;
                            break;
                    }
                    break;

                case ItemClass.SubService.CommercialHigh:
                    switch (level)
                    {
                        case ItemClass.Level.Level1:
                            multiplier = 2.666666667f;
                            break;
                        case ItemClass.Level.Level2:
                            multiplier = 3f;
                            break;
                        default:
                            multiplier = 3.2f;
                            break;
                    }
                    break;

                case ItemClass.SubService.CommercialLeisure:
                case ItemClass.SubService.CommercialTourist:
                    multiplier = 2.5f;
                    break;

                default:
                    // Commercial eco.
                    multiplier = 1f;
                    break;
            }

            // Multiply total workers by multipler and overall multiplier (from settings) to get result.
            int result = (int)((workplaces * comVisitMults[GetIndex(subService)] * multiplier) / 100f);

            // Scale result - 100% of 0-200, 75% of 201-400, 50% of 401-600, 25% after that.
            int twoHundredPlus = result - 200;
            if (twoHundredPlus > 0)
            {
                // Base numbers for scaling
                const int TwoHundredBase = 200;
                const int FourHundredBase = TwoHundredBase + (int)(200 * 0.75f);
                const int SixHundredBase = FourHundredBase + (int)(200 * 0.5f);

                // Result is greater than 200.
                int fourHundredPlus = result - 400;
                if (fourHundredPlus > 0)
                {
                    int sixHundredPlus = result - 600;
                    if (sixHundredPlus > 0)
                    {
                        // Result is greater than 600.
                        return SixHundredBase + (int)(sixHundredPlus * 0.25f);
                    }
                    else
                    {
                        // Result is greater than 400, but less than 600.
                        return FourHundredBase + (int)(fourHundredPlus * 0.5f);
                    }
                }
                else
                {
                    // Result is greater than 200, but less than 400.
                    return TwoHundredBase + (int)(twoHundredPlus * 0.75f);
                }
            }

            // Bounds check for minimum value.
            if (result < MinVisitCount)
            {
                result = MinVisitCount;
            }

            return result;
        }


        /// <summary>
        /// Legacy visitplace count calculations.
        /// </summary>
        /// <param name="prefab">Building prefab</param>
        /// <param name="level">Building level </param>
        /// <returns>Calculated visitplace count</returns>
        internal static int LegacyVisitCount(BuildingInfo prefab, ItemClass.Level level) => UnityEngine.Mathf.Max(200, prefab.GetWidth() * prefab.GetWidth() * LegacyAIUtils.GetCommercialArray(prefab, (int)level)[DataStore.VISIT]) / 100;


        /// <summary>
        /// Sets the visit mode for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetVisitModes
        {
            set
            {
                for (int i = 0; i < comVisitModes.Length; ++i)
                {
                    comVisitModes[i] = value;
                }
            }
        }

        /// <summary>
        /// Sets the visit percentage multiplier for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetVisitMults
        {
            set
            {
                for (int i = 0; i < comVisitMults.Length; ++i)
                {
                    comVisitMults[i] = value;
                }
            }
        }


        /// <summary>
        /// Gets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit calculation mode</returns>
        internal static int GetVisitMode(ItemClass.SubService subService) => comVisitModes[GetIndex(subService)];


        /// <summary>
        /// Sets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>Visit calculaiton mode</returns>
        internal static void SetVisitMode(ItemClass.SubService subService, int value) => comVisitModes[GetIndex(subService)] = value;


        /// <summary>
        /// Gets the current commerical visit multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Visit multiplier</returns>
        internal static int GetVisitMult(ItemClass.SubService subService) => comVisitMults[GetIndex(subService)];


        /// <summary>
        /// Sets the current commerical visit multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set</param>
        /// <param name="value">Value to set</param>
        internal static void SetVisitMult(ItemClass.SubService subService, int value) => comVisitMults[GetIndex(subService)] = value;


        /// <summary>
        /// Gets the default commerical visit multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default visit multiplier</returns>
        internal static int DefaultVisitMult(ItemClass.SubService subService)
        {
            if (subService == ItemClass.SubService.CommercialHigh)
            {
                return DefaultVisitMultHigh;
            }

            return DefaultVisitMultLow;
        }


        /// <summary>
        /// Serializes the current visitor mode settings ready for XML.
        /// </summary>
        /// <returns>New list of visitor mode entries ready for serialization</returns>
        internal static List<SubServiceMode> SerializeVisits()
        {
            List<SubServiceMode> entries = new List<SubServiceMode>();

            for (int i = 0; i < comVisitModes.Length; ++i)
            {
                entries.Add(new SubServiceMode
                {
                    subService = subServices[i],
                    mode = comVisitModes[i],
                    multiplier = comVisitMults[i]
                });
            }

            return entries;
        }


        /// <summary>
        /// Deserializes XML visitor mode entries.
        /// </summary>
        /// <param name="entries">List of visitor mode entries to deserialize</param>
        /// <returns>New list of visitor mode entries ready for serialization</returns>
        internal static void DeserializeVisits(List<SubServiceMode> entries)
        {
            foreach (SubServiceMode entry in entries)
            {
                SetVisitMode(entry.subService, entry.mode);
                SetVisitMult(entry.subService, entry.multiplier);
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
                case ItemClass.SubService.CommercialLow:
                    return (int)SubServiceIndex.CommercialLow;
                case ItemClass.SubService.CommercialHigh:
                    return (int)SubServiceIndex.CommercialHigh;
                case ItemClass.SubService.CommercialLeisure:
                    return (int)SubServiceIndex.CommercialLeisure;
                case ItemClass.SubService.CommercialTourist:
                    return (int)SubServiceIndex.CommercialTourist;
                case ItemClass.SubService.CommercialEco:
                    return (int)SubServiceIndex.CommercialEco;
                default:
                    Logging.Error("invalid subservice ", subService, " passed to RealisticVisitplaceCount.GetIndex");
                    return (int)SubServiceIndex.CommercialLow;
            }
        }
    }
}
