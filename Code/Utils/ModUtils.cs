// <copyright file="ModUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Class that manages interactions with other mods, including compatibility and functionality checks.
    /// </summary>
    internal static class ModUtils
    {
        // RICO methods.
        private static MethodInfo s_ricoPopManaged;

        /// <summary>
        /// Checks to see whether the given prefab is currently having its population controlled by Ploppable RICO Revisited.
        /// Here as a separate method on its own to avoid issues with unfound binaries breaking other methods.
        /// </summary>
        /// <param name="prefab">Prefab to check.</param>
        /// <returns>True if Ploppable RICO is managing this prefab, false otherwise.</returns>
        internal static bool CheckRICOPopControl(BuildingInfo prefab)
        {
            // If we haven't got the RICO method by reflection, the answer is always false.
            if (s_ricoPopManaged != null)
            {
                object result = s_ricoPopManaged.Invoke(null, new object[] { prefab });

                if (result is bool boolResult)
                {
                    return boolResult;
                }
            }

            // Default result.
            return false;
        }

        /// <summary>
        /// Uses reflection to find the IsRICOPopManaged and ClearWorkplaceCache methods of Ploppable RICO Revisited.
        /// If successful, sets ricoPopManaged and ricoClearWorkplace fields.
        /// </summary>
        internal static void RICOReflection()
        {
            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("ploppablerico") && plugin.isEnabled)
                    {
                        Logging.Message("Found Ploppable RICO Revisited");

                        // Found ploppablerico.dll that's part of an enabled plugin; try to get its Interfaces class.
                        Type ricoModUtils = assembly.GetType("PloppableRICO.Interfaces");

                        if (ricoModUtils != null)
                        {
                            // Try to get IsRICOPopManaged method.
                            s_ricoPopManaged = ricoModUtils.GetMethod("IsRICOPopManaged", BindingFlags.Public | BindingFlags.Static);
                            if (s_ricoPopManaged != null)
                            {
                                // Success!
                                Logging.Message("found IsRICOPopManaged");
                            }
                        }

                        // At this point, we're done; return.
                        return;
                    }
                }
            }

            // If we got here, we were unsuccessful.
            Logging.Message("Ploppable RICO Revisited not found");
        }

        /// <summary>
        /// Uses reflection to find the LevelUtils.CustomBuildingUpgraded method of Advanced Building Level Control.
        /// </summary>
        /// <returns>LevelUtils.CustomBuildingUpgraded method of Advanced Building Level Control (null if not found).</returns>
        internal static MethodInfo ABLCCustomUpgraded()
        {
            // Iterate through each loaded plugin assembly.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("AdvancedBuildingLevelControl") && plugin.isEnabled)
                    {
                        Logging.Message("Found Advanced Building Level Control");

                        // Found AdvancedBuildingLevelControl.dll that's part of an enabled plugin; try to get its Interfaces class.
                        Type ablcLevelUtils = assembly.GetType("ABLC.LevelUtils");

                        if (ablcLevelUtils != null)
                        {
                            // Try to get ABLCCustomUpgraded method.
                            MethodInfo ablcCustomUpgraded = ablcLevelUtils.GetMethod("CustomBuildingUpgraded", BindingFlags.NonPublic | BindingFlags.Static);
                            if (ablcCustomUpgraded != null)
                            {
                                // Success!
                                Logging.Message("found CustomBuildingUpgraded");
                                return ablcCustomUpgraded;
                            }
                        }
                    }
                }
            }

            // If we got here, we were unsuccessful.
            Logging.Message("AdvancedBuildingLevelControl CustomBuildingUpgraded not found");
            return null;
        }
    }
}
