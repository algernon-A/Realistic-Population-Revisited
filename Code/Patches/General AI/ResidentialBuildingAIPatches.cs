// <copyright file="ResidentialBuildingAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to ResidentialBuildingAI to implement homecount, consumption, and pollution changes for residential buildings.
    /// </summary>
    [HarmonyPatch(typeof(ResidentialBuildingAI))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class ResidentialBuildingAIPatches
    {
        /// <summary>
        /// Harmony Prefix patch to ResidentialBuildingAI.CalculateHomeCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="__instance">ResidentialBuildingAI instance.</param>
        /// <param name="level">Building level.</param>
        /// <returns>False (never execute original method) if anything other than vanilla calculations are set for the building, true (fall through to game code) otherwise.</returns>
        [HarmonyPatch(nameof(ResidentialBuildingAI.CalculateHomeCount))]
        [HarmonyPrefix]
        public static bool Prefix(ref int __result, ResidentialBuildingAI __instance, ItemClass.Level level)
        {
            // Get population value from cache.
            int result = PopData.Instance.HouseholdCache(__instance.m_info, (int)level);

            // Always set at least one.
            if (result < 1)
            {
                Logging.Error("invalid homecount result ", result, " for ", __instance.m_info.name, "; setting to 1");
                result = 1;
            }

            // Check for vanilla calc setting.
            else if (result == ushort.MaxValue)
            {
                // Vanilla calculations; fall through to original game code.
                return true;
            }

            // Don't execute base method after this.
            __result = result;
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony Prefix patch for ResidentialBuildingAI.GetConsumptionRates, to implement the mod's consumption calculations.
        /// </summary>
        /// <param name="__instance">AI instance reference.</param>
        /// <param name="level">Building level.</param>
        /// <param name="r">Randomizer.</param>
        /// <param name="productionRate">Building production rate.</param>
        /// <param name="electricityConsumption">Building electricity consumption.</param>
        /// <param name="waterConsumption">Building water consumption.</param>
        /// <param name="sewageAccumulation">Building sewage accumulation.</param>
        /// <param name="garbageAccumulation">Building garbage accumulation.</param>
        /// <param name="incomeAccumulation">Building income accumulation.</param>
        /// <param name="mailAccumulation">Building mail accumulation.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(ResidentialBuildingAI.GetConsumptionRates))]
        [HarmonyPrefix]
        public static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            // Get relevant array from datastore.
            int[] array = LegacyAIUtils.GetResidentialArray(__instance.m_info, (int)level);

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
        /// Pre-emptive Harmony Prefix patch to ResidentialBuildingAI.GetPollutionRates to implement custom pollution settings.
        /// </summary>
        /// <param name="__instance">ResidentialBuildingAI instance.</param>
        /// <param name="level">Building level.</param>
        /// <param name="groundPollution">Calculated ground pollution.</param>
        /// <param name="noisePollution">Calculated noise pollution.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(ResidentialBuildingAI.GetPollutionRates))]
        [HarmonyPrefix]
        public static bool Prefix(ResidentialBuildingAI __instance, ItemClass.Level level, out int groundPollution, out int noisePollution)
        {
            int[] array = LegacyAIUtils.GetResidentialArray(__instance.m_info, (int)level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = array[DataStore.NOISE_POLLUTION];

            // Don't execute base method after this.
            return false;
        }
    }
}