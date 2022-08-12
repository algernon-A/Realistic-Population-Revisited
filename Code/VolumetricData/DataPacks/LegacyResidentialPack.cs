// <copyright file="LegacyResidentialPack.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Legacy WG residential calculation pack.
    /// </summary>
    internal class LegacyResidentialPack : PopDataPack
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyResidentialPack"/> class.
        /// </summary>
        internal LegacyResidentialPack()
            : base(DataVersion.Legacy, ItemClass.Service.Residential)
        {
        }

        /// <summary>
        /// Returns the legacy calculation workplaces for the given building prefab and level.
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
                // No volumetric override - use legacy calcs.
                int[] array = LegacyAIUtils.GetResidentialArray(buildingPrefab, (int)level);
                return LegacyAIUtils.CalculatePrefabHousehold(buildingPrefab.GetWidth(), buildingPrefab.GetWidth(), ref buildingPrefab, ref array);
            }

            return value;
        }
    }
}