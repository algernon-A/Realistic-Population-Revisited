// <copyright file="PopDataPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Population clculation data pack - provides parameters for calculating building populations.
    /// </summary>
    internal class PopDataPack : DataPack
    {
        // Service restrictions.
        private readonly ItemClass.Service _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopDataPack"/> class.
        /// </summary>
        /// <param name="version">Datapack version.</param>
        /// <param name="service">Service restrictions.</param>
        internal PopDataPack(DataVersion version, ItemClass.Service service)
            : base(version)
        {
            _service = service;
        }

        /// <summary>
        /// Gets any assigned service applicable to this pack.
        /// </summary>
        internal ItemClass.Service Service => _service;

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
                    ushort value = PopData.Instance.GetOverride(buildingPrefab.name);
                    if (value > 0)
                    {
                        // Manual override present - use that value.
                        return value;
                    }

                    // No override - use pack value.
                    return Population(buildingPrefab, (int)buildingPrefab.m_class.m_level, Multipliers.Instance.ActiveMultiplier(buildingPrefab));
                }

                // Custom school settings not enabled; use default.
                return SchoolData.Instance.OriginalStudentCount(buildingPrefab);
            }

            // If we got here, it's not a school building; return 0.
            return 0;
        }
    }
}