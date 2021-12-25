using System;
using UnityEngine;


namespace RealPop2
{
    internal class LegacyAIUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="item"></param>
        /// <param name="minWorkers"></param>
        /// <param name="array"></param>
        /// <param name="output"></param>
        internal static WorkplaceLevels CalculatePrefabWorkers(int width, int length, ref BuildingInfo item, int minWorkers, ref int[] array)
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
            WorkplaceLevels output = new WorkplaceLevels
            {
                level0 = 1,
                level1 = 0,
                level2 = 0,
                level3 = 0
            };

            if (num > 0 && num2 > 0)
            {
                // First, check for volumetric population override - that trumps everything else.
                value = PopData.instance.GetOverride(item.name);
                if (value == 0)
                {
                    // No volumetric override - use legacy approach.
                    Vector3 v = item.m_size;
                    int floorSpace = CalcBase(width, length, ref array, v);
                    int floorCount = Mathf.Max(1, Mathf.FloorToInt(v.y / array[DataStore.LEVEL_HEIGHT])) + array[DataStore.DENSIFICATION];
                    value = (floorSpace * floorCount) / array[DataStore.PEOPLE];

                    if ((array[DataStore.CALC_METHOD] == 0)) // Plot only will ignore any over ride or bonus
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

                output.level1 = (ushort)((num * level1) / num2);
                output.level2 = (ushort)((num * level2) / num2);
                output.level3 = (ushort)((num * level3) / num2);

                output.level0 = (ushort)(Mathf.Max(0, num - output.level1 - output.level2 - output.level3));  // Whatever is left
            }

            return output;

            // Set the visitors here since we're calculating
            //if (num != 0)
            //{
            //value = Mathf.Max(200, width * length * array[DataStore.VISIT]) / 100;
            //}
            //output.visitors = value;
        } // end calculateprefabWorkerVisit


        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="item"></param>
        /// <param name="returnValue"></param>
        internal static ushort CalculatePrefabHousehold(int width, int length, ref BuildingInfo item, ref int[] array)
        {
            Vector3 v = item.m_size;
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

            if ((array[DataStore.CALC_METHOD] == 0)) // Plot only will ignore any over ride or bonus
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
        }  // end calculatePrefabHousehold


        /// <summary>
        /// 
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="length"></param>
        /// <param name="array"></param>
        /// <param name="v"></param>
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


        /// <summary>
        /// Returns the datastore array for residential buildings.
        /// </summary>
        /// <param name="item">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Datastore array</returns>
        public static int[] GetResidentialArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.residentialLow;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.ResidentialHighEco:
                        array = DataStore.resEcoHigh;
                        break;

                    case ItemClass.SubService.ResidentialLowEco:
                        array = DataStore.resEcoLow;
                        break;

                    case ItemClass.SubService.ResidentialHigh:
                        array = DataStore.residentialHigh;
                        break;

                    case ItemClass.SubService.ResidentialLow:
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
        /// Returns the datastore array for industry buildings.
        /// </summary>
        /// <param name="item">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Datastore array</returns>
        public static int[] GetIndustryArray(BuildingInfo item, int level)
        {
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

                    case ItemClass.SubService.IndustrialGeneric:  // Deliberate fall through
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
        /// <param name="item">Building prefab</param>
        /// <returns>Datastore array</returns>
        public static int[] GetExtractorArray(BuildingInfo item)
        {
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

                    case ItemClass.SubService.IndustrialGeneric:  // Deliberate fall through
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
        /// <param name="item">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Datastore array</returns>
        public static int[] GetCommercialArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.commercialLow;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.CommercialLeisure:
                        array = DataStore.commercialLeisure;
                        break;

                    case ItemClass.SubService.CommercialTourist:
                        array = DataStore.commercialTourist;
                        break;

                    case ItemClass.SubService.CommercialEco:
                        array = DataStore.commercialEco;
                        break;

                    case ItemClass.SubService.CommercialHigh:
                        array = DataStore.commercialHigh;
                        break;

                    case ItemClass.SubService.CommercialLow:
                    default:
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
        /// <param name="item">Building prefab</param>
        /// <param name="level">Building level</param>
        /// <returns>Datastore array</returns>
        public static int[] GetOfficeArray(BuildingInfo item, int level)
        {
            int[][] array = DataStore.office;

            try
            {
                switch (item.m_class.m_subService)
                {
                    case ItemClass.SubService.OfficeHightech:
                        array = DataStore.officeHighTech;
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
    }
}
