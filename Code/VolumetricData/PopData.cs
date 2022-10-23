// <copyright file="PopData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using AlgernonCommons;

    /// <summary>
    /// Centralised store and management of population calculation data.
    /// </summary>
    internal class PopData : CalcData
    {
        // Instance reference.
        private static PopData s_instance;

        // Household, workplace, and visitplace calculation result caches (so we don't have to do the full calcs every SimulationStep for every building....).
        private readonly Dictionary<BuildingInfo, HouseholdCacheEntry> _householdCache;
        private readonly Dictionary<BuildingInfo, WorkplaceCacheEntry> _workplaceCache;
        private readonly Dictionary<BuildingInfo, VisitplaceCacheEntry> _visitplaceCache;

        // Dictionary of manual population count overrides.
        private readonly Dictionary<string, ushort> _overrides;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopData"/> class.
        /// </summary>
        public PopData()
        {
            // Create caches.
            _householdCache = new Dictionary<BuildingInfo, HouseholdCacheEntry>();
            _workplaceCache = new Dictionary<BuildingInfo, WorkplaceCacheEntry>();
            _visitplaceCache = new Dictionary<BuildingInfo, VisitplaceCacheEntry>();

            // Legacy residential.
            CalcPacks.Add(new LegacyResidentialPack()
            {
                Name = "resWG",
                NameKey = "RPR_PCK_LEG_NAM",
                DescriptionKey = "RPR_PCK_LEG_DES",
            });

            // Legacy industrial.
            CalcPacks.Add(new LegacyIndustrialPack()
            {
                Name = "indWG",
                NameKey = "RPR_PCK_LEG_NAM",
                DescriptionKey = "RPR_PCK_LEG_DES",
            });

            // Legacy commercial.
            CalcPacks.Add(new LegacyCommercialPack()
            {
                Name = "comWG",
                NameKey = "RPR_PCK_LEG_NAM",
                DescriptionKey = "RPR_PCK_LEG_DES",
            });

            // Legacy office.
            CalcPacks.Add(new LegacyOfficePack
            {
                Name = "offWG",
                NameKey = "RPR_PCK_LEG_NAM",
                DescriptionKey = "RPR_PCK_LEG_DES",
            });

            // Vanilla calcs.
            CalcPacks.Add(new VanillaPack
            {
                Name = "vanilla",
                NameKey = "RPR_PCK_VAN_NAM",
                DescriptionKey = "RPR_PCK_VAN_DES",
            });

            // Low-density residential.
            VolumetricPopPack newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Residential)
            {
                Name = "reslow",
                NameKey = "RPR_PCK_RLS_NAM",
                DescriptionKey = "RPR_PCK_RLS_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -1f, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -1f, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -1f, MultiFloorUnits = true };
            newPack.Levels[3] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -1f, MultiFloorUnits = true };
            newPack.Levels[4] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -1f, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // Duplexes.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Residential)
            {
                Name = "duplex",
                NameKey = "RPR_PCK_RLD_NAM",
                DescriptionKey = "RPR_PCK_RLD_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -2f, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -2f, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -2f, MultiFloorUnits = true };
            newPack.Levels[3] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -2f, MultiFloorUnits = true };
            newPack.Levels[4] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = -2f, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // European apartments (modern).
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Residential)
            {
                Name = "resEUmod",
                NameKey = "RPR_PCK_REM_NAM",
                DescriptionKey = "RPR_PCK_REM_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 10, AreaPer = 85f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 12, AreaPer = 90f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 16, AreaPer = 100f, MultiFloorUnits = false };
            newPack.Levels[3] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 18, AreaPer = 105f, MultiFloorUnits = false };
            newPack.Levels[4] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 18, AreaPer = 110f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // European apartments (older).
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Residential)
            {
                Name = "resEUold",
                NameKey = "RPR_PCK_REO_NAM",
                DescriptionKey = "RPR_PCK_REO_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 75f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 1, AreaPer = 80f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 2, AreaPer = 85f, MultiFloorUnits = false };
            newPack.Levels[3] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 3, AreaPer = 90f, MultiFloorUnits = false };
            newPack.Levels[4] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 4, AreaPer = 95f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // High-density residential US - empty percentages from AssetInsights.net, areas from 2018 RentCafe.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Residential)
            {
                Name = "reshighUS",
                NameKey = "RPR_PCK_RUH_NAM",
                DescriptionKey = "RPR_PCK_RUH_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 36, AreaPer = 70f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 37, AreaPer = 78f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 38, AreaPer = 86f, MultiFloorUnits = false };
            newPack.Levels[3] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 39, AreaPer = 94f, MultiFloorUnits = false };
            newPack.Levels[4] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 40, AreaPer = 102f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // US commmercial.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "comUS",
                NameKey = "RPR_PCK_CUS_NAM",
                DescriptionKey = "RPR_PCK_CUS_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0, EmptyPercent = 0, AreaPer = 37, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0, EmptyPercent = 0, AreaPer = 37, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0, EmptyPercent = 0, AreaPer = 37, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // UK commercial.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "comUK",
                NameKey = "RPR_PCK_CUK_NAM",
                DescriptionKey = "RPR_PCK_CUK_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 20f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 17.5f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 15f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Retail warehouses.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "retailware",
                NameKey = "RPR_PCK_CRW_NAM",
                DescriptionKey = "RPR_PCK_CRW_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 90f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 90f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 90f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Hotels.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "hotel",
                NameKey = "RPR_PCK_THT_NAM",
                DescriptionKey = "RPR_PCK_THT_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 120f, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 120f, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 120f, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // Restaurants and cafes.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "restaurant",
                NameKey = "RPR_PCK_LFD_NAM",
                DescriptionKey = "RPR_PCK_LFD_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 20f, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 17.5f, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 15, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // Entertainment centres.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "entertainment",
                NameKey = "RPR_PCK_LEN_NAM",
                DescriptionKey = "RPR_PCK_LEN_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 20f, EmptyPercent = 20, AreaPer = 70f, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 20f, EmptyPercent = 20, AreaPer = 65f, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 20f, EmptyPercent = 20, AreaPer = 60f, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // Cinemas.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GIA to NIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Commercial)
            {
                Name = "cinema",
                NameKey = "RPR_PCK_LCN_NAM",
                DescriptionKey = "RPR_PCK_LCN_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 5, AreaPer = 200f, MultiFloorUnits = true };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 5, AreaPer = 200f, MultiFloorUnits = true };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 5, AreaPer = 200f, MultiFloorUnits = true };
            CalcPacks.Add(newPack);

            // Light industry.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Industrial)
            {
                Name = "lightind",
                NameKey = "RPR_PCK_ILG_NAM",
                DescriptionKey = "RPR_PCK_ILG_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 47f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 47f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 47f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Manufacturing.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Industrial)
            {
                Name = "factory",
                NameKey = "RPR_PCK_IMN_NAM",
                DescriptionKey = "RPR_PCK_IMN_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 5, AreaPer = 36f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 5, AreaPer = 36f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 5, AreaPer = 36f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Industry warehouses (local distribution).
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Industrial)
            {
                Name = "localware",
                NameKey = "RPR_PCK_IWL_NAM",
                DescriptionKey = "RPR_PCK_IWL_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 70f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 70f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 70f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Industry warehouses (national distribution).
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Industrial)
            {
                Name = "natware",
                NameKey = "RPR_PCK_IWN_NAM",
                DescriptionKey = "RPR_PCK_IWN_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 95f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 95f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 95f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Corporate offices.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Office)
            {
                Name = "offcorp",
                NameKey = "RPR_PCK_OCP_NAM",
                DescriptionKey = "RPR_PCK_OCP_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 13f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 13f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 20, AreaPer = 13f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Financial offices.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Office)
            {
                Name = "offfin",
                NameKey = "RPR_PCK_OFN_NAM",
                DescriptionKey = "RPR_PCK_OFN_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 17, AreaPer = 10f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 17, AreaPer = 10f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 17, AreaPer = 10f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Call centres.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Office)
            {
                Name = "offcall",
                NameKey = "RPR_PCK_OCS_NAM",
                DescriptionKey = "RPR_PCK_OCS_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 8f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 8f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 15, AreaPer = 8f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Data centres.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Office)
            {
                Name = "datacent",
                NameKey = "RPR_PCK_ODT_NAM",
                DescriptionKey = "RPR_PCK_ODT_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 200f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 200f, MultiFloorUnits = false };
            newPack.Levels[2] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 200f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Suburban schools.
            // Level 1 is elementary, level 2 is high school.
            // Figures are from NSW Department of Education 2013 targets.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Education)
            {
                Name = "schoolsub",
                NameKey = "RPR_PCK_SSB_NAM",
                DescriptionKey = "RPR_PCK_SSB_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 8f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 8f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (lowest density).
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Education)
            {
                Name = "schoolmnlow",
                NameKey = "RPR_PCK_SML_NAM",
                DescriptionKey = "RPR_PCK_SML_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 14f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 30f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (middle density).
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Education)
            {
                Name = "schoolmnmed",
                NameKey = "RPR_PCK_SMM_NAM",
                DescriptionKey = "RPR_PCK_SMM_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 12f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 23f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (highest density).
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Education)
            {
                Name = "schoolmnhigh",
                NameKey = "RPR_PCK_SMH_NAM",
                DescriptionKey = "RPR_PCK_SMH_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 9f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 0f, EmptyPercent = 0, AreaPer = 14f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // UK schools.
            // Figures are from Planning Statement - Warwickshire County Council.
            newPack = new VolumetricPopPack(DataPack.DataVersion.One, ItemClass.Service.Education)
            {
                Name = "schoolukhigh",
                NameKey = "RPR_PCK_SUK_NAM",
                DescriptionKey = "RPR_PCK_SUK_DES",
            };
            newPack.Levels[0] = new VolumetricPopPack.LevelData { EmptyArea = 350f, EmptyPercent = 0, AreaPer = 4.1f, MultiFloorUnits = false };
            newPack.Levels[1] = new VolumetricPopPack.LevelData { EmptyArea = 1400f, EmptyPercent = 0, AreaPer = 6.3f, MultiFloorUnits = false };
            CalcPacks.Add(newPack);

            // Initialise student overrides dictionary.
            _overrides = new Dictionary<string, ushort>();

            // Convert legacy overrides (if any).
            ConvertOverrides(DataStore.householdCache);
            ConvertOverrides(DataStore.workerCache);
        }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        internal static PopData Instance => s_instance;

        /// <summary>
        /// Ensures that a valid instance is instantiated and ready for use.
        /// </summary>
        internal static void EnsureInstance()
        {
            if (s_instance == null)
            {
                s_instance = new PopData();
            }
        }

        /// <summary>
        /// Clears all cached houshold counts.
        /// </summary>
        internal void ClearHousholdCache() => _householdCache.Clear();

        /// <summary>
        /// Clears cached houshold counts for the specified prefab.
        /// </summary>
        /// <param name="prefab">Prefab whose entries should be removed from cache.</param>
        internal void ClearHousholdCache(BuildingInfo prefab) => _householdCache.Remove(prefab);

        /// <summary>
        /// Clears all cached workplace counts.
        /// </summary>
        internal void ClearWorkplaceCache() => _workplaceCache.Clear();

        /// <summary>
        /// Clears cached houshold counts for the specified prefab.
        /// </summary>
        /// <param name="prefab">Prefab whose entries should be removed from cache.</param>
        internal void ClearWorkplaceCache(BuildingInfo prefab) => _workplaceCache.Remove(prefab);

        /// <summary>
        /// Clears all cached visitplace counts.
        /// </summary>
        internal void ClearVisitplaceCache() => _visitplaceCache.Clear();

        /// <summary>
        /// Clears cached houshold counts for the specified prefab.
        /// </summary>
        /// <param name="prefab">Prefab whose entries should be removed from cache.</param>
        internal void ClearVisitplaceCache(BuildingInfo prefab) => _visitplaceCache.Remove(prefab);

        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Workplace breakdowns and visitor count.</returns>
        internal WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => ((PopDataPack)ActivePack(buildingPrefab)).Workplaces(buildingPrefab, level);

        /// <summary>
        /// Returns the student count for the given building prefab.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab.</param>
        /// <returns>Student count.</returns>
        internal int Students(BuildingInfo buildingPrefab) => ((PopDataPack)ActivePack(buildingPrefab)).Students(buildingPrefab);

        /// <summary>
        /// Adds or updates cached housholds for the specified building prefab and level.
        /// </summary>
        /// <param name="info">BuildingInfo to cache for.</param>
        /// <param name="level">Building level to cache for.</param>
        /// <returns>Calculated population.</returns>
        internal int HouseholdCache(BuildingInfo info, int level)
        {
            // Check if key is already in cache.
            if (!_householdCache.TryGetValue(info, out HouseholdCacheEntry cacheEntry))
            {
                // No - create new record.
                cacheEntry = new HouseholdCacheEntry
                {
                    // Calculate results for each of the five levels.
                    Level0 = Math.Max((ushort)1, Population(info, 0)),
                    Level1 = Math.Max((ushort)1, Population(info, 1)),
                    Level2 = Math.Max((ushort)1, Population(info, 2)),
                    Level3 = Math.Max((ushort)1, Population(info, 3)),
                    Level4 = Math.Max((ushort)1, Population(info, 4)),
                };

                // Add new key to cache
                _householdCache.Add(info, cacheEntry);
            }

            // Return record relevant to level.
            switch (level)
            {
                case 0:
                    return cacheEntry.Level0;
                case 1:
                    return cacheEntry.Level1;
                case 2:
                    return cacheEntry.Level2;
                case 3:
                    return cacheEntry.Level3;
                default:
                    return cacheEntry.Level4;
            }
        }

        /// <summary>
        /// Returns the cached workplaces for the specified building prefab and level, adding to the cache if the record isn't already there.
        /// </summary>
        /// <param name="info">BuildingInfo to cache for.</param>
        /// <param name="level">Building level to cache for.</param>
        /// <returns>Calculated workplaces.</returns>
        internal WorkplaceLevels WorkplaceCache(BuildingInfo info, int level)
        {
            // Check if key is already in cache.
            if (!_workplaceCache.TryGetValue(info, out WorkplaceCacheEntry cacheEntry))
            {
                // No - create new record.
                cacheEntry = new WorkplaceCacheEntry
                {
                    // Calculate results for each of the three levels.
                    Level0 = Workplaces(info, 0),
                    Level1 = Workplaces(info, 1),
                    Level2 = Workplaces(info, 2),
                };

                // Add new key to cache
                _workplaceCache.Add(info, cacheEntry);
            }

            // Return record relevant to level.
            switch (level)
            {
                case 0:
                    return cacheEntry.Level0;
                case 1:
                    return cacheEntry.Level1;
                default:
                    return cacheEntry.Level2;
            }
        }

        /// <summary>
        /// Returns the cached workplaces for the specified building prefab and level, adding to the cache if the record isn't already there.
        /// </summary>
        /// <param name="info">BuildingInfo to cache for.</param>
        /// <param name="level">Building level to cache for.</param>
        /// <returns>Calculated workplaces.</returns>
        internal ushort VisitplaceCache(BuildingInfo info, int level)
        {
            // Check if key is already in cache.
            if (!_visitplaceCache.TryGetValue(info, out VisitplaceCacheEntry cacheEntry))
            {
                // No - create new record.
                cacheEntry = default;

                // Check for vanilla calcs.
                WorkplaceLevels workplaces = WorkplaceCache(info, 0);
                if (workplaces.Level0 == ushort.MaxValue)
                {
                    // Vanilla calcs - set all entries to ushort.MaxValue; the Prefix patch will detect this and fall through to game code.
                    cacheEntry.Level0 = ushort.MaxValue;
                    cacheEntry.Level1 = ushort.MaxValue;
                    cacheEntry.Level2 = ushort.MaxValue;
                }
                else
                {
                    // Not vanulla - calculate results for each of the three levels.
                    cacheEntry.Level0 = (ushort)Visitors.CalculateVisitCount(info, workplaces.Level0 + workplaces.Level1 + workplaces.Level2 + workplaces.Level3);
                    workplaces = WorkplaceCache(info, 1);
                    cacheEntry.Level1 = (ushort)Visitors.CalculateVisitCount(info, workplaces.Level0 + workplaces.Level1 + workplaces.Level2 + workplaces.Level3);
                    workplaces = WorkplaceCache(info, 2);
                    cacheEntry.Level2 = (ushort)Visitors.CalculateVisitCount(info, workplaces.Level0 + workplaces.Level1 + workplaces.Level2 + workplaces.Level3);
                }

                // Add new key to cache
                _visitplaceCache.Add(info, cacheEntry);
            }

            // Return record relevant to level.
            switch (level)
            {
                case 0:
                    return cacheEntry.Level0;
                case 1:
                    return cacheEntry.Level1;
                default:
                    return cacheEntry.Level2;
            }
        }

        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record.</param>
        /// <param name="level">Building level.</param>
        /// <param name="multiplier">Optional population multiplier (default 1.0).</param>
        /// <returns>Population.</returns>
        internal ushort Population(BuildingInfo buildingPrefab, int level, float multiplier = 1.0f)
        {
            // First, check for population override.
            ushort population = GetOverride(buildingPrefab.name);
            if (population > 0)
            {
                // Yes - return override.
                return population;
            }

            // If we got here, there's no override; return pack default.
            return ((PopDataPack)ActivePack(buildingPrefab)).Population(buildingPrefab, level, multiplier);
        }

        /// <summary>
        /// Calculates the population for the given building using the given LevelData and FloorDataPack.
        /// </summary>
        /// <param name="buildingInfoGen">Building info record.</param>
        /// <param name="levelData">LevelData record to use for calculations.</param>
        /// <param name="floorData">FloorDataPack record to use for calculations.</param>
        /// <param name="multiplier">Population multiplier.</param>
        /// <param name="floorList">Optional precalculated list of calculated floors (to save time; will be generated if not provided).</param>
        /// <param name="totalArea">Optional precalculated total building area  (to save time; will be generated if not provided).</param>
        /// <param name="perFloor">Ccalculated per-floor population values will be added to this list if provided (null to ignore).</param>
        /// <returns>Calculated population.</returns>
        internal ushort VolumetricPopulation(
            BuildingInfoGen buildingInfoGen,
            VolumetricPopPack.LevelData levelData,
            FloorDataPack floorData,
            float multiplier,
            SortedList<int, float> floorList = null,
            float totalArea = 0,
            List<KeyValuePair<ushort,
                ushort>> perFloor = null)
        {
            // Return value.
            ushort totalUnits = 0;

            // See if we're using area calculations for numbers of units, i.e. areaPer is at least one.
            if (levelData.AreaPer > 0)
            {
                // Get local references.
                float floorArea = totalArea;
                float emptyArea = levelData.EmptyArea;
                SortedList<int, float> floors = floorList;

                // Get list of floors and total building area, if one hasn't already been provided.
                if (floors == null || floorArea == 0)
                {
                    floors = VolumetricFloors(buildingInfoGen, floorData, out floorArea);
                }

                // Determine area percentage to use for calculations (inverse of empty area percentage).
                float areaPercent = 1 - (levelData.EmptyPercent / 100f);

                // See if we're calculating based on total building floor area, not per floor.
                if (levelData.MultiFloorUnits)
                {
                    // Units based on total floor area: calculate number of units in total building (always rounded down), after subtracting empty space.
                    totalUnits = (ushort)(((floorArea - emptyArea) * areaPercent) / levelData.AreaPer);

                    // Adjust by multiplier (after rounded calculation above).
                    totalUnits = (ushort)(totalUnits * multiplier);
                }
                else
                {
                    // Calculating per floor.
                    // Iterate through each floor, assigning population as we go.
                    for (ushort i = 0; i < floors.Count; ++i)
                    {
                        // Subtract any unallocated empty space.
                        if (emptyArea > 0)
                        {
                            // Get the space to be allocated against this floor - minimum of remaining (unallocated) empty space and this floor size.
                            float emptyAllocation = UnityEngine.Mathf.Min(emptyArea, floors[i]);

                            // Subtract empty space to be allocated from both this floor area and our unallocated empty space (because it's now allocated).
                            floors[i] -= emptyAllocation;
                            emptyArea -= emptyAllocation;
                        }

                        // Number of units on this floor - always rounded down.
                        ushort floorUnits = (ushort)((floors[i] * areaPercent) / levelData.AreaPer);

                        // Adjust by multiplier (after rounded calculation above).
                        floorUnits = (ushort)(floorUnits * multiplier);
                        totalUnits += floorUnits;

                        // Add to per-floor units list, if one was provided.
                        if (perFloor != null)
                        {
                            perFloor.Add(new KeyValuePair<ushort, ushort>(i, floorUnits));
                        }
                    }
                }
            }
            else
            {
                // areaPer is 0 or less; use a fixed number of units.
                totalUnits = (ushort)-levelData.AreaPer;
            }

            // Always have at least one unit, regardless of size.
            if (totalUnits < 1)
            {
                totalUnits = 1;
            }

            return totalUnits;
        }

        /// <summary>
        /// Returns a list of floors and populations for the given building.
        /// </summary>
        /// <param name="buildingInfoGen">Building info record.</param>
        /// <param name="floorData">Floor calculation pack to use for calculations.</param>
        /// <param name="totalArea">Total area of all floors.</param>
        /// <returns>Sorted list of floors (key = floor number, value = floor area).</returns>
        internal SortedList<int, float> VolumetricFloors(BuildingInfoGen buildingInfoGen, FloorDataPack floorData, out float totalArea)
        {
            // Initialise required fields.
            float floorArea = 0;
            SortedList<int, float> floors = new SortedList<int, float>();

            // Calculate our heighmap grid (16 x 16).
            float gridSizeX = (buildingInfoGen.m_max.x - buildingInfoGen.m_min.x) / 16f;
            float gridSizeY = (buildingInfoGen.m_max.z - buildingInfoGen.m_min.z) / 16f;
            float gridArea = gridSizeX * gridSizeY;

            // Iterate through our heights, adding each area to our floor count.
            float[] heights = buildingInfoGen.m_heights;
            for (int i = 0; i < heights.Length; ++i)
            {
                // Get local reference.
                float thisHeight = heights[i];

                // Check to see if we have at least one floor in this segment.
                if (thisHeight > floorData.m_firstFloorMin)
                {
                    // Starting number of floors is either 1 or zero, depending on setting of 'ignore first floor' checkbox.
                    int numFloors = floorData.m_firstFloorEmpty ? 0 : 1;

                    // Calculate any height left over from the maximum (minimum plus extra) first floor height.
                    float surplusHeight = thisHeight - floorData.m_firstFloorMin - floorData.m_firstFloorExtra;

                    // See if we have more than one floor, i.e. our height is greater than the first floor maximum height.
                    if (surplusHeight > 0)
                    {
                        // Number of floors for this grid segment is the truncated division (rounded down); no partial floors here!
                        numFloors += (int)(surplusHeight / floorData.m_floorHeight);
                    }

                    // Total incremental floor area is simply the number of floors multipled by the area of this grid segment.
                    floorArea += numFloors * gridArea;

                    // Iterate through each floor in this grid area and add this grid area to each floor.
                    for (int j = 0; j < numFloors; ++j)
                    {
                        // Check to see if we already an entry for this floor in our list.
                        if (floors.ContainsKey(j))
                        {
                            // We already have an entry for this floor; add this segment's area.
                            floors[j] += gridArea;
                        }
                        else
                        {
                            // We don't have an entry for this floor yet: add one with this segment's area as the initial value.
                            floors.Add(j, gridArea);
                        }
                    }
                }
            }

            totalArea = floorArea;
            return floors;
        }

        /// <summary>
        /// Returns the inbuilt default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Default calculation data pack.</returns>
        internal override DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            string defaultName;

            switch (service)
            {
                case ItemClass.Service.Residential:
                    switch (ModSettings.ThisSaveDefaultRes)
                    {
                        case DefaultMode.Vanilla:
                            defaultName = "vanilla";
                            break;
                        case DefaultMode.Legacy:
                            defaultName = "resWG";
                            break;
                        default:
                            // Default is volumetric.
                            switch (subService)
                            {
                                // Default is high residential.
                                case ItemClass.SubService.ResidentialLow:
                                case ItemClass.SubService.ResidentialLowEco:
                                    defaultName = "reslow";
                                    break;
                                case ItemClass.SubService.ResidentialHighEco:
                                case ItemClass.SubService.ResidentialWallToWall:
                                    defaultName = "resEUmod";
                                    break;
                                default:
                                case ItemClass.SubService.ResidentialHigh:
                                    defaultName = "reshighUS";
                                    break;
                            }

                            break;
                    }

                    break;

                case ItemClass.Service.Industrial:
                    switch (ModSettings.ThisSaveDefaultInd)
                    {
                        case DefaultMode.Vanilla:
                            defaultName = "vanilla";
                            break;
                        case DefaultMode.Legacy:
                            defaultName = "indWG";
                            break;
                        default:
                            // Default is volumetric.
                            defaultName = "factory";
                            break;
                    }

                    break;

                case ItemClass.Service.Office:
                    switch (ModSettings.ThisSaveDefaultOff)
                    {
                        case DefaultMode.Vanilla:
                            defaultName = "vanilla";
                            break;
                        case DefaultMode.Legacy:
                            defaultName = "offWG";
                            break;
                        default:
                            // Default is volumetric.
                            defaultName = "offcorp";
                            break;
                    }

                    break;

                case ItemClass.Service.Education:
                    defaultName = "schoolsub";
                    break;

                default:
                    // Default is commercial.
                    switch (ModSettings.ThisSaveDefaultCom)
                    {
                        case DefaultMode.Vanilla:
                            defaultName = "vanilla";
                            break;
                        case DefaultMode.Legacy:
                            defaultName = "comWG";
                            break;
                        default:
                            // Default is volumetric.
                            switch (subService)
                            {
                                case ItemClass.SubService.CommercialLow:
                                case ItemClass.SubService.CommercialEco:
                                    defaultName = "comUS";
                                    break;

                                case ItemClass.SubService.CommercialTourist:
                                    defaultName = "hotel";
                                    break;

                                case ItemClass.SubService.CommercialLeisure:
                                    defaultName = "restaurant";
                                    break;

                                default:
                                case ItemClass.SubService.CommercialHigh:
                                case ItemClass.SubService.CommercialWallToWall:
                                    // Default is high-density commercial.
                                    defaultName = "comUS";
                                    break;
                            }

                            break;
                    }

                    break;
            }

            // Match name to floorpack.
            return CalcPacks.Find(pack => pack.Name.Equals(defaultName));
        }

        /// <summary>
        /// Returns a list of calculation packs available for the given prefab.
        /// </summary>
        /// <param name="prefab">BuildingInfo prefab.</param>
        /// <returns>Array of available calculation packs.</returns>
        internal PopDataPack[] GetPacks(BuildingInfo prefab) => GetPacks(prefab.GetService());

        /// <summary>
        /// Returns a list of calculation packs available for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <returns>Array of available calculation packs.</returns>
        internal PopDataPack[] GetPacks(ItemClass.Service service)
        {
            // Return list.
            List<PopDataPack> list = new List<PopDataPack>();

            // Iterate through each floor pack and see if it applies.
            for (int i = 0; i < CalcPacks.Count; i++)
            {
                if (CalcPacks[i] is PopDataPack pack)
                {
                    // Check for matching service.
                    if (pack.Service == service || pack.Service == ItemClass.Service.None)
                    {
                        // Service matches; add pack.
                        list.Add(pack);
                    }
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Sets a manual population override for the given building prefab, but does NOT update live prefab data or save the configuration file.
        /// Used to populate dictionary when the prefab isn't available (e.g. before loading is complete).
        /// </summary>
        /// <param name="prefabName">Building prefab.</param>
        /// <param name="popOverride">Manual population override.</param>
        internal void SetOverride(string prefabName, ushort popOverride)
        {
            // Override needs to be at least 1.
            if (popOverride > 1)
            {
                // Check for existing entry.
                if (_overrides.ContainsKey(prefabName))
                {
                    // Existing entry found; update it.
                    _overrides[prefabName] = popOverride;
                }
                else
                {
                    // No existing entry found; add one.
                    _overrides.Add(prefabName, popOverride);
                }
            }
            else
            {
                Logging.Error("invalid pop override '", popOverride, "' for prefab ", prefabName);
            }
        }

        /// <summary>
        /// Sets a manual population override for the given building prefab, and saves the updated configuration; and also UPDATES live prefab data.
        /// Used to add an entry in-game after prefabs have loaded.
        /// </summary>
        /// <param name="prefab">Building prefab.</param>
        /// <param name="popOverride">Manual population override.</param>
        internal void SetOverride(BuildingInfo prefab, ushort popOverride)
        {
            // Override needs to be at least 1.
            if (popOverride > 0)
            {
                // Apply changes.
                SetOverride(prefab.name, popOverride);

                // Apply school changes if this is a school.
                if (prefab.GetService() == ItemClass.Service.Education)
                {
                    SchoolData.Instance.UpdateSchoolPrefab(prefab);
                }

                // Save updated configuration file.
                ConfigurationUtils.SaveSettings();

                // Refresh the prefab's population settings to reflect changes.
                RefreshPrefab(prefab);
            }
            else
            {
                Logging.Error("invalid pop override '", popOverride, "' for prefab ", prefab.name);
            }
        }

        /// <summary>
        /// Removes any manual population override for the given building prefab, and saves the updated configuration if an override was actually removed (i.e. one actually existed).
        /// </summary>
        /// <param name="prefab">Building prefab.</param>
        internal void DeleteOverride(BuildingInfo prefab)
        {
            // Remove prefab record from dictionary.
            if (_overrides.Remove(prefab.name))
            {
                // An entry was removed (i.e. dictionary contained an entry); apply changes to relevant school.
                if (prefab.GetService() == ItemClass.Service.Education)
                {
                    SchoolData.Instance.UpdateSchoolPrefab(prefab);
                }

                // Save the updated configuration file.
                ConfigurationUtils.SaveSettings();

                // Refresh the prefab's population settings to reflect changes.
                RefreshPrefab(prefab);
            }
        }

        /// <summary>
        /// Gets the manual population override in effect for the given building prefab, if any.
        /// </summary>
        /// <param name="prefabName">Building prefab name.</param>
        /// <returns>Manual population override if one exists; otherwise 0.</returns>
        internal ushort GetOverride(string prefabName)
        {
            // Check for entry.
            if (_overrides.ContainsKey(prefabName))
            {
                // Found entry; return the override.
                return _overrides[prefabName];
            }

            // If we got here, no override was found; return zero.
            return 0;
        }

        /// <summary>
        /// Serializes manual population overrides to XCML.
        /// </summary>
        /// <returns>Serialized list of population overrides suitable for writing to XML.</returns>
        internal List<Configuration.PopCountOverride> SerializeOverrides()
        {
            // Return list.
            List<Configuration.PopCountOverride> returnList = new List<Configuration.PopCountOverride>();

            // Iterate through each entry in population override dictionary, converting into PopCountOverride XML record and adding to list.
            foreach (KeyValuePair<string, ushort> popOverride in _overrides)
            {
                returnList.Add(new Configuration.PopCountOverride
                {
                    Prefab = popOverride.Key,
                    Population = popOverride.Value,
                });
            }

            return returnList;
        }

        /// <summary>
        /// Deserializes manual population overrides from XML.  Note: does not apply settings, merely populates dictionary.
        /// </summary>
        /// <param name="popCountOverrides">List of population count overrides to deserialize.</param>
        internal void DeserializeOverrides(List<Configuration.PopCountOverride> popCountOverrides)
        {
            foreach (Configuration.PopCountOverride popOverride in popCountOverrides)
            {
                try
                {
                    SetOverride(popOverride.Prefab, popOverride.Population);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, " exception deserializing pop override for prefab ", popOverride.Prefab ?? "null");
                }
            }
        }

        /// <summary>
        /// Serializes building pack settings to XML.  Intended to be passed directly to FloorData.SerializeBuildings.
        /// </summary>
        /// <returns>New sorted list with building pack settings.</returns>
        internal SortedList<string, Configuration.BuildingRecord> SerializeBuildings()
        {
            // Return list.
            SortedList<string, Configuration.BuildingRecord> returnList = new SortedList<string, Configuration.BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary.
            foreach (string prefabName in BuildingDict.Keys)
            {
                // Serialise it into a BuildingRecord and add it to the list.
                Configuration.BuildingRecord newRecord = new Configuration.BuildingRecord { Prefab = prefabName, PopPack = BuildingDict[prefabName].Name };
                returnList.Add(prefabName, newRecord);
            }

            return returnList;
        }

        /// <summary>
        /// Extracts the relevant pack name (floor or pop) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from.</param>
        /// <returns>Floor pack name (if any).</returns>
        protected override string BuildingPack(Configuration.BuildingRecord buildingRecord) => buildingRecord.PopPack;

        /// <summary>
        /// Converts population overrides from old WG dictionaries (loaded from legacy WG files) to new-format PopData overrides.
        /// </summary>
        /// <param name="dictionary">Legacy override dictionary to convert.</param>
        private void ConvertOverrides(Dictionary<string, int> dictionary)
        {
            foreach (KeyValuePair<string, int> entry in dictionary)
            {
                SetOverride(entry.Key, (ushort)entry.Value);
            }
        }

        /// <summary>
        /// Household cache data structure.
        /// </summary>
        internal struct HouseholdCacheEntry
        {
            /// <summary>
            /// Household count for building level 1.
            /// </summary>
            public ushort Level0;

            /// <summary>
            /// Household count for building level 2.
            /// </summary>
            public ushort Level1;

            /// <summary>
            /// Household count for building level 3.
            /// </summary>
            public ushort Level2;

            /// <summary>
            /// Household count for building level 4.
            /// </summary>
            public ushort Level3;

            /// <summary>
            /// Household count for building level 5.
            /// </summary>
            public ushort Level4;
        }

        /// <summary>
        /// Workplace cache data structure.
        /// </summary>
        internal struct WorkplaceCacheEntry
        {
            /// <summary>
            /// Workpace count for building level 1.
            /// </summary>
            public WorkplaceLevels Level0;

            /// <summary>
            /// Workpace count for building level 2.
            /// </summary>
            public WorkplaceLevels Level1;

            /// <summary>
            /// Workpace count for building level 3.
            /// </summary>
            public WorkplaceLevels Level2;
        }

        /// <summary>
        /// Workplace education levels data structure.
        /// </summary>
        internal struct WorkplaceLevels
        {
            /// <summary>
            /// Uneducated workplaces count.
            /// </summary>
            public ushort Level0;

            /// <summary>
            /// Educated workplaces count.
            /// </summary>
            public ushort Level1;

            /// <summary>
            /// Well=-ducated workplaces count.
            /// </summary>
            public ushort Level2;

            /// <summary>
            /// Highly-educated workplaces count.
            /// </summary>
            public ushort Level3;
        }

        /// <summary>
        /// Visitplace cache data structure.
        /// </summary>
        internal struct VisitplaceCacheEntry
        {
            /// <summary>
            /// Visitor count for building level 1.
            /// </summary>
            public ushort Level0;

            /// <summary>
            /// Visitor count for building level 2.
            /// </summary>
            public ushort Level1;

            /// <summary>
            /// Visitor count for building level 3.
            /// </summary>
            public ushort Level2;
        }
    }
}