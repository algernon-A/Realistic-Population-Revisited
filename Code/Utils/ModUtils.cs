using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ICities;
using ColossalFramework.Plugins;


namespace RealPop2
{
    /// <summary>
    /// Class that manages interactions with other mods, including compatibility and functionality checks.
    /// </summary>
    internal static class ModUtils
    {
        // RICO methods.
        internal static MethodInfo ricoPopManaged;
        internal static MethodInfo ricoClearWorkplace;
        internal static MethodInfo ricoClearAllWorkplaces;

        // List of conflcting mod names.
        internal static List<string> conflictingModNames;


        /// <summary>
        /// Returns the filepath of the current mod assembly.
        /// </summary>
        /// <returns>Mod assembly filepath</returns>
        internal static string GetAssemblyPath()
        {
            // Get list of currently active plugins.
            IEnumerable<PluginManager.PluginInfo> plugins = PluginManager.instance.GetPluginsInfo();

            // Iterate through list.
            foreach (PluginManager.PluginInfo plugin in plugins)
            {
                try
                {
                    // Get all (if any) mod instances from this plugin.
                    IUserMod[] mods = plugin.GetInstances<IUserMod>();

                    // Check to see if the primary instance is this mod.
                    if (mods.FirstOrDefault() is RealPopMod)
                    {
                        // Found it! Return path.
                        return plugin.modPath;
                    }
                }
                catch
                {
                    // Don't care.
                }
            }

            // If we got here, then we didn't find the assembly.
            Logging.Message("assembly path not found");
            throw new FileNotFoundException(RealPopMod.ModName + ": assembly path not found!");
        }


        /// <summary>
        /// Checks for any known fatal mod conflicts.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise</returns>
        internal static bool IsModConflict()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            conflictingModNames = new List<string>();

            // Duplicate real pop mod detection.
            bool realPopModFound = false;

            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    switch (assembly.GetName().Name)
                    {
                        case "RealPopRevisited":
                            // Have we already found an instance?
                            if (realPopModFound)
                            {
                                // Yes - flag as duplicate.
                                conflictDetected = true;
                                conflictingModNames.Add("Realistic Population Revisited");
                            }
                            // Flag instance as found.
                            realPopModFound = true;
                            break;
                        case "WG_BalancedPopMod":
                            // Original WG mod.
                            conflictDetected = true;
                            conflictingModNames.Add("Realistic Population and Consumption Mod");
                            break;
                        case "EnhancedBuildingCapacity":
                            // Enhanced building capacity.
                            conflictDetected = true;
                            conflictingModNames.Add("Enhanced Building Capacity");
                            break;
                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            conflictingModNames.Add("Garbage Bin Controller");
                            break;
                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...)
                            if (plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                conflictingModNames.Add("Painter");
                            }
                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }
                Logging.Error("exiting due to mod conflict");
            }

            return conflictDetected;
        }


        /// <summary>
        /// Checks to see if another mod is installed, based on a provided assembly name.
        /// Case-sensitive!  PloppableRICO is not the same as ploppablerico!
        /// </summary>
        /// <param name="assemblyName">Name of the mod assembly</param>
        /// <param name="enabledOnly">True if the mod needs to be enabled for the purposes of this check; false if it doesn't matter</param>
        /// <returns>True if the mod is installed (and, if enabledOnly is true, is also enabled), false otherwise</returns>
        internal static bool IsModInstalled(string assemblyName, bool enabledOnly = false)
        {
            // Iterate through the full list of plugins.
            foreach (PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach (Assembly assembly in plugin.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals(assemblyName))
                    {
                        Logging.Message("found mod assembly ", assemblyName, ", version ", assembly.GetName().Version);
                        if (enabledOnly)
                        {
                            return plugin.isEnabled;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            // If we've made it here, then we haven't found a matching assembly.
            return false;
        }


        /// <summary>
        /// Checks to see whether the given prefab is currently having its population controlled by Ploppable RICO Revisited.
        /// Here as a separate method on its own to avoid issues with unfound binaries breaking other methods.
        /// </summary>
        /// <param name="prefab">Prefab to check</param>
        /// <returns>True if Ploppable RICO is managing this prefab, false otherwise.</returns>
        internal static bool CheckRICOPopControl(BuildingInfo prefab)
        {
            // If we haven't got the RICO method by reflection, the answer is always false.
            if (ricoPopManaged != null)
            {
                object result = ricoPopManaged.Invoke(null, new object[] { prefab });

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
                            ricoPopManaged = ricoModUtils.GetMethod("IsRICOPopManaged", BindingFlags.Public | BindingFlags.Static);
                            if (ricoPopManaged != null)
                            {
                                // Success!
                                Logging.Message("found IsRICOPopManaged");
                            }

                            // Try to get ClearWorkplaceCache method.
                            ricoClearWorkplace = ricoModUtils.GetMethod("ClearWorkplaceCache", BindingFlags.Public | BindingFlags.Static);
                            if (ricoClearWorkplace != null)
                            {
                                // Success!
                                Logging.Message("found RICO ClearWorkplaceCache");
                            }

                            // Try to get ClearAllWorkplaceCache method.
                            ricoClearAllWorkplaces = ricoModUtils.GetMethod("ClearAllWorkplaceCache", BindingFlags.Public | BindingFlags.Static);
                            if (ricoClearAllWorkplaces != null)
                            {
                                // Success!
                                Logging.Message("found RICO ClearAllWorkplaceCache");
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
        /// <returns>LevelUtils.CustomBuildingUpgraded method of Advanced Building Level Control (null if not found)</returns>
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
