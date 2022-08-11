// <copyright file="Configuration.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Xml;
    using System.Xml.Serialization;
    using AlgernonCommons;
    using UnityEngine;

    /// <summary>
    /// Defines the XML configuration file.
    /// </summary>
    [XmlRoot("Configuration")]
    public class Configuration
    {

        /// <summary>
        /// Gets or sets the list of custom population packs.
        /// </summary>
        [XmlArray("pop_packs")]
        [XmlArrayItem("pop_pack")]
        public List<PopPackXML> PopPacks { get; set; }

        /// <summary>
        /// Gets or sets the list of custom floor packs.
        /// </summary>
        [XmlArray("floor_packs")]
        [XmlArrayItem("floor_pack")]
        public List<FloorPackXML> FloorPacks { get; set; }

        /// <summary>
        /// Gets or sets the list of custom consumption values.
        /// </summary>
        [XmlArray("consumption")]
        [XmlArrayItem("consumption")]
        public List<ConsumptionRecord> Consumption { get; set; }

        /// <summary>
        /// Gets or sets the list of default population packs.
        /// </summary>
        [XmlArray("default-pop")]
        [XmlArrayItem("default")]
        public List<DefaultPack> PopDefaults { get; set; }

        /// <summary>
        /// Gets or sets the list of default floor packs.
        /// </summary>
        [XmlArray("default-floor")]
        [XmlArrayItem("default")]
        public List<DefaultPack> FloorDefaults { get; set; }

        /// <summary>
        /// Gets or sets the list of custom building pack selections.
        /// </summary>
        [XmlArray("buildings")]
        [XmlArrayItem("building")]
        public List<BuildingRecord> Buildings { get; set; }

        /// <summary>
        /// Gets or sets the list of custom population overrides.
        /// </summary>
        [XmlArray("override_population")]
        [XmlArrayItem("population")]
        public List<PopCountOverride> PopOverrides { get; set; }

        /// <summary>
        /// Gets or sets the list of custom floor overrides.
        /// </summary>
        [XmlArray("override_floors")]
        [XmlArrayItem("floors")]
        public List<FloorCalcOverride> Floors { get; set; }

        /// <summary>
        /// Gets or sets the list of visitor mode selections.
        /// </summary>
        [XmlArray("visitors")]
        [XmlArrayItem("visitors")]
        public List<SubServiceMode> VisitorModes { get; set; }

        /// <summary>
        /// Gets or sets the list of commercial sales multipliers.
        /// </summary>
        [XmlArray("commercial_sales")]
        [XmlArrayItem("percentage")]
        public List<SubServiceValue> SalesMults { get; set; }

        /// <summary>
        /// Gets or sets the list of office production multipliers.
        /// </summary>
        [XmlArray("office_production")]
        [XmlArrayItem("percentage")]
        public List<SubServiceValue> OffProdMults { get; set; }

        /// <summary>
        /// Gets or sets the list of industrial production modes.
        /// </summary>
        [XmlArray("industrial_production")]
        [XmlArrayItem("production")]
        public List<SubServiceMode> IndProdModes { get; set; }

        /// <summary>
        /// Gets or sets the list of extractor production modes.
        /// </summary>
        [XmlArray("extractor_production")]
        [XmlArrayItem("production")]
        public List<SubServiceMode> ExtProdModes { get; set; }

        /// <summary>
        /// Gets or sets the list of commercial inventory caps.
        /// </summary>
        [XmlArray("commercial_inventory_caps")]
        [XmlArrayItem("inventory")]
        public List<SubServiceValue> ComIndCaps { get; set; }

        /// <summary>
        /// Default calculation pack dictionary record.
        /// </summary>
        public struct DefaultPack
        {
            /// <summary>
            /// Service.
            /// </summary>
            [XmlAttribute("service")]
            public ItemClass.Service Service;

            /// <summary>
            /// Sub-service.
            /// </summary>
            [XmlAttribute("subservice")]
            public ItemClass.SubService SubService;

            /// <summary>
            /// Default calculation pack.
            /// </summary>
            [XmlAttribute("pack")]
            [DefaultValue("")]
            public string Pack;
        }

        /// <summary>
        /// Building setting dictionary record.
        /// Mutable - so a class, not a struct.
        /// </summary>
        public class BuildingRecord
        {
            // Building multiplier.
            [XmlIgnore]
            private float _multiplier;

            /// <summary>
            /// Building prefab name.
            /// </summary>
            [XmlAttribute("prefab")]
            public string Prefab { get; set; }

            /// <summary>
            /// Gets or sets the assigned population pack.
            /// </summary>
            [XmlAttribute("pop")]
            [DefaultValue("")]
            public string PopPack { get; set; }

            /// <summary>
            /// Gets or sets the assigned floor calculation pack.
            /// </summary>
            [XmlAttribute("floor")]
            [DefaultValue("")]
            public string FloorPack { get; set; }

            /// <summary>
            /// Gets or sets the assigned school properties pack.
            /// </summary>
            // School properties pack.
            [XmlAttribute("school")]
            [DefaultValue("")]
            public string SchoolPack { get; set; }

            /// <summary>
            /// Gets or sets the building multiplier as a string.
            /// </summary>
            [XmlAttribute("multiplier")]
            [DefaultValue("")]
            public string MultiplierString
            {
                // Only serialize if multiplier is at least one.
                get => _multiplier >= 1 ? _multiplier.ToString() : string.Empty;

                set
                {
                    // Attempt to parse value as float.
                    if (!float.TryParse(value, out _multiplier))
                    {
                        Logging.Error("unable to parse multiplier as float; setting to default");
                        _multiplier = ModSettings.DefaultSchoolMult;
                    }

                    // Minimum value of 1.
                    _multiplier = Mathf.Max(1f, _multiplier);
                }
            }

            /// <summary>
            /// Gets or sets the building multiplier.
            /// </summary>
            [XmlIgnore]
            internal float Multiplier { get => _multiplier; set => _multiplier = value; }
        }

        /// <summary>
        /// Building population count override record.
        /// </summary>
        public struct PopCountOverride
        {
            /// <summary>
            /// Building prefab name.
            /// </summary>
            [XmlAttribute("prefab")]
            public string prefab;

            /// <summary>
            /// Population overrride value.
            /// </summary>
            [XmlAttribute("population")]
            [DefaultValue(0)]
            public ushort population;
        }

        /// <summary>
        /// Building floor calculation override record.
        /// </summary>
        public struct FloorCalcOverride
        {
            /// <summary>
            /// Building prefab name.
            /// </summary>
            [XmlAttribute("prefab")]
            public string Prefab;

            /// <summary>
            /// First floor height.
            /// </summary>
            [XmlAttribute("height_first")]
            public float FirstHeight;

            /// <summary>
            /// Standard floor height.
            /// </summary>
            [XmlAttribute("height_other")]
            public float FloorHeight;
        }

        /// <summary>
        /// Custom population calculation pack record.
        /// </summary>
        public struct PopPackXML
        {
            /// <summary>
            /// Population pack name.
            /// </summary>
            [XmlAttribute("name")]
            public string Name;

            /// <summary>
            /// Population pack service type.
            /// </summary>
            [XmlAttribute("service")]
            public ItemClass.Service Service;

            /// <summary>
            /// Population pack level records.
            /// </summary>
            [XmlArray("levels")]
            [XmlArrayItem("level")]
            public List<PopLevel> CalculationLevels;
        }

        /// <summary>
        /// Custom calculation pack individual level line.
        /// </summary>
        public struct PopLevel
        {
            /// <summary>
            /// Level ID.
            /// </summary>
            [XmlAttribute("level")]
            public int Level;

            /// <summary>
            /// Empty area, fixed.
            /// </summary>
            [XmlAttribute("empty")]
            public float EmptyArea;

            /// <summary>
            /// Empty area, percentage.
            /// </summary>
            [XmlAttribute("emptypercent")]
            public int EmptyPercent;

            /// <summary>
            /// Area per unit.
            /// </summary>
            [XmlAttribute("areaper")]
            public float AreaPer;

            /// <summary>
            /// Multi-level units.
            /// </summary>
            [XmlAttribute("multilevel")]
            public bool MultiLevel;
        }

        /// <summary>
        /// Custom floor calculation pack record.
        /// </summary>
        public struct FloorPackXML
        {
            /// <summary>
            /// Floor pack name.
            /// </summary>
            [XmlAttribute("name")]
            public string Name;

            /// <summary>
            /// Standard floor height.
            /// </summary>
            [XmlAttribute("floorheight")]
            public float FloorHeight;

            /// <summary>
            /// First floor minimum floor height.
            /// </summary>
            [XmlAttribute("firstmin")]
            public float FirstMin;

            /// <summary>
            /// First floor additional floor height.
            /// </summary>
            [XmlAttribute("firstextra")]
            public float FirstExtra;

            /// <summary>
            /// Indicates whether the first floor is empty.
            /// </summary>
            [XmlAttribute("firstempty")]
            public bool FirstEmpty;
        }

        /// <summary>
        /// Service consumption record.
        /// </summary>
        public struct ConsumptionRecord
        {
            /// <summary>
            /// Record service.
            /// </summary>
            [XmlAttribute("service")]
            public ItemClass.Service Service;

            /// <summary>
            /// Record sub-service.
            /// </summary>
            [XmlAttribute("subservice")]
            public ItemClass.SubService SubService;

            /// <summary>
            /// Record levels.
            /// </summary>
            [XmlArray("levels")]
            [XmlArrayItem("level")]
            public List<ConsumptionLine> Levels;
        }

        /// <summary>
        /// Consumption record individual level line.
        /// </summary>
        public struct ConsumptionLine
        {
            /// <summary>
            /// Building level for this record.
            /// </summary>
            [XmlAttribute("level")]
            public ItemClass.Level Level;

            /// <summary>
            /// Power consumption.
            /// </summary>
            [XmlAttribute("power")]
            public int Power;

            /// <summary>
            /// Water consumption.
            /// </summary>
            [XmlAttribute("water")]
            public int Water;

            /// <summary>
            /// Sewage generation.
            /// </summary>
            [XmlAttribute("sewage")]
            public int Sewage;

            /// <summary>
            /// Garbage generation.
            /// </summary>
            [XmlAttribute("garbage")]
            public int Garbage;

            /// <summary>
            /// Pollution generation.
            /// </summary>
            [XmlAttribute("pollution")]
            public int Pollution;

            /// <summary>
            /// Noise generation.
            /// </summary>
            [XmlAttribute("noise")]
            public int Noise;

            /// <summary>
            /// Mail generation.
            /// </summary>
            [XmlAttribute("mail")]
            public int Mail;

            /// <summary>
            /// Income generation.
            /// </summary>
            [XmlAttribute("income")]
            public int Income;
        }

        /// <summary>
        /// Basic serializable KeyValuePair for SubService dictionaries. 
        /// </summary>
        [Serializable]
        [XmlType(TypeName = "subservice")]
        public struct SubServiceValue
        {
            /// <summary>
            /// Sub-service.
            /// </summary>
            [XmlAttribute("subservice")]
            public ItemClass.SubService SubService;

            /// <summary>
            /// Value for this sub-service.
            /// </summary>
            [XmlAttribute("value")]
            public int Value;
        }

        /// <summary>
        /// Sub-service mode/multiplier record.
        /// </summary>
        [Serializable]
        [XmlType(TypeName = "subservice")]
        public struct SubServiceMode
        {
            /// <summary>
            /// Sub-service.
            /// </summary>
            [XmlAttribute("subservice")]
            public ItemClass.SubService SubService;

            /// <summary>
            /// Mode.
            /// </summary>
            [XmlAttribute("mode")]
            public int Mode;

            /// <summary>
            /// Multiplier.
            /// </summary>
            [XmlAttribute("multiplier")]
            public int Multiplier;
        }
    }
}