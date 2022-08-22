// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using CitiesHarmony.API;
    using HarmonyLib;

    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public class Patcher : PatcherBase
    {
        /// <summary>
        /// Patch Advanced Building Level Control's 'CustomBuildingUpgraded' method.
        /// </summary>
        internal void PatchABLC()
        {
            // Ensure Harmony is ready before patching.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                // Try to get ABLC method.
                MethodInfo ablcCustomUpgraded = ModUtils.ABLCCustomUpgraded();
                if (ablcCustomUpgraded != null)
                {
                    // Got method - apply patch.
                    Logging.Message("patching ABLC.LevelUtils.CustomBuildingUpgraded");
                    Harmony harmonyInstance = new Harmony(HarmonyID);
                    harmonyInstance.Patch(ablcCustomUpgraded, postfix: new HarmonyMethod(typeof(BuildingUpgradedPatch).GetMethod(nameof(BuildingUpgradedPatch.ABLCPostfix))));
                }
            }
        }
    }
}