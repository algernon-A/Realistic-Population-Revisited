// <copyright file="CalcPacks.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;

    internal enum DataVersion
    {
        vanilla = 0,
        legacy,
        one,
        customOne,
        overrideOne,
    }

    /// <summary>
    /// Calculation data pack - provides parameters for calculating building populations for given services and (optional) subservices.
    /// </summary>
    internal class DataPack
    {
        // Basic data.
        internal DataVersion version;
        internal string name;
        internal string nameKey;
        internal string descriptionKey;

        /// <summary>
        /// Pack display name, in current language if available.
        /// </summary>
        internal string DisplayName => !string.IsNullOrEmpty(nameKey) ? Translations.Translate(nameKey) : name;

        /// <summary>
        /// Pack description, in current language if available.
        /// </summary>
        internal string Description => !string.IsNullOrEmpty(descriptionKey) ? Translations.Translate(descriptionKey) : string.Empty;
    }

    /// <summary>
    /// Floor calculation data pack - provides parameters for calculating building floors.
    /// </summary>
    internal class FloorDataPack : DataPack
    {
        // Height per floor in metres.
        internal float floorHeight;

        // Height needs to be at least this high, in metres, for any floor to exist.
        internal float firstFloorMin;

        // Extend first floor height by this additional amount, in metres.
        internal float firstFloorExtra;

        // True if the first floor should be excluded from calculations (e.g. for foyers/lobbies).
        internal bool firstFloorEmpty;
    }

    /// <summary>
    /// Population clculation data pack - provides parameters for calculating building populations.
    /// </summary>
    internal class PopDataPack : DataPack
    {
        // Service restrictions.
        internal ItemClass.Service service;

        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <param name="multiplier">Population multiplier.</param>
        /// <returns>Population.</returns>
        internal virtual ushort Population(BuildingInfo buildingPrefab, int level, float multiplier) => 0;

        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count.</returns>
        internal virtual PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => new PopData.WorkplaceLevels { Level0 = 1, Level1 = 0, Level2 = 0, Level3 = 0 };

        /// <summary>
        /// Returns the student count for the given building prefab.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <returns>Student count (0 if not a school building).</returns>
        internal virtual int Students(BuildingInfo buildingPrefab)
        {
            // Check for school.
            if (buildingPrefab.m_buildingAI is SchoolAI)
            {
                // It's a school; are custom school calcs enabled?
                if (ModSettings.EnableSchoolPop)
                {
                    // Custom calcs enabled - check for any override.
                    ushort value = PopData.instance.GetOverride(buildingPrefab.name);
                    if (value > 0)
                    {
                        // Manual override present - use that value.
                        return value;
                    }
                    // No override - use pack value.
                    return Population(buildingPrefab, (int)buildingPrefab.m_class.m_level, Multipliers.instance.ActiveMultiplier(buildingPrefab));
                }

                // Custom school settings not enabled; use default.
                return SchoolData.instance.OriginalStudentCount(buildingPrefab);
            }

            // If we got here, it's not a school building; return 0.
            return 0;
        }
    }

    /// <summary>
    /// Population calculation data pack - provides parameters for calculating building populations.
    /// </summary>
    internal class SchoolDataPack : DataPack
    {
        // Education ItemClass.Level.
        internal ItemClass.Level level;

        // Employment calculation arrays.
        internal int[] baseWorkers;
        internal int[] perWorker;

        // Cost and maintenance fields.
        internal int baseCost;
        internal int costPer;
        internal int baseMaint;
        internal int maintPer;
    }

    /// <summary>
    /// Struct holding data for volumetric calculations for a given building level.
    /// </summary>
    internal struct LevelData
    {
        // Empty area (if any) to be subtracted from building before any calculations are made.
        internal float emptyArea;

        // Empty area (if any) as a percentage to to be subtracted from each floor area AFTER fixed empty area (above) is subtracted.
        internal int emptyPercent;

        // Area per unit (household/worker), in square metres.
        internal float areaPer;

        // True if unit areas should be calculated as though they extend through all levels (ground to roof - e.g. detached housing, rowhouses, etc.),
        // false if units should only be treated as single-floor entities (e.g. apartment blocks).
        // Generally speaking, true for low-density, false for high-density.
        internal bool multiFloorUnits;
    }

    /// <summary>
    /// Volumetric calculation data pack.
    /// </summary>
    internal class VolumetricPopPack : PopDataPack
    {
        // Building level records.
        internal LevelData[] levels;

        /// <summary>
        /// Returns the volumetric population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <param name="multiplier">Population multiplier.</param>
        /// <returns>Population</returns>
        internal override ushort Population(BuildingInfo buildingPrefab, int level, float multiplier)
        {
            // Bounds check for building level (zero-based).
            int thisLevel = level;
            if (thisLevel >= levels.Length)
            {
                Logging.Error("Building level out of range: ", level);
                
                // Set level to maximum (zero-based, so subtract one from levels.Length).
                thisLevel = levels.Length - 1;
            }
            return PopData.instance.VolumetricPopulation(buildingPrefab.m_generatedInfo, levels[thisLevel], (FloorDataPack)FloorData.instance.ActivePack(buildingPrefab), multiplier);
        }

        /// <summary>
        /// Returns the workplace breakdown for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count.</returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => EmploymentData.CalculateWorkplaces(buildingPrefab, level);
    }

    /// <summary>
    /// Legacy WG residential calculation pack.
    /// </summary>
    internal class LegacyResPack : PopDataPack
    {
        /// <summary>
        /// Returns the legacy calculation workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <param name="multiplier">Ignored</param>
        /// <returns>Population</returns>
        internal override ushort Population(BuildingInfo buildingPrefab, int level, float multiplier)
        {
            // First, check for volumetric population override - that trumps everything else.
            ushort value = PopData.instance.GetOverride(buildingPrefab.name);
            if (value == 0)
            {
                // No volumetric override - use legacy calcs.
                int[] array = LegacyAIUtils.GetResidentialArray(buildingPrefab, (int)level);
                return LegacyAIUtils.CalculatePrefabHousehold(buildingPrefab.GetWidth(), buildingPrefab.GetWidth(), ref buildingPrefab, ref array);
            }

            return value;
        }
    }

    /// <summary>
    /// Legacy WG commercial calculation pack.
    /// </summary>
    internal class LegacyComPack : PopDataPack
    {
        /// <summary>
        /// Returns the legacy calculation workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array = LegacyAIUtils.GetCommercialArray(buildingPrefab, level);
            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 4, ref array);
        }
    }

    /// <summary>
    /// Legacy WG industrial calculation pack.
    /// </summary>
    internal class LegacyIndPack : PopDataPack
    {
        /// <summary>
        /// Returns the legacy calculation workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count.</returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array;
            int minWorkers;

            // Need to test if we're an extractor or not for this one.
            if (buildingPrefab.GetAI() is IndustrialExtractorAI)
            {
                array = LegacyAIUtils.GetExtractorArray(buildingPrefab);
                minWorkers = 3;
            }
            else
            {
                array = LegacyAIUtils.GetIndustryArray(buildingPrefab, level);
                minWorkers = 4;
            }

            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, minWorkers, ref array);
        }
    }

    /// <summary>
    /// Legacy WG office calculation pack.
    /// </summary>
    internal class LegacyOffPack : PopDataPack
    {
        /// <summary>
        /// Returns the legacy calculation workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count.</returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array = LegacyAIUtils.GetOfficeArray(buildingPrefab, level);
            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 10, ref array);
        }
    }

    /// <summary>
    /// Vanilla calculation pack.
    /// </summary>
    internal class VanillaPack : PopDataPack
    {
        /// <summary>
        /// Returns the vanilla calculation workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <param name="multiplier">Ignored.</param>
        /// <returns>Population.</returns>
        internal override ushort Population(BuildingInfo buildingPrefab, int level, float multiplier)
        {
            // First, check for volumetric population override - that trumps everything else.
            ushort value = PopData.instance.GetOverride(buildingPrefab.name);
            if (value == 0)
            {
                // No volumetric override - use short.MaxValue; the Prefix patch will detect this and fall through to game code.
                return ushort.MaxValue;
            }

            return value;
        }

        /// <summary>
        /// Returns the vanilla workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdown.</returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            // First, check for volumetric population override - that trumps everything else.
            ushort customValue = PopData.instance.GetOverride(buildingPrefab.name);
            if (customValue > 0)
            {
                // Active override - calculate workplace level breakdown.
                return EmploymentData.CalculateWorkplaces(buildingPrefab, level, customValue);
            }

            // No override - for vanilla, we just set all workplace levels to ushort.MaxValue; the Prefix patch will detect this and fall through to game code.
            return new PopData.WorkplaceLevels
            {
                Level0 = ushort.MaxValue,
                Level1 = ushort.MaxValue,
                Level2 = ushort.MaxValue,
                Level3 = ushort.MaxValue
            };
        }

        /// <summary>
        /// Returns the vanilla student count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <returns>Workplace breakdown.</returns>
        internal override int Students(BuildingInfo buildingPrefab)
        {
            // Set m_studentCount to original value.
            if (buildingPrefab.m_buildingAI is SchoolAI schoolAI)
            {
                schoolAI.m_studentCount = SchoolData.instance.OriginalStudentCount(buildingPrefab);
                return VanillaPopMethods.StudentCount(schoolAI);
            }

            // If we got here, no valid vanilla settings were found; return 0.
            return 0;
        }
    }
}