// <copyright file="Multipliers.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using ColossalFramework;

    /// <summary>
    /// Building capacity multipliers.
    /// </summary>
    internal class Multipliers
    {
        // Default multiplier.
        internal const float DefaultMultiplier = 1.0f;

        // Instance reference.
        internal static Multipliers instance;

        // List of building settings.
        private readonly Dictionary<string, float> buildingDict;

        /// <summary>
        /// Constructor - initializes dictionary and performs other setup tasks.
        /// </summary>
        public Multipliers()
        {
            // Initialise building dictionary.
            buildingDict = new Dictionary<string, float>();
        }

        /// <summary>
        /// Checks if there is a current mulitplier override for the given prefab.
        /// </summary>
        /// <param name="buildingName">Name of selected prefab.</param>
        /// <returns>True if a muliplier override currently exists, false otherwise.</returns>
        internal bool HasOverride(string buildingName) => buildingDict.ContainsKey(buildingName);

        /// <summary>
        /// Deletes the building mulitplier override (if any) for the given prefab.
        /// </summary>
        /// <param name="buildingName">Name of selected prefab</param>
        internal void DeleteMultiplier(string buildingName) => buildingDict.Remove(buildingName);

        /// <summary>
        /// Returns the currently active multipler for the given prefab (custom if set, otherwise global default).
        /// </summary>
        /// <param name="building">Selected prefab.</param>
        /// <returns>Currently active multiplier, or 1.0 by default if no override in place.</returns>
        internal float ActiveMultiplier(BuildingInfo building)
        {
            // Only doing education buildings for now.
            if (building != null && building.GetService() == ItemClass.Service.Education)
            {
                // Check to see if we have a multiplier override in effect.
                if (buildingDict.ContainsKey(building.name))
                {
                    // Yes - return the mutlplier.
                    return buildingDict[building.name];
                }

                // No entry - return school default.
                return ModSettings.DefaultSchoolMult;
            }

            // If we got here, we don't have a multiplier override; return the default.
            return DefaultMultiplier;
        }

        /// <summary>
        /// Changes (adding or updating) the currently set multiplier for the given building prefab.
        /// </summary>
        /// <param name="prefab">Selected prefab.</param>
        /// <param name="multiplier">New multiplier to apply.</param>
        internal void UpdateMultiplier(BuildingInfo prefab, float multiplier)
        {
            string buildingName = prefab.name;

            // Currently only accepting multipliers for school buildings.
            if (prefab.GetService() != ItemClass.Service.Education)
            {
                // Warn if an attempt was made to set a mulitplier other than one.
                if (multiplier != 1f)
                {
                    Logging.Error("attempting to set multiplier ", multiplier," for non-education building ", buildingName);
                }
                return;
            }

            // Check to see if we have an existing entry.
            if (buildingDict.ContainsKey(buildingName))
            {
                // Udate dictionary entry.
                buildingDict[buildingName] = multiplier;
            }
            else
            {
                // No existing entry - create a new dictionary entry.
                buildingDict.Add(buildingName, multiplier);
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

                // Get multiplier.
                float multiplier = buildingRecord.Multiplier;

                // Ignore invalid or default records.
                if (buildingRecord.Prefab.IsNullOrWhiteSpace() || multiplier <= 1.0f)
                {
                    continue;
                }

                // Add building to our dictionary.
                buildingDict.Add(buildingRecord.Prefab, multiplier);
            }
        }

        /// <summary>
        /// Serializes building pack settings to XML.
        /// </summary>
        /// <param name="existingList">Existing list to modify, from population pack serialization (null if none).</param>
        /// <returns>New list of building pack settings ready for XML.</returns>
        internal SortedList<string, Configuration.BuildingRecord> SerializeBuildings(SortedList<string, Configuration.BuildingRecord> existingList)
        {
            // Return list.
            SortedList<string, Configuration.BuildingRecord> returnList = existingList ?? new SortedList<string, Configuration.BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
            foreach (string prefabName in buildingDict.Keys)
            {
                // Get multiplier.
                float multiplier = buildingDict[prefabName];

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this multiplier.
                    returnList[prefabName].Multiplier = multiplier;
                }
                else
                {
                    // No; add a new record with this multiplier.
                    Configuration.BuildingRecord newRecord = new Configuration.BuildingRecord { Prefab = prefabName, Multiplier = multiplier };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList;
        }
    }
}