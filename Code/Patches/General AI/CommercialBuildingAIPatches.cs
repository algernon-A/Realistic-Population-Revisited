// <copyright file="CommercialBuildingAIPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using ColossalFramework.Math;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patches to CommercialBuildingAI to implement consumption, production rate, and pollution changes for commercial buildings.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class CommercialBuildingAIPatches
    {
        /// <summary>
        /// Pre-emptive Harmony Prefix patch for CommercialBuildingAI.GetConsumptionRates, to implement the mod's consumption calculations.
        /// </summary>
        /// <param name="__instance">CommercialBuildingAI instance.</param>
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
        [HarmonyPatch(nameof(CommercialBuildingAI.GetConsumptionRates))]
        [HarmonyPrefix]
        public static bool GetConsumptionRates(CommercialBuildingAI __instance, ItemClass.Level level, Randomizer r, int productionRate, out int electricityConsumption, out int waterConsumption, out int sewageAccumulation, out int garbageAccumulation, out int incomeAccumulation, out int mailAccumulation)
        {
            // Get relevant array from datastore.
            int[] array = LegacyAIUtils.GetCommercialArray(__instance.m_info, (int)level);

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
        /// Pre-emptive Harmony Prefix patch to CommercialBuildingAI.CalculateProductionCapacity to implement custom production settings.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="__instance">CommercialBuildingAI instance.</param>
        /// <param name="level">Building level.</param>
        /// <param name="width">Building lot width.</param>
        /// <param name="length">Building lot length.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(CommercialBuildingAI.CalculateProductionCapacity))]
        [HarmonyPrefix]
        public static bool CalculateProductionCapacity(ref int __result, CommercialBuildingAI __instance, ItemClass.Level level, int width, int length)
        {
            int[] array = LegacyAIUtils.GetCommercialArray(__instance.m_info, (int)level);

            // Original method return value.
            __result = Mathf.Max(100, width * length * array[DataStore.PRODUCTION]) / 100;

            // Don't execute base method after this.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony Prefix patch to CommercialBuildingAI.GetPollutionRates to implement custom pollution settings.
        /// </summary>
        /// <param name="__instance">CommercialBuildingAI instance.</param>
        /// <param name="level">Building level.</param>
        /// <param name="productionRate">Building production rate.</param>
        /// <param name="cityPlanningPolicies">Applicable city policies.</param>
        /// <param name="groundPollution">Calculated ground pollution.</param>
        /// <param name="noisePollution">Calculated noise pollution.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(CommercialBuildingAI.GetPollutionRates))]
        [HarmonyPrefix]
        public static bool GetPollutionRates(CommercialBuildingAI __instance, ItemClass.Level level, int productionRate, DistrictPolicies.CityPlanning cityPlanningPolicies, out int groundPollution, out int noisePollution)
        {
            ItemClass item = __instance.m_info.m_class;
            int[] array = LegacyAIUtils.GetCommercialArray(__instance.m_info, (int)level);

            groundPollution = array[DataStore.GROUND_POLLUTION];
            noisePollution = productionRate * array[DataStore.NOISE_POLLUTION] / 100;
            if (item.m_subService == ItemClass.SubService.CommercialLeisure)
            {
                if ((cityPlanningPolicies & DistrictPolicies.CityPlanning.NoLoudNoises) != DistrictPolicies.CityPlanning.None)
                {
                    noisePollution /= 2;
                }
            }

            // Don't execute base method after this.
            return false;
        }
    }
}