using System;
using System.Collections.Generic;


namespace RealPop2
{
    /// <summary>
    /// Centralised store and management of population calculation data.
    /// </summary>
    internal class PopData : CalcData
    {
        // Instance reference.
        internal static PopData instance;

        // Dictionary of manual population count overrides.
        private readonly Dictionary<string, ushort> overrides;

        // Household and workplace calculation result caches (so we don't have to do the full calcs every SimulationStep for every building....).
        internal readonly Dictionary<BuildingInfo, HouseholdCache> householdCache;
        internal readonly Dictionary<BuildingInfo, WorkplaceCache> workplaceCache;


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="prefab">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        internal WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => ((PopDataPack)ActivePack(buildingPrefab)).Workplaces(buildingPrefab, level);


        /// <summary>
        /// Adds or updates cached housholds for the specified building prefab and level.
        /// </summary>
        /// <param name="info">BuildingInfo to cache for</param>
        /// <param name="level">Building level to cache for</param>
        /// <returns>Calculated population</returns>
        internal int HouseholdCache(BuildingInfo info, int level)
        {
            // Null check for safety.
            if (info?.name != null)
            {
                // Check if key is already in cache.
                if (!householdCache.TryGetValue(info, out HouseholdCache cacheEntry))
                {
                    // No - create new record.
                    cacheEntry = new HouseholdCache
                    {
                        // Calculate results for each of the five levels.
                        level0 = Population(info, 0),
                        level1 = Population(info, 1),
                        level2 = Population(info, 2),
                        level3 = Population(info, 3),
                        level4 = Population(info, 4)
                    };

                    // Add new key to cache
                    householdCache.Add(info, cacheEntry);
                }

                // Return record relevant to level.
                switch (level)
                {
                    case 0:
                        return cacheEntry.level0;
                    case 1:
                        return cacheEntry.level1;
                    case 2:
                        return cacheEntry.level2;
                    case 3:
                        return cacheEntry.level3;
                    default:
                        return cacheEntry.level4;
                }
            }

            // If we got here, something went wrong; return 1.
            Logging.Error("null prefab passed to HouseholdCache");
            return 1;
        }


        /// <summary>
        /// Returns the cached workplaces for the specified building prefab and level, adding to the cache if the record isn't already there.
        /// </summary>
        /// <param name="info">BuildingInfo to cache for</param>
        /// <param name="level">Building level to cache for</param>
        /// <returns>Calculated workplaces</returns>
        internal WorkplaceLevels WorkplaceCache(BuildingInfo info, int level)
        {
            // Null check for safety.
            if (info?.name != null)
            {
                // Check if key is already in cache.
                if (!workplaceCache.TryGetValue(info, out WorkplaceCache cacheEntry))
                {
                    // No - create new record.
                    cacheEntry = new WorkplaceCache
                    {
                        // Calculate results for each of the five levels.
                        level0 = Workplaces(info, 0),
                        level1 = Workplaces(info, 1),
                        level2 = Workplaces(info, 2),
                    };

                    // Add new key to cache
                    workplaceCache.Add(info, cacheEntry);
                }

                // Return record relevant to level.
                switch (level)
                {
                    case 0:
                        return cacheEntry.level0;
                    case 1:
                        return cacheEntry.level1;
                    default:
                        return cacheEntry.level2;
                }
            }

            // If we got here, something went wrong; return 1.
            Logging.Error("null prefab passed to WorkplaceCache");
            return new WorkplaceLevels
            {
                level0 = 1,
                level1 = 0,
                level2 = 0,
                level3 = 0
            };
        }


        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <param name="multiplier">Optional population multiplier (default 1.0)</param>
        /// <returns>Population</returns>
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
        /// <param name="buildingInfoGen">Building info record</param>
        /// <param name="levelData">LevelData record to use for calculations</param>
        /// <param name="floorData">FloorDataPack record to use for calculations</param>
        /// <param name="multiplier"Population multiplier</param>
        /// <param name="floorList">Optional precalculated list of calculated floors (to save time; will be generated if not provided)</param>
        /// <param name="totalArea">Optional precalculated total building area  (to save time; will be generated if not provided)</param>
        /// <returns>Calcularted population</returns>
        internal ushort VolumetricPopulation(BuildingInfoGen buildingInfoGen, LevelData levelData, FloorDataPack floorData, float multiplier, SortedList<int, float> floorList = null, float totalArea = 0)
        {
            // Return value.
            ushort totalUnits = 0;


            // See if we're using area calculations for numbers of units, i.e. areaPer is at least one.
            if (levelData.areaPer > 0)
            {
                // Get local references.
                float floorArea = totalArea;
                float emptyArea = levelData.emptyArea;
                SortedList<int, float> floors = floorList;

                // Get list of floors and total building area, if one hasn't already been provided.
                if (floors == null || floorArea == 0)
                {
                    floors = VolumetricFloors(buildingInfoGen, floorData, out floorArea);
                }

                // Determine area percentage to use for calculations (inverse of empty area percentage).
                float areaPercent = 1 - (levelData.emptyPercent / 100f);

                // See if we're calculating based on total building floor area, not per floor.
                if (levelData.multiFloorUnits)
                {
                    // Units based on total floor area: calculate number of units in total building (always rounded down), after subtracting empty space.
                    totalUnits = (ushort)(((floorArea - emptyArea) * areaPercent) / levelData.areaPer);
                    // Adjust by multiplier (after rounded calculation above).
                    totalUnits = (ushort)(totalUnits * multiplier);
                }
                else
                {
                    // Calculating per floor.
                    // Iterate through each floor, assigning population as we go.
                    for (int i = 0; i < floors.Count; ++i)
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
                        ushort floorUnits = (ushort)((floors[i] * areaPercent) / levelData.areaPer);
                        // Adjust by multiplier (after rounded calculation above).
                        floorUnits = (ushort)(floorUnits * multiplier);
                        totalUnits += floorUnits;
                    }
                }
            }
            else
            {
                // areaPer is 0 or less; use a fixed number of units.
                totalUnits = (ushort)-levelData.areaPer;
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
        /// <param name="buildingInfoGen">Building info record</param>
        /// <param name="floorData">Floor calculation pack to use for calculations</param>
        /// <param name="total">Total area of all floors</param>
        /// <returns>Sorted list of floors (key = floor number, value = floor area)</returns>
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
                if (thisHeight > floorData.firstFloorMin)
                {
                    // Starting number of floors is either 1 or zero, depending on setting of 'ignore first floor' checkbox.
                    int numFloors = floorData.firstFloorEmpty ? 0 : 1;

                    // Calculate any height left over from the maximum (minimum plus extra) first floor height.
                    float surplusHeight = thisHeight - floorData.firstFloorMin - floorData.firstFloorExtra;

                    // See if we have more than one floor, i.e. our height is greater than the first floor maximum height.
                    if (surplusHeight > 0)
                    {
                        // Number of floors for this grid segment is the truncated division (rounded down); no partial floors here!
                        numFloors += (int)(surplusHeight / floorData.floorHeight);
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
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        public PopData()
        {
            // Create caches.
            householdCache = new Dictionary<BuildingInfo, HouseholdCache>();
            workplaceCache = new Dictionary<BuildingInfo, WorkplaceCache>();

            // Legacy residential.
            calcPacks.Add(new LegacyResPack
            {
                name = "resWG",
                displayName = Translations.Translate("RPR_PCK_LEG_NAM"),
                description = Translations.Translate("RPR_PCK_LEG_DES"),
                version = DataVersion.legacy,
                service = ItemClass.Service.Residential
            });

            // Legacy industrial.
            calcPacks.Add(new LegacyIndPack()
            {
                name = "indWG",
                displayName = Translations.Translate("RPR_PCK_LEG_NAM"),
                description = Translations.Translate("RPR_PCK_LEG_DES"),
                version = DataVersion.legacy,
                service = ItemClass.Service.Industrial
            });

            // Legacy commercial.
            calcPacks.Add(new LegacyComPack()
            {
                name = "comWG",
                displayName = Translations.Translate("RPR_PCK_LEG_NAM"),
                description = Translations.Translate("RPR_PCK_LEG_DES"),
                version = DataVersion.legacy,
                service = ItemClass.Service.Commercial
            });

            // Legacy office.
            calcPacks.Add(new LegacyOffPack
            {
                name = "offWG",
                displayName = Translations.Translate("RPR_PCK_LEG_NAM"),
                description = Translations.Translate("RPR_PCK_LEG_DES"),
                version = DataVersion.legacy,
                service = ItemClass.Service.Office
            });

            // Vanilla calcs.
            calcPacks.Add(new VanillaPack
            {
                name = "vanilla",
                displayName = Translations.Translate("RPR_PCK_VAN_NAM"),
                description = Translations.Translate("RPR_PCK_VAN_DES"),
                version = DataVersion.vanilla,
                service = ItemClass.Service.None
            });

            // Low-density residential.
            VolumetricPopPack newPack = new VolumetricPopPack
            {
                name = "reslow",
                displayName = Translations.Translate("RPR_PCK_RLS_NAM"),
                description = Translations.Translate("RPR_PCK_RLS_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Residential,
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -1f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Duplexes.
            newPack = new VolumetricPopPack
            {
                name = "duplex",
                displayName = Translations.Translate("RPR_PCK_RLD_NAM"),
                description = Translations.Translate("RPR_PCK_RLD_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Residential,
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -2f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -2f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -2f, multiFloorUnits = true };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -2f, multiFloorUnits = true };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = -2f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // European apartments (modern).
            newPack = new VolumetricPopPack
            {
                name = "resEUmod",
                displayName = Translations.Translate("RPR_PCK_REM_NAM"),
                description = Translations.Translate("RPR_PCK_REM_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Residential,
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 10, areaPer = 85f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 12, areaPer = 90f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 16, areaPer = 100f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 18, areaPer = 105f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 18, areaPer = 110f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // European apartments (older).
            newPack = new VolumetricPopPack()
            {
                name = "resEUold",
                displayName = Translations.Translate("RPR_PCK_REO_NAM"),
                description = Translations.Translate("RPR_PCK_REO_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Residential,
                levels = new LevelData[5],
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 75f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 1, areaPer = 80f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 2, areaPer = 85f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 3, areaPer = 90f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 4, areaPer = 95f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // High-density residential US - empty percentages from AssetInsights.net, areas from 2018 RentCafe.
            newPack = new VolumetricPopPack()
            {
                name = "reshighUS",
                displayName = Translations.Translate("RPR_PCK_RUH_NAM"),
                description = Translations.Translate("RPR_PCK_RUH_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Residential,
                levels = new LevelData[5]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 36, areaPer = 70f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 37, areaPer = 78f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 38, areaPer = 86f, multiFloorUnits = false };
            newPack.levels[3] = new LevelData { emptyArea = 0f, emptyPercent = 39, areaPer = 94f, multiFloorUnits = false };
            newPack.levels[4] = new LevelData { emptyArea = 0f, emptyPercent = 40, areaPer = 102f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // US commmercial.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack()
            {
                name = "comUS",
                displayName = Translations.Translate("RPR_PCK_CUS_NAM"),
                description = Translations.Translate("RPR_PCK_CUS_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0, emptyPercent = 0, areaPer = 37, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0, emptyPercent = 0, areaPer = 37, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0, emptyPercent = 0, areaPer = 37, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // UK commercial.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack()
            {
                name = "comUK",
                displayName = Translations.Translate("RPR_PCK_CUK_NAM"),
                description = Translations.Translate("RPR_PCK_CUK_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 20f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 17.5f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 15f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Retail warehouses.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack()
            {
                name = "retailware",
                displayName = Translations.Translate("RPR_PCK_CRW_NAM"),
                description = Translations.Translate("RPR_PCK_CRW_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 90f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 90f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 90f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Hotels.
            // Figures are from Montgomery County round 7.0.
            newPack = new VolumetricPopPack()
            {
                name = "hotel",
                displayName = Translations.Translate("RPR_PCK_THT_NAM"),
                description = Translations.Translate("RPR_PCK_THT_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 120f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 120f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 120f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Restaurants and cafes.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack()
            {
                name = "restaurant",
                displayName = Translations.Translate("RPR_PCK_LFD_NAM"),
                description = Translations.Translate("RPR_PCK_LFD_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 20f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 17.5f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 15, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Entertainment centres.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack()
            {
                name = "entertainment",
                displayName = Translations.Translate("RPR_PCK_LEN_NAM"),
                description = Translations.Translate("RPR_PCK_LEN_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 20f, emptyPercent = 20, areaPer = 70f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 20f, emptyPercent = 20, areaPer = 65f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 20f, emptyPercent = 20, areaPer = 60f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Cinemas.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GIA to NIA.
            newPack = new VolumetricPopPack()
            {
                name = "cinema",
                displayName = Translations.Translate("RPR_PCK_LCN_NAM"),
                description = Translations.Translate("RPR_PCK_LCN_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Commercial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 5, areaPer = 200f, multiFloorUnits = true };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 5, areaPer = 200f, multiFloorUnits = true };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 5, areaPer = 200f, multiFloorUnits = true };
            calcPacks.Add(newPack);

            // Light industry.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to NIA.
            newPack = new VolumetricPopPack()
            {
                name = "lightind",
                displayName = Translations.Translate("RPR_PCK_ILG_NAM"),
                description = Translations.Translate("RPR_PCK_ILG_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Industrial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 47f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 47f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 47f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Manufacturing.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "factory",
                displayName = Translations.Translate("RPR_PCK_IMN_NAM"),
                description = Translations.Translate("RPR_PCK_IMN_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Industrial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 5, areaPer = 36f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 5, areaPer = 36f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 5, areaPer = 36f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Industry warehouses (local distribution).
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            newPack = new VolumetricPopPack()
            {
                name = "localware",
                displayName = Translations.Translate("RPR_PCK_IWL_NAM"),
                description = Translations.Translate("RPR_PCK_IWL_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Industrial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 70f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 70f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 70f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Industry warehouses (national distribution).
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            newPack = new VolumetricPopPack()
            {
                name = "natware",
                displayName = Translations.Translate("RPR_PCK_IWN_NAM"),
                description = Translations.Translate("RPR_PCK_IWN_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Industrial,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 95f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 95f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 95f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Corporate offices.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "offcorp",
                displayName = Translations.Translate("RPR_PCK_OCP_NAM"),
                description = Translations.Translate("RPR_PCK_OCP_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Office,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 13f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 13f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 20, areaPer = 13f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Financial offices.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "offfin",
                displayName = Translations.Translate("RPR_PCK_OFN_NAM"),
                description = Translations.Translate("RPR_PCK_OFN_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Office,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 17, areaPer = 10f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 17, areaPer = 10f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 17, areaPer = 10f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Call centres.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "offcall",
                displayName = Translations.Translate("RPR_PCK_OCS_NAM"),
                description = Translations.Translate("RPR_PCK_OCS_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Office,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 8f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 8f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 15, areaPer = 8f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Data centres.
            // Densities from 2015 Homes & Communities Agenct 'Employment Densities Guide' 3rd Edition.
            // Empty percent is markdown from GEA to GIA.
            newPack = new VolumetricPopPack()
            {
                name = "datacent",
                displayName = Translations.Translate("RPR_PCK_ODT_NAM"),
                description = Translations.Translate("RPR_PCK_ODT_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Office,
                levels = new LevelData[3]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 200f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 200f, multiFloorUnits = false };
            newPack.levels[2] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 200f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Level 1 is elementary, level 2 is high school.
            // Figures are from NSW Department of Education 2013 targets.
            newPack = new VolumetricPopPack()
            {
                name = "schoolsub",
                displayName = Translations.Translate("RPR_PCK_SSB_NAM"),
                description = Translations.Translate("RPR_PCK_SSB_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Education,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 8f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 8f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (lowest density).
            newPack = new VolumetricPopPack()
            {
                name = "schoolmnlow",
                displayName = Translations.Translate("RPR_PCK_SML_NAM"),
                description = Translations.Translate("RPR_PCK_SML_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Education,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 14f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 30f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (middle density).
            newPack = new VolumetricPopPack()
            {
                name = "schoolmnmed",
                displayName = Translations.Translate("RPR_PCK_SMM_NAM"),
                description = Translations.Translate("RPR_PCK_SMM_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Education,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 12f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 23f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Suburban schools.
            // Figures are from MN Department of Education Guide for Planning School Construction Projects (highest density).
            newPack = new VolumetricPopPack()
            {
                name = "schoolmnhigh",
                displayName = Translations.Translate("RPR_PCK_SMH_NAM"),
                description = Translations.Translate("RPR_PCK_SMH_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Education,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 9f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 0f, emptyPercent = 0, areaPer = 14f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // UK schools.
            // Figures are from Planning Statement - Warwickshire County Council.
            newPack = new VolumetricPopPack()
            {
                name = "schoolukhigh",
                displayName = Translations.Translate("RPR_PCK_SUK_NAM"),
                description = Translations.Translate("RPR_PCK_SUK_DES"),
                version = DataVersion.one,
                service = ItemClass.Service.Education,
                levels = new LevelData[2]
            };
            newPack.levels[0] = new LevelData { emptyArea = 350f, emptyPercent = 0, areaPer = 4.1f, multiFloorUnits = false };
            newPack.levels[1] = new LevelData { emptyArea = 1400f, emptyPercent = 0, areaPer = 6.3f, multiFloorUnits = false };
            calcPacks.Add(newPack);

            // Initialise student overrides dictionary.
            overrides = new Dictionary<string, ushort>();

            // Convert legacy overrides (if any).
            ConvertOverrides(DataStore.householdCache);
            ConvertOverrides(DataStore.workerCache);
        }


        /// <summary>
        /// Returns the inbuilt default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default calculation data pack</returns>
        internal override DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            string defaultName;

            switch (service)
            {
                case ItemClass.Service.Residential:
                    // Residential - legacy defaults or new?
                    if (ModSettings.ThisSaveLegacyRes)
                    {
                        // Legacy.
                        defaultName = "resWG";
                    }
                    else
                    {
                        // Volumetric.
                        switch (subService)
                        {
                            case ItemClass.SubService.ResidentialHigh:
                                defaultName = "reshighUS";
                                break;
                            case ItemClass.SubService.ResidentialHighEco:
                                defaultName = "resEUmod";
                                break;
                            default:
                                defaultName = "reslow";
                                break;
                        }
                    }
                    break;

                case ItemClass.Service.Industrial:
                    defaultName = ModSettings.ThisSaveLegacyInd ? "indWG" : "factory";
                    break;

                case ItemClass.Service.Office:
                    defaultName = ModSettings.ThisSaveLegacyOff ? "offWG" : "offcorp";
                    break;

                case ItemClass.Service.Education:
                    defaultName = "schoolsub";
                    break;

                default:
                    // Default is commercial.

                    if (ModSettings.ThisSaveLegacyCom)
                    {
                        // Legacy settings.
                        defaultName = "comWG";
                    }
                    else
                    {
                        // New settings.
                        switch (subService)
                        {
                            case ItemClass.SubService.CommercialHigh:
                                defaultName = "comUS";
                                break;
                            case ItemClass.SubService.CommercialTourist:
                                defaultName = "hotel";
                                break;
                            case ItemClass.SubService.CommercialLeisure:
                                defaultName = "restaurant";
                                break;
                            default:
                                // Default is low-density commercial.
                                defaultName = "comUS";
                                break;
                        }
                    }
                    break;
            }

            // Match name to floorpack.
            return calcPacks.Find(pack => pack.name.Equals(defaultName));
        }


        /// <summary>
        /// Returns a list of calculation packs available for the given prefab.
        /// </summary>
        /// <param name="prefab">BuildingInfo prefab</param>
        /// <returns>Array of available calculation packs</returns>
        internal PopDataPack[] GetPacks(BuildingInfo prefab) => GetPacks(prefab.GetService());


        /// <summary>
        /// Returns a list of calculation packs available for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <returns>Array of available calculation packs</returns>
        internal PopDataPack[] GetPacks(ItemClass.Service service)
        {
            // Return list.
            List<PopDataPack> list = new List<PopDataPack>();

            // Iterate through each floor pack and see if it applies.
            foreach (PopDataPack pack in calcPacks)
            {
                // Check for matching service.
                if (pack.service == service || pack.service == ItemClass.Service.None)
                {
                    // Service matches; add pack.
                    list.Add(pack);
                }
            }

            return list.ToArray();
        }


        /// <summary>
        /// Sets a manual population override for the given building prefab, but does NOT update live prefab data or save the configuration file.
        /// Used to populate dictionary when the prefab isn't available (e.g. before loading is complete).
        /// </summary>
        /// <param name="prefabName">Building prefab/param>
        /// <param name="popOverride">Manual population override</param>
        /// <param name="saveConfig">True (default) to save configuration file afterwards, false to not save</param>
        internal void SetOverride(string prefabName, ushort popOverride)
        {
            // Override needs to be at least 1.
            if (popOverride > 1)
            {
                // Check for existing entry.
                if (overrides.ContainsKey(prefabName))
                {
                    // Existing entry found; update it.
                    overrides[prefabName] = popOverride;
                }
                else
                {
                    // No existing entry found; add one.
                    overrides.Add(prefabName, popOverride);
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
        /// <param name="prefab">Building prefab/param>
        /// <param name="popOverride">Manual population override</param>
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
                    SchoolData.instance.UpdateSchoolPrefab(prefab);
                }

                // Save updated configuration file.
                ConfigUtils.SaveSettings();

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
        /// <param name="prefab">Building prefab/param>
        internal void DeleteOverride(BuildingInfo prefab)
        {
            // Remove prefab record from dictionary.
            if (overrides.Remove(prefab.name))
            {
                // An entry was removed (i.e. dictionary contained an entry); apply changes to relevant school.
                if (prefab.GetService() == ItemClass.Service.Education)
                {
                    SchoolData.instance.UpdateSchoolPrefab(prefab);
                }

                // Save the updated configuration file.
                ConfigUtils.SaveSettings();

                // Refresh the prefab's population settings to reflect changes.
                RefreshPrefab(prefab);
            }
        }


        /// <summary>
        /// Gets the manual population override in effect for the given building prefab, if any.
        /// </summary>
        /// <param name="prefabName">Building prefab name</param>
        /// <returns>Manual population override if one exists; otherwise 0</returns>
        internal ushort GetOverride(string prefabName)
        {
            // Check for entry.
            if (overrides.ContainsKey(prefabName))
            {
                // Found entry; return the override.
                return overrides[prefabName];
            }

            // If we got here, no override was found; return zero.
            return 0;
        }


        /// <summary>
        /// Serializes manual population overrides to XCML.
        /// </summary>
        /// <returns>Serialized list of population overrides suitable for writing to XML</returns>
        internal List<PopCountOverride> SerializeOverrides()
        {
            // Return list.
            List<PopCountOverride> returnList = new List<PopCountOverride>();

            // Iterate through each entry in population override dictionary, converting into PopCountOverride XML record and adding to list.
            foreach (KeyValuePair<string, ushort> popOverride in overrides)
            {
                returnList.Add(new PopCountOverride
                {
                    prefab = popOverride.Key,
                    population = popOverride.Value
                });
            }

            return returnList;
        }


        /// <summary>
        /// Deserializes manual population overrides from XML.  Note: does not apply settings, merely populates dictionary.
        /// </summary>
        /// <param name="popCountOverrides">List of population count overrides to deserialize</param>
        internal void DeserializeOverrides(List<PopCountOverride> popCountOverrides)
        {
            foreach (PopCountOverride popOverride in popCountOverrides)
            {
                try
                {
                    SetOverride(popOverride.prefab, popOverride.population);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, " exception deserializing pop override for prefab ", popOverride?.prefab ?? "null");
                }
            }
        }


        /// <summary>
        /// Serializes building pack settings to XML.  Intended to be passed directly to FloorData.SerializeBuildings.
        /// </summary>
        /// <returns>New sorted list with building pack settings</returns>
        internal SortedList<string, BuildingRecord> SerializeBuildings()
        {
            // Return list.
            SortedList<string, BuildingRecord> returnList = new SortedList<string, BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary.
            foreach (string prefabName in buildingDict.Keys)
            {
                // Serialise it into a BuildingRecord and add it to the list.
                BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, popPack = buildingDict[prefabName].name };
                returnList.Add(prefabName, newRecord);
            }

            return returnList;
        }


        /// <summary>
        /// Extracts the relevant pack name (floor or pop) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from</param>
        /// <returns>Floor pack name (if any)</returns>
        protected override string BuildingPack(BuildingRecord buildingRecord) => buildingRecord.popPack;


        /// <summary>
        /// Converts population overrides from old WG dictionaries (loaded from legacy WG files) to new-format PopData overrides.
        /// </summary>
        /// <param name="dictionary">Legacy override dictionary to convert</param>
        private void ConvertOverrides(Dictionary<string, int> dictionary)
        {
            foreach (KeyValuePair<string, int> entry in dictionary)
            {
                SetOverride(entry.Key, (ushort)entry.Value);
            }
        }
    }
}
 