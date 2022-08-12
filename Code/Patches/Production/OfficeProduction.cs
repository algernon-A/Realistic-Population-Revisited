// <copyright file="OfficeProduction.cs" company="algernon (K. Algernon A. Sheppard)">
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
    [HarmonyPatch(typeof(OfficeBuildingAI), nameof(OfficeBuildingAI.CalculateProductionCapacity))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class OfficeProduction
    {
        /// <summary>
        /// Default production multiplier.
        /// </summary>
        internal const int DefaultProdMult = 100;

        /// <summary>
        /// Maximum production multiplier.
        /// </summary>
        internal const int MaxProdMult = 200;

        // Settings per subservice.
        private static int genericOfficeProdMult = DefaultProdMult;
        private static int highTechOfficeProdMult = DefaultProdMult;

        /// <summary>
        /// Harmony Prefix patch to OfficeBuildingAI.CalculateProductionCapacity to implement mod production calculations.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="__instance">Original AI instance reference.</param>
        /// <param name="level">Building level.</param>
        /// <param name="r">Randomizer.</param>
        /// <param name="width">Building width in cells.</param>
        /// <param name="length">Building length in cells.</param>
        /// <returns>False (don't execute base game method after this).</returns>
        public static bool Prefix(ref int __result, OfficeBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            // Get builidng info.
            BuildingInfo info = __instance.m_info;
            ItemClass.SubService subService = info.GetSubService();

            // Calculate total workplaces.
            __instance.CalculateWorkplaceCount(level, r, width, length, out int level0, out int level1, out int level2, out int level3);
            int totalWorkers = level0 + level1 + level2 + level3;

            // Using legacy settings?
            if (PopData.Instance.ActivePack(info).Version == DataPack.DataVersion.Legacy)
            {
                // Legacy settings.
                int[] array = LegacyAIUtils.GetOfficeArray(info, (int)level);

                // Original method return value.
                __result = totalWorkers / array[DataStore.PRODUCTION];
            }
            else
            {
                // Hew settings - multiply total workers by overall multiplier (from settings) to get result; divisor is 1,000 to match original mod 1/10th when at 100% production.
                __result = totalWorkers * (subService == ItemClass.SubService.OfficeHightech ? highTechOfficeProdMult : genericOfficeProdMult) / 1000;
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
        /// <param name="subService">Sub-service.</param>
        /// <returns>Visit mode.</returns>
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
        /// <param name="subService">Sub-service to set.</param>
        /// <param name="value">Value to set.</param>
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
        /// <returns>New list of office production multiplier entries ready for serialization.</returns>
        internal static List<Configuration.SubServiceValue> SerializeProdMults()
        {
            return new List<Configuration.SubServiceValue>()
            {
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.OfficeGeneric,
                    Value = genericOfficeProdMult,
                },
                new Configuration.SubServiceValue
                {
                    SubService = ItemClass.SubService.OfficeHightech,
                    Value = highTechOfficeProdMult,
                },
            };
        }

        /// <summary>
        /// Deserializes XML office production multiplier entries.
        /// </summary>
        /// <param name="entries">List of office production multiplier entries to deserialize.</param>
        internal static void DeserializeProdMults(List<Configuration.SubServiceValue> entries)
        {
            foreach (Configuration.SubServiceValue entry in entries)
            {
                SetProdMult(entry.SubService, entry.Value);
            }
        }
    }
}
