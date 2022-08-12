// <copyright file="XML_VersionFive.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RealPop2
{
    using System;
    using System.Xml;
    using AlgernonCommons;

    /// <summary>
    /// Legacy WG configuration file, version 5.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1303:Const field names should begin with upper-case letter", Justification = "Legacy code")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Legacy code")]
    public class XML_VersionFive : WG_XMLBaseVersion
    {
        private const string popNodeName = "population";
        private const string bonusHouseName = "bonusHouseHold";
        private const string bonusWorkName = "bonusWorker";
        private const string meshName = "meshName";
        private const string consumeNodeName = "consumption";
        private const string visitNodeName = "visitor";
        private const string pollutionNodeName = "pollution";
        private const string productionNodeName = "production";

        public override void ReadXML(XmlDocument doc)
        {
            XmlElement root = doc.DocumentElement;
            try
            {
                // DataStore.enableExperimental = Convert.ToBoolean(root.Attributes["experimental"].InnerText);
                // DataStore.timeBasedRealism = Convert.ToBoolean(root.Attributes["enableTimeVariation"].InnerText);
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
                    else if (node.Name.Equals(bonusHouseName))
                    {
                        ReadBonusHouseNode(node);
                    }
                    else if (node.Name.Equals(bonusWorkName))
                    {
                        ReadBonusWorkers(node);
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e, "XML readNodes exception");
                }
            }
        } // end readXML

        public override bool WriteXML(string fullPathFileName)
        {
            // Should not be called now
            return false;
        } // end writeXML

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
                // TODO - These two to be removed in Jan 2017
                if (node.Name.Equals(bonusHouseName))
                {
                    ReadBonusHouseNode(node);
                }
                else if (node.Name.Equals(bonusWorkName))
                {
                    ReadBonusWorkers(node);
                }
                else
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

                    if (!name.Contains("Residential"))
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
                    dividor = 5;   // 5 people
                    break;
            }

            if (toXML)
            {
                return value / dividor;
            }
            else
            {
                return value * dividor;
            }
        }

        private void ReadBonusHouseNode(XmlNode parent)
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
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int bonus = 1;
                        bonus = Convert.ToInt32(node.Attributes["bonus"].InnerText);

                        if (name.Length > 0)
                        {
                            // Needs a value to be valid
                            DataStore.bonusHouseholdCache.Add(name, bonus);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readBonusHouseNode exception...setting to 1");
                    }
                }
            }
        }

        private void ReadBonusWorkers(XmlNode parent)
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
                if (node.Name.Equals(meshName))
                {
                    try
                    {
                        string name = node.InnerText;
                        int bonus = 5;
                        bonus = Convert.ToInt32(node.Attributes["bonus"].InnerText);

                        if (name.Length > 0)
                        {
                            // Needs a value to be valid
                            DataStore.bonusWorkerCache.Add(name, bonus);
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "readBonusWorkers exception...setting to 5");
                    }
                }
            }
        }

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

        private int[] GetArray(string name, int level, string callingFunction)
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

                case "CommercialLow":
                    array = DataStore.commercialLow[level];
                    break;

                case "CommercialHigh":
                    array = DataStore.commercialHigh[level];
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
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member