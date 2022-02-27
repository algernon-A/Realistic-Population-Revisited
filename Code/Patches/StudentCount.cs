using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony patch for realistic student counts.
    /// </summary>
    [HarmonyPatch(typeof(SchoolAI), nameof(SchoolAI.StudentCount), MethodType.Getter)]
    public static class StudentCountPatch
    {
        /// <summary>
        /// Harmony SchoolAI.StudentCount Prefix getter patch to return realistic student counts (if enabled).
        /// </summary>
        /// <param name="__instance">SchoolAI instance</param>
        /// <param name="__result">Method result</param>
        /// <returns></returns>
        public static bool Prefix(SchoolAI __instance, ref int __result)
        {
            // Check to see if we're using realistic school populations, and school level is elementary or high school.
            if (ModSettings.EnableSchoolPop && __instance.m_info.GetClassLevel() <= ItemClass.Level.Level2)
            {
                // We are - set the result to our realistic population lookup.
                BuildingInfo thisInfo = __instance.m_info;
                __result = PopData.instance.Students(thisInfo);

                // Don't continue on to original method.
                return false;
            }

            // Not using realistic school populations - continue on to original method.
            return true;
        }
    }
}