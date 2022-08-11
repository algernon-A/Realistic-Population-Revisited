// <copyright file="XMLSettingsFile.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using UnityEngine;
    using Realistic_Population_Revisited.Code.Patches.Population;

    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class XMLSettingsFile : SettingsXMLBase
    {
        // Settings file name.
        [XmlIgnore]
        private static readonly string SettingsFileName = "RealisticPopulation.xml";

        // User settings directory.
        [XmlIgnore]
        private static readonly string UserSettingsDir = ColossalFramework.IO.DataLocation.localApplicationData;

        // Full userdir settings file name.
        [XmlIgnore]
        private static readonly string SettingsFile = Path.Combine(UserSettingsDir, SettingsFileName);

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets building details panel hotkey (backwards-compatibility).
        /// </summary>
        [XmlElement("hotkey")]
        [DefaultValue("")]
        public string Hotkey { get => ""; set => UIThreading.HotKey.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether the building details panel hotkey has the control key modifer (backwards-compatibility).
        /// </summary>
        [XmlElement("ctrl")]
        [DefaultValue(false)]
        public bool Ctrl { get => false; set => UIThreading.HotKey.Control = value; }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether the building details panel hotkey has the alt key modifer (backwards-compatibility).
        /// </summary>
        [XmlElement("alt")]
        [DefaultValue(false)]
        public bool Alt { get => false; set => UIThreading.HotKey.Alt = value; }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether the building details panel hotkey has the shidt key modifer (backwards-compatibility).
        /// </summary>
        [XmlElement("shift")]
        [DefaultValue(false)]
        public bool Shift { get => false; set => UIThreading.HotKey.Shift = value; }

        /// <summary>
        /// Gets or sets the panel hotkey.
        /// </summary>
        [XmlElement("PanelKey")]
        public Keybinding PanelKey { get => UIThreading.HotKey; set => UIThreading.HotKey = value; }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether legacy calculations should be used by default on new saves (deprecated all-in-one setting).
        /// </summary>
        [XmlElement("NewSavesLegacy")]
        public bool DefaultLegacy
        {
            set
            {
                ModSettings.newSaveDefaultRes = DefaultMode.Legacy;
                ModSettings.newSaveDefaultCom = DefaultMode.Legacy;
                ModSettings.newSaveDefaultInd = DefaultMode.Legacy;
                ModSettings.newSaveDefaultOff = DefaultMode.Legacy;
            }
        }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether to use legacy calculations by default for residential buildings in new saves (deprecated granular setting).
        /// </summary>
        [XmlElement("NewSavesLegacyRes")]
        public bool DefaultLegacyRes { set => ModSettings.newSaveDefaultRes = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether to use legacy calculations by default for commercial buildings in new saves (deprecated granular setting).
        /// </summary>
        [XmlElement("NewSavesLegacyCom")]
        public bool DefaultLegacyCom { set => ModSettings.newSaveDefaultCom = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether to use legacy calculations by default for industrial buildings in new saves (deprecated granular setting).
        /// </summary>
        [XmlElement("NewSavesLegacyInd")]
        public bool DefaultLegacyInd { set => ModSettings.newSaveDefaultInd = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// DEPRECATED.
        /// Gets or sets a value indicating whether to use legacy calculations by default for office buildings in new saves (deprecated granular setting).
        /// </summary>
        [XmlElement("NewSavesLegacyOff")]
        public bool DefaultLegacyOff { set => ModSettings.newSaveDefaultOff = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// Gets or sets the default mode for residential buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultRes")]
        public DefaultMode XMLNewSavesDefaultRes { get => ModSettings.newSaveDefaultRes; set => ModSettings.newSaveDefaultRes = value; }

        /// <summary>
        /// Gets or sets the default mode for commercial buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultCom")]
        public DefaultMode NewSavesDefaultCom { get => ModSettings.newSaveDefaultCom; set => ModSettings.newSaveDefaultCom = value; }

        /// <summary>
        /// Gets or sets the default mode for industrial buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultInd")]
        public DefaultMode NewSavesDefaultInd { get => ModSettings.newSaveDefaultInd; set => ModSettings.newSaveDefaultInd = value; }

        /// <summary>
        /// Gets or sets the default mode for office buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultOff")]
        public DefaultMode NewSavesDefaultOff { get => ModSettings.newSaveDefaultOff; set => ModSettings.newSaveDefaultOff = value; }

        /// <summary>
        /// Gets or sets the list of commercial vistplace calculation modes.
        /// </summary>
        [XmlArray("CommercialVisitsModes")]
        [XmlArrayItem("Mode")]
        public List<Configuration.SubServiceValue> ComVisitModes;

        /// <summary>
        /// Gets or sets the list of commercial visitplace multipliers.
        /// </summary>
        [XmlArray("CommercialVisitsPercentages")]
        [XmlArrayItem("Percentage")]
        public List<Configuration.SubServiceValue> ComVisitMults;

        /// <summary>
        /// Gets or sets a value indicating whether custom school populations are in effect.
        /// </summary>
        [XmlElement("EnableSchoolPop")]
        public bool EnableSchools { get => ModSettings.EnableSchoolPop; set => ModSettings.EnableSchoolPop = value; }

        /// <summary>
        /// Gets or sets a value indicating whether custom school properties are in effect.
        /// </summary>
        [XmlElement("EnableSchoolProperties")]
        public bool EnableSchoolProperties { get => ModSettings.enableSchoolProperties; set => ModSettings.enableSchoolProperties = value; }

        /// <summary>
        /// Gets or sets the dedfault school capacity multiplier.
        /// </summary>
        [XmlElement("DefaultSchoolMultiplier")]
        public float DefaultSchoolModifier { get => ModSettings.DefaultSchoolMult; set => ModSettings.DefaultSchoolMult = value; }

        /// <summary>
        /// Gets or sets the crime rate multiplier.
        /// </summary>
        [XmlElement("CrimeRateMultiplier")]
        public float CrimeRateMultiplier { get => ModSettings.crimeMultiplier; set => ModSettings.crimeMultiplier = value; }

        /// <summary>
        /// Gets or sets a value indicating whether metric (true) or US customary (false) measures should be displayed.
        /// </summary>
        [XmlElement("Metric")]
        public bool UsingMetric { get => Measures.UsingMetric; set => Measures.UsingMetric = value; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed logging is in effect.
        /// </summary>
        [XmlElement("DetailedLogging")]
        public bool DetailLogging { get => Logging.DetailLogging; set => Logging.DetailLogging = value; }

        /// <summary>
        /// Gets or sets a value indicating whether detailed logging is in effect.
        /// </summary>
        // Logging detail.
        [XmlElement("DontRebuildUnits")]
        public bool DontRebuildUnits { get => ModSettings.dontRebuildUnits; set => ModSettings.dontRebuildUnits = value; }

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void Load()
        {
            try
            {
                string fileName = null;

                // See if a userdir settings file exists.
                if (File.Exists(SettingsFile))
                {
                    fileName = SettingsFile;
                }
                else if (File.Exists(SettingsFileName))
                {
                    // Otherwise, if an application settings file exists, use that one.
                    fileName = SettingsFileName;
                }

                // Check to see if we found an existing configuration file.
                if (fileName != null)
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(fileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLSettingsFile));
                        if (!(xmlSerializer.Deserialize(reader) is XMLSettingsFile xmlSettingsFile))
                        {
                            Logging.Error("couldn't deserialize settings file");
                        }
                        else
                        {
                            // Iterate through each KeyValuePair parsed and add update entry in commercial visit modes dictionary.
                            foreach (Configuration.SubServiceValue entry in xmlSettingsFile.ComVisitModes)
                            {
                                Visitors.SetVisitMode(entry.SubService, entry.Value);
                            }

                            // Iterate through each KeyValuePair parsed and add update entry in commercial visit multipliers dictionary.
                            foreach (Configuration.SubServiceValue entry in xmlSettingsFile.ComVisitMults)
                            {
                                Visitors.SetVisitMult(entry.SubService, entry.Value);
                            }
                        }
                    }
                }
                else
                {
                    Logging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading XML settings file");
            }
        }

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void Save()
        {
            try
            {
                // Save settings file.
                XMLFileUtils.Save<XMLSettingsFile>(SettingsFile);

                // Delete any old settings in app directory.
                if (File.Exists(SettingsFileName))
                {
                    File.Delete(SettingsFileName);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving XML settings file");
            }
        }
    }
}