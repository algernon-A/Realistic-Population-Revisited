// <copyright file="Workplaces.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Harmony patch to implement population count changes for workplace buildings.
    /// </summary>
    [HarmonyPatch]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    public static class Workplaces
    {
        /// <summary>
        /// Target method to patch - CalculateWorkplaceCount methods of base-game workplace AIs.
        /// </summary>
        /// <returns>List of methods to patch.</returns>
        public static IEnumerable<MethodBase> TargetMethods()
        {
            string methodName = "CalculateWorkplaceCount";

            Logging.Message("patching ", methodName);

            yield return typeof(CommercialBuildingAI).GetMethod(methodName);
            yield return typeof(OfficeBuildingAI).GetMethod(methodName);
            yield return typeof(IndustrialBuildingAI).GetMethod(methodName);
            yield return typeof(IndustrialExtractorAI).GetMethod(methodName);
        }

        /// <summary>
        /// Harmony pre-emptive Prefix patch to replace game workplace calculations with the mod's.
        /// </summary>
        /// <param name="__instance">Original AI instance reference.</param>
        /// <param name="level">Building level.</param>
        /// <param name="level0">Uneducated worker count output.</param>
        /// <param name="level1">Educated worker count output.</param>
        /// <param name="level2">Well-educated worker count output.</param>
        /// <param name="level3">Highly-educated worker count output.</param>
        /// <returns>False (never execute original method) if anything other than vanilla calculations are set for the building, true (fall through to game code) otherwise.</returns>
        public static bool Prefix(PrivateBuildingAI __instance, ItemClass.Level level, ref int level0, ref int level1, ref int level2, ref int level3)
        {
            // Get cached workplace count.
            PopData.WorkplaceLevels workplaces = PopData.Instance.WorkplaceCache(__instance.m_info, (int)level);

            // Check for vanilla calc setting.
            if (workplaces.Level0 == ushort.MaxValue)
            {
                // Vanilla calculations; fall through to original game code.
                return true;
            }

            // Set return values.
            level0 = workplaces.Level0;
            level1 = workplaces.Level1;
            level2 = workplaces.Level2;
            level3 = workplaces.Level3;

            // Don't execute base method after this.
            return false;
        }
    }
}