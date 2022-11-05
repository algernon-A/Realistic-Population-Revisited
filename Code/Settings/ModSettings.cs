// <copyright file="ModSettings.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using ColossalFramework;

    /// <summary>
    /// Default calculation modes.
    /// </summary>
    public enum DefaultMode : byte
    {
        /// <summary>
        /// Use new (volumetric) calculations by default.
        /// </summary>
        New = 0,

        /// <summary>
        /// USe vanilla calculations by default.
        /// </summary>
        Vanilla = 1,

        /// <summary>
        /// Use legacy calcuations by default.
        /// </summary>
        Legacy = 2,
    }

    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        // Enable additional features.
        private static bool s_enableSchoolPop = false;

        // Status flag.
        private static float s_defaultSchoolMult = 3f;

        // Default mode settings.
        private static DefaultMode s_thisSaveDefaultRes = DefaultMode.New;
        private static DefaultMode s_thisSaveDefaultCom = DefaultMode.New;
        private static DefaultMode s_thisSaveDefaultInd = DefaultMode.New;
        private static DefaultMode s_thisSaveDefaultOff = DefaultMode.New;

        /// <summary>
        /// Gets or sets a value indicating whether this save has been made with the mod active.
        /// </summary>
        internal static bool IsRealPop2Save { get; set; } = false;

        /// <summary>
        /// Gets or sets the default calculation mode for residential buildings in new saves.
        /// </summary>
        internal static DefaultMode NewSaveDefaultRes { get; set; } = DefaultMode.New;

        /// <summary>
        /// Gets or sets the default calculation mode for commercial buildings in new saves.
        /// </summary>
        internal static DefaultMode NewSaveDefaultCom { get; set; } = DefaultMode.New;

        /// <summary>
        /// Gets or sets the default calculation mode for industrial buildings in new saves.
        /// </summary>
        internal static DefaultMode NewSaveDefaultInd { get; set; } = DefaultMode.New;

        /// <summary>
        /// Gets or sets the default calculation mode for office buildings in new saves.
        /// </summary>
        internal static DefaultMode NewSaveDefaultOff { get; set; } = DefaultMode.New;

        /// <summary>
        /// Gets or sets the default calculation mode for residential buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultRes
        {
            // Simple getter.
            get => s_thisSaveDefaultRes;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != s_thisSaveDefaultRes)
                {
                    // Yes - clear caches.
                    PopData.Instance.ClearHousholdCache();

                    // Update value.
                    s_thisSaveDefaultRes = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default calculation mode for commercial buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultCom
        {
            // Simple getter.
            get => s_thisSaveDefaultCom;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != s_thisSaveDefaultCom)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();

                    // Update value.
                    s_thisSaveDefaultCom = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default calculation mode for industrial buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultInd
        {
            // Simple getter.
            get => s_thisSaveDefaultInd;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != s_thisSaveDefaultInd)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();

                    // Update value.
                    s_thisSaveDefaultInd = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the default calculation mode for commercial buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultOff
        {
            // Simple getter.
            get => s_thisSaveDefaultOff;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != s_thisSaveDefaultOff)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();

                    // Update value.
                    s_thisSaveDefaultOff = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the old 'use legacy by default for residential' option is in effect.
        /// </summary>
        internal static bool ThisSaveLegacyRes
        {
            // Simple getter.
            get => ThisSaveDefaultRes == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultRes = value ? DefaultMode.Legacy : DefaultMode.New;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the old 'use legacy by default for commercial' option is in effect.
        /// </summary>
        internal static bool ThisSaveLegacyCom
        {
            // Simple getter.
            get => ThisSaveDefaultCom == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultCom = value ? DefaultMode.Legacy : DefaultMode.New;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the old 'use legacy by default for industrial' option is in effect.
        /// </summary>
        internal static bool ThisSaveLegacyInd
        {
            // Simple getter.
            get => ThisSaveDefaultInd == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultInd = value ? DefaultMode.Legacy : DefaultMode.New;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the old 'use legacy by default for office' option is in effect.
        /// </summary>
        internal static bool ThisSaveLegacyOff
        {
            // Simple getter.
            get => ThisSaveDefaultOff == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultOff = value ? DefaultMode.Legacy : DefaultMode.New;
        }

        /// <summary>
        /// Gets or sets a value indicating whether custom school population counts are enabled.
        /// </summary>
        internal static bool EnableSchoolPop
        {
            // Simple getter.
            get => s_enableSchoolPop;

            // Setter needs to update schools if after game load, otherwise don't.
            set
            {
                s_enableSchoolPop = value;
                UpdateSchools();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether custom school population properties are enabled.
        /// </summary>
        internal static bool EnableSchoolProperties { get; set; }

        /// <summary>
        /// Gets or sets the current default multiplier for schools.
        /// </summary>
        internal static float DefaultSchoolMult
        {
            // Simple getter.
            get => s_defaultSchoolMult;

            // Setter needs to update schools if after game load, otherwise don't.
            set
            {
                s_defaultSchoolMult = value;
                UpdateSchools();
            }
        }

        /// <summary>
        /// Triggers an update of existing school buildings to current settings, if loading is complete.
        /// </summary>
        private static void UpdateSchools()
        {
            // Check for loading complete.
            if (Singleton<LoadingManager>.instance.m_loadingComplete)
            {
                // Update school buildings via simulation thread.
                Singleton<SimulationManager>.instance.AddAction(() => SchoolData.Instance.UpdateSchoolPrefabs());
            }
        }

        /// <summary>
        /// Clears all workplace caches.
        /// </summary>
        private static void ClearWorkplaceCaches()
        {
            // Clear workplace cache.
            PopData.Instance.ClearWorkplaceCache();

            // Clear visitplace cache.
            PopData.Instance.ClearVisitplaceCache();
        }
    }
}