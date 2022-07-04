using ColossalFramework;


namespace RealPop2
{
    /// <summary>
    /// Default calculation modes.
    /// </summary>
    public enum DefaultMode : byte
    {
        New = 0,
        Vanilla = 1,
        Legacy = 2
    }


    /// <summary>
    /// Static class to hold global mod settings.
    /// </summary>
    internal static class ModSettings
    {
        // Load modes.
        internal static DefaultMode newSaveDefaultRes = DefaultMode.New;
        internal static DefaultMode newSaveDefaultCom = DefaultMode.New;
        internal static DefaultMode newSaveDefaultInd = DefaultMode.New;
        internal static DefaultMode newSaveDefaultOff = DefaultMode.New;

        // Enable additional features.
        private static bool enableSchoolPop = false;
        internal static bool enableSchoolProperties = true;
        internal static float crimeMultiplier = 50f;
        internal static bool dontRebuildUnits = false;

        // Status flags.
        internal static bool isRealPop2Save = false;
        private static float defaultSchoolMult = 3f;

        // What's new notification version.
        internal static string whatsNewVersion = "0.0";


        /// <summary>
        /// Default calculation mode for residential buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultRes
        {
            // Simple getter.
            get => thisSaveDefaultRes;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveDefaultRes)
                {
                    // Yes - clear caches.
                    PopData.instance.householdCache.Clear();

                    // Update value.
                    thisSaveDefaultRes = value;
                }
            }
        }
        private static DefaultMode thisSaveDefaultRes = DefaultMode.New;


        /// <summary>
        /// Default calculation mode for commercial buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultCom
        {
            // Simple getter.
            get => thisSaveDefaultCom;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveDefaultCom)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();

                    // Update value.
                    thisSaveDefaultCom = value;
                }
            }
        }
        private static DefaultMode thisSaveDefaultCom = DefaultMode.New;


        /// <summary>
        /// Default calculation mode for industrial buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultInd
        {
            // Simple getter.
            get => thisSaveDefaultInd;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveDefaultInd)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();

                    // Update value.
                    thisSaveDefaultInd = value;
                }
            }
        }
        private static DefaultMode thisSaveDefaultInd = DefaultMode.New;


        /// <summary>
        /// Default calculation mode for commercial buildings for this save.
        /// </summary>
        internal static DefaultMode ThisSaveDefaultOff
        {
            // Simple getter.
            get => thisSaveDefaultOff;

            // Setter needs to clear out DataStore cache if the setting has changed (to force calculation of new values).
            set
            {
                // Has setting changed?
                if (value != thisSaveDefaultOff)
                {
                    // Yes - clear caches.
                    ClearWorkplaceCaches();

                    // Update value.
                    thisSaveDefaultOff = value;
                }
            }
        }
        private static DefaultMode thisSaveDefaultOff = DefaultMode.New;


        /// <summary>
        /// Old 'use legacy by default for residential' option.
        /// </summary>
        internal static bool ThisSaveLegacyRes
        {
            // Simple getter.
            get => ThisSaveDefaultRes == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultRes = value ? DefaultMode.Legacy : DefaultMode.New;
        }


        /// <summary>
        /// Old 'use legacy by default for commercial' option.
        /// </summary>
        internal static bool ThisSaveLegacyCom
        {
            // Simple getter.
            get => ThisSaveDefaultCom == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultCom = value ? DefaultMode.Legacy : DefaultMode.New;
        }


        /// <summary>
        /// Old 'use legacy by default for industrial' option.
        /// </summary>
        internal static bool ThisSaveLegacyInd
        {
            // Simple getter.
            get => ThisSaveDefaultInd == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultInd = value ? DefaultMode.Legacy : DefaultMode.New;
        }


        /// <summary>
        /// Old 'use legacy by default for office' option.
        /// </summary>
        internal static bool ThisSaveLegacyOff
        {
            // Simple getter.
            get => ThisSaveDefaultOff == DefaultMode.Legacy;

            // Setter only toggles between legacy and new (vanilla setting wasn't implemented for this).
            set => ThisSaveDefaultOff = value ? DefaultMode.Legacy : DefaultMode.New;
        }


        /// <summary>
        /// Enables/disables custom school population counts.
        /// </summary>
        internal static bool EnableSchoolPop
        {
            // Simple getter.
            get => enableSchoolPop;

            // Setter needs to update schools if after game load, otherwise don't.
            set
            {
                enableSchoolPop = value;
                UpdateSchools(null);
            }
        }


        /// <summary>
        /// Handles current default multiplier for schools.
        /// </summary>
        internal static float DefaultSchoolMult
        {
            // Simple getter.
            get => defaultSchoolMult;

            // Setter needs to update schools if after game load, otherwise don't.
            set
            {
                defaultSchoolMult = value;
                UpdateSchools(null);
            }
        }


        /// <summary>
        /// Triggers an update of existing school buildings to current settings, if loading is complete.
        /// </summary>
        /// <param name="schoolPrefab>Building prefab to update (null to update all schools)</param>
        private static void UpdateSchools(BuildingInfo schoolPrefab)
        {
            // Check for loading complete.
            if (Singleton<LoadingManager>.instance.m_loadingComplete)
            {
                // Update school buildings via simulation thread.
                Singleton<SimulationManager>.instance.AddAction(delegate { SchoolData.instance.UpdateSchoolPrefabs(); });
            }
        }


        /// <summary>
        /// Clears all workplace caches.
        /// </summary>
        private static void ClearWorkplaceCaches()
        {
            // Clear workplace cache.
            PopData.instance.workplaceCache.Clear();

            // Clear visitplace cache.
            PopData.instance.visitplaceCache.Clear();

            // Clear RICO cache too.
            if (ModUtils.ricoClearAllWorkplaces != null)
            {
                ModUtils.ricoClearAllWorkplaces.Invoke(null, null);
            }
        }
    }
}