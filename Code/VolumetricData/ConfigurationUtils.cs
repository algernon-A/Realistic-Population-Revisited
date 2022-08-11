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
    using Realistic_Population_Revisited.Code.Patches.Population;
    using Realistic_Population_Revisited.Code.Patches.Production;

    /// <summary>
    /// XML serialization/deserialization utilities class.
    /// </summary>
    internal static class ConfigUtils
    {
        // Filename and location.
        internal static readonly string ConfigFileName = "RealisticPopulationConfig.xml";
        internal static readonly string UserDirPath = ColossalFramework.IO.DataLocation.localApplicationData;

        // Read flag.
        internal static bool configRead = false;

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

                    if (!(xmlSerializer.Deserialize(reader) is Configuration configFile))
                    {
                        Logging.Error("couldn't deserialize configuration file");
                    }
                    else
                    {
                        // Deserialise population calculation packs.
                        foreach (Configuration.PopPackXML xmlPack in configFile.PopPacks)
                        {
                            // Convert to volumetric pack.
                            VolumetricPopPack volPack = new VolumetricPopPack()
                            {
                                name = xmlPack.Name,
                                service = xmlPack.Service,
                                version = DataVersion.customOne,
                                levels = new LevelData[xmlPack.CalculationLevels.Count]
                            };

                            // Iterate through each level in the xml and add to our volumetric pack.
                            foreach (Configuration.PopLevel calculationLevel in xmlPack.CalculationLevels)
                            {
                                volPack.levels[calculationLevel.Level] = new LevelData()
                                {
                                    //floorHeight = calculationLevel.floorHeight,
                                    emptyArea = calculationLevel.EmptyArea,
                                    emptyPercent = calculationLevel.EmptyPercent,
                                    areaPer = calculationLevel.AreaPer,
                                    multiFloorUnits = calculationLevel.MultiLevel
                                };
                            }

                            // Add new pack to our dictionary.
                            PopData.instance.AddCalculationPack(volPack);
                        }

                        // Deserialise floor calculation packs.
                        foreach (Configuration.FloorPackXML xmlPack in configFile.FloorPacks)
                        {
                            // Convert to floor pack.
                            FloorDataPack floorPack = new FloorDataPack()
                            {
                                name = xmlPack.Name,
                                version = DataVersion.customOne,
                                floorHeight = xmlPack.FloorHeight,
                                firstFloorMin = xmlPack.FirstMin,
                                firstFloorExtra = xmlPack.FirstExtra,
                                firstFloorEmpty = xmlPack.FirstEmpty
                            };

                            // Add new pack to our dictionary.
                            FloorData.instance.AddCalculationPack(floorPack);
                        }


                        // Deserialise consumption records.
                        DataMapping mapper = new DataMapping();
                        foreach (Configuration.ConsumptionRecord consumptionRecord in configFile.Consumption)
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
                        PopData.instance.DeserializeDefaults(configFile.PopDefaults);
                        FloorData.instance.DeserializeDefaults(configFile.FloorDefaults);

                        // Deserialise building pack lists.
                        PopData.instance.DeserializeBuildings(configFile.Buildings);
                        FloorData.instance.DeserializeBuildings(configFile.Buildings);
                        SchoolData.instance.DeserializeBuildings(configFile.Buildings);
                        Multipliers.instance.DeserializeBuildings(configFile.Buildings);

                        // Deserialise building population overrides.
                        PopData.instance.DeserializeOverrides(configFile.PopOverrides);

                        // Deserialize floor overrides.
                        foreach (Configuration.FloorCalcOverride floorOverride in configFile.Floors)
                        {
                            FloorData.instance.SetOverride(floorOverride.Prefab, new FloorDataPack
                            {
                                firstFloorMin = floorOverride.FirstHeight,
                                floorHeight = floorOverride.FloorHeight
                            });
                        }

                        // Deserialise visit modes.
                        Visitors.DeserializeVisits(configFile.VisitorModes);

                        // Deserialise commercial sales multipliers.
                        GoodsUtils.DeserializeSalesMults(configFile.SalesMults);

                        // Deserialise office production multipliers.
                        OfficeProduction.DeserializeProdMults(configFile.OffProdMults);

                        // Deserialize industrial production calculation modes.
                        IndustrialProduction.DeserializeProds(configFile.IndProdModes);
                        ExtractorProduction.DeserializeProds(configFile.ExtProdModes);

                        // Deserialise commercial inventory caps.
                        GoodsUtils.DeserializeInvCaps(configFile.ComIndCaps);
                    }
                }

                // Set status flag.
                configRead = true;
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
                // Pretty straightforward.  Serialisation is within settings file class.
                using (StreamWriter writer = new StreamWriter(Path.Combine(UserDirPath, ConfigFileName)))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Configuration));
                    Configuration configFile = new Configuration
                    {
                        // Serialise custom packs.
                        PopPacks = new List<Configuration.PopPackXML>()
                    };

                    // Iterate through all calculation packs in our dictionary.
                    foreach (PopDataPack calcPack in PopData.instance.calcPacks)
                    {
                        // Look for volumetric packs.
                        if (calcPack is VolumetricPopPack volPack)
                        {
                            // Look for packs marked as custom.
                            if (volPack.version == DataVersion.customOne)
                            {
                                // Found one - serialise it.
                                Configuration.PopPackXML xmlPack = new Configuration.PopPackXML()
                                {
                                    Name = volPack.name,
                                    Service = volPack.service,
                                    CalculationLevels = new List<Configuration.PopLevel>()
                                };

                                // Iterate through each level and add it to our serialisation.
                                for (int i = 0; i < volPack.levels.Length; ++i)
                                {
                                    Configuration.PopLevel xmlLevel = new Configuration.PopLevel()
                                    {
                                        Level = i,
                                        EmptyArea = volPack.levels[i].emptyArea,
                                        EmptyPercent = volPack.levels[i].emptyPercent,
                                        AreaPer = volPack.levels[i].areaPer,
                                        MultiLevel = volPack.levels[i].multiFloorUnits
                                    };

                                    xmlPack.CalculationLevels.Add(xmlLevel);
                                }

                                // Add to file.
                                configFile.PopPacks.Add(xmlPack);
                            }
                        }
                    }

                    // Serialise custom packs.
                    // TODO method with above
                    configFile.FloorPacks = new List<Configuration.FloorPackXML>();

                    // Iterate through all calculation packs in our dictionary.
                    foreach (DataPack calcPack in FloorData.instance.calcPacks)
                    {
                        // Look for volumetric packs.
                        if (calcPack is FloorDataPack floorPack)
                        {
                            // Look for packs marked as custom.
                            if (floorPack.version == DataVersion.customOne)
                            {
                                // Found one - serialise it.
                                Configuration.FloorPackXML xmlPack = new Configuration.FloorPackXML()
                                {
                                    Name = floorPack.name,
                                    FloorHeight = floorPack.floorHeight,
                                    FirstMin = floorPack.firstFloorMin,
                                    FirstExtra = floorPack.firstFloorExtra,
                                    FirstEmpty = floorPack.firstFloorEmpty
                                };

                                // Add to file.
                                configFile.FloorPacks.Add(xmlPack);
                            }
                        }
                    }

                    // Serialise consumption information.
                    configFile.Consumption = GetConsumptionRecords();

                    // Serialise default dictionaries.
                    configFile.PopDefaults = PopData.instance.SerializeDefaults();
                    configFile.FloorDefaults = FloorData.instance.SerializeDefaults();

                    // Serialise building pack dictionaries, in order.
                    SortedList<string, Configuration.BuildingRecord> buildingList = PopData.instance.SerializeBuildings();
                    buildingList = FloorData.instance.SerializeBuildings(buildingList);
                    buildingList = SchoolData.instance.SerializeBuildings(buildingList);
                    buildingList = Multipliers.instance.SerializeBuildings(buildingList);
                    configFile.Buildings = buildingList.Values.ToList();

                    // Serialise building population overrides.
                    configFile.PopOverrides = PopData.instance.SerializeOverrides();

                    // Serialise floor overrides.
                    configFile.Floors = new List<Configuration.FloorCalcOverride>();
                    foreach (KeyValuePair<string, FloorDataPack> floorOverride in FloorData.instance.overrides)
                    {
                        configFile.Floors.Add(new Configuration.FloorCalcOverride
                        {
                            Prefab = floorOverride.Key,
                            FirstHeight = floorOverride.Value.firstFloorMin,
                            FloorHeight = floorOverride.Value.floorHeight
                        });
                    }

                    // Serialise visit modes.
                    configFile.VisitorModes = Visitors.SerializeVisits();

                    // Serialise commercial sales multipliers.
                    configFile.SalesMults = GoodsUtils.SerializeSalesMults();

                    // Serialise office production multipliers.
                    configFile.OffProdMults = OfficeProduction.SerializeProdMults();

                    // Serialize industrial production calculation modes.
                    configFile.IndProdModes = IndustrialProduction.SerializeProds();
                    configFile.ExtProdModes = ExtractorProduction.SerializeProds();

                    // Serialise commercial inventory caps.
                    configFile.ComIndCaps = GoodsUtils.SerializeInvCaps();

                    // Write to file.
                    xmlSerializer.Serialize(writer, configFile);
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
        /// Returns a list of serialised consumption records from available DataStore arrays.
        /// </summary>
        /// <returns>New list of consumption records.</returns>
        private static List<Configuration.ConsumptionRecord> GetConsumptionRecords()
        {
            List<Configuration.ConsumptionRecord> list = new List<Configuration.ConsumptionRecord>(DataMapping.numData);
            DataMapping mapper = new DataMapping();


            // Iterate through each data structure.
            for (int i = 0; i < DataMapping.numData; ++i)
            {
                // Create new consumption record with relevant data and add it to our list.
                Configuration.ConsumptionRecord newRecord = new Configuration.ConsumptionRecord()
                {
                    Service = mapper.services[i],
                    SubService = mapper.subServices[i],
                    Levels = SerializeConsumption(mapper.dataArrays[i])
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