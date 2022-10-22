// <copyright file="CalcData.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Centralised store and management of floor calculation data.
    /// </summary>
    internal abstract class CalcData
    {
        /// <summary>
        /// Dictionary of per-building settings.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected internal field")]
        protected internal readonly Dictionary<string, DataPack> BuildingDict;

        // List of (sub)service default settings.
        private readonly Dictionary<ItemClass.Service, Dictionary<ItemClass.SubService, DataPack>> _defaultsDict;

        // List of data definition packs.
        private readonly List<DataPack> _calcPacks;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcData"/> class.
        /// </summary>
        internal CalcData()
        {
            // Initialise list of data packs.
            _calcPacks = new List<DataPack>();

            // Initialise building and service dictionaries.
            _defaultsDict = new Dictionary<ItemClass.Service, Dictionary<ItemClass.SubService, DataPack>>();
            BuildingDict = new Dictionary<string, DataPack>();
        }

        /// <summary>
        /// Gets the list of data definition packs.
        /// </summary>
        internal List<DataPack> CalcPacks => _calcPacks;

        /// <summary>
        /// Performs required data and configuration setup.
        /// </summary>
        internal static void Setup()
        {
            // Ensure instances are available.
            PopData.EnsureInstance();
            FloorData.EnsureInstance();
            SchoolData.EnsureInstance();
            Multipliers.EnsureInstance();

            // Load (volumetric) building settings file if we haven't already..
            if (!ConfigurationUtils.ConfigRead)
            {
                ConfigurationUtils.LoadSettings();
            }
        }

        /// <summary>
        /// Gets the base default pack for the given service and subservice.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Base default pack (null if not applicable).</returns>
        internal virtual DataPack BaseDefaultPack(ItemClass.Service service, ItemClass.SubService subService) => null;

        /// <summary>
        /// Updates our building setting dictionary for the selected building prefab to the indicated calculation pack.
        /// </summary>
        /// <param name="prefab">Building prefab to update.</param>
        /// <param name="pack">New data pack to apply.</param>
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
            if (BuildingDict.ContainsKey(buildingName))
            {
                // Contains an entry - check to see if this pack matches the default.
                if (isDefault)
                {
                    // Matches the default - just remove the custom entry.
                    BuildingDict.Remove(buildingName);
                }
                else
                {
                    // Doesn't match the default - update the existing entry.
                    BuildingDict[buildingName] = pack;
                }
            }
            else if (!isDefault)
            {
                // No entry yet and the pack isn't the default - add a custom entry.
                BuildingDict.Add(buildingName, pack);
            }

            // Refresh the prefab's population settings to reflect changes.
            RefreshPrefab(prefab);
        }

        /// <summary>
        /// Returns the currently active calculation data pack record for the given prefab.
        /// </summary>
        /// <param name="building">Selected prefab.</param>
        /// <returns>Currently active size pack.</returns>
        internal virtual DataPack ActivePack(BuildingInfo building) => HasPackOverride(building.name) ?? CurrentDefaultPack(building);

        /// <summary>
        /// Returns the currently active calculation data pack record for the given prefab if an override is in place, or null if none (using the default).
        /// </summary>
        /// <param name="buildingName">Name of selected prefab.</param>
        /// <returns>Currently active calculation pack override for the building if one exists, otherwise null.</returns>
        internal DataPack HasPackOverride(string buildingName)
        {
            // Check to see if this building has an entry in the custom settings dictionary.
            if (BuildingDict.ContainsKey(buildingName))
            {
                // Custom settings available - use them.
                return BuildingDict[buildingName];
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
        internal void ClearDefaultPacks() => _defaultsDict.Clear();

        /// <summary>
        /// Returns the currently set default calculation pack for the given prefab's service/subservice.
        /// </summary>
        /// <param name="building">Building prefab.</param>
        /// <returns>Default calculation data pack.</returns>
        internal virtual DataPack CurrentDefaultPack(BuildingInfo building) => CurrentDefaultPack(building.GetService(), building.GetSubService());

        /// <summary>
        /// Returns the currently set default calculation pack for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="subService">Sub-service.</param>
        /// <returns>Default calculation data pack.</returns>
        internal DataPack CurrentDefaultPack(ItemClass.Service service, ItemClass.SubService subService)
        {
            // See if we've got an entry for this service.
            if (_defaultsDict.ContainsKey(service))
            {
                // We do; check for sub-service entry.
                if (_defaultsDict[service].ContainsKey(subService))
                {
                    // Got an entry!  Return it.
                    return _defaultsDict[service][subService];
                }
            }

            // If we got here, we didn't get a match; return base default entry.
            return BaseDefaultPack(service, subService);
        }

        /// <summary>
        /// Adds/replaces default dictionary entry for the given service/subservice combination.
        /// </summary>
        /// <param name="service">Service.</param>
        /// <param name="subService">Sub-service.</param>
        /// <param name="calcPack">New default calculation pack to apply.</param>
        internal void ChangeDefault(ItemClass.Service service, ItemClass.SubService subService, DataPack calcPack)
        {
            // Get base default pack.
            DataPack baseDefault = BaseDefaultPack(service, subService);

            // If base default pack is the same as the new pack, simply delete any existing record (if one exists).
            if (calcPack.Name.Equals(baseDefault.Name))
            {
                // Check for matching service.
                if (_defaultsDict.ContainsKey(service))
                {
                    // Chech for matching sub-service.
                    if (_defaultsDict[service].ContainsKey(subService))
                    {
                        // Remove sub-service entry.
                        _defaultsDict[service].Remove(subService);

                        // If not sub-service entries left under this service entry, remove the entire service entry.
                        if (_defaultsDict[service].Count == 0)
                        {
                            _defaultsDict.Remove(service);
                        }
                    }
                }

                // Done here; return.
                return;
            }

            // If we got here, then the entry to be applied isn't the base default - first, check for existing key in our services dictionary for this service.
            if (!_defaultsDict.ContainsKey(service))
            {
                // No existing entry - add one.
                _defaultsDict.Add(service, new Dictionary<ItemClass.SubService, DataPack>());
            }

            // Check for existing sub-service key.
            if (_defaultsDict[service].ContainsKey(subService))
            {
                // Existing key found - update entry.
                _defaultsDict[service][subService] = calcPack;
            }
            else
            {
                // No existing key found - add entry.
                _defaultsDict[service].Add(subService, calcPack);
            }
        }

        /// <summary>
        /// Deserializes the provided XML default list.
        /// </summary>
        /// <param name="list">XML DefaultPack list to deserialize.</param>
        internal void DeserializeDefaults(List<Configuration.DefaultPack> list)
        {
            // Deserialise default pop pack list into dictionary.
            for (int i = 0; i < list.Count; ++i)
            {
                Configuration.DefaultPack defaultPack = list[i];

                // Find target preset.
                DataPack calcPack = _calcPacks?.Find(pack => (pack?.Name != null && pack.Name.Equals(defaultPack.Pack)));
                if (calcPack?.Name == null)
                {
                    Logging.Error("Couldn't find pop calculation pack ", defaultPack.Pack, " for sub-service ", defaultPack.SubService);
                    continue;
                }

                // Add service to our dictionary.
                ChangeDefault(defaultPack.Service, defaultPack.SubService, calcPack);
            }
        }

        /// <summary>
        /// Deserializes a provided building record list.
        /// </summary>
        /// <param name="recordList">List to deserialize.</param>
        internal void DeserializeBuildings(List<Configuration.BuildingRecord> recordList)
        {
            // Iterate through each record in list.
            for (int i = 0; i < recordList.Count; ++i)
            {
                Configuration.BuildingRecord buildingRecord = recordList[i];

                // Get relevant pack (pop or floor) name.
                string packName = BuildingPack(buildingRecord);

                // Safety first!
                if (buildingRecord.Prefab.IsNullOrWhiteSpace() || packName.IsNullOrWhiteSpace())
                {
                    continue;
                }

                // Find target preset.
                DataPack calcPack = _calcPacks?.Find(pack => pack?.Name != null && pack.Name.Equals(packName));
                if (calcPack?.Name == null)
                {
                    Logging.Error("Couldn't find calculation pack ", packName, " for ", buildingRecord.Prefab);
                    continue;
                }

                // Add building to our dictionary.
                BuildingDict.Add(buildingRecord.Prefab, calcPack);
            }
        }

        /// <summary>
        /// Serializes the current list of defaults to XML.
        /// </summary>
        /// <returns>New list of serialized defaults.</returns>
        internal List<Configuration.DefaultPack> SerializeDefaults()
        {
            // Return list.
            List<Configuration.DefaultPack> defaultList = new List<Configuration.DefaultPack>();

            // Iterate through each key (ItemClass.Service) in our dictionary.
            foreach (ItemClass.Service service in _defaultsDict.Keys)
            {
                // Iterate through each key (ItemClass.SubService) in our sub-dictionary and serialise it into a DefaultPack.
                foreach (ItemClass.SubService subService in _defaultsDict[service].Keys)
                {
                    Configuration.DefaultPack defaultPack = new Configuration.DefaultPack
                    {
                        Service = service,
                        SubService = subService,
                        Pack = _defaultsDict[service][subService].Name,
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
        /// <param name="calcPack">Calculation pack to add.</param>
        internal void AddCalculationPack(DataPack calcPack)
        {
            // Iterate through the list of packs, looking for a name match.
            for (int i = 0; i < _calcPacks.Count; ++i)
            {
                if (_calcPacks[i].Name.Equals(calcPack.Name))
                {
                    // Found a match - replace with our new entry and return.
                    _calcPacks[i] = calcPack;
                    return;
                }
            }

            // If we got here, we didn't find a match; add this pack to the list.
            _calcPacks.Add(calcPack);
        }

        /// <summary>
        /// Triggers recalculation of buildings in-game when the pack changes.
        /// </summary>
        /// <param name="calcPack">Pack that's been changed.</param>
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
        /// Extracts the relevant pack name (floor or pop) from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from.</param>
        /// <returns>Floor pack name (if any).</returns>
        protected abstract string BuildingPack(Configuration.BuildingRecord buildingRecord);

        /// <summary>
        /// Refreshes a prefab's population settings to reflect changes.
        /// </summary>
        /// <param name="prefab">Prefab to refresh.</param>
        protected void RefreshPrefab(BuildingInfo prefab)
        {
            // Clear out any cached calculations for households.workplaces (depending on whether or not this is residential).
            if (prefab.GetService() == ItemClass.Service.Residential)
            {
                // Remove from household cache.
                PopData.Instance.ClearHousholdCache(prefab);
            }
            else
            {
                // Remove from workplace cache.
                PopData.Instance.ClearWorkplaceCache(prefab);

                // Remove from visitplace cache.
                PopData.Instance.ClearVisitplaceCache(prefab);

                // Force RICO refresh, if we're using Ploppable RICO Revisited.
                ModUtils.ClearRICOWorkplaces(prefab);
            }

            // Update CitizenUnits for existing instances of this building.
            CitizenUnitUtils.UpdateCitizenUnits(prefab.name, ItemClass.Service.None, prefab.GetSubService(), false);
        }
    }
}
