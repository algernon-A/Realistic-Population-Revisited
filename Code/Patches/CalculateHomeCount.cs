using ColossalFramework.Math;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch to implement population count changes for residential buildings.
    /// </summary>
    [HarmonyPatch(typeof(ResidentialBuildingAI), nameof(ResidentialBuildingAI.CalculateHomeCount))]
    public static class RealisticHomeCount
    {
        /// <summary>
        /// Harmony Prefix patch to ResidentialBuildingAI.CalculateHomeCount to implement mod population calculations.
        /// </summary>
        /// <param name="__result">Original method result</param>
        /// <param name="__instance">Original AI instance reference</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer (unused)</param>
        /// <param name="width">Building lot width (unused)</param>
        /// <param name="length">Building lot length (unused)</param>
        /// <returns>False (never execute original method) if anything other than vanilla calculations are set for the building, true (fall through to game code) otherwise</returns>
        public static bool Prefix(ref int __result, ResidentialBuildingAI __instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            // Get population value from cache.
            int result = PopData.instance.HouseholdCache(__instance.m_info, (int)level);

            // Always set at least one.
            if (result < 1)
            {
                Logging.Error("invalid homecount result ", result, " for ", __instance.m_info.name, "; setting to 1");
                result = 1;
            }

            // Check for vanilla calc setting.
            else if (result == ushort.MaxValue)
            {
                // Vanilla calculations; fall through to original game code.
                return true;
            }

            // Don't execute base method after this.
            __result = result;
            return false;
        }
    }
}
