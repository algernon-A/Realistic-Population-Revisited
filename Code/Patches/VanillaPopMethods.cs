using System;
using System.Runtime.CompilerServices;
using ColossalFramework.Math;
using HarmonyLib;

namespace RealPop2
{
    /// <summary>
    /// Class to access vanilla population calculation methods.
    /// </summary>
    [HarmonyPatch]
    public static class VanillaPopMethods
    {
        /// <summary>
        /// Returns the vanilla employment totals for the given building AI and details.
        /// </summary>
        /// <param name="buildingAI">Building AI instance</param>
        /// <param name="level">Building level</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <param name="level0">Level 0 workplaces</param>
        /// <param name="level1">Level 1 workplaces</param>
        /// <param name="level2">Level 2 workplaces</param>
        /// <param name="level3">Level 3 workplaces</param>
        public static void WorkplaceCount(BuildingAI buildingAI, ItemClass.Level level, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            // Setup randomizer
            Randomizer randomizer = new Randomizer();

            switch (buildingAI)
            {
                case IndustrialBuildingAI indAI:
                    IndustrialWorkplaceCount(indAI, level, randomizer, width, length, out level0, out level1, out level2, out level3);
                    return;
                case IndustrialExtractorAI indAI:
                    ExtractorWorkplaceCount(indAI, level, randomizer, width, length, out level0, out level1, out level2, out level3);
                    return;
                case OfficeBuildingAI indAI:
                    OfficeWorkplaceCount(indAI, level, randomizer, width, length, out level0, out level1, out level2, out level3);
                    return;
                case CommercialBuildingAI comAI:
                    CommercialWorkplaceCount(comAI, level, randomizer, width, length, out level0, out level1, out level2, out level3);
                    return;
                default:
                    Logging.Error("invalid building AI passed to VanillaPopMehtods.WorkplaceCount");
                    level0 = 0;
                    level1 = 0;
                    level2 = 0;
                    level3 = 0;
                    return;
            }
        }


        /// <summary>
        /// Reverse patch for ResidentialBuildingAI.CalculateHomeCount to access original game method without any Harmony patches (including ours).
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied</exception>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(ResidentialBuildingAI)), nameof(ResidentialBuildingAI.CalculateHomeCount))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int CalculateHomeCount(object instance, ItemClass.Level level, Randomizer r, int width, int length)
        {
            string message = "ResidentialBuildingAI.CalculateHomeCount reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, level, r, width, length);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for CommercialBuildingAI.CalculateWorkplaceCount to access original game method without any Harmony patches (including ours).
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <param name="level0">Level 0 workplaces</param>
        /// <param name="level1">Level 1 workplaces</param>
        /// <param name="level2">Level 2 workplaces</param>
        /// <param name="level3">Level 3 workplaces</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied</exception>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(CommercialBuildingAI)), nameof(CommercialBuildingAI.CalculateWorkplaceCount))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void CommercialWorkplaceCount(object instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            level0 = 0;
            level1 = 0;
            level2 = 0;
            level3 = 0;
            string message = "CommercialBuildingAI.CalculateWorkplaceCount reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, level, r, width, length);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for IndustrialBuildingAI.CalculateWorkplaceCount to access original game method without any Harmony patches (including ours).
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <param name="level0">Level 0 workplaces</param>
        /// <param name="level1">Level 1 workplaces</param>
        /// <param name="level2">Level 2 workplaces</param>
        /// <param name="level3">Level 3 workplaces</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied</exception>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(IndustrialBuildingAI)), nameof(IndustrialBuildingAI.CalculateWorkplaceCount))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void IndustrialWorkplaceCount(object instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            level0 = 0;
            level1 = 0;
            level2 = 0;
            level3 = 0;
            string message = "IndustrialBuildingAI.CalculateWorkplaceCount reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, level, r, width, length);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for IndustrialExtractorAI.CalculateWorkplaceCount to access original game method without any Harmony patches (including ours).
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <param name="level0">Level 0 workplaces</param>
        /// <param name="level1">Level 1 workplaces</param>
        /// <param name="level2">Level 2 workplaces</param>
        /// <param name="level3">Level 3 workplaces</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied</exception>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(IndustrialExtractorAI)), nameof(IndustrialExtractorAI.CalculateWorkplaceCount))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ExtractorWorkplaceCount(object instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            level0 = 0;
            level1 = 0;
            level2 = 0;
            level3 = 0;
            string message = "IndustrialExtractorAI.CalculateWorkplaceCount reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, level, r, width, length);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for OfficeBuildingAI.CalculateWorkplaceCount to access original game method without any Harmony patches (including ours).
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <param name="level0">Level 0 workplaces</param>
        /// <param name="level1">Level 1 workplaces</param>
        /// <param name="level2">Level 2 workplaces</param>
        /// <param name="level3">Level 3 workplaces</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied</exception>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(OfficeBuildingAI)), nameof(OfficeBuildingAI.CalculateWorkplaceCount))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void OfficeWorkplaceCount(object instance, ItemClass.Level level, Randomizer r, int width, int length, out int level0, out int level1, out int level2, out int level3)
        {
            level0 = 0;
            level1 = 0;
            level2 = 0;
            level3 = 0;
            string message = "IndustrialBuildingAI.CalculateWorkplaceCount reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, level, r, width, length);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for SchoolAI.StudentCount to access original game method without any Harmony patches (including ours).
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="level">Building level</param>
        /// <param name="r">Randomizer</param>
        /// <param name="width">Building lot width (in cells)</param>
        /// <param name="length">Building lot depth (in cells)</param>
        /// <exception cref="NotImplementedException">Harmony reverse patch wasn't applied</exception>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(SchoolAI)), nameof(SchoolAI.StudentCount), MethodType.Getter)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int StudentCount(object instance)
        {
            string message = "SchoolAI.StudentCount reverse Harmony patch wasn't applied";
            Logging.Error(message, instance);
            throw new NotImplementedException(message);
        }
    }
}