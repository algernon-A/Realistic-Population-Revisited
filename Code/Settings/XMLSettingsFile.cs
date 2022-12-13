// <copyright file="XMLSettingsFile.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.XML;
    using UnityEngine;

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
        /// Gets or sets building details panel hotkey (backwards-compatibility).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("hotkey")]
        [DefaultValue("")]
        public string Hotkey { get => string.Empty; set => HotkeyThreading.HotKey.Key = (int)Enum.Parse(typeof(KeyCode), value); }

        /// <summary>
        /// Gets or sets a value indicating whether the building details panel hotkey has the control key modifer (backwards-compatibility).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("ctrl")]
        [DefaultValue(false)]
        public bool Ctrl { get => false; set => HotkeyThreading.HotKey.Control = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the building details panel hotkey has the alt key modifer (backwards-compatibility).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("alt")]
        [DefaultValue(false)]
        public bool Alt { get => false; set => HotkeyThreading.HotKey.Alt = value; }

        /// <summary>
        /// Gets or sets a value indicating whether the building details panel hotkey has the shidt key modifer (backwards-compatibility).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("shift")]
        [DefaultValue(false)]
        public bool Shift { get => false; set => HotkeyThreading.HotKey.Shift = value; }

        /// <summary>
        /// Gets or sets the panel hotkey.
        /// </summary>
        [XmlElement("PanelKey")]
        public Keybinding PanelKey { get => HotkeyThreading.HotKey; set => HotkeyThreading.HotKey = value; }

        /// <summary>
        /// Sets a value indicating whether legacy calculations should be used by default on new saves (deprecated all-in-one setting).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("NewSavesLegacy")]
        public bool DefaultLegacy
        {
            set
            {
                ModSettings.NewSaveDefaultRes = DefaultMode.Legacy;
                ModSettings.NewSaveDefaultCom = DefaultMode.Legacy;
                ModSettings.NewSaveDefaultInd = DefaultMode.Legacy;
                ModSettings.NewSaveDefaultOff = DefaultMode.Legacy;
            }
        }

        /// <summary>
        /// Sets a value indicating whether to use legacy calculations by default for residential buildings in new saves (deprecated granular setting).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("NewSavesLegacyRes")]
        public bool DefaultLegacyRes { set => ModSettings.NewSaveDefaultRes = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// Sets a value indicating whether to use legacy calculations by default for commercial buildings in new saves (deprecated granular setting).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("NewSavesLegacyCom")]
        public bool DefaultLegacyCom { set => ModSettings.NewSaveDefaultCom = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// Sets a value indicating whether to use legacy calculations by default for industrial buildings in new saves (deprecated granular setting).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("NewSavesLegacyInd")]
        public bool DefaultLegacyInd { set => ModSettings.NewSaveDefaultInd = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// Sets a value indicating whether to use legacy calculations by default for office buildings in new saves (deprecated granular setting).
        /// DEPRECATED.
        /// </summary>
        [XmlElement("NewSavesLegacyOff")]
        public bool DefaultLegacyOff { set => ModSettings.NewSaveDefaultOff = value ? DefaultMode.Legacy : DefaultMode.New; }

        /// <summary>
        /// Gets or sets the default mode for residential buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultRes")]
        public DefaultMode XMLNewSavesDefaultRes { get => ModSettings.NewSaveDefaultRes; set => ModSettings.NewSaveDefaultRes = value; }

        /// <summary>
        /// Gets or sets the default mode for commercial buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultCom")]
        public DefaultMode NewSavesDefaultCom { get => ModSettings.NewSaveDefaultCom; set => ModSettings.NewSaveDefaultCom = value; }

        /// <summary>
        /// Gets or sets the default mode for industrial buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultInd")]
        public DefaultMode NewSavesDefaultInd { get => ModSettings.NewSaveDefaultInd; set => ModSettings.NewSaveDefaultInd = value; }

        /// <summary>
        /// Gets or sets the default mode for office buildings in new saves.
        /// </summary>
        [XmlElement("NewSavesDefaultOff")]
        public DefaultMode NewSavesDefaultOff { get => ModSettings.NewSaveDefaultOff; set => ModSettings.NewSaveDefaultOff = value; }

        /// <summary>
        /// Gets or sets the list of commercial vistplace calculation modes.
        /// </summary>
        [XmlArray("CommercialVisitsModes")]
        [XmlArrayItem("Mode")]
        public List<Configuration.SubServiceValue> ComVisitModes { get; set; }

        /// <summary>
        /// Gets or sets the list of commercial visitplace multipliers.
        /// </summary>
        [XmlArray("CommercialVisitsPercentages")]
        [XmlArrayItem("Percentage")]
        public List<Configuration.SubServiceValue> ComVisitMults { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether custom school populations are in effect.
        /// </summary>
        [XmlElement("EnableSchoolPop")]
        public bool EnableSchools { get => ModSettings.EnableSchoolPop; set => ModSettings.EnableSchoolPop = value; }

        /// <summary>
        /// Gets or sets a value indicating whether custom school properties are in effect.
        /// </summary>
        [XmlElement("EnableSchoolProperties")]
        public bool EnableSchoolProperties { get => ModSettings.EnableSchoolProperties; set => ModSettings.EnableSchoolProperties = value; }

        /// <summary>
        /// Gets or sets the dedfault school capacity multiplier.
        /// </summary>
        [XmlElement("DefaultSchoolMultiplier")]
        public float DefaultSchoolModifier { get => ModSettings.DefaultSchoolMult; set => ModSettings.DefaultSchoolMult = value; }

        /// <summary>
        /// Gets or sets the crime rate multiplier.
        /// </summary>
        [XmlElement("CrimeRateMultiplier")]
        public float CrimeRateMultiplier { get => HandleCrimeTranspiler.CrimeMultiplier; set => HandleCrimeTranspiler.CrimeMultiplier = value; }

        /// <summary>
        /// Gets or sets a value indicating whether metric (true) or US customary (false) measures should be displayed.
        /// </summary>
        [XmlElement("Metric")]
        public bool UsingMetric { get => Measures.UsingMetric; set => Measures.UsingMetric = value; }

        /// <summary>
        /// Gets or sets a value indicating whether zoning milestone progression logging is in effect.
        /// </summary>
        [XmlElement("UnlockZoning")]
        public bool UnlockZoning { get => UnlockedZonePatch.UnlockZoning; set => UnlockedZonePatch.UnlockZoning = value; }

        /// <summary>
        /// Sets a value indicating whether detailed logging is in effect (legacy option).
        /// </summary>
        [XmlElement("DetailLogging")]
        public bool DetailLogging { set => Logging.DetailLogging = value; }

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