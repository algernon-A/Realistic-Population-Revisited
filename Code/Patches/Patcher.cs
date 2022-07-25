using System.Reflection;
using HarmonyLib;
using CitiesHarmony.API;


namespace RealPop2
{
    /// <summary>
    /// Class to manage the mod's Harmony patches.
    /// </summary>
    public static class Patcher
    {
        // Unique harmony identifier.
        private const string harmonyID = "algernon-A.csl.realpop2";

        // Flag.
        internal static bool Patched => _patched;
        private static bool _patched = false;


        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public static void PatchAll()
        {
            // Don't do anything if already patched.
            if (!_patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Logging.Message("deploying Harmony patches");

                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(harmonyID);
                    harmonyInstance.PatchAll();
                    _patched = true;
                }
                else
                {
                    Logging.Message("Harmony not ready");
                }
            }
        }


        /// <summary>
        /// Remove all Harmony patches.
        /// </summary>
        public static void UnpatchAll()
        {
            // Only unapply if patches appplied.
            if (_patched)
            {
                Logging.Message("reverting Harmony patches");

                // Unapply patches, but only with our HarmonyID.
                Harmony harmonyInstance = new Harmony(harmonyID);
                harmonyInstance.UnpatchAll(harmonyID);
                _patched = false;
            }
        }


        /// <summary>
        /// Patch Advanced Building Level Control's 'CustomBuildingUpgraded' method.
        /// </summary>
        internal static void PatchABLC()
        {
            // Ensure Harmony is ready before patching.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                // Try to get ABLC method.
                MethodInfo ablcCustomUpgraded = AssemblyUtils.ABLCCustomUpgraded();
                if (ablcCustomUpgraded != null)
                {
                    // Got method - apply patch.
                    Logging.Message("patching ABLC.LevelUtils.CustomBuildingUpgraded");
                    Harmony harmonyInstance = new Harmony(harmonyID);
                    harmonyInstance.Patch(ablcCustomUpgraded, postfix: new HarmonyMethod(typeof(ABLCBuildingUpgradedPatch).GetMethod("Postfix")));
                }
            }
        }
    }
}