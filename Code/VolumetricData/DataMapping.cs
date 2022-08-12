// <copyright file="DataMapping.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Class for mapping serives to DataStore structures.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Internal reference arrays")]
    internal class DataMapping
    {
        /// <summary>
        /// Number of data structures (service/subservice combinations) to map.
        /// </summary>
        internal const int NumData = 16;

        /// <summary>
        /// Service reference array.
        /// </summary>
        internal readonly ItemClass.Service[] Services;

        /// <summary>
        /// Subservice reference array.
        /// </summary>
        internal readonly ItemClass.SubService[] SubServices;

        /// <summary>
        /// Data arrays.
        /// </summary>
        internal readonly int[][][] DataArrays;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataMapping"/> class.
        /// </summary>
        internal DataMapping()
        {
            // Services.
            Services = new ItemClass.Service[NumData]
            {
                ItemClass.Service.Residential,
                ItemClass.Service.Residential,
                ItemClass.Service.Residential,
                ItemClass.Service.Residential,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Commercial,
                ItemClass.Service.Office,
                ItemClass.Service.Office,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
                ItemClass.Service.Industrial,
            };

            // Sub-services.
            SubServices = new ItemClass.SubService[NumData]
            {
                ItemClass.SubService.ResidentialLow,
                ItemClass.SubService.ResidentialHigh,
                ItemClass.SubService.ResidentialLowEco,
                ItemClass.SubService.ResidentialHighEco,
                ItemClass.SubService.CommercialLow,
                ItemClass.SubService.CommercialHigh,
                ItemClass.SubService.CommercialEco,
                ItemClass.SubService.CommercialLeisure,
                ItemClass.SubService.CommercialTourist,
                ItemClass.SubService.OfficeGeneric,
                ItemClass.SubService.OfficeHightech,
                ItemClass.SubService.IndustrialGeneric,
                ItemClass.SubService.IndustrialFarming,
                ItemClass.SubService.IndustrialForestry,
                ItemClass.SubService.IndustrialOil,
                ItemClass.SubService.IndustrialOre,
            };

            // Data arrays.
            DataArrays = new int[NumData][][]
            {
                DataStore.residentialLow,
                DataStore.residentialHigh,
                DataStore.resEcoLow,
                DataStore.resEcoHigh,
                DataStore.commercialLow,
                DataStore.commercialHigh,
                DataStore.commercialEco,
                DataStore.commercialLeisure,
                DataStore.commercialTourist,
                DataStore.office,
                DataStore.officeHighTech,
                DataStore.industry,
                DataStore.industry_farm,
                DataStore.industry_forest,
                DataStore.industry_oil,
                DataStore.industry_ore,
            };
        }

        /// <summary>
        /// Returns the DataStore array for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="subService">Sub-service.</param>
        /// <returns>DataStore array (null if no match).</returns>
        internal int[][] GetArray(ItemClass.Service service, ItemClass.SubService subService)
        {
            // Iterate through each data structure.
            for (int i = 0; i < NumData; ++i)
            {
                // Look for a service and sub-service match.
                if (service == Services[i] && subService == SubServices[i])
                {
                    // Matched - return the relevant DataStore array.
                    return DataArrays[i];
                }
            }

            // If we got here, no match was found; return null.
            return null;
        }
    }
}
