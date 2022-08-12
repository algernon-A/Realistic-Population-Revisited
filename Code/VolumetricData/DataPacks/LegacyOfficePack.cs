// <copyright file="LegacyOfficePack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;

    /// <summary>
    /// Legacy WG office calculation pack.
    /// </summary>
    internal class LegacyOfficePack : PopDataPack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyOfficePack"/> class.
        /// </summary>
        internal LegacyOfficePack()
            : base(DataVersion.Legacy, ItemClass.Service.Office)
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
            int[] array = LegacyAIUtils.GetOfficeArray(buildingPrefab, level);
            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 10, ref array);
        }
    }
}