// <copyright file="CityServiceWorldInfoPanelPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using HarmonyLib;

    /// <summary>
    /// Harmony Postfix patch to toggle Realistic Population building info panel button visibility when building selection changes.
    /// </summary>
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
    internal static class CityServiceWorldInfoPanelPatch
    {
        /// <summary>
        /// Harmony Postfix patch to toggle Realistic Population building info panel button visibility when building selection changes.
        /// </summary>
        public static void Postfix()
        {
            BuildingDetailsPanelManager.UpdateServicePanelButton();
        }
    }
}