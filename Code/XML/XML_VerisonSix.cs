namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security;
    using System.Xml;
    using AlgernonCommons;

    public class XML_VersionSix : WG_XMLBaseVersion
    {
        private const string popNodeName = "population";
        private const string overrideHouseName = "overrideHouseHold";
        private const string overrideWorkName = "overrideWorker";
        private const string bonusHouseName = "bonusHouseHold";
        private const string bonusWorkName = "bonusWorker";
        private const string printHouseName = "printHouseHold";
        private const string printWorkName = "printWorker";
        private const string meshName = "meshName";
        private const string consumeNodeName = "consumption";
        private const string visitNodeName = "visitor";
        private const string pollutionNodeName = "pollution";
        private const string productionNodeName = "production";

        /// <param name="doc"></param>
        public override void ReadXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;
            try
            {
                //DataStore.enableExperimental = Convert.ToBoolean(root.Attributes["experimental"].InnerText);
                //DataStore.timeBasedRealism = Convert.ToBoolean(root.Attributes["enableTimeVariation"].InnerText);
            }
            catch (Exception)
            {
                DataStore.enableExperimental = false;
            }

            foreach (XmlNode node in root.ChildNodes)
            {
                try
                {
                    if (node.Name.Equals(popNodeName))
                    {
                        ReadPopulationNode(node);
                    }
                    else if (node.Name.Equals(consumeNodeName))
                    {
                        ReadConsumptionNode(node);
                    }
                    else if (node.Name.Equals(visitNodeName))
                    {
                        ReadVisitNode(node);
                    }
                    else if (node.Name.Equals(pollutionNodeName))
                    {
                        ReadPollutionNode(node);
                    }
                    else if (node.Name.Equals(productionNodeName))
                    {
                        ReadProductionNode(node);
                    }
                    else if (node.Name.Equals(overrideHouseName))
                    {
                        ReadOverrideHouseNode(node);
                    }
                    else if (node.Name.Equals(overrideWorkName))
                    {
                        ReadOverrideWorkers(node);
                    }
                    else if (node.Name.Equals(bonusHouseName))
                    {
                        ReadBonusHouseNode(node);
                    }
                    else if (node.Name.Equals(bonusWorkName))
                    {
                        ReadBonusWorkers(node);
                    }
                    else if (node.Name.Equals(printHouseName))
                    {
                        ReadPrintHouseNode(node);
                    }
                    else if (node.Name.Equals(printWorkName))
                    {
                        ReadPrintWorkers(node);
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "exception parsing root.ChildNodes");
                }
            }
        } // end readXML

        public override bool WriteXML(string fullPathFileName)
        {
            Logging.Message("writing legacy XML");

            XmlDocument xmlDoc = new XmlDocument();

            XmlNode rootNode = xmlDoc.CreateElement("WG_CityMod");
            XmlAttribute attribute = xmlDoc.CreateAttribute("version");
            attribute.Value = "6";
            rootNode.Attributes.Append(attribute);

            /*
            attribute = xmlDoc.CreateAttribute("experimental");
            attribute.Value = DataStore.enableExperimental ? "true" : "false";
            rootNode.Attributes.Append(attribute);
            */

            xmlDoc.AppendChild(rootNode);

            XmlNode popNode = xmlDoc.CreateElement(popNodeName);
            attribute = xmlDoc.CreateAttribute("strictCapacity");
            attribute.Value = DataStore.strictCapacity ? "true" : "false";
            popNode.Attributes.Append(attribute);

            XmlNode consumeNode = xmlDoc.CreateElement(consumeNodeName);
            XmlNode visitNode = xmlDoc.CreateElement(visitNodeName);
            XmlNode pollutionNode = xmlDoc.CreateElement(pollutionNodeName);
            XmlNode productionNode = xmlDoc.CreateElement(productionNodeName);

            try
            {
                MakeNodes(xmlDoc, "ResidentialLow", DataStore.residentialLow, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "ResidentialHigh", DataStore.residentialHigh, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "ResEcoLow", DataStore.resEcoLow, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "ResEcoHigh", DataStore.resEcoHigh, popNode, consumeNode, visitNode, pollutionNode, productionNode);

                MakeNodes(xmlDoc, "CommercialLow", DataStore.commercialLow, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "CommercialHigh", DataStore.commercialHigh, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "CommercialEco", DataStore.commercialEco, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "CommercialTourist", DataStore.commercialTourist, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "CommercialLeisure", DataStore.commercialLeisure, popNode, consumeNode, visitNode, pollutionNode, productionNode);

                MakeNodes(xmlDoc, "Office", DataStore.office, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "OfficeHighTech", DataStore.officeHighTech, popNode, consumeNode, visitNode, pollutionNode, productionNode);

                MakeNodes(xmlDoc, "Industry", DataStore.industry, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "IndustryFarm", DataStore.industry_farm, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "IndustryForest", DataStore.industry_forest, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "IndustryOre", DataStore.industry_ore, popNode, consumeNode, visitNode, pollutionNode, productionNode);
                MakeNodes(xmlDoc, "IndustryOil", DataStore.industry_oil, popNode, consumeNode, visitNode, pollutionNode, productionNode);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "XML MakeNodes exception");
            }

            // First segment
            CreatePopulationNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(popNode);
            CreateConsumptionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(consumeNode);
            CreateVisitNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(visitNode);
            CreateProductionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(productionNode);
            CreatePollutionNodeComment(xmlDoc, rootNode);
            rootNode.AppendChild(pollutionNode);

            // Add mesh names to XML for house holds
            XmlComment comment = xmlDoc.CreateComment(" ******* House hold data ******* ");
            rootNode.AppendChild(comment);
            XmlNode overrideHouseholdNode = xmlDoc.CreateElement(overrideHouseName);
            attribute = xmlDoc.CreateAttribute("printResNames");
            attribute.Value = DataStore.printResidentialNames ? "true" : "false";
            overrideHouseholdNode.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("mergeResNames");
            attribute.Value = DataStore.mergeResidentialNames ? "true" : "false";
            overrideHouseholdNode.Attributes.Append(attribute);

            SortedList<string, int> list = new SortedList<string, int>(DataStore.householdCache);
            foreach (string name in list.Keys)
            {
                XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                meshNameNode.InnerXml = SecurityElement.Escape(name);
                attribute = xmlDoc.CreateAttribute("value");
                DataStore.householdCache.TryGetValue(name, out int value);
                attribute.Value = Convert.ToString(value);
                meshNameNode.Attributes.Append(attribute);
                overrideHouseholdNode.AppendChild(meshNameNode);
            }
            rootNode.AppendChild(overrideHouseholdNode); // Append the overrideHousehold to root

            // Add mesh names to XML
            comment = xmlDoc.CreateComment(" ******* Printed out house hold data. To activate the value, move the line into the override segment ******* ");
            rootNode.AppendChild(comment);
            XmlNode printHouseholdNode = xmlDoc.CreateElement(printHouseName);
            list = new SortedList<string, int>(DataStore.housePrintOutCache);
            foreach (string data in list.Keys)
            {
                XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                meshNameNode.InnerXml = data;
                attribute = xmlDoc.CreateAttribute("value");
                DataStore.housePrintOutCache.TryGetValue(data, out int value);
                attribute.Value = Convert.ToString(value);
                meshNameNode.Attributes.Append(attribute);
                printHouseholdNode.AppendChild(meshNameNode);
            }
            rootNode.AppendChild(printHouseholdNode); // Append the printHousehold to root

            // Add mesh names to XML
            list = new SortedList<string, int>(DataStore.bonusHouseholdCache);
            if (list.Keys.Count != 0)
            {
                XmlNode bonusHouseholdNode = xmlDoc.CreateElement(bonusHouseName);
                foreach (string data in list.Keys)
                {
                    XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                    meshNameNode.InnerXml = data;
                    attribute = xmlDoc.CreateAttribute("value");
                    DataStore.bonusHouseholdCache.TryGetValue(data, out int value);
                    attribute.Value = Convert.ToString(value);
                    meshNameNode.Attributes.Append(attribute);
                    bonusHouseholdNode.AppendChild(meshNameNode);
                }
                rootNode.AppendChild(bonusHouseholdNode); // Append the bonusHousehold to root
            }

            // Add mesh names to XML for workers
            comment = xmlDoc.CreateComment(" ******* Worker data ******* ");
            rootNode.AppendChild(comment);
            XmlNode overrideWorkNode = xmlDoc.CreateElement(overrideWorkName);
            attribute = xmlDoc.CreateAttribute("printWorkNames");
            attribute.Value = DataStore.printEmploymentNames ? "true" : "false";
            overrideWorkNode.Attributes.Append(attribute);
            attribute = xmlDoc.CreateAttribute("mergeWorkNames");
            attribute.Value = DataStore.mergeEmploymentNames ? "true" : "false";
            overrideWorkNode.Attributes.Append(attribute);

            SortedList<string, int> wList = new SortedList<string, int>(DataStore.workerCache);
            foreach (string name in wList.Keys)
            {
                XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                meshNameNode.InnerXml = SecurityElement.Escape(name);
                DataStore.workerCache.TryGetValue(name, out int value);
                attribute = xmlDoc.CreateAttribute("value");
                attribute.Value = Convert.ToString(value);
                meshNameNode.Attributes.Append(attribute);
                overrideWorkNode.AppendChild(meshNameNode);
            }
            rootNode.AppendChild(overrideWorkNode); // Append the overrideWorkers to root

            // Add mesh names to dictionary
            comment = xmlDoc.CreateComment(" ******* Printed out worker data. To activate the value, move the line into the override segment ******* ");
            rootNode.AppendChild(comment);
            XmlNode printWorkNode = xmlDoc.CreateElement(printWorkName);
            wList = new SortedList<string, int>(DataStore.workerPrintOutCache);
            foreach (string data in wList.Keys)
            {
                if (!DataStore.workerCache.ContainsKey(data))
                {
                    XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                    meshNameNode.InnerXml = data;
                    DataStore.workerPrintOutCache.TryGetValue(data, out int value);
                    attribute = xmlDoc.CreateAttribute("value");
                    attribute.Value = Convert.ToString(value);
                    meshNameNode.Attributes.Append(attribute);
                    printWorkNode.AppendChild(meshNameNode);
                }
            }
            rootNode.AppendChild(printWorkNode); // Append the printWorkers to root

            // Add mesh names to dictionary
            wList = new SortedList<string, int>(DataStore.bonusWorkerCache);
            if (wList.Keys.Count != 0)
            {
                XmlNode bonusWorkNode = xmlDoc.CreateElement(bonusWorkName);
                foreach (string data in wList.Keys)
                {
                    XmlNode meshNameNode = xmlDoc.CreateElement(meshName);
                    meshNameNode.InnerXml = data;
                    DataStore.bonusWorkerCache.TryGetValue(data, out int value);
                    attribute = xmlDoc.CreateAttribute("value");
                    attribute.Value = Convert.ToString(value);
                    meshNameNode.Attributes.Append(attribute);
                    bonusWorkNode.AppendChild(meshNameNode);
                }
                rootNode.AppendChild(bonusWorkNode); // Append the bonusWorkers to root
            }

            try
            {
                if (File.Exists(fullPathFileName))
                {
                    if (File.Exists(fullPathFileName + ".bak"))
                    {
                        File.Delete(fullPathFileName + ".bak");
                    }

                    File.Move(fullPathFileName, fullPathFileName + ".bak");
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e, "PathFileName exception");
            }

            try
            {
                xmlDoc.Save(fullPathFileName);
            }
            catch (Exception e)
            {
                Logging.LogException(e, "XML save exception");
                return false;  // Only time when we say there's an error
            }

            return true;
        } // end writeXML

        private void CreatePopulationNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("space_pp = Square metres per person");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("level_height = Height of a floor. The height of chimneys have been taken into account");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("calc = model or plot. To calculate the base using either the building model, or by the land size");
            rootNode.AppendChild(comment);
            //comment = xmlDoc.CreateComment("visit_mult = The number of visitors as a multiple of workers to 1 decimal place. This is used for commercial only");
            //            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("lvl_0 ... lvl_3 = Proportional values between the education levels (uneducated, educated, well educated, highly educated). Does not need to be percentages.");
            rootNode.AppendChild(comment);
        }

        private void CreateConsumptionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Consumption values are per household, or per production unit");
            rootNode.AppendChild(comment);
        }

        private void CreateVisitNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Visitor Values are multiplies of 100th of a person per cell.");
            rootNode.AppendChild(comment);
        }

        private void CreatePollutionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Ground pollution is not used by residential, commercial and offices.");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("Noise pollution is not spread over the land by residential or offices.");
            rootNode.AppendChild(comment);
        }

        private void CreateProductionNodeComment(XmlDocument xmlDoc, XmlNode rootNode)
        {
            XmlComment comment = xmlDoc.CreateComment("Production for offices is number of employees per production unit.");
            rootNode.AppendChild(comment);
            comment = xmlDoc.CreateComment("For industry, it used as hundredths of a unit per cell block.");
            rootNode.AppendChild(comment);
        }

        private void MakeNodes(XmlDocument xmlDoc, String buildingType, int[][] array, XmlNode rootPopNode, XmlNode consumNode, XmlNode visitNode, XmlNode pollutionNode, XmlNode productionNode)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                MakeNodes(xmlDoc, buildingType, array[i], i, rootPopNode, consumNode, visitNode, pollutionNode, productionNode);
            }
        }

        private void MakeNodes(XmlDocument xmlDoc, String buildingType, int[] array, int level, XmlNode rootPopNode, XmlNode consumNode, XmlNode visitNode, XmlNode pollutionNode, XmlNode productionNode)
        {
            MakePopNode(rootPopNode, xmlDoc, buildingType, level, array);
            MakeConsumeNode(consumNode, xmlDoc, buildingType, level, array[DataStore.POWER], array[DataStore.WATER], array[DataStore.SEWAGE], array[DataStore.GARBAGE], array[DataStore.INCOME]);
            MakeVisitNode(visitNode, xmlDoc, buildingType, level, array);
            MakePollutionNode(pollutionNode, xmlDoc, buildingType, level, array[DataStore.GROUND_POLLUTION], array[DataStore.NOISE_POLLUTION]);
            MakeProductionNode(productionNode, xmlDoc, buildingType, level, array[DataStore.PRODUCTION]);
        }

        private void MakePopNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int[] array)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("space_pp");
            attribute.Value = Convert.ToString(TransformPopulationModifier(buildingType, array[DataStore.PEOPLE], true));
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("level_height");
            attribute.Value = Convert.ToString(array[DataStore.LEVEL_HEIGHT]);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("calc");
            attribute.Value = array[DataStore.CALC_METHOD] == 0 ? "model" : "plot";
            node.Attributes.Append(attribute);

            if (array[DataStore.WORK_LVL0] >= 0)
            {
                attribute = xmlDoc.CreateAttribute("ground_mult");
                attribute.Value = Convert.ToString(array[DataStore.DENSIFICATION]);
                node.Attributes.Append(attribute);

                for (int i = 0; i < 4; i++)
                {
                    attribute = xmlDoc.CreateAttribute("lvl_" + i);
                    attribute.Value = Convert.ToString(array[DataStore.WORK_LVL0 + i]);
                    node.Attributes.Append(attribute);
                }
            }

            root.AppendChild(node);
        }

        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="array"></param>
        private void MakeVisitNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int[] array)
        {
            if (array[DataStore.VISIT] >= 0)
            {
                XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("visit");
                attribute.Value = Convert.ToString(array[DataStore.VISIT]);
                node.Attributes.Append(attribute);

                root.AppendChild(node);
            }
        }


        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="power"></param>
        /// <param name="water"></param>
        /// <param name="sewage"></param>
        /// <param name="garbage"></param>
        /// <param name="wealth"></param>
        private void MakeConsumeNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int power, int water, int sewage, int garbage, int wealth)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("power");
            attribute.Value = Convert.ToString(power);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("water");
            attribute.Value = Convert.ToString(water);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("sewage");
            attribute.Value = Convert.ToString(sewage);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("garbage");
            attribute.Value = Convert.ToString(garbage);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("wealth");
            attribute.Value = Convert.ToString(wealth);
            node.Attributes.Append(attribute);

            root.AppendChild(node);
        }

        private void MakePollutionNode(XmlNode root, XmlDocument xmlDoc, String buildingType, int level, int ground, int noise)
        {
            XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

            XmlAttribute attribute = xmlDoc.CreateAttribute("ground");
            attribute.Value = Convert.ToString(ground);
            node.Attributes.Append(attribute);

            attribute = xmlDoc.CreateAttribute("noise");
            attribute.Value = Convert.ToString(noise);
            node.Attributes.Append(attribute);

            root.AppendChild(node);
        }


        /// <param name="root"></param>
        /// <param name="xmlDoc"></param>
        /// <param name="buildingType"></param>
        /// <param name="level"></param>
        /// <param name="production"></param>
        private void MakeProductionNode(XmlNode root, XmlDocument xmlDoc, string buildingType, int level, int production)
        {
            if (production >= 0)
            {
                XmlNode node = xmlDoc.CreateElement(buildingType + "_" + (level + 1));

                XmlAttribute attribute = xmlDoc.CreateAttribute("production");
                attribute.Value = Convert.ToString(production);
                node.Attributes.Append(attribute);

                root.AppendChild(node);
            }
        }


        /// <param name="pollutionNode"></param>
        private void ReadPollutionNode(XmlNode pollutionNode)
        {
            foreach (XmlNode node in pollutionNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int ground = Convert.ToInt32(node.Attributes["ground"].InnerText);
                    int noise = Convert.ToInt32(node.Attributes["noise"].InnerText);

                    switch (name)
                    {
                        case "ResidentialLow":
                            SetPollutionRates(DataStore.residentialLow[level], ground, noise);
                            break;

                        case "ResidentialHigh":
                            SetPollutionRates(DataStore.residentialHigh[level], ground, noise);
                            break;

                        case "CommercialLow":
                            SetPollutionRates(DataStore.commercialLow[level], ground, noise);
                            break;

                        case "CommercialHigh":
                            SetPollutionRates(DataStore.commercialHigh[level], ground, noise);
                            break;

                        case "CommercialTourist":
                            SetPollutionRates(DataStore.commercialTourist[level], ground, noise);
                            break;

                        case "CommercialLeisure":
                            SetPollutionRates(DataStore.commercialLeisure[level], ground, noise);
                            break;

                        case "Office":
                            SetPollutionRates(DataStore.office[level], ground, noise);
                            break;

                        case "Industry":
                            SetPollutionRates(DataStore.industry[level], ground, noise);
                            break;

                        case "IndustryOre":
                            SetPollutionRates(DataStore.industry_ore[level], ground, noise);
                            break;

                        case "IndustryOil":
                            SetPollutionRates(DataStore.industry_oil[level], ground, noise);
                            break;

                        case "IndustryForest":
                            SetPollutionRates(DataStore.industry_forest[level], ground, noise);
                            break;

                        case "IndustryFarm":
                            SetPollutionRates(DataStore.industry_farm[level], ground, noise);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readPollutionNode exception");
                }
            } // end foreach
        }

        private void ReadConsumptionNode(XmlNode consumeNode)
        {
            foreach (XmlNode node in consumeNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int power = Convert.ToInt32(node.Attributes["power"].InnerText);
                    int water = Convert.ToInt32(node.Attributes["water"].InnerText);
                    int sewage = Convert.ToInt32(node.Attributes["sewage"].InnerText);
                    int garbage = Convert.ToInt32(node.Attributes["garbage"].InnerText);
                    int wealth = Convert.ToInt32(node.Attributes["wealth"].InnerText);
                    int[] array = GetArray(name, level, "readConsumptionNode");

                    SetConsumptionRates(array, power, water, sewage, garbage, wealth);
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readConsumptionNode exception");
                }
            }
        }

        /// <param name="popNode"></param>
        private void ReadPopulationNode(XmlNode popNode)
        {
            try
            {
                DataStore.strictCapacity = Convert.ToBoolean(popNode.Attributes["strictCapacity"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (XmlNode node in popNode.ChildNodes)
            {
                {
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int[] array = new int[12];

                    try
                    {
                        array = GetArray(name, level, "readPopulationNode");
                        int temp = Convert.ToInt32(node.Attributes["level_height"].InnerText);
                        array[DataStore.LEVEL_HEIGHT] = temp > 0 ? temp : 10;

                        temp = Convert.ToInt32(node.Attributes["space_pp"].InnerText);
                        if (temp <= 0)
                        {
                            temp = 100;  // Bad person trying to give negative or div0 error. 
                        }
                        array[DataStore.PEOPLE] = TransformPopulationModifier(name, temp, false);

                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readPopulationNode exception");
                    }

                    try
                    {
                        if (Convert.ToBoolean(node.Attributes["calc"].InnerText.Equals("plot")))
                        {
                            array[DataStore.CALC_METHOD] = 1;
                        }
                        else
                        {
                            array[DataStore.CALC_METHOD] = 0;
                        }
                    }
                    catch
                    {

                    }

                    if (!name.StartsWith("Res"))
                    {
                        try
                        {
                            int dense = Convert.ToInt32(node.Attributes["ground_mult"].InnerText);
                            array[DataStore.DENSIFICATION] = dense >= 0 ? dense : 0;  // Force to be greater than 0

                            int level0 = Convert.ToInt32(node.Attributes["lvl_0"].InnerText);
                            int level1 = Convert.ToInt32(node.Attributes["lvl_1"].InnerText);
                            int level2 = Convert.ToInt32(node.Attributes["lvl_2"].InnerText);
                            int level3 = Convert.ToInt32(node.Attributes["lvl_3"].InnerText);

                            // Ensure all is there first
                            array[DataStore.WORK_LVL0] = level0;
                            array[DataStore.WORK_LVL1] = level1;
                            array[DataStore.WORK_LVL2] = level2;
                            array[DataStore.WORK_LVL3] = level3;
                        }
                        catch (Exception e)
                        {
                            Logging.LogException(e, "readPopulationNode, part b exception");
                        }
                    }
                } // end if
            } // end foreach
        }

        private int TransformPopulationModifier(string name, int value, bool toXML)
        {
            int dividor = 1;

            switch (name)
            {
                case "ResidentialLow":
                case "ResidentialHigh":
                case "ResEcoLow":
                case "ResEcoHigh":
                    dividor = 5;   // 5 people
                    break;
            }

            if (toXML)
            {
                return (value / dividor);
            }
            else
            {
                return (value * dividor);
            }
        }

        private void ReadOverrideHouseNode(XmlNode parent)
        {
            try
            {
                DataStore.printResidentialNames = Convert.ToBoolean(parent.Attributes["printResNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            try
            {
                DataStore.mergeResidentialNames = Convert.ToBoolean(parent.Attributes["mergeResNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (XmlNode node in parent.ChildNodes)
            {
                string name = node.InnerText;
                int overrideValue;
                if (node.Name.Equals(meshName) && (name.Length > 0))
                {
                    try
                    {
                        overrideValue = Convert.ToInt32(node.Attributes["value"].InnerText);
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readOverrideHouseNode exception...setting to 1");
                        overrideValue = 1;
                    }

                    try
                    {
                        // Don't do anything if this key is already in the dictionary.
                        if (!DataStore.householdCache.ContainsKey(name))
                        {
                            DataStore.householdCache.Add(name, overrideValue);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "ReadOverrideHouseNode exception");
                    }
                }
            }
        }

        private void ReadOverrideWorkers(XmlNode parent)
        {
            try
            {
                DataStore.printEmploymentNames = Convert.ToBoolean(parent.Attributes["printWorkNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            try
            {
                DataStore.mergeEmploymentNames = Convert.ToBoolean(parent.Attributes["mergeWorkNames"].InnerText);
            }
            catch (Exception)
            {
                // Do nothing
            }

            foreach (XmlNode node in parent.ChildNodes)
            {
                string name = node.InnerText;
                int overrideValue;
                if (node.Name.Equals(meshName) && (name.Length > 0))
                {
                    try
                    {
                        overrideValue = Convert.ToInt32(node.Attributes["value"].InnerText);
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readOverrideWorkers exception...setting to 5");
                        overrideValue = 5;
                    }

                    try
                    {
                        // Don't do anything if this key is already in the dictionary.
                        if (!DataStore.workerCache.ContainsKey(name))
                        {
                            DataStore.workerCache.Add(name, overrideValue);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "ReadOverrideWorkers exception");
                    }
                }
            }
        }

        private void ReadBonusHouseNode(XmlNode parent)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int BonusValue = 1;
                        BonusValue = Convert.ToInt32(node.Attributes["value"].InnerText);

                        // Needs a value to be valid
                        if (name.Length > 0)
                        {
                            // Don't do anything if this key is already in the dictionary.
                            if (!DataStore.bonusHouseholdCache.ContainsKey(name))
                            {
                                DataStore.bonusHouseholdCache.Add(name, BonusValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "ReadBonusHouseNode exception...setting to 1");
                    }
                }
            }
        }

        private void ReadBonusWorkers(XmlNode parent)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int BonusValue = 5;
                        BonusValue = Convert.ToInt32(node.Attributes["value"].InnerText);

                        // Needs a value to be valid
                        if (name.Length > 0)
                        {
                            // Don't do anything if this key is already in the dictionary.
                            if (!DataStore.bonusWorkerCache.ContainsKey(name))
                            {
                                DataStore.bonusWorkerCache.Add(name, BonusValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "ReadBonusWorkers exception...setting to 5");
                    }
                }
            }
        }

        private void ReadPrintHouseNode(XmlNode parent)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int PrintValue = 1;
                        PrintValue = Convert.ToInt32(node.Attributes["value"].InnerText);

                        // Needs a value to be valid
                        if (name.Length > 0)
                        {
                            // Don't do anything if this key is already in the dictionary.
                            if (!DataStore.housePrintOutCache.ContainsKey(name))
                            {
                                DataStore.housePrintOutCache.Add(name, PrintValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "ReadPrintHouseNode exception...setting to 1");
                    }
                }
            }
        }

        private void ReadPrintWorkers(XmlNode parent)
        {
            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int PrintValue = 5;
                        PrintValue = Convert.ToInt32(node.Attributes["value"].InnerText);

                        // Needs a value to be valid
                        if (name.Length > 0)
                        {
                            // Don't do anything if this key is already in the dictionary.
                            if (!DataStore.workerPrintOutCache.ContainsKey(name))
                            {
                                DataStore.workerPrintOutCache.Add(name, PrintValue);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "ReadPrintWorkers exception...setting to 5");
                    }
                }
            }
        }

        /// <param name="produceNode"></param>
        private void ReadVisitNode(XmlNode produceNode)
        {
            foreach (XmlNode node in produceNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int[] array = GetArray(name, level, "readVisitNode");

                    array[DataStore.VISIT] = Convert.ToInt32(node.Attributes["visit"].InnerText);
                    if (array[DataStore.VISIT] <= 0)
                    {
                        array[DataStore.VISIT] = 1;
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readVisitNode exception");
                }
            }
        }


        /// <param name="produceNode"></param>
        private void ReadProductionNode(XmlNode produceNode)
        {
            foreach (XmlNode node in produceNode.ChildNodes)
            {
                try
                {
                    // Extract power, water, sewage, garbage and wealth
                    string[] attr = node.Name.Split(new char[] { '_' });
                    string name = attr[0];
                    int level = Convert.ToInt32(attr[1]) - 1;
                    int[] array = GetArray(name, level, "readProductionNode");

                    array[DataStore.PRODUCTION] = Convert.ToInt32(node.Attributes["production"].InnerText);
                    if (array[DataStore.PRODUCTION] <= 0)
                    {
                        array[DataStore.PRODUCTION] = 1;
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "readProductionNode exception");
                }
            }
        }

        private static int[] GetArray(string name, int level, string callingFunction)
        {
            int[] array = new int[14];

            switch (name)
            {
                case "ResidentialLow":
                    array = DataStore.residentialLow[level];
                    break;

                case "ResidentialHigh":
                    array = DataStore.residentialHigh[level];
                    break;

                case "ResEcoLow":
                    array = DataStore.resEcoLow[level];
                    break;

                case "ResEcoHigh":
                    array = DataStore.resEcoHigh[level];
                    break;

                case "CommercialLow":
                    array = DataStore.commercialLow[level];
                    break;

                case "CommercialHigh":
                    array = DataStore.commercialHigh[level];
                    break;

                case "CommercialEco":
                    array = DataStore.commercialEco[level];
                    break;

                case "CommercialTourist":
                    array = DataStore.commercialTourist[level];
                    break;

                case "CommercialLeisure":
                    array = DataStore.commercialLeisure[level];
                    break;

                case "Office":
                    array = DataStore.office[level];
                    break;

                case "OfficeHighTech":
                    array = DataStore.officeHighTech[level];
                    break;

                case "Industry":
                    array = DataStore.industry[level];
                    break;

                case "IndustryOre":
                    array = DataStore.industry_ore[level];
                    break;

                case "IndustryOil":
                    array = DataStore.industry_oil[level];
                    break;

                case "IndustryForest":
                    array = DataStore.industry_forest[level];
                    break;

                case "IndustryFarm":
                    array = DataStore.industry_farm[level];
                    break;

                default:
                    Logging.Error("callingFunction ", callingFunction, ". unknown element name: ", name);
                    break;
            }
            return array;
        } // end getArray

        private void SetConsumptionRates(int[] p, int power, int water, int sewage, int garbage, int wealth)
        {
            p[DataStore.POWER] = power;
            p[DataStore.WATER] = water;
            p[DataStore.SEWAGE] = sewage;
            p[DataStore.GARBAGE] = garbage;
            p[DataStore.INCOME] = wealth;
        }

        private void SetPollutionRates(int[] p, int ground, int noise)
        {
            p[DataStore.GROUND_POLLUTION] = ground;
            p[DataStore.NOISE_POLLUTION] = noise;
        }
    }
}