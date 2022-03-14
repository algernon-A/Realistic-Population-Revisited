using ColossalFramework;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony Postfix patch for OnLevelLoaded.  This enables us to perform setup tasks after all loading has been completed.
    /// </summary>
    [HarmonyPatch(typeof(LoadingWrapper))]
    [HarmonyPatch("OnLevelLoaded")]
    public static class OnLevelLoadedPatch
    {
        // Flag to determine whether or not we ned to rebuild the CitizenUnit array/
        // Enabled by default, is disabled if we load a save with RealPop2 data version 6 or later (indicating that the rebuild has already taken place).
        internal static bool rebuildUnitArray = true;


        // Hook for other mods to disable the auto rebuildbuild.
        public static bool StopRebuild
        {
            set => _stopRebuild= value;
        }
        private static bool _stopRebuild = false;


        /// <summary>
        /// Harmony postfix to perform actions require after the level has loaded.
        /// </summary>
        public static void Postfix()
        {
            // If we're rebuilding the CitizenUnit array, do so (via simulation thread).
            if (rebuildUnitArray && !ModSettings.dontRebuildUnits && !_stopRebuild)
            {
                Singleton<SimulationManager>.instance.AddAction(() => UnitUtils.ResetUnits());
            }
        }
    }
}