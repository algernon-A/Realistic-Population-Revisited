// <copyright file="VanillaPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Vanilla calculation pack.
    /// </summary>
    internal class VanillaPack : PopDataPack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VanillaPack"/> class.
        /// </summary>
        internal VanillaPack()
            : base(DataVersion.Vanilla, ItemClass.Service.None)
        {
        }

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
            ushort value = PopData.Instance.GetOverride(buildingPrefab.name);
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
            ushort customValue = PopData.Instance.GetOverride(buildingPrefab.name);
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
                Level3 = ushort.MaxValue,
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
                schoolAI.m_studentCount = SchoolData.Instance.OriginalStudentCount(buildingPrefab);
                return VanillaPopMethods.StudentCount(schoolAI);
            }

            // If we got here, no valid vanilla settings were found; return 0.
            return 0;
        }
    }
}