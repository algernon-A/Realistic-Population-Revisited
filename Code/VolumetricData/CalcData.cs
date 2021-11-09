using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;


namespace RealPop2
{
    /// <summary>
    /// Static data management utility class.
    /// </summary>
    internal static class DataUtils
    {
        /// <summary>
        /// Performs required data and configuration setup.
        /// </summary>
        internal static void Setup()
        {
            // Don't initialise PopData if we've already done it, but make sure we do it if we haven't already.
            if (PopData.instance == null)
            {
                PopData.instance = new PopData();
            }

            // Do FloorData as well if we need to.
            if (FloorData.instance == null)
            {
                FloorData.instance = new FloorData();
            }

            // Ditto SchoolData.
            if (SchoolData.instance == null)
            {
                SchoolData.instance = new SchoolData();
            }

            // Ditto Multipliers.
            if (Multipliers.instance == null)
            {
                Multipliers.instance = new Multipliers();
            }

            // Load (volumetric) building settings file if we haven't already..
            if (!ConfigUtils.configRead)
            {
                ConfigUtils.LoadSettings();
            }
        }
    }


    /// <summary>
    /// Centralised store and management of floor calculation data.
    /// </summary>
    internal abstract class CalcData
    {
        // List of data definition packs.
        internal List<DataPack> calcPacks;

        // List of building settings.
        protected Dictionary<string, DataPack> buildingDict;

        // List of (sub)service default settings.
        private readonly Dictionary<ItemClass.Service, Dictionary<ItemClass.SubService, DataPack>> defaultsDict;


        // Methods that should generally be overriden.
        internal virtual DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService) => null;
        protected abstract string BuildingPack(BuildingRecord buildingRecord);


        /// <summary>
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        internal CalcData()
        {
            // Initialise list of data packs.
            calcPacks = new List<DataPack>();

            // Initialise building and service dictionaries.
            defaultsDict = new Dictionary<ItemClass.Service, Dictionary<ItemClass.SubService, DataPack>>();
            buildingDict = new Dictionary<string, DataPack>();
        }


        /// <summary>
        /// Updates our building setting dictionary for the selected building prefab to the indicated calculation pack.
        /// </summary>
        /// <param name="prefab">Building prefab to update</param>
        /// <param name="pack">New data pack to apply</param>
        internal virtual void UpdateBuildingPack(BuildingInfo prefab, DataPack pack)
        {
            // Don't do anything with null packs (e.g. null school packs).
            if (pack == null)
            {
                return;
            }

            // Local reference.
            string buildingName = prefab.name;

            // Check to see if this pack matches the default.
            bool isDefault = pack == CurrentDefaultPack(prefab);

            // Check to see if this building already has an entry.
            if (buildingDict.ContainsKey(buildingName))
            {
                // Contains an entry - check to see if this pack matches the default.
                if (isDefault)
                {
                    // Matches the default - just remove the custom entry.
                    buildingDict.Remove(buildingName);
                }
                else
                {
                    // Doesn't match the default - update the existing entry.
                    buildingDict[buildingName] = pack;
                }
            }
            else if (!isDefault)
            {
                // No entry yet and the pack isn't the default - add a custom entry.
                buildingDict.Add(buildingName, pack);
            }

            // Refresh the prefab's population settings to reflect changes.
            RefreshPrefab(prefab);
        }


        /// <summary>
        /// Returns the currently active calculation data pack record for the given prefab.
        /// </summary>
        /// <param name="building">Selected prefab</param>
        /// <returns>Currently active size pack</returns>
        internal virtual DataPack ActivePack(BuildingInfo building) => HasPackOverride(building.name) ?? CurrentDefaultPack(building);


        /// <summary>
        /// Returns the currently active calculation data pack record for the given prefab if an override is in place, or null if none (using the default).
        /// </summary>
        /// <param name="buildingName">Name of selected prefab</param>
        /// <returns>Currently active calculation pack override for the building if one exists, otherwise null.</returns>
        internal DataPack HasPackOverride(string buildingName)
        {
            // Check to see if this building has an entry in the custom settings dictionary.
            if (buildingDict.ContainsKey(buildingName))
            {
                // Custom settings available - use them.
                return buildingDict[buildingName];
            }
            else
            {
                // Use default selection.
                return null;
            }
        }


        /// <summary>
        /// Clears all default pack overrides (effectively restoring default settings).
        /// </summary>
        internal void ClearDefaultPacks() => defaultsDict.Clear();


        /// <summary>
        /// Returns the currently set default calculation pack for the given prefab's service/subservice.
        /// </summary>
        /// <param name="building">Building prefab</param>
        /// <returns>Default calculation data pack</returns>
        internal virtual DataPack CurrentDefaultPack(BuildingInfo building) => CurrentDefaultPack(building.GetService(), building.GetSubService());


        /// <summary>
        /// Returns the currently set default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <returns>Default calculation data pack</returns>
        internal DataPack CurrentDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            // See if we've got an entry for this service.
            if (defaultsDict.ContainsKey(service))
            {
                // We do; check for sub-service entry.
                if (defaultsDict[service].ContainsKey(subService))
                {
                    // Got an entry!  Return it.
                    return defaultsDict[service][subService];
                }
            }

            // If we got here, we didn't get a match; return base default entry.
            return BaseDefaultPack(service, subService);
        }


        /// <summary>
        /// Adds/replaces default dictionary entry for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service</param>
        /// <param name="subService">Sub-service</param>
        /// <param name="calcPack">New default calculation pack to apply</param>
        internal void ChangeDefault(ItemClass.Service service, ItemClass.SubService subService, DataPack calcPack)
        {
            // Get base default pack.
            DataPack baseDefault = BaseDefaultPack(service, subService);

            // If base default pack is the same as the new pack, simply delete any existing record (if one exists).
            if (calcPack.name.Equals(baseDefault.name))
            {
                // Check for matching service.
                if (defaultsDict.ContainsKey(service))
                {
                    // Chech for matching sub-service.
                    if (defaultsDict[service].ContainsKey(subService))
                    {
                        // Remove sub-service entry.
                        defaultsDict[service].Remove(subService);
                        
                        // If not sub-service entries left under this service entry, remove the entire service entry.
                        if (defaultsDict[service].Count == 0)
                        {
                            defaultsDict.Remove(service);
                        }
                    }
                }

                // Done here; return.
                return;
            }

            // If we got here, then the entry to be applied isn't the base default - first, check for existing key in our services dictionary for this service.
            if (!defaultsDict.ContainsKey(service))
            {
                // No existing entry - add one.
                defaultsDict.Add(service, new Dictionary<ItemClass.SubService, DataPack>());
            }

            // Check for existing sub-service key.
            if (defaultsDict[service].ContainsKey(subService))
            {
                // Existing key found - update entry.
                defaultsDict[service][subService] = calcPack;
            }
            else
            {
                // No existing key found - add entry.
                defaultsDict[service].Add(subService, calcPack);
            }
        }


        /// <summary>
        /// Deserializes the provided XML default list.
        /// </summary>
        /// <param name="list">XML DefaultPack list to deserialize</param>
        internal void DeserializeDefaults(List<DefaultPack> list)
        {
            // Deserialise default pop pack list into dictionary.
            for (int i = 0; i < list.Count; ++i)
            {
                DefaultPack defaultPack = list[i];

                // Find target preset.
                DataPack calcPack = calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(defaultPack.pack)));
                if (calcPack?.name == null)
                {
                    Logging.Error("Couldn't find pop calculation pack ", defaultPack.pack, " for sub-service ", defaultPack.subService);
                    continue;
                }

                // Add service to our dictionary.
                ChangeDefault(defaultPack.service, defaultPack.subService, calcPack);
            }
        }


        /// <summary>
        /// Deserializes a provided building record list.
        /// </summary>
        /// <param name="recordList">List to deserialize</param>
        internal void DeserializeBuildings(List<BuildingRecord> recordList)
        {
            // Iterate through each record in list.
            for (int i = 0; i < recordList.Count; ++i)
            {
                BuildingRecord buildingRecord = recordList[i];

                // Get relevant pack (pop or floor) name.
                string packName = BuildingPack(buildingRecord);

                // Safety first!
                if (buildingRecord.prefab.IsNullOrWhiteSpace() || packName.IsNullOrWhiteSpace())
                {
                    continue;
                }

                // Find target preset.
                DataPack calcPack = calcPacks?.Find(pack => (pack?.name != null && pack.name.Equals(packName)));
                if (calcPack?.name == null)
                {
                    Logging.Error("Couldn't find calculation pack ", packName," for ", buildingRecord.prefab);
                    continue;
                }

                // Add building to our dictionary.
                buildingDict.Add(buildingRecord.prefab, calcPack);
            }
        }


        /// <summary>
        /// Serializes the current list of defaults to XML.
        /// </summary>
        /// <returns>New list of serialized defaults</returns>
        internal List<DefaultPack> SerializeDefaults()
        {
            // Return list.
            List<DefaultPack> defaultList = new List<DefaultPack>();


            // Iterate through each key (ItemClass.Service) in our dictionary.
            foreach (ItemClass.Service service in defaultsDict.Keys)
            {
                // Iterate through each key (ItemClass.SubService) in our sub-dictionary and serialise it into a DefaultPack.
                foreach (ItemClass.SubService subService in defaultsDict[service].Keys)
                {
                    DefaultPack defaultPack = new DefaultPack
                    {
                        service = service,
                        subService = subService,
                        pack = defaultsDict[service][subService].name
                    };

                    // Add new building record to return list.e.
                    defaultList.Add(defaultPack);
                }
            }

            return defaultList;
        }


        /// <summary>
        /// Adds or updates a calculation pack entry to our list.
        /// </summary>
        /// <param name="calcPack">Calculation pack to add</param>
        internal void AddCalculationPack(DataPack calcPack)
        {
            // Iterate through the list of packs, looking for a name match.
            for (int i = 0; i < calcPacks.Count; ++i)
            {
                if (calcPacks[i].name.Equals(calcPack.name))
                {
                    // Found a match - replace with our new entry and return.
                    calcPacks[i] = calcPack;
                    return;
                }
            }

            // If we got here, we didn't find a match; add this pack to the list.
            calcPacks.Add(calcPack);
        }


        /// <summary>
        /// Triggers recalculation of buildings in-game when the pack changes.
        /// </summary>
        /// <param name="calcPack">Pack that's been changed</param>
        internal void CalcPackChanged(DataPack calcPack)
        {
            // Iterate through each loaded BuildingInfo.
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); ++i)
            {
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetLoaded(i);

                // Check to see if the currently active pack for the prefab is the one that's been changed.
                if (ActivePack(prefab) == calcPack)
                {
                    // If so, refresh the prefab's population settings to reflect changes.
                    RefreshPrefab(prefab);
                }
            }
        }


        /// <summary>
        /// Refreshes a prefab's population settings to reflect changes.
        /// </summary>
        /// <param name="prefab">Prefab to refresh</param>
        protected void RefreshPrefab(BuildingInfo prefab)
        {
            // Clear out any cached calculations for households.workplaces (depending on whether or not this is residential).
            if (prefab.GetService() == ItemClass.Service.Residential)
            {
                // Remove from household cache.
                PopData.instance.householdCache.Remove(prefab);
            }
            else
            {
                // Remove from workplace cache.
                PopData.instance.workplaceCache.Remove(prefab);

                // Force RICO refresh, if we're using Ploppable RICO Revisited.
                if (ModUtils.ricoClearWorkplace != null)
                {
                    ModUtils.ricoClearWorkplace.Invoke(null, new object[] { prefab });
                }
            }

            // Update CitizenUnits for existing instances of this building.
            CitizenUnitUtils.UpdateCitizenUnits(prefab.name, ItemClass.Service.None, prefab.GetSubService(), false);
        }
    }
}
