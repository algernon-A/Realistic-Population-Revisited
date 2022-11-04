// <copyright file="UnlockedZonePatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using HarmonyLib;
    using UnityEngine;

    /// <summary>
    /// Harmony patch to implement zone unlocking.
    /// </summary>
    [HarmonyPatch(typeof(UnlockManager), nameof(UnlockManager.Unlocked), new Type[] { typeof(ItemClass.Zone) })]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class UnlockedZonePatch
    {
        // Unlocking statis.
        private static bool s_unlockZones = false;

        /// <summary>
        /// Gets or sets a value indicating whether all zone types should be unlocked from game start.
        /// </summary>
        internal static bool UnlockZoning
        {
            get => s_unlockZones;

            set
            {
                // Don't do anything if no change.
                if (value == s_unlockZones)
                {
                    return;
                }

                s_unlockZones = value;

                // Apply zoning unlocks if we're in-game.
                if (Loading.IsLoaded)
                {
                    UpdateZoningLock();
                }
            }
        }

        /// <summary>
        /// Harmony prefix for UnlockManager.Unlock(Zone), to implement zone unlocking setting.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <returns>False (don't execute original method) if override is in place, true (execute original method) otherwise.</returns>
        public static bool Prefix(ref bool __result)
        {
            __result = s_unlockZones;
            return !s_unlockZones;
        }

        /// <summary>
        /// Updates zoning milestone progression unlocking according to the current setting.
        /// </summary>
        internal static void UpdateZoningLock() => GameObject.FindObjectOfType<ZoningPanel>()?.RefreshPanel();
    }
}