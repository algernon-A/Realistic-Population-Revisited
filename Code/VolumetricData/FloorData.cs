using System.Linq;
using System.Collections.Generic;


namespace RealPop2
{
    /// <summary>
    /// Centralised store and management of floor calculation data.
    /// </summary>
    internal class FloorData : CalcData
    {
        // Instance reference.
        internal static FloorData instance;

        // Custom overrides.
        protected internal Dictionary<string, FloorDataPack> overrides;


        /// <summary>
        /// Returns a list of available calculation packs.
        /// </summary>
        /// <returns>Array of available calculation packs</returns>
        internal DataPack[] Packs => calcPacks.ToArray();


        /// <summary>
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        public FloorData()
        {
            // Default; standard 3m stories.
            FloorDataPack newPack = new FloorDataPack
            {
                name = "generic",
                displayName = Translations.Translate("RPR_PCK_FDF_NAM"),
                description = Translations.Translate("RPR_PCK_FDF_DES"),
                version = DataVersion.one,
                floorHeight = 3f,
                firstFloorMin = 3f,
                firstFloorExtra = 0f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Standalone houses.
            newPack = new FloorDataPack
            {
                name = "house",
                displayName = Translations.Translate("RPR_PCK_FHO_NAM"),
                description = Translations.Translate("RPR_PCK_FHO_DES"),
                version = DataVersion.one,
                floorHeight = 3f,
                firstFloorMin = 3f,
                firstFloorExtra = 0f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Buildings with lobbies.
            newPack = new FloorDataPack
            {
                name = "lobbies",
                displayName = Translations.Translate("RPR_PCK_FDL_NAM"),
                description = Translations.Translate("RPR_PCK_FDL_DES"),
                version = DataVersion.one,
                floorHeight = 3f,
                firstFloorMin = 3f,
                firstFloorExtra = 1f,
                firstFloorEmpty = true
            };
            calcPacks.Add(newPack);

            // Commercial buildings
            newPack = new FloorDataPack
            {
                name = "commercial",
                displayName = Translations.Translate("RPR_PCK_FCM_NAM"),
                description = Translations.Translate("RPR_PCK_FCM_DES"),
                version = DataVersion.one,
                floorHeight = 4f,
                firstFloorMin = 3f,
                firstFloorExtra = 3f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Warehouses (commercial and industrial)
            newPack = new FloorDataPack
            {
                name = "warehouse",
                displayName = Translations.Translate("RPR_PCK_FWH_NAM"),
                description = Translations.Translate("RPR_PCK_FWH_DES"),
                version = DataVersion.one,
                floorHeight = 9f,
                firstFloorMin = 3f,
                firstFloorExtra = 6f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // High-bay warehouses (commercial and industrial)
            newPack = new FloorDataPack
            {
                name = "highbay",
                displayName = Translations.Translate("RPR_PCK_FHB_NAM"),
                description = Translations.Translate("RPR_PCK_FHB_DES"),
                version = DataVersion.one,
                floorHeight = 12f,
                firstFloorMin = 3f,
                firstFloorExtra = 9f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Extractors and processors
            newPack = new FloorDataPack
            {
                name = "extractor",
                displayName = Translations.Translate("RPR_PCK_FEX_NAM"),
                description = Translations.Translate("RPR_PCK_FEX_DES"),
                version = DataVersion.one,
                floorHeight = 99f,
                firstFloorMin = 3f,
                firstFloorExtra = 9f,
                firstFloorEmpty = false
            };
            calcPacks.Add(newPack);

            // Initialise overrides dictionary.
            overrides = new Dictionary<string, FloorDataPack>();
        }


        /// <summary>
        /// Checks to see if a builing has a custom floor override, and if so, returns it.
        /// </summary>
        /// <param name="buildingName">Name of building prefab to check</param>
        /// <returns>Override floor pack if the building has one, othewise null</returns>
        internal FloorDataPack HasOverride(string buildingName) => overrides.ContainsKey(buildingName) ? overrides[buildingName] : null;


        /// <summary>
        /// Adds a custom floor override to a building prefab, but does NOT update live prefab data or save the configuration file.
        /// Used to populate dictionary when the prefab isn't available (e.g. before loading is complete).
        /// </summary>
        /// <param name="building">Name of building prefab to add</param>
        /// <param name="overridePack">Override floor pack to set</param>
        internal void SetOverride(string buildingName, FloorDataPack overridePack)
        {
            // Check to see if we already have an entry for this building.
            if (!overrides.ContainsKey(buildingName))
            {
                // No - create new entry.
                overrides.Add(buildingName, overridePack);
            }
            else
            {
                // An entry for this building already exists - update it.
                overrides[buildingName] = overridePack;
            }
        }


        /// <summary>
        /// Sets a custom floor override override for the given building prefab, and saves the updated configuration; and also UPDATES live prefab data.
        /// Used to add an entry in-game after prefabs have loaded.
        /// </summary>
        /// <param name="prefab">Building prefab/param>
        /// <param name="overridePack">Override floor pack to set</param>
        internal void SetOverride(BuildingInfo prefab, FloorDataPack overridePack)
        {
            // Apply changes.
            SetOverride(prefab.name, overridePack);

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
        /// Returns the currently active floor pack for a building prefab.
        /// </summary>
        /// <param name="building">Building prefab to get floor pack for</param>
        /// <returns></returns>
        internal override DataPack ActivePack(BuildingInfo building) => HasOverride(building.name) ?? base.ActivePack(building);


        /// <summary>
        /// Returns the inbuilt default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default calculation data pack</returns>
        internal override DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            string defaultName;

            // Manual breakdown.
            switch (service)
            {
                case ItemClass.Service.Residential:
                    switch (subService)
                    {
                        case ItemClass.SubService.ResidentialHigh:
                        case ItemClass.SubService.ResidentialHighEco:
                            defaultName = "generic";
                            break;
                        default:
                            defaultName = "house";
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
            return calcPacks.Find(pack => pack.name.Equals(defaultName));
        }


        /// <summary>
        /// Serializes building pack settings to XML.
        /// </summary>
        /// <param name="existingList">Existing list to modify, from population pack serialization (null if none)</param>
        /// <returns>New sorted list with building pack settings</returns>
        internal SortedList<string, BuildingRecord> SerializeBuildings(SortedList<string, BuildingRecord> existingList)
        {
            // Return list.
            SortedList<string, BuildingRecord> returnList = existingList ?? new SortedList<string, BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
            foreach (string prefabName in buildingDict.Keys)
            {
                string packName = buildingDict[prefabName].name;

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this floor pack.
                    returnList[prefabName].floorPack = packName;
                }
                else
                {
                    // No; add a new record with this floor pack.
                    BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, floorPack = packName };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList;
        }


        /// <summary>
        /// Extracts the relevant pack name (floor or pop pack) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from</param>
        /// <returns>Floor pack name (if any)</returns>
        protected override string BuildingPack(BuildingRecord buildingRecord) => buildingRecord.floorPack;
    }
}