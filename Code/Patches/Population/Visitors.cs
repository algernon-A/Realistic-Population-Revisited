// <copyright file="Visitors.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony patch to implement visit count changes for commercial buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI), nameof(CommercialBuildingAI.CalculateVisitplaceCount))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class Visitors
    {
        /// <summary>
        /// Default visitor count mulriplier for low-density commercial buildings.
        /// </summary>
        internal const int DefaultVisitMultLow = 100;

        /// <summary>
        /// Default visitor count mulriplier for high-density commercial buildings.
        /// </summary>
        internal const int DefaultVisitMultHigh = 100;

        /// <summary>
        /// Maximum visitor count multiplier.
        /// </summary>
        internal const int MaxVisitMult = 100;

        /// <summary>
        /// Minimum visitor count multiplier.
        /// </summary>
        private const int MinVisitCount = 5;

        // Sub-service mapping.
        private static readonly ItemClass.SubService[] SubServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialWallToWall,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.CommercialEco,
        };

        // Arrays for calculation mode and multipliers.
        private static readonly int[] Modes =
        {
            (int)ComVisitModes.PopCalcs,
            (int)ComVisitModes.PopCalcs,
            (int)ComVisitModes.PopCalcs,
            (int)ComVisitModes.PopCalcs,
            (int)ComVisitModes.PopCalcs,
            (int)ComVisitModes.PopCalcs,
        };

        private static readonly int[] Mults =
        {
            DefaultVisitMultLow,
            DefaultVisitMultHigh,
            DefaultVisitMultHigh,
            DefaultVisitMultLow,
            DefaultVisitMultLow,
            DefaultVisitMultLow,
        };

        /// <summary>
        /// Commercial visitplace calculation modes.
        /// </summary>
        internal enum ComVisitModes
        {
            /// <summary>
            /// Visitor calculation based on population.
            /// </summary>
            PopCalcs = 0,

            /// <summary>
            /// Visitor calculation based on legacy method (lot size).Apo
            /// </summary>
            Legacy,
        }

        // Array indexes.
        private enum SubServiceIndex
        {
            CommercialLow = 0,
            CommercialHigh,
            CommercialWallToWall,
            CommercialLeisure,
            CommercialTourist,
            CommercialEco,
            NumSubServices,
        }

        /// <summary>
        /// Sets the visit mode for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetVisitModes
        {
            set
            {
                for (int i = 0; i < Modes.Length; ++i)
                {
                    Modes[i] = value;
                }

                // Clear visitplace cache.
                PopData.Instance.ClearVisitplaceCache();
            }
        }

        /// <summary>
        /// Sets the visit percentage multiplier for all commercial subservices to the specified mode.
        /// </summary>
        internal static int SetVisitMults
        {
            set
            {
                for (int i = 0; i < Mults.Length; ++i)
                {
                    Mults[i] = value;
                }

                // Clear visitplace cache.
                PopData.Instance.ClearVisitplaceCache();
            }
        }

        /// <summary>
        /// Harmony Prefix patch to CommercialBuildingAI.CalculateVisitplaceCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="__instance">CommercialBuildingAI instance.</param>
        /// <param name="level">Building level.</param>
        /// <returns>False (never execute original method) if anything other than vanilla calculations are set for the building, true (fall through to game code) otherwise.</returns>
        public static bool Prefix(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level)
        {
            int result = PopData.Instance.VisitplaceCache(__instance.m_info, (int)level);

            // Check for vanilla calc setting.
            if (result == ushort.MaxValue)
            {
                // Vanilla calculations; fall through to original game code.
                return true;
            }

            // Don't execute base method after this.
            __result = result;
            return false;
        }

        /// <summary>
        /// Calculates visitplace count according to current settings for the given prefab and workforce total.
        /// </summary>
        /// <param name="prefab">Prefab to check.</param>
        /// <param name="workplaces">Number of workplaces to apply.</param>
        /// <returns>Calculated visitplaces.</returns>
        internal static int CalculateVisitCount(BuildingInfo prefab, int workplaces)
        {
            // Get builidng info.
            ItemClass.SubService subService = prefab.GetSubService();

            // Array index.
            int arrayIndex = GetIndex(subService);

            // New or old calculations?
            if (Modes[arrayIndex] == (int)ComVisitModes.PopCalcs)
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
        /// Calculates the visitplace count for the given subservice, level and workforce, using new volumetric calculations.
        /// </summary>
        /// <param name="subService">Building subservice.</param>
        /// <param name="level">Building level.</param>
        /// <param name="workplaces">Total workplaces.</param>
        /// <returns>Calculated visitplace count.</returns>
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

                case ItemClass.SubService.CommercialLeisure:
                case ItemClass.SubService.CommercialTourist:
                    multiplier = 2.5f;
                    break;

                case ItemClass.SubService.CommercialEco:
                    multiplier = 1f;
                    break;

                // Default is commercial high.
                default:
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
            }

            // Multiply total workers by multipler and overall multiplier (from settings) to get result.
            int result = (int)(workplaces * Mults[GetIndex(subService)] * multiplier / 100f);

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
        /// <param name="prefab">Building prefab.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Calculated visitplace count.</returns>
        internal static int LegacyVisitCount(BuildingInfo prefab, ItemClass.Level level) => UnityEngine.Mathf.Max(200, prefab.GetWidth() * prefab.GetWidth() * LegacyAIUtils.GetCommercialArray(prefab, (int)level)[DataStore.VISIT]) / 100;

        /// <summary>
        /// Gets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Visit calculation mode.</returns>
        internal static int GetVisitMode(ItemClass.SubService subService) => Modes[GetIndex(subService)];

        /// <summary>
        /// Sets the current commerical visit mode for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
        internal static void SetVisitMode(ItemClass.SubService subService, int value)
        {
            Modes[GetIndex(subService)] = value;

            // Clear visitplace cache.
            PopData.Instance.ClearVisitplaceCache();
        }

        /// <summary>
        /// Gets the current commerical visit multiplier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Visit multiplier.</returns>
        internal static int GetVisitMult(ItemClass.SubService subService) => Mults[GetIndex(subService)];

        /// <summary>
        /// Sets the current commerical visit multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
        internal static void SetVisitMult(ItemClass.SubService subService, int value)
        {
            Mults[GetIndex(subService)] = value;

            // Clear visitplace cache.
            PopData.Instance.ClearVisitplaceCache();
        }

        /// <summary>
        /// Gets the default commerical visit multipier for the specified sub-service.
        /// </summary>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Default visit multiplier.</returns>
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
        /// <returns>New list of visitor mode entries ready for serialization.</returns>
        internal static List<Configuration.SubServiceMode> SerializeVisits()
        {
            List<Configuration.SubServiceMode> entries = new List<Configuration.SubServiceMode>();

            for (int i = 0; i < Modes.Length; ++i)
            {
                entries.Add(new Configuration.SubServiceMode
                {
                    SubService = SubServices[i],
                    Mode = Modes[i],
                    Multiplier = Mults[i],
                });
            }

            return entries;
        }

        /// <summary>
        /// Deserializes XML visitor mode entries.
        /// </summary>
        /// <param name="entries">List of visitor mode entries to deserialize.</param>
        internal static void DeserializeVisits(List<Configuration.SubServiceMode> entries)
        {
            foreach (Configuration.SubServiceMode entry in entries)
            {
                SetVisitMode(entry.SubService, entry.Mode);
                SetVisitMult(entry.SubService, entry.Multiplier);
            }
        }

        /// <summary>
        /// Returns the sub-service array index for the given sub-service.
        /// </summary>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Array index.</returns>
        private static int GetIndex(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.CommercialLow:
                    return (int)SubServiceIndex.CommercialLow;

                case ItemClass.SubService.CommercialLeisure:
                    return (int)SubServiceIndex.CommercialLeisure;

                case ItemClass.SubService.CommercialTourist:
                    return (int)SubServiceIndex.CommercialTourist;

                case ItemClass.SubService.CommercialEco:
                    return (int)SubServiceIndex.CommercialEco;

                // Default is commercial high.
                default:
                case ItemClass.SubService.CommercialHigh:
                    return (int)SubServiceIndex.CommercialHigh;
            }
        }
    }
}
