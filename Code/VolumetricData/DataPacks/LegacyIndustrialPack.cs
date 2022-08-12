// <copyright file="LegacyIndustrialPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Legacy WG industrial calculation pack.
    /// </summary>
    internal class LegacyIndustrialPack : PopDataPack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyIndustrialPack"/> class.
        /// </summary>
        internal LegacyIndustrialPack()
            : base(DataVersion.Legacy, ItemClass.Service.Industrial)
        {
        }

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
}