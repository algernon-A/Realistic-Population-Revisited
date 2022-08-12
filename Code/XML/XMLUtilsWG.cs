// <copyright file="XMLUtilsWG.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Witefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using AlgernonCommons;
    using ColossalFramework.Math;

    /// <summary>
    /// Class for XML configuration file utility methods.
    /// </summary>
    internal static class XMLUtilsWG
    {
        /// <summary>
        /// Configuration file name.
        /// </summary>
        private const string XmlFile = "WG_RealisticCity.xml";

        /// <summary>
        /// Gets or sets a value indicating whether we're writing to this legacy file.
        /// </summary>
        internal static bool WriteToLegacy { get; set; } = false;

        /// <summary>
        /// Loads the configuration XML file and sets the datastore.
        /// </summary>
        internal static void ReadFromXML()
        {
            // Check the exe directory first
            DataStore.currentFileLocation = ColossalFramework.IO.DataLocation.executableDirectory + Path.DirectorySeparatorChar + XmlFile;
            bool fileAvailable = File.Exists(DataStore.currentFileLocation);

            if (!fileAvailable)
            {
                // Switch to default which is the cities skylines in the application data area.
                DataStore.currentFileLocation = ColossalFramework.IO.DataLocation.localApplicationData + Path.DirectorySeparatorChar + XmlFile;
                fileAvailable = File.Exists(DataStore.currentFileLocation);
            }

            if (fileAvailable)
            {
                Logging.KeyMessage("loading legacy configuration file ", DataStore.currentFileLocation);

                // Load in from XML - Designed to be flat file for ease
                WG_XMLBaseVersion reader = new XML_VersionSix();
                XmlDocument doc = new XmlDocument();
                try
                {
                    doc.Load(DataStore.currentFileLocation);

                    int version = Convert.ToInt32(doc.DocumentElement.Attributes["version"].InnerText);
                    if (version > 3 && version <= 5)
                    {
                        // Use version 5
                        reader = new XML_VersionFive();

                        // Make a back up copy of the old system to be safe
                        File.Copy(DataStore.currentFileLocation, DataStore.currentFileLocation + ".ver5", true);
                        Logging.KeyMessage("Detected an old version of the XML (v5). ", DataStore.currentFileLocation, ".ver5 has been created for future reference and will be upgraded to the new version.");
                    }
                    else if (version <= 3)
                    {
                        // Uh oh... version 4 was a while back..
                        Logging.KeyMessage("Detected an unsupported version of the XML (v4 or less). Backing up for a new configuration as :", DataStore.currentFileLocation + ".ver4");
                        File.Copy(DataStore.currentFileLocation, DataStore.currentFileLocation + ".ver4", true);
                        return;
                    }

                    reader.ReadXML(doc);
                }
                catch (Exception e)
                {
                    // Game will now use defaults
                    Logging.LogException(e, "Exception(s) were detected while loading the XML file. Some (or all) values may not be loaded");
                }
            }
            else
            {
                Logging.KeyMessage("legacy configuration file not found");
            }
        }

        /// <summary>
        /// Updates (or creates a new) XML configuration file with current DataStore settings.
        /// </summary>
        internal static void WriteToXML()
        {
            // Only write to files if the relevant setting is set (either through a legacy configuration file already existing, or through the user specifically creating one via the options panel).
            if (WriteToLegacy)
            {
                try
                {
                    WG_XMLBaseVersion xml = new XML_VersionSix();
                    xml.WriteXML(DataStore.currentFileLocation);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "XML writing exception");
                }
            }
        }

        /// <summary>
        /// Performs legacy datastore setup.
        /// </summary>
        internal static void Setup()
        {
            if (DataStore.mergeResidentialNames)
            {
                foreach (KeyValuePair<string, int> entry in DataStore.defaultHousehold)
                {
                    try
                    {
                        DataStore.householdCache.Add(entry.Key, entry.Value);
                    }
                    catch (Exception)
                    {
                        // Don't care
                    }
                }
            }

            if (DataStore.mergeEmploymentNames)
            {
                foreach (KeyValuePair<string, int> entry in DataStore.defaultWorker)
                {
                    try
                    {
                        DataStore.workerCache.Add(entry.Key, entry.Value);
                    }
                    catch (Exception)
                    {
                        // Don't care
                    }
                }
            }

            // Remove bonus names from over rides
            foreach (string name in DataStore.bonusHouseholdCache.Keys)
            {
                DataStore.householdCache.Remove(name);
            }

            foreach (string name in DataStore.bonusWorkerCache.Keys)
            {
                DataStore.workerCache.Remove(name);
            }

            DataStore.seedToId.Clear();

            // Up to 1M buildings apparently is ok
            for (int i = 0; i <= ushort.MaxValue; ++i)
            {
                // This creates a unique number
                try
                {
                    Randomizer number = new Randomizer(i);
                    DataStore.seedToId.Add(number.seed, (ushort)i);
                }
                catch (Exception)
                {
                    // Don't care
                }
            }
        }
    }
}