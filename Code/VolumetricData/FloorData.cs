// <copyright file="FloorData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;

    /// <summary>
    /// Centralised store and management of floor calculation data.
    /// </summary>
    internal class FloorData : CalcData
    {
        // Instance reference.
        private static FloorData s_instance;

        // Custom overrides.
        private readonly Dictionary<string, FloorDataPack> _overrides;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorData"/> class.
        /// </summary>
        private FloorData()
        {
            // Default; standard 3m stories.
            FloorDataPack newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "generic",
                NameKey = "RPR_PCK_FDF_NAM",
                DescriptionKey = "RPR_PCK_FDF_DES",
                m_floorHeight = 3f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 0f,
                m_firstFloorEmpty = false,
            };
            CalcPacks.Add(newPack);

            // Standalone houses.
            newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "house",
                NameKey = "RPR_PCK_FHO_NAM",
                DescriptionKey = "RPR_PCK_FHO_DES",
                m_floorHeight = 3f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 0f,
                m_firstFloorEmpty = false,
            };
            CalcPacks.Add(newPack);

            // Buildings with lobbies.
            newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "lobbies",
                NameKey = "RPR_PCK_FDL_NAM",
                DescriptionKey = "RPR_PCK_FDL_DES",
                m_floorHeight = 3f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 1f,
                m_firstFloorEmpty = true,
            };
            CalcPacks.Add(newPack);

            // Commercial buildings
            newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "commercial",
                NameKey = "RPR_PCK_FCM_NAM",
                DescriptionKey = "RPR_PCK_FCM_DES",
                m_floorHeight = 4f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 3f,
                m_firstFloorEmpty = false,
            };
            CalcPacks.Add(newPack);

            // Warehouses (commercial and industrial)
            newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "warehouse",
                NameKey = "RPR_PCK_FWH_NAM",
                DescriptionKey = "RPR_PCK_FWH_DES",
                m_floorHeight = 9f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 6f,
                m_firstFloorEmpty = false,
            };
            CalcPacks.Add(newPack);

            // High-bay warehouses (commercial and industrial)
            newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "highbay",
                NameKey = "RPR_PCK_FHB_NAM",
                DescriptionKey = "RPR_PCK_FHB_DES",
                m_floorHeight = 12f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 9f,
                m_firstFloorEmpty = false,
            };
            CalcPacks.Add(newPack);

            // Extractors and processors
            newPack = new FloorDataPack(DataPack.DataVersion.One)
            {
                Name = "extractor",
                NameKey = "RPR_PCK_FEX_NAM",
                DescriptionKey = "RPR_PCK_FEX_DES",
                m_floorHeight = 99f,
                m_firstFloorMin = 3f,
                m_firstFloorExtra = 9f,
                m_firstFloorEmpty = false,
            };
            CalcPacks.Add(newPack);

            // Initialise overrides dictionary.
            _overrides = new Dictionary<string, FloorDataPack>();
        }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        internal static FloorData Instance => s_instance;

        /// <summary>
        /// Gets the list of available calculation packs.
        /// </summary>
        /// <returns>Array of available calculation packs.</returns>
        internal DataPack[] Packs => CalcPacks.ToArray();

        /// <summary>
        /// Ensures that a valid instance is instantiated and ready for use.
        /// </summary>
        internal static void EnsureInstance()
        {
            if (s_instance == null)
            {
                s_instance = new FloorData();
            }
        }

        /// <summary>
        /// Checks to see if a builing has a custom floor override, and if so, returns it.
        /// </summary>
        /// <param name="buildingName">Name of building prefab to check.</param>
        /// <returns>Override floor pack if the building has one, othewise null.</returns>
        internal FloorDataPack HasOverride(string buildingName) => _overrides.ContainsKey(buildingName) ? _overrides[buildingName] : null;

        /// <summary>
        /// Adds a custom floor override to a building prefab, but does NOT update live prefab data or save the configuration file.
        /// Used to populate dictionary when the prefab isn't available (e.g. before loading is complete).
        /// </summary>
        /// <param name="buildingName">Name of building prefab to add.</param>
        /// <param name="overridePack">Override floor pack to set.</param>
        internal void SetOverride(string buildingName, FloorDataPack overridePack)
        {
            // Check to see if we already have an entry for this building.
            if (!_overrides.ContainsKey(buildingName))
            {
                // No - create new entry.
                _overrides.Add(buildingName, overridePack);
            }
            else
            {
                // An entry for this building already exists - update it.
                _overrides[buildingName] = overridePack;
            }
        }

        /// <summary>
        /// Sets a custom floor override override for the given building prefab, and saves the updated configuration; and also UPDATES live prefab data.
        /// Used to add an entry in-game after prefabs have loaded.
        /// </summary>
        /// <param name="prefab">Building prefab.</param>
        /// <param name="overridePack">Override floor pack to set.</param>
        internal void SetOverride(BuildingInfo prefab, FloorDataPack overridePack)
        {
            // Apply changes.
            SetOverride(prefab.name, overridePack);

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
        /// Returns the currently active floor pack for a building prefab.
        /// </summary>
        /// <param name="building">Building prefab to get floor pack for.</param>
        /// <returns>The currently active floor pack.</returns>
        internal override DataPack ActivePack(BuildingInfo building) => HasOverride(building.name) ?? base.ActivePack(building);

        /// <summary>
        /// Returns the inbuilt default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Default calculation data pack.</returns>
        internal override DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            string defaultName;

            // Manual breakdown.
            switch (service)
            {
                case ItemClass.Service.Residential:
                    switch (subService)
                    {
                        case ItemClass.SubService.ResidentialLow:
                        case ItemClass.SubService.ResidentialLowEco:
                            defaultName = "house";
                            break;
                        default:
                        case ItemClass.SubService.ResidentialHigh:
                        case ItemClass.SubService.ResidentialHighEco:
                            defaultName = "generic";
                            break;
                    }

                    break;

                case ItemClass.Service.Industrial:
                    switch (subService)
                    {
                        case ItemClass.SubService.IndustrialFarming:
                        case ItemClass.SubService.IndustrialForestry:
                        case ItemClass.SubService.IndustrialOil:
                        case ItemClass.SubService.IndustrialOre:
                            defaultName = "extractor";
                            break;
                        default:
                            defaultName = "commercial";
                            break;
                    }

                    break;

                default:
                    // Default is commercial.
                    defaultName = "commercial";
                    break;
            }

            // Match name to floorpack.
            return CalcPacks.Find(pack => pack.Name.Equals(defaultName));
        }

        /// <summary>
        /// Serializes building pack settings to XML.
        /// </summary>
        /// <param name="existingList">Existing list to modify, from population pack serialization (null if none).</param>
        /// <returns>New sorted list with building pack settings.</returns>
        internal SortedList<string, Configuration.BuildingRecord> SerializeBuildings(SortedList<string, Configuration.BuildingRecord> existingList)
        {
            // Return list.
            SortedList<string, Configuration.BuildingRecord> returnList = existingList ?? new SortedList<string, Configuration.BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
            foreach (string prefabName in BuildingDict.Keys)
            {
                string packName = BuildingDict[prefabName].Name;

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this floor pack.
                    returnList[prefabName].FloorPack = packName;
                }
                else
                {
                    // No; add a new record with this floor pack.
                    Configuration.BuildingRecord newRecord = new Configuration.BuildingRecord { Prefab = prefabName, FloorPack = packName };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Serializes current floor overrides to XML.
        /// </summary>
        /// <returns>New list of current floor overrides.</returns>
        internal List<Configuration.FloorCalcOverride> SerializeOverrides()
        {
            List<Configuration.FloorCalcOverride> returnList = new List<Configuration.FloorCalcOverride>();

            // Iterate though each entry in dictionary and add to list as a new XML record.
            foreach (KeyValuePair<string, FloorDataPack> floorOverride in _overrides)
            {
                returnList.Add(new Configuration.FloorCalcOverride
                {
                    Prefab = floorOverride.Key,
                    FirstHeight = floorOverride.Value.m_firstFloorMin,
                    FloorHeight = floorOverride.Value.m_floorHeight,
                });
            }

            return returnList;
        }

        /// <summary>
        /// Extracts the relevant pack name (floor or pop pack) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from.</param>
        /// <returns>Floor pack name (if any).</returns>
        protected override string BuildingPack(Configuration.BuildingRecord buildingRecord) => buildingRecord.FloorPack;
    }
}