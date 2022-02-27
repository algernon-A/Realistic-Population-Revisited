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
            // Check to see if school level is elementary or high school.
            BuildingInfo thisInfo = __instance.m_info;
            if (thisInfo.GetClassLevel() <= ItemClass.Level.Level2)
            {
                // It's a school - check to see if we're using custom school calculations.
                if (ModSettings.EnableSchoolPop)
                {
                    // Custom calcs enabled - set the result to our realistic population lookup.
                    __result = PopData.instance.Students(thisInfo);

                    // Don't continue on to original method.
                    return false;
                }

                // Not using custom calcs - ensure default is set.
                __instance.m_studentCount = SchoolData.instance.OriginalStudentCount(thisInfo);
            }

            // Not using realistic school populations - continue on to original method.
            return true;
        }
    }
}