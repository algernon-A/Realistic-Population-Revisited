// <copyright file="OverrideUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    /// <summary>
    /// Utilities to handle building overrides.
    /// </summary>
    internal static class OverrideUtils
    {
        /// <summary>
        /// Returns the customised number of households for a given prefab.
        /// Returns 0 if no custom settings exist.
        /// </summary>
        /// <param name="prefab">The prefab (BuldingInfo) to query</param>
        /// <returns>The custom household count (0 if no settings).</returns>
        public static int GetResidential(BuildingInfo prefab)
        {
            if (DataStore.householdCache.TryGetValue(prefab.name, out int returnValue))
            {
                return returnValue;
            }
            return 0;
        }

        /// <summary>
        /// Removes the custom household record (if any) for a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab (BuildingInfo) to remove the record from.</param>
        public static void RemoveResidential(BuildingInfo prefab)
        {
            // Remove the entry from the configuration file cache.
            DataStore.householdCache.Remove(prefab.name);

            // Save the updated configuration files.
            XMLUtilsWG.WriteToXML();
            ConfigUtils.SaveSettings();

            // Remove current building's record from 'live' dictionary.
            PopData.instance.householdCache.Remove(prefab);
        }

        /// <summary>
        /// Returns the customised number of workers for a given prefab.
        /// Returns 0 if no custom settings exist.
        /// </summary>
        /// <param name="prefab">The custom worker count (0 if no settings).</param>
        /// <returns></returns>
        public static int GetWorker(BuildingInfo prefab)
        {
            if (DataStore.workerCache.TryGetValue(prefab.name, out int returnValue))
            {
                return returnValue;
            }
            return 0;
        }

        /// <summary>
        /// Removes the custom worker record (if any) for a given prefab.
        /// </summary>
        /// <param name="prefab">The prefab (BuildingInfo) to remove the record from.</param>
        public static void RemoveWorker(BuildingInfo prefab)
        {
            // Remove the entry from the configuration file cache.
            DataStore.workerCache.Remove(prefab.name);

            // Save the updated configuration files.
            XMLUtilsWG.WriteToXML();
            ConfigUtils.SaveSettings();

            // Remove current building's record from 'live' dictionary.
            PopData.instance.workplaceCache.Remove(prefab);

            // Also remove visitor cache entry, if any.
            PopData.instance.visitplaceCache.Remove(prefab);
        }
    }
}
