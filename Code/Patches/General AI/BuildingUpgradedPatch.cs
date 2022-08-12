// <copyright file="BuildingUpgradedPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using ColossalFramework.Math;
    using HarmonyLib;

    /// <summary>
    /// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades, applied to base game PrivateBuildingAI.BuildingUpgraded.
    /// </summary>
    [HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.BuildingUpgraded))]
    [HarmonyBefore("com.github.algernon-A.csl.ablc")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class BuildingUpgradedPatch
    {
        /// <summary>
        /// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades.
        /// </summary>
        /// <param name="__instance">Instance reference.</param>
        /// <param name="buildingID">Building instance ID.</param>
        /// <param name="data">Building data.</param>
        public static void Postfix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
        {
            // Only interested in residential builidngs.
            if (__instance is ResidentialBuildingAI)
            {
                // Recalculate homecount.
                int homeCount = __instance.CalculateHomeCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length);

                Logging.Message("residential building ", buildingID, " (", data.Info.name, ") upgraded to level ", data.m_level + 1, "; calculated homecount is ", homeCount);

                // Remove any extra households.
                CitizenUnitUtils.RemoveCitizenUnits(ref data, homeCount, 0, 0, 0, false);
            }
        }

        /// <summary>
        /// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades.
        /// </summary>
        /// <param name="buildingAI">Building AI instance.</param>
        /// <param name="buildingID">Building instance ID.</param>
        /// <param name="data">Building data.</param>
        public static void ABLCPostfix(PrivateBuildingAI buildingAI, ushort buildingID, ref Building data) => BuildingUpgradedPatch.Postfix(buildingAI, buildingID, ref data);
    }
}
