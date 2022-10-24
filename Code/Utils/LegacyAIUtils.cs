// <copyright file="LegacyAIUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard) and Whitefang Greytail. All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Utilities for working with legacy AI.
    /// </summary>
    internal class LegacyAIUtils
    {
        /// <summary>
        /// Calculate workers for this prefab.
        /// </summary>
        /// <param name="width">Prefab lot width.</param>
        /// <param name="length">Prefab lot length.</param>
        /// <param name="item">Buiding prefab.</param>
        /// <param name="minWorkers">Minimum number of workers to allocate.</param>
        /// <param name="array">Workplace level array.</param>
        /// <returns>Workplace levels struct with calculated workers by education level.</returns>
        internal static PopData.WorkplaceLevels CalculatePrefabWorkers(int width, int length, ref BuildingInfo item, int minWorkers, ref int[] array)
        {
            // Prefabs are tied to a level
            int value;
            int num = array[DataStore.PEOPLE];
            int level0 = array[DataStore.WORK_LVL0];
            int level1 = array[DataStore.WORK_LVL1];
            int level2 = array[DataStore.WORK_LVL2];
            int level3 = array[DataStore.WORK_LVL3];
            int num2 = level0 + level1 + level2 + level3;

            // Output.
            PopData.WorkplaceLevels output = new PopData.WorkplaceLevels
            {
                Level0 = 1,
                Level1 = 0,
                Level2 = 0,
                Level3 = 0,
            };

            if (num > 0 && num2 > 0)
            {
                // First, check for volumetric population override - that trumps everything else.
                value = PopData.Instance.GetOverride(item.name);
                if (value == 0)
                {
                    // No volumetric override - use legacy approach.
                    Vector2 v = item.m_size;
                    int floorSpace = CalcBase(width, length, ref array, v);
                    int floorCount = Mathf.Max(1, Mathf.FloorToInt(v.y / array[DataStore.LEVEL_HEIGHT])) + array[DataStore.DENSIFICATION];
                    value = (floorSpace * floorCount) / array[DataStore.PEOPLE];

                    // Plot only will ignore any over ride or bonus
                    if (array[DataStore.CALC_METHOD] == 0)
                    {
                        // Check over ride
                        string name = item.gameObject.name;
                        if (DataStore.workerCache.TryGetValue(name, out int outValue))
                        {
                            value = outValue;
                        }
                        else if (DataStore.bonusWorkerCache.TryGetValue(name, out outValue))
                        {
                            value += outValue;
                            DataStore.workerCache.Add(name, value);
                            DataStore.bonusWorkerCache.Remove(name);
                        }
                        else if (DataStore.printEmploymentNames)
                        {
                            try
                            {
                                DataStore.workerPrintOutCache.Add(item.gameObject.name, value);
                            }
                            catch (ArgumentException)
                            {
                                // Don't care
                            }
                        }
                    }
                }

                num = Mathf.Max(minWorkers, value);

                output.Level1 = (ushort)((num * level1) / num2);
                output.Level2 = (ushort)((num * level2) / num2);
                output.Level3 = (ushort)((num * level3) / num2);

                output.Level0 = (ushort)Mathf.Max(0, num - output.Level1 - output.Level2 - output.Level3);  // Whatever is left
            }

            return output;
        } // end calculateprefabWorkerVisit

        /// <summary>
        /// Calculate households for this prefab.
        /// </summary>
        /// <param name="width">Prefab lot width.</param>
        /// <param name="length">Prefab lot length.</param>
        /// <param name="item">Buiding prefab.</param>
        /// <param name="array">Workplace level array.</param>
        /// <returns>Calculated houshold count.</returns>
        internal static ushort CalculatePrefabHousehold(int width, int length, ref BuildingInfo item, ref int[] array)
        {
            Vector2 v = item.m_size;
            int floorCount = Mathf.Max(1, Mathf.FloorToInt(v.y / array[DataStore.LEVEL_HEIGHT]));
            int returnValue = (CalcBase(width, length, ref array, v) * floorCount) / array[DataStore.PEOPLE];

            if ((item.m_class.m_subService == ItemClass.SubService.ResidentialHigh) || (item.m_class.m_subService == ItemClass.SubService.ResidentialHighEco))
            {
                // Minimum of 2, or ceiling of 90% number of floors, which ever is greater. This helps the 1x1 high density
                returnValue = Mathf.Max(Mathf.Max(2, Mathf.CeilToInt(0.9f * floorCount)), returnValue);
            }
            else
            {
                returnValue = Mathf.Max(1, returnValue);
            }

            // Plot only will ignore any over ride or bonus
            if (array[DataStore.CALC_METHOD] == 0)
            {
                // Check over ride
                string name = item.gameObject.name;
                if (DataStore.householdCache.TryGetValue(name, out int outValue))
                {
                    returnValue = outValue;
                }
                else if (DataStore.bonusHouseholdCache.TryGetValue(name, out outValue))
                {
                    returnValue += outValue;
                    DataStore.householdCache.Add(name, returnValue);
                    DataStore.bonusHouseholdCache.Remove(name);
                }
                else if (DataStore.printResidentialNames)
                {
                    try
                    {
                        DataStore.housePrintOutCache.Add(item.gameObject.name, returnValue);
                    }
                    catch (ArgumentException)
                    {
                        // Don't care
                    }
                }
            }

            return (ushort)returnValue;
        } // end calculatePrefabHousehold

        /// <summary>
        /// Gets the land value compinent of income for this building.
        /// </summary>
        /// <param name="seed">Random seed.</param>
        /// <returns>Land value income component.</returns>
        internal static int GetLandValueIncomeComponent(ulong seed)
        {
            int landValue = 0;

            if (DataStore.seedToId.TryGetValue(seed, out ushort buildingID))
            {
                Building buildingData = ColossalFramework.Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID];
                ColossalFramework.Singleton<ImmaterialResourceManager>.instance.CheckLocalResource(ImmaterialResourceManager.Resource.LandValue, buildingData.m_position, out landValue);
            }

            return landValue;
        }

        /// <summary>
        /// Returns the datastore array for residential buildings.
        /// </summary>
        /// <param name="item">Building prefab.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Datastore array.</returns>
        internal static int[] GetResidentialArray(BuildingInfo item, int level)
        {
            // Default is high-density residential.
            int[][] array = DataStore.residentialHigh;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.ResidentialLow:
                        array = DataStore.residentialLow;
                        break;

                    case ItemClass.SubService.ResidentialHighEco:
                        array = DataStore.resEcoHigh;
                        break;

                    case ItemClass.SubService.ResidentialLowEco:
                        array = DataStore.resEcoLow;
                        break;

                    case ItemClass.SubService.ResidentialWallToWall:
                        array = DataStore.residentialW2W;
                        break;

                    default:
                    case ItemClass.SubService.ResidentialHigh:
                        break;
                }

                return array[level];
            }
            catch
            {
                return array[0];
            }
        }

        /// <summary>
        /// Returns the datastore array for industry buildings.
        /// </summary>
        /// <param name="item">Building prefab.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Datastore array.</returns>
        internal static int[] GetIndustryArray(BuildingInfo item, int level)
        {
            // Default is generic industry.
            int tempLevel;
            int[][] array = DataStore.industry;

            try
            {
                // Adding 1 to specialized industry to capture correct processor level.
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.IndustrialOre:
                        array = DataStore.industry_ore;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialForestry:
                        array = DataStore.industry_forest;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialFarming:
                        array = DataStore.industry_farm;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialOil:
                        array = DataStore.industry_oil;
                        tempLevel = level + 1;
                        break;

                    case ItemClass.SubService.IndustrialGeneric: // Deliberate fall through
                    default:
                        tempLevel = level;
                        break;
                }

                return array[tempLevel];
            }
            catch
            {
                return array[0];
            }
        }

        /// <summary>
        /// Returns the datastore array for extractor buildings.
        /// </summary>
        /// <param name="item">Building prefab.</param>
        /// <returns>Datastore array.</returns>
        internal static int[] GetExtractorArray(BuildingInfo item)
        {
            // Default is extraction.
            int[][] array = DataStore.industry;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.IndustrialOre:
                        array = DataStore.industry_ore;
                        break;

                    case ItemClass.SubService.IndustrialForestry:
                        array = DataStore.industry_forest;
                        break;

                    case ItemClass.SubService.IndustrialFarming:
                        array = DataStore.industry_farm;
                        break;

                    case ItemClass.SubService.IndustrialOil:
                        array = DataStore.industry_oil;
                        break;

                    case ItemClass.SubService.IndustrialGeneric: // Deliberate fall through
                    default:
                        break;
                }

                // Extracting is always level 1 (To make it easier to code)
                return array[0];
            }
            catch
            {
                return array[0];
            }
        }

        /// <summary>
        /// Returns the datastore array for commercial buildings.
        /// </summary>
        /// <param name="item">Building prefab.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Datastore array.</returns>
        internal static int[] GetCommercialArray(BuildingInfo item, int level)
        {
            // Default is high-density commercial.
            int[][] array = DataStore.commercialHigh;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLow:
                        array = DataStore.commercialLow;
                        break;

                    case ItemClass.SubService.CommercialWallToWall:
                        array = DataStore.commercialW2W;
                        break;

                    case ItemClass.SubService.CommercialLeisure:
                        array = DataStore.commercialLeisure;
                        break;

                    case ItemClass.SubService.CommercialTourist:
                        array = DataStore.commercialTourist;
                        break;

                    case ItemClass.SubService.CommercialEco:
                        array = DataStore.commercialEco;
                        break;

                    default:
                    case ItemClass.SubService.CommercialHigh:
                        break;
                }

                return array[level];
            }
            catch (System.Exception)
            {
                return array[0];
            }
        }

        /// <summary>
        /// Returns the datastore array for office buildings.
        /// </summary>
        /// <param name="item">Building prefab.</param>
        /// <param name="level">Building level.</param>
        /// <returns>Datastore array.</returns>
        internal static int[] GetOfficeArray(BuildingInfo item, int level)
        {
            // Default is generic office.
            int[][] array = DataStore.office;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.OfficeHightech:
                        array = DataStore.officeHighTech;
                        break;

                    case ItemClass.SubService.OfficeWallToWall:
                        array = DataStore.officeW2W;
                        break;

                    case ItemClass.SubService.OfficeGeneric:
                    default:
                        break;
                }

                return array[level];
            }
            catch
            {
                return array[0];
            }
        }

        /// <summary>
        /// Calculates a building's base area in squre metres.
        /// </summary>
        /// <param name="width">Building lot width.</param>
        /// <param name="length">Buiding lot length.</param>
        /// <param name="array">Building data array.</param>
        /// <param name="v">Building prefab size.</param>
        private static int CalcBase(int width, int length, ref int[] array, Vector3 v)
        {
            if (array[DataStore.CALC_METHOD] == 0)
            {
                // Check x and z just incase they are 0. A few user created assets are.
                // If they are, then base the calculation of 3/4 of the width and length given
                if (v.x <= 1)
                {
                    width *= 6;
                }
                else
                {
                    width = (int)v.x;
                }

                if (v.z <= 1)
                {
                    length *= 6;
                }
                else
                {
                    length = (int)v.z;
                }
            }
            else
            {
                width *= 64; // Combine the eights
            }

            return width * length;
        }
    }
}
