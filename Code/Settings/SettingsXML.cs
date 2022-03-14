using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot("SettingsFile")]
    public class XMLSettingsFile
    {
        // Settings file.
        [XmlIgnore]
        private static readonly string SettingsFileName = "RealisticPopulation.xml";
        private static readonly string SettingsFile = Path.Combine(ColossalFramework.IO.DataLocation.localApplicationData, SettingsFileName);


        [XmlElement("WhatsNewVersion")]
        public string WhatsNewVersion { get => ModSettings.whatsNewVersion; set => ModSettings.whatsNewVersion = value; }

        [XmlElement("WhatsNewBetaVersion")]
        [DefaultValue(0)]

        // Language.
        [XmlElement("Language")]
        public string Language
        {
            get
            {
                return Translations.CurrentLanguage;
            }
            set
            {
                Translations.CurrentLanguage = value;
            }
        }

        // Building details panel hotkey backwards-compatibility.
        [XmlElement("hotkey")]
        [DefaultValue("")]
        public string Hotkey { get => ""; set => UIThreading.hotKey = (KeyCode)Enum.Parse(typeof(KeyCode), value); }

        [XmlElement("ctrl")]
        [DefaultValue(false)]
        public bool Ctrl { get => false; set => UIThreading.hotCtrl = value; }

        [XmlElement("alt")]
        [DefaultValue(false)]
        public bool Alt { get => false; set => UIThreading.hotAlt = value; }

        [XmlElement("shift")]
        [DefaultValue(false)]
        public bool Shift { get => false; set => UIThreading.hotShift = value; }

        // New building details panel hotkey element.
        [XmlElement("PanelKey")]
        public KeyBinding PanelKey
        {
            get
            {
                return new KeyBinding
                {
                    keyCode = (int)UIThreading.hotKey,
                    control = UIThreading.hotCtrl,
                    shift = UIThreading.hotShift,
                    alt = UIThreading.hotAlt
                };
            }
            set
            {
                // Backwads compatibility - this won't exist in older-format configuration files.
                if (value != null)
                {
                    UIThreading.hotKey = (KeyCode)value.keyCode;
                    UIThreading.hotCtrl = value.control;
                    UIThreading.hotShift = value.shift;
                    UIThreading.hotAlt = value.alt;
                }
            }
        }

        // Use legacy calculations by default (deprecated all-in-one setting).
        [XmlElement("NewSavesLegacy")]
        public bool DefaultLegacy
        {
            set
            {
                ModSettings.newSaveLegacyRes = value;
                ModSettings.newSaveLegacyCom = value;
                ModSettings.newSaveLegacyInd = value;
                ModSettings.newSaveLegacyOff = value;
            }
        }


        // Use legacy calculations by default (new granular setting).
        [XmlElement("NewSavesLegacyRes")]
        public bool DefaultLegacyRes { get => ModSettings.newSaveLegacyRes; set => ModSettings.newSaveLegacyRes = value; }

        [XmlElement("NewSavesLegacyCom")]
        public bool DefaultLegacyCom { get => ModSettings.newSaveLegacyCom; set => ModSettings.newSaveLegacyCom = value; }

        [XmlElement("NewSavesLegacyInd")]
        public bool DefaultLegacyInd { get => ModSettings.newSaveLegacyInd; set => ModSettings.newSaveLegacyInd = value; }

        [XmlElement("NewSavesLegacyExt")]
        public bool DefaultLegacyExt { get => ModSettings.newSaveLegacyExt; set => ModSettings.newSaveLegacyExt = value; }

        [XmlElement("NewSavesLegacyOff")]
        public bool DefaultLegacyOff { get => ModSettings.newSaveLegacyOff; set => ModSettings.newSaveLegacyOff = value; }


        // Commercial visitor calculations - clamp to 0 or 1 at this stage.
        [XmlArray("CommercialVisitsModes")]
        [XmlArrayItem("Mode")]
        public List<SubServiceValue> comVisitModes;

        // Commercial visitor multiplier.
        [XmlArray("CommercialVisitsPercentages")]
        [XmlArrayItem("Percentage")]
        public List<SubServiceValue> comVisitMults;

        // Realistic education.
        [XmlElement("EnableSchoolPop")]
        public bool EnableSchools { get => ModSettings.EnableSchoolPop; set => ModSettings.EnableSchoolPop = value; }

        // School properties.
        [XmlElement("EnableSchoolProperties")]
        public bool EnableSchoolProperties { get => ModSettings.enableSchoolProperties; set => ModSettings.enableSchoolProperties = value; }

        // Default school capacity multiplier.
        [XmlElement("DefaultSchoolMultiplier")]
        public float DefaultSchoolModifier { get => ModSettings.DefaultSchoolMult; set => ModSettings.DefaultSchoolMult = value; }

        // Crime rate multiplier.
        [XmlElement("CrimeRateMultiplier")]
        public float CrimeRateMultiplier { get => ModSettings.crimeMultiplier; set => ModSettings.crimeMultiplier = value; }

        // Measurement system.
        [XmlElement("Metric")]
        public bool UsingMetric { get => Measures.UsingMetric; set => Measures.UsingMetric = value; }

        // Logging detail.
        [XmlElement("DetailedLogging")]
        public bool DetailLogging { get => Logging.detailLogging; set => Logging.detailLogging = value; }

        // Logging detail.
        [XmlElement("DontRebuildUnits")]
        public bool DontRebuildUnits { get => ModSettings.dontRebuildUnits; set => ModSettings.dontRebuildUnits = value; }


        /// <summary>
        /// Load settings from XML file - Beta legacy.
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
                            foreach (SubServiceValue entry in xmlSettingsFile.comVisitModes)
                            {
                                RealisticVisitplaceCount.SetVisitMode(entry.subService, entry.value);
                            }

                            // Iterate through each KeyValuePair parsed and add update entry in commercial visit multipliers dictionary.
                            foreach (SubServiceValue entry in xmlSettingsFile.comVisitMults)
                            {
                                RealisticVisitplaceCount.SetVisitMult(entry.subService, entry.value);
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
                // Pretty straightforward.  Serialisation is within settings file class.
                using (StreamWriter writer = new StreamWriter(SettingsFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLSettingsFile));
                    XMLSettingsFile settingsFile = new XMLSettingsFile();

                    // Save to file.
                    xmlSerializer.Serialize(writer, settingsFile);
                }

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


    /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public class KeyBinding
    {
        [XmlAttribute("KeyCode")]
        public int keyCode;

        [XmlAttribute("Control")]
        public bool control;

        [XmlAttribute("Shift")]
        public bool shift;

        [XmlAttribute("Alt")]
        public bool alt;
    }
}