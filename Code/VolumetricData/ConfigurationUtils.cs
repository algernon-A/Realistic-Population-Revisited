// <copyright file="ConfigurationUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Serialization;
    using AlgernonCommons;

    /// <summary>
    /// XML serialization/deserialization utilities class.
    /// </summary>
    internal static class ConfigurationUtils
    {
        // Filename and location.
        private static readonly string ConfigFileName = "RealisticPopulationConfig.xml";
        private static readonly string UserDirPath = ColossalFramework.IO.DataLocation.localApplicationData;

        /// <summary>
        /// Gets a value indicating whether the configuration file has been read.
        /// </summary>
        internal static bool ConfigRead { get; private set; }

        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                // Check if config exists in userdir.
                string fileName = Path.Combine(UserDirPath, ConfigFileName);
                if (!File.Exists(fileName))
                {
                    // Userdir config doesn't exist; try application directory.
                    if (File.Exists(ConfigFileName))
                    {
                        fileName = ConfigFileName;
                    }
                    else
                    {
                        Logging.Message("no configuration file found");
                        return;
                    }
                }

                // Read it.
                using (StreamReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));

                    if (xmlSerializer.Deserialize(reader) is Configuration configFile)
                    {
                        Deserialize(configFile);
                    }
                    else
                    {
                        Logging.Error("couldn't deserialize configuration file");
                    }
                }

                // Record configuation as being read.
                ConfigRead = true;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception reading configuration file");
            }
        }

        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void SaveSettings()
        {
            try
            {
                // Pretty straightforward.
                using (StreamWriter writer = new StreamWriter(Path.Combine(UserDirPath, ConfigFileName)))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));

                    // Write to file.
                    xmlSerializer.Serialize(writer, Serialize());
                }

                // Delete any legacy (appdir) config.
                if (File.Exists(ConfigFileName))
                {
                    File.Delete(ConfigFileName);
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception saving configuration file");
            }
        }

        /// <summary>
        /// Serializes current settings into a new configuration.
        /// </summary>
        /// <returns>New configuration.</returns>
        private static Configuration Serialize()
        {
            try
            {
                Configuration configuration = new Configuration
                {
                    // Serialise custom packs.
                    PopPacks = new List<Configuration.PopPackXML>(),
                };

                // Iterate through all calculation packs in our dictionary.
                foreach (DataPack calcPack in PopData.Instance.CalcPacks)
                {
                    // Look for volumetric packs.
                    if (calcPack is VolumetricPopPack volPack)
                    {
                        // Look for packs marked as custom.
                        if (volPack.Version == DataPack.DataVersion.CustomOne)
                        {
                            // Found one - serialise it.
                            Configuration.PopPackXML xmlPack = new Configuration.PopPackXML()
                            {
                                Name = volPack.Name,
                                Service = volPack.Service,
                                CalculationLevels = new List<Configuration.PopLevel>(),
                            };

                            // Iterate through each level and add it to our serialisation.
                            for (int i = 0; i < volPack.Levels.Length; ++i)
                            {
                                Configuration.PopLevel xmlLevel = new Configuration.PopLevel()
                                {
                                    Level = i,
                                    EmptyArea = volPack.Levels[i].EmptyArea,
                                    EmptyPercent = volPack.Levels[i].EmptyPercent,
                                    AreaPer = volPack.Levels[i].AreaPer,
                                    MultiLevel = volPack.Levels[i].MultiFloorUnits,
                                };

                                xmlPack.CalculationLevels.Add(xmlLevel);
                            }

                            // Add to file.
                            configuration.PopPacks.Add(xmlPack);
                        }
                    }
                }

                // Serialise custom packs.
                // TODO method with above
                configuration.FloorPacks = new List<Configuration.FloorPackXML>();

                // Iterate through all calculation packs in our dictionary.
                foreach (DataPack calcPack in FloorData.Instance.CalcPacks)
                {
                    // Look for volumetric packs.
                    if (calcPack is FloorDataPack floorPack)
                    {
                        // Look for packs marked as custom.
                        if (floorPack.Version == DataPack.DataVersion.CustomOne)
                        {
                            // Found one - serialise it.
                            Configuration.FloorPackXML xmlPack = new Configuration.FloorPackXML()
                            {
                                Name = floorPack.Name,
                                FloorHeight = floorPack.m_floorHeight,
                                FirstMin = floorPack.m_firstFloorMin,
                                FirstExtra = floorPack.m_firstFloorExtra,
                                FirstEmpty = floorPack.m_firstFloorEmpty,
                            };

                            // Add to file.
                            configuration.FloorPacks.Add(xmlPack);
                        }
                    }
                }

                // Serialise consumption information.
                configuration.Consumption = GetConsumptionRecords();

                // Serialise default dictionaries.
                configuration.PopDefaults = PopData.Instance.SerializeDefaults();
                configuration.FloorDefaults = FloorData.Instance.SerializeDefaults();

                // Serialise building pack dictionaries, in order.
                SortedList<string, Configuration.BuildingRecord> buildingList = PopData.Instance.SerializeBuildings();
                buildingList = FloorData.Instance.SerializeBuildings(buildingList);
                buildingList = SchoolData.Instance.SerializeBuildings(buildingList);
                buildingList = Multipliers.Instance.SerializeBuildings(buildingList);
                configuration.Buildings = buildingList.Values.ToList();

                // Serialise building population overrides.
                configuration.PopOverrides = PopData.Instance.SerializeOverrides();

                // Serialise floor overrides.
                configuration.Floors = FloorData.Instance.SerializeOverrides();

                // Serialise visit modes.
                configuration.VisitorModes = Visitors.SerializeVisits();

                // Serialise commercial sales multipliers.
                configuration.SalesMults = GoodsUtils.SerializeSalesMults();

                // Serialise office production multipliers.
                configuration.OffProdMults = OfficeProduction.SerializeProdMults();

                // Serialize industrial production calculation modes.
                configuration.IndProdModes = IndustrialProduction.SerializeProds();
                configuration.ExtProdModes = ExtractorProduction.SerializeProds();

                // Serialise commercial inventory caps.
                configuration.ComIndCaps = GoodsUtils.SerializeInvCaps();

                return configuration;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception serializing configuration");
                return null;
            }
        }

        /// <summary>
        /// Deserializes the provided configuration file.
        /// </summary>
        /// <param name="configuration">Configuration to deserialize.</param>
        private static void Deserialize(Configuration configuration)
        {
            try
            {
                // Deserialise population calculation packs.
                foreach (Configuration.PopPackXML xmlPack in configuration.PopPacks)
                {
                    // Convert to volumetric pack.
                    VolumetricPopPack volPack = new VolumetricPopPack(DataPack.DataVersion.CustomOne, xmlPack.Service)
                    {
                        Name = xmlPack.Name,
                    };

                    // Iterate through each level in the xml and add to our volumetric pack.
                    foreach (Configuration.PopLevel calculationLevel in xmlPack.CalculationLevels)
                    {
                        volPack.Levels[calculationLevel.Level] = new VolumetricPopPack.LevelData()
                        {
                            EmptyArea = calculationLevel.EmptyArea,
                            EmptyPercent = calculationLevel.EmptyPercent,
                            AreaPer = calculationLevel.AreaPer,
                            MultiFloorUnits = calculationLevel.MultiLevel,
                        };
                    }

                    // Add new pack to our dictionary.
                    PopData.Instance.AddCalculationPack(volPack);
                }

                // Deserialise floor calculation packs.
                foreach (Configuration.FloorPackXML xmlPack in configuration.FloorPacks)
                {
                    // Convert to floor pack.
                    FloorDataPack floorPack = new FloorDataPack(DataPack.DataVersion.CustomOne)
                    {
                        Name = xmlPack.Name,
                        m_floorHeight = xmlPack.FloorHeight,
                        m_firstFloorMin = xmlPack.FirstMin,
                        m_firstFloorExtra = xmlPack.FirstExtra,
                        m_firstFloorEmpty = xmlPack.FirstEmpty,
                    };

                    // Add new pack to our dictionary.
                    FloorData.Instance.AddCalculationPack(floorPack);
                }

                // Deserialise consumption records.
                DataMapping mapper = new DataMapping();
                foreach (Configuration.ConsumptionRecord consumptionRecord in configuration.Consumption)
                {
                    // Get relevant DataStore array for this record.
                    int[][] dataArray = mapper.GetArray(consumptionRecord.Service, consumptionRecord.SubService);

                    // Iterate through each consumption line and populate relevant DataStore fields.
                    foreach (Configuration.ConsumptionLine consumptionLine in consumptionRecord.Levels)
                    {
                        int level = (int)consumptionLine.Level;
                        dataArray[level][DataStore.POWER] = consumptionLine.Power;
                        dataArray[level][DataStore.WATER] = consumptionLine.Water;
                        dataArray[level][DataStore.SEWAGE] = consumptionLine.Sewage;
                        dataArray[level][DataStore.GARBAGE] = consumptionLine.Garbage;
                        dataArray[level][DataStore.GROUND_POLLUTION] = consumptionLine.Pollution;
                        dataArray[level][DataStore.NOISE_POLLUTION] = consumptionLine.Noise;
                        dataArray[level][DataStore.MAIL] = consumptionLine.Mail;
                        dataArray[level][DataStore.INCOME] = consumptionLine.Income;
                    }
                }

                // Deserialise default pack lists.
                PopData.Instance.DeserializeDefaults(configuration.PopDefaults);
                FloorData.Instance.DeserializeDefaults(configuration.FloorDefaults);

                // Deserialise building pack lists.
                PopData.Instance.DeserializeBuildings(configuration.Buildings);
                FloorData.Instance.DeserializeBuildings(configuration.Buildings);
                SchoolData.Instance.DeserializeBuildings(configuration.Buildings);
                Multipliers.Instance.DeserializeBuildings(configuration.Buildings);

                // Deserialise building population overrides.
                PopData.Instance.DeserializeOverrides(configuration.PopOverrides);

                // Deserialize floor overrides.
                foreach (Configuration.FloorCalcOverride floorOverride in configuration.Floors)
                {
                    FloorData.Instance.SetOverride(floorOverride.Prefab, new FloorDataPack(DataPack.DataVersion.OverrideOne)
                    {
                        m_firstFloorMin = floorOverride.FirstHeight,
                        m_floorHeight = floorOverride.FloorHeight,
                    });
                }

                // Deserialise visit modes.
                Visitors.DeserializeVisits(configuration.VisitorModes);

                // Deserialise commercial sales multipliers.
                GoodsUtils.DeserializeSalesMults(configuration.SalesMults);

                // Deserialise office production multipliers.
                OfficeProduction.DeserializeProdMults(configuration.OffProdMults);

                // Deserialize industrial production calculation modes.
                IndustrialProduction.DeserializeProds(configuration.IndProdModes);
                ExtractorProduction.DeserializeProds(configuration.ExtProdModes);

                // Deserialise commercial inventory caps.
                GoodsUtils.DeserializeInvCaps(configuration.ComIndCaps);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception deserializing configuration");
            }
        }

        /// <summary>
        /// Returns a list of serialised consumption records from available DataStore arrays.
        /// </summary>
        /// <returns>New list of consumption records.</returns>
        private static List<Configuration.ConsumptionRecord> GetConsumptionRecords()
        {
            List<Configuration.ConsumptionRecord> list = new List<Configuration.ConsumptionRecord>(DataMapping.NumData);
            DataMapping mapper = new DataMapping();

            // Iterate through each data structure.
            for (int i = 0; i < DataMapping.NumData; ++i)
            {
                // Create new consumption record with relevant data and add it to our list.
                Configuration.ConsumptionRecord newRecord = new Configuration.ConsumptionRecord()
                {
                    Service = mapper.Services[i],
                    SubService = mapper.SubServices[i],
                    Levels = SerializeConsumption(mapper.DataArrays[i]),
                };
                list.Add(newRecord);
            }

            return list;
        }

        /// <summary>
        /// Serialise consumption levels for provided service/subservice data.
        /// </summary>
        /// <param name="dataArray">DataStore integer array to serialise.</param>
        /// <returns>New list of serialised consumption data level records.</returns>
        private static List<Configuration.ConsumptionLine> SerializeConsumption(int[][] dataArray)
        {
            List<Configuration.ConsumptionLine> lines = new List<Configuration.ConsumptionLine>();

            // Iterate through each row in the provided data array.
            for (int i = 0; i < dataArray.Length; ++i)
            {
                // Create new consumption line record from data array row.
                Configuration.ConsumptionLine newLine = new Configuration.ConsumptionLine()
                {
                    Level = (ItemClass.Level)i,
                    Power = dataArray[i][DataStore.POWER],
                    Water = dataArray[i][DataStore.WATER],
                    Sewage = dataArray[i][DataStore.SEWAGE],
                    Garbage = dataArray[i][DataStore.GARBAGE],
                    Pollution = dataArray[i][DataStore.GROUND_POLLUTION],
                    Noise = dataArray[i][DataStore.NOISE_POLLUTION],
                    Mail = dataArray[i][DataStore.MAIL],
                    Income = dataArray[i][DataStore.INCOME],
                };

                // Add new record to the return list.
                lines.Add(newLine);
            }

            return lines;
        }
    }
}