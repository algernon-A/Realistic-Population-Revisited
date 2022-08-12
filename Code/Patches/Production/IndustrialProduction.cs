// <copyright file="IndustrialProduction.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patch to implement production changes for office buildings, and supporting methods.
    /// </summary>
    [HarmonyPatch(typeof(IndustrialBuildingAI), nameof(IndustrialBuildingAI.CalculateProductionCapacity))]
    public static class IndustrialProduction
    {
        /// <summary>
        /// Default production multiplier.
        /// </summary>
        internal const int DefaultProdMult = 100;

        /// <summary>
        /// Maximum production multiplier.
        /// </summary>
        internal const int MaxProdMult = 200;

        // Sub-service mapping.
        private static readonly ItemClass.SubService[] SubServices =
        {
            ItemClass.SubService.IndustrialGeneric,
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre,
        };

        // Arrays for calculation mode and multipliers.
        private static readonly int[] Modes =
        {
            (int)ProdModes.Legacy,
            (int)ProdModes.Legacy,
            (int)ProdModes.Legacy,
            (int)ProdModes.Legacy,
            (int)ProdModes.Legacy,
        };

        private static readonly int[] Mults =
        {
            DefaultProdMult,
            DefaultProdMult,
            DefaultProdMult,
            DefaultProdMult,
            DefaultProdMult,
        };

        /// <summary>
        /// Industrial production calculation modes.
        /// </summary>
        internal enum ProdModes
        {
            /// <summary>
            /// Producation calculation based on population.
            /// </summary>
            PopCalcs = 0,

            /// <summary>
            /// Production calculation based on legacy method (lot size).Apo
            /// </summary>
            Legacy,
        }

        // Array indexes.
        private enum SubServiceIndex
        {
            IndustrialGeneric = 0,
            IndustrialFarming,
            IndustrialForestry,
            IndustrialOil,
            IndustrialOre,
            NumSubServices,
        }

        /// <summary>
        /// Sets the industrial production mode for all subservices to the specified mode.
        /// </summary>
        internal static int SetProdModes
        {
            set
            {
                for (int i = 0; i < Modes.Length; ++i)
                {
                    Modes[i] = value;
                }
            }
        }

        /// <summary>
        /// Harmony Prefix patch to IndustrialBuildingAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="__instance">IndustrialBuildingAI instance.</param>
        /// <param name="level">Building level.</param>
        /// <param name="r">Randomizer.</param>
        /// <param name="width">Building width in cells.</param>
        /// <param name="length">Building length in cells.</param>
        /// <returns>False (don't execute base game method after this).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
        public static bool Prefix(ref int __result, IndustrialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // Array index.
            int arrayIndex = GetIndex(subService);

            // New or old method?
            if (Modes[arrayIndex] == (int)ProdModes.PopCalcs)
            {
                // New settings, based on population.
                float multiplier;
                switch (info.GetClassLevel())
                {
                    case ItemClass.Level.Level1:
                        multiplier = 1f;
                        break;
                    case ItemClass.Level.Level2:
                        multiplier = 0.933333f;
                        break;
                    default:
                        multiplier = 0.8f;
                        break;
                }

                // Calculate total workplaces.
                __instance.CalculateWorkplaceCount(level, r, width, length, out int level0, out int level1, out int level2, out int level3);
                int totalWorkers = level0 + level1 + level2 + level3;

                // Multiply total workers by multipler and overall multiplier (from settings) to get result.
                __result = (int)(totalWorkers * multiplier * Mults[arrayIndex] / 100f);
            }
            else
            {
                // Legacy calcs.
                int[] array = LegacyAIUtils.GetIndustryArray(__instance.m_info, (int)level);

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
        /// Gets the current industrial production calculation mode.
        /// </summary>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Production calculation mode.</returns>
        internal static int GetProdMode(ItemClass.SubService subService) => Modes[GetIndex(subService)];

        /// <summary>
        /// Sets the current industrial production calculation mode.
        /// </summary>
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
        internal static void SetProdMode(ItemClass.SubService subService, int value) => Modes[GetIndex(subService)] = value;

        /// <summary>
        /// Gets the current industrial production multiplier.
        /// </summary>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Production multiplier.</returns>
        internal static int GetProdMult(ItemClass.SubService subService) => Mults[GetIndex(subService)];

        /// <summary>
        /// Sets the current industrial production multipier.
        /// </summary>
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
        internal static void SetProdMult(ItemClass.SubService subService, int value) => Mults[GetIndex(subService)] = value;

        /// <summary>
        /// Serializes the current industrial production mode settings ready for XML.
        /// </summary>
        /// <returns>New list of industrial production mode entries ready for serialization.</returns>
        internal static List<Configuration.SubServiceMode> SerializeProds()
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
        /// Deserializes XML industrial production mode entries.
        /// </summary>
        /// <param name="entries">List of industrial mode entries to deserialize.</param>
        internal static void DeserializeProds(List<Configuration.SubServiceMode> entries)
        {
            foreach (Configuration.SubServiceMode entry in entries)
            {
                if (entry.SubService == ItemClass.SubService.IndustrialGeneric)
                {
                    SetProdMode(entry.SubService, entry.Mode);
                    SetProdMult(entry.SubService, entry.Multiplier);
                }
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
                case ItemClass.SubService.IndustrialGeneric:
                    return (int)SubServiceIndex.IndustrialGeneric;
                case ItemClass.SubService.IndustrialFarming:
                    return (int)SubServiceIndex.IndustrialFarming;
                case ItemClass.SubService.IndustrialForestry:
                    return (int)SubServiceIndex.IndustrialForestry;
                case ItemClass.SubService.IndustrialOil:
                    return (int)SubServiceIndex.IndustrialOil;
                case ItemClass.SubService.IndustrialOre:
                    return (int)SubServiceIndex.IndustrialOre;
                default:
                    Logging.Error("invalid subservice ", subService, " passed to RealisticIndustrialProduction.GetIndex");
                    return (int)SubServiceIndex.IndustrialGeneric;
            }
        }
    }
}
