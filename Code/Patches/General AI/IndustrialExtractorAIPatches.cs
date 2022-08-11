// <copyright file="IndustrialExtractorAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to IndustrialExtractorAI to implement consumption and pollution changes for industrial buildings.
    /// </summary>
    [HarmonyPatch(typeof(IndustrialExtractorAI))]
    public static class IndustrialExtractorAIPatches
    {
        /// <summary>
        /// Pre-emptive Harmony Prefix patch for IndustrialExtractorAI.GetConsumptionRates, to implement the mod's consumption calculations.
        /// </summary>
        /// <param name="__instance">AI instance reference</param>
        /// <param name="r">Randomizer</param>
        /// <param name="productionRate">Building production rate</param>
        /// <param name="electricityConsumption">Building electricity consumption</param>
        /// <param name="waterConsumption">Building water consumption</param>
        /// <param name="sewageAccumulation">Building sewage accumulation</param>
        /// <param name="garbageAccumulation">Building garbage accumulation</param>
        /// <param name="incomeAccumulation">Building income accumulation</param>
        /// <param name="mailAccumulation">Building mail accumulation</param>
        /// <returns>Always false (never execute original method)</returns>
        [HarmonyPatch(nameof(IndustrialExtractorAI.GetConsumptionRates))]
        [HarmonyPrefix]
        public static bool GetConsumptionRates(IndustrialExtractorAI __instance, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            // Get relevant array from datastore.
            int[] array = LegacyAIUtils.GetExtractorArray(__instance.m_info);

            // Get consumption rates from array.
            electricityConsumption = array[DataStore.POWER];
            waterConsumption = array[DataStore.WATER];
            sewageAccumulation = array[DataStore.SEWAGE];
            garbageAccumulation = array[DataStore.GARBAGE];
            mailAccumulation = array[DataStore.MAIL];

            // Calculate land value.
            int landValue = LegacyAIUtils.GetLandValueIncomeComponent(r.seed);
            incomeAccumulation = array[DataStore.INCOME] + landValue;

            // Apply consumption rates.
            electricityConsumption = Mathf.Max(100, productionRate * electricityConsumption) / 100;
            waterConsumption = Mathf.Max(100, productionRate * waterConsumption) / 100;
            sewageAccumulation = Mathf.Max(100, productionRate * sewageAccumulation) / 100;
            garbageAccumulation = Mathf.Max(100, productionRate * garbageAccumulation) / 100;
            incomeAccumulation = productionRate * incomeAccumulation;
            mailAccumulation = Mathf.Max(100, productionRate * mailAccumulation) / 100;

            // Don't execute base method after this.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony Prefix patch to IndustrialExtractorAI.GetPollutionRates to implement custom pollution settings.
        /// </summary>
        /// <param name="__instance">IndustrialExtractorAI instance.</param>
        /// <param name="productionRate">Building production rate.</param>
        /// <param name="groundPollution">Calculated ground pollution.</param>
        /// <param name="noisePollution">Calculated noise pollution.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(IndustrialExtractorAI.GetPollutionRates))]
        [HarmonyPrefix]
        public static bool GetPollutionRates(IndustrialExtractorAI __instance, int productionRate, out int groundPollution, out int noisePollution)
        {
            int[] array = LegacyAIUtils.GetExtractorArray(__instance.m_info);

            groundPollution = (productionRate * array[DataStore.GROUND_POLLUTION]) / 100;
            noisePollution = (productionRate * array[DataStore.NOISE_POLLUTION]) / 100;

            // Don't execute base method after this.
            return false;
        }
    }
}