// <copyright file="LegacyCommercialPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Legacy WG commercial calculation pack.
    /// </summary>
    internal class LegacyCommercialPack : PopDataPack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyCommercialPack"/> class.
        /// </summary>
        internal LegacyCommercialPack()
            : base(DataVersion.Legacy, ItemClass.Service.Commercial)
        {
        }

        /// <summary>
        /// Returns the legacy calculation workplaces for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count. </returns>
        internal override PopData.WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array = LegacyAIUtils.GetCommercialArray(buildingPrefab, level);
            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 4, ref array);
        }
    }
}