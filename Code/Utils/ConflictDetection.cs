// <copyright file="ConflictDetection.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using System.Reflection;
    using AlgernonCommons;
    using ColossalFramework.Plugins;

    /// <summary>
    /// Mod conflict detection.
    /// </summary>
    internal static class ConflictDetection
    {
        // List of conflcting mod names.
        private static List<string> s_conflictingModNames;

        /// <summary>
        /// Gets the recorded list of conflicting mod names.
        /// </summary>
        internal static List<string> ConflictingModNames => s_conflictingModNames;

        /// <summary>
        /// Checks for any known fatal mod conflicts.
        /// </summary>
        /// <returns>True if a mod conflict was detected, false otherwise.</returns>
        internal static bool IsModConflict()
        {
            // Initialise flag and list of conflicting mods.
            bool conflictDetected = false;
            s_conflictingModNames = new List<string>();

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
                                s_conflictingModNames.Add("Realistic Population Revisited");
                            }

                            // Flag instance as found.
                            realPopModFound = true;
                            break;

                        case "WG_BalancedPopMod":
                            // Original WG mod.
                            conflictDetected = true;
                            s_conflictingModNames.Add("Realistic Population and Consumption Mod");
                            break;

                        case "EnhancedBuildingCapacity":
                            // Enhanced building capacity.
                            conflictDetected = true;
                            s_conflictingModNames.Add("Enhanced Building Capacity");
                            break;

                        case "VanillaGarbageBinBlocker":
                            // Garbage Bin Controller
                            conflictDetected = true;
                            s_conflictingModNames.Add("Garbage Bin Controller");
                            break;

                        case "Painter":
                            // Painter - this one is trickier because both Painter and Repaint use Painter.dll (thanks to CO savegame serialization...)
                            if (plugin.userModInstance.GetType().ToString().Equals("Painter.UserMod"))
                            {
                                conflictDetected = true;
                                s_conflictingModNames.Add("Painter");
                            }

                            break;
                    }
                }
            }

            // Was a conflict detected?
            if (conflictDetected)
            {
                // Yes - log each conflict.
                foreach (string conflictingMod in s_conflictingModNames)
                {
                    Logging.Error("Conflicting mod found: ", conflictingMod);
                }

                Logging.Error("exiting due to mod conflict");
            }

            return conflictDetected;
        }
    }
}
