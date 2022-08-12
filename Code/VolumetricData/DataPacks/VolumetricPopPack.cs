// <copyright file="VolumetricPopPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;

    /// <summary>
    /// Volumetric calculation data pack.
    /// </summary>
    internal class VolumetricPopPack : PopDataPack
    {
        // Building level records.
        private LevelData[] _levels;

        /// <summary>
        /// Initializes a new instance of the <see cref="VolumetricPopPack"/> class.
        /// </summary>
        /// <param name="version">Datapack version.</param>
        /// <param name="service">Service restrictions.</param>
        internal VolumetricPopPack(DataVersion version, ItemClass.Service service)
            : base(version, service)
        {
            // Initialise level array based on service.
            switch (service)
            {
                case ItemClass.Service.Residential:
                    _levels = new LevelData[5];
                    break;

                case ItemClass.Service.Industrial:
                case ItemClass.Service.Commercial:
                case ItemClass.Service.Office:
                    _levels = new LevelData[3];
                    break;

                case ItemClass.Service.Education:
                    _levels = new LevelData[2];
                    break;

                default:
                    _levels = new LevelData[1];
                    break;
            }
        }

        /// <summary>
        /// Gets the level record array.
        /// </summary>
        internal LevelData[] Levels => _levels;

        /// <summary>
        /// Returns the volumetric population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <param name="multiplier">Population multiplier.</param>
        /// <returns>Population.</returns>
        internal override ushort Population(BuildingInfo buildingPrefab, int level, float multiplier)
        {
            // Bounds check for building level (zero-based).
            int thisLevel = level;
            if (thisLevel >= _levels.Length)
            {
                Logging.Error("Building level out of range: ", level);

                // Set level to maximum (zero-based, so subtract one from levels.Length).
                thisLevel = _levels.Length - 1;
            }

            return PopData.Instance.VolumetricPopulation(buildingPrefab.m_generatedInfo, _levels[thisLevel], (FloorDataPack)FloorData.Instance.ActivePack(buildingPrefab), multiplier);
        }

        /// <summary>
        /// Returns the workplace breakdown for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count.</returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => EmploymentData.CalculateWorkplaces(buildingPrefab, level);

        /// <summary>
        /// Struct holding data for volumetric calculations for a given building level.
        /// </summary>
        internal struct LevelData
        {
            /// <summary>
            /// Empty area (if any) to be subtracted from building before any calculations are made.
            /// </summary>
            internal float EmptyArea;

            /// <summary>
            /// Empty area (if any) as a percentage to to be subtracted from each floor area AFTER fixed empty area (above) is subtracted.
            /// </summary>
            internal int EmptyPercent;

            /// <summary>
            /// Area per unit (household/worker), in square metres.
            /// </summary>
            internal float AreaPer;

            /// <summary>
            /// True if unit areas should be calculated as though they extend through all levels (ground to roof - e.g. detached housing, rowhouses, etc.),
            /// false if units should only be treated as single-floor entities (e.g. apartment blocks).
            /// Generally speaking, true for low-density, false for high-density.
            /// </summary>
            internal bool MultiFloorUnits;
        }
    }
}