using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Defines the XML configuration file.
    /// </summary>
    [XmlRoot("Configuration")]
    public class XMLConfigurationFile
    {
        // Stores the version of the most recent update notification that the user has decided to "Don't show again".
        [XmlElement("version")]
        public int notificationVersion;

        [XmlArray("pop_packs")]
        [XmlArrayItem("pop_pack")]
        public List<PopPackXML> popPacks;

        [XmlArray("floor_packs")]
        [XmlArrayItem("floor_pack")]
        public List<FloorPackXML> floorPacks;

        [XmlArray("consumption")]
        [XmlArrayItem("consumption")]
        public List<ConsumptionRecord> consumption;

        [XmlArray("default-pop")]
        [XmlArrayItem("default")]
        public List<DefaultPack> popDefaults;

        [XmlArray("default-floor")]
        [XmlArrayItem("default")]
        public List<DefaultPack> floorDefaults;

        [XmlArray("buildings")]
        [XmlArrayItem("building")]
        public List<BuildingRecord> buildings;

        [XmlArray("override_population")]
        [XmlArrayItem("population")]
        public List<PopCountOverride> popOverrides;

        [XmlArray("override_floors")]
        [XmlArrayItem("floors")]
        public List<FloorCalcOverride> floors;

        [XmlArray("visitors")]
        [XmlArrayItem("visitors")]
        public List<SubServiceMode> visitorModes;

        [XmlArray("commercial_sales")]
        [XmlArrayItem("percentage")]
        public List<SubServiceValue> salesMults;

        [XmlArray("office_production")]
        [XmlArrayItem("percentage")]
        public List<SubServiceValue> offProdMults;

        [XmlArray("industrial_production")]
        [XmlArrayItem("production")]
        public List<SubServiceMode> indProdModes;

        [XmlArray("extractor_production")]
        [XmlArrayItem("production")]
        public List<SubServiceMode> extProdModes;

        [XmlArray("commercial_inventory_caps")]
        [XmlArrayItem("inventory")]
        public List<SubServiceValue> comIndCaps;
    }


    /// <summary>
    /// Default calculation pack dictionary record.
    /// </summary>
    public class DefaultPack
    {
        // Service ID.
        [XmlAttribute("service")]
        public ItemClass.Service service;

        // Sub-service ID.
        [XmlAttribute("subservice")]
        public ItemClass.SubService subService;

        // Calculation pack.
        [XmlAttribute("pack")]
        [DefaultValue("")]
        public string pack;
    }


    /// <summary>
    /// Building setting dictionary record.
    /// </summary>
    public class BuildingRecord
    {
        // Building prefab name.
        [XmlAttribute("prefab")]
        public string prefab;

        // Calculation pack.
        [XmlAttribute("pop")]
        [DefaultValue("")]
        public string popPack;

        // Calculation pack.
        [XmlAttribute("floor")]
        [DefaultValue("")]
        public string floorPack;

        // School properties pack.
        [XmlAttribute("school")]
        [DefaultValue("")]
        public string schoolPack;

        // Multiplier - don't want to serialize default or invalid values (1 or less).
        [XmlAttribute("multiplier")]
        [DefaultValue("")]
        public string Multiplier
        {
            // Only serialize if multiplier is at least one.
            get => multiplier >= 1 ? multiplier.ToString() : string.Empty;

            set
            {
                // Attempt to parse value as float.
                if (!float.TryParse(value, out multiplier))
                {
                    Logging.Error("unable to parse multiplier as float; setting to default");
                    multiplier = ModSettings.DefaultSchoolMult;
                }

                // Minimum value of 1.
                multiplier = Mathf.Max(1f, multiplier);
            }
        }

        [XmlIgnore]
        public float multiplier;
    }


    /// <summary>
    /// Building population count override record.
    /// </summary>
    public class PopCountOverride
    {
        // Building prefab name.
        [XmlAttribute("prefab")]
        public string prefab;

        //Household count.
        [XmlAttribute("population")]
        [DefaultValue(0)]
        public ushort population;
    }


    /// <summary>
    /// Building floor calculation override record.
    /// </summary>
    public class FloorCalcOverride
    {
        // Building prefab name.
        [XmlAttribute("prefab")]
        public string prefab;

        // First floor height.
        [XmlAttribute("height_first")]
        public float firstHeight;

        // Other floor height.
        [XmlAttribute("height_other")]
        public float floorHeight;
    }


    /// <summary>
    /// Custom population calculation pack record.
    /// </summary>
    public class PopPackXML
    {
        // Pack name.
        [XmlAttribute("name")]
        public string name;

        // Service type.
        [XmlAttribute("service")]
        public ItemClass.Service service;

        // Level records.
        [XmlArray("levels")]
        [XmlArrayItem("level")]
        public List<PopLevel> calculationLevels;
    }


    /// <summary>
    /// Custom calculation pack individual level line.
    /// </summary>
    public class PopLevel
    {
        // Level ID.
        [XmlAttribute("level")]
        public int level;

        // Empty area, fixed.
        [XmlAttribute("empty")]
        public float emptyArea;

        // Empty area, percentage.
        [XmlAttribute("emptypercent")]
        public int emptyPercent;

        // Area per unit.
        [XmlAttribute("areaper")]
        public float areaPer;

        // Multi-level units.
        [XmlAttribute("multilevel")]
        public bool multiLevel;
    }


    /// <summary>
    /// Custom floor calculation pack record.
    /// </summary>
    public class FloorPackXML
    {
        // Pack name.
        [XmlAttribute("name")]
        public string name;

        // Floor height.
        [XmlAttribute("floorheight")]
        public float floorHeight;

        // First floor minimum floor height.
        [XmlAttribute("firstmin")]
        public float firstMin;

        // First floor additional floor height.
        [XmlAttribute("firstextra")]
        public float firstExtra;

        // First floor empty.
        [XmlAttribute("firstempty")]
        public bool firstEmpty;
    }


    /// <summary>
    /// Service consumption record.
    /// </summary>
    public class ConsumptionRecord
    {
        // Service ID.
        [XmlAttribute("service")]
        public ItemClass.Service service;

        // Sub-service ID.
        [XmlAttribute("subservice")]
        public ItemClass.SubService subService;

        // Level records.
        [XmlArray("levels")]
        [XmlArrayItem("level")]
        public List<ConsumptionLine> levels;
    }


    /// <summary>
    /// Consumption record individual level line.
    /// </summary>
    public class ConsumptionLine
    {
        // Level.
        [XmlAttribute("level")]
        public ItemClass.Level level;

        // Power consumption.
        [XmlAttribute("power")]
        public int power;

        // Water consumption.
        [XmlAttribute("water")]
        public int water;

        // Sewage generation.
        [XmlAttribute("sewage")]
        public int sewage;

        // Garbage generation.
        [XmlAttribute("garbage")]
        public int garbage;

        // Pollution generation.
        [XmlAttribute("pollution")]
        public int pollution;

        // Noise generation.
        [XmlAttribute("noise")]
        public int noise;

        // Mail generation.
        [XmlAttribute("mail")]
        public int mail;

        // Income generation.
        [XmlAttribute("income")]
        public int income;
    }


    /// <summary>
    /// Basic serializable KeyValuePair for SubService dictionaries. 
    /// </summary>
    [Serializable]
    [XmlType(TypeName = "subservice")]
    public struct SubServiceValue
    {
        [XmlAttribute("subservice")]
        public ItemClass.SubService subService;

        [XmlAttribute("value")]
        public int value;
    }


    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [XmlType(TypeName = "subservice")]
    public struct SubServiceMode
    {
        [XmlAttribute("subservice")]
        public ItemClass.SubService subService;

        [XmlAttribute("mode")]
        public int mode;

        [XmlAttribute("multiplier")]
        public int multiplier;
    }
}