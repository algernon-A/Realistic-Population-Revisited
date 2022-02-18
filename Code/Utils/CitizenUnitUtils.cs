using System;
using System.Runtime.CompilerServices;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Utility class for dealing with CitizenUnits.
    /// </summary>
    [HarmonyPatch]
    public static class CitizenUnitUtils
    {
        /// <summary>
        /// CitizenUnit removal flag enum.
        /// </summary>
        private enum RemovingType
        {
            NotRemoving = 0,
            Household = 1,
            Workplace = 2,
            Visitplace = 3
        }


        /// <summary>
        /// Reverse patch for ResidentAI.FinishSchoolOrWork to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this citizen (for game method)</param>
        /// <param name="data">Citizen data (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(BuildingAI)), "EnsureCitizenUnits")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void EnsureCitizenUnits(object instance, ushort buildingID, ref Building data, int homeCount, int workCount, int visitCount, int studentCount)
        {
            string message = "EnsureCitizenUnits reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, buildingID.ToString(), data, homeCount, workCount, visitCount, studentCount);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Reverse patch for ResidentAI.FinishSchoolOrWork to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this citizen (for game method)</param>
        /// <param name="data">Citizen data (for game method)</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(CitizenManager)), "ReleaseUnitImplementation")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReleaseUnitImplementation(object instance, uint unit, ref CitizenUnit data)
        {
            string message = "ReleaseUnitImplementation reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, unit, data);
            throw new NotImplementedException(message);
        }


        /// <summary>
        /// Updates the CitizenUnits of already existing (placed/grown) building instances of the specified prefab, or all buildings of the specified service or subservice if prefab name is null.
        /// Called after updating a prefab's household/worker/visitor count, or when applying new default calculations, in order to apply changes to existing buildings.
        /// </summary>
        /// <param name="prefabName">The (raw BuildingInfo) name of the prefab (null to ignore name match)</param>
        /// <param name="service">The service to apply to (ignored if 'none')</param>
        /// <param name="subService">The subservice to apply to (ignored if 'none')</param>
        /// <param name="preserveOccupied">Set to true to preserve occupied residential households from removal, false otherwise</param>
        internal static void UpdateCitizenUnits(string prefabName, ItemClass.Service service, ItemClass.SubService subService, bool preserveOccupied)
        {
            // Local references.
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            Building[] buildingBuffer = Singleton<BuildingManager>.instance?.m_buildings?.m_buffer;

            // Don't do anything if we couldn't get the building buffer or if we're not in-game.
            if (buildingBuffer == null || Singleton<ToolManager>.instance?.m_properties?.m_mode != ItemClass.Availability.Game)
            {
                return;
            }

            // Iterate through each building in the scene.
            for (ushort i = 0; i < buildingBuffer.Length; i++)
            {
                // Get current building instance.
                Building thisBuilding = buildingBuffer[i];

                // Only interested in buildings with private AI.
                if (thisBuilding.Info?.GetAI() is PrivateBuildingAI privateAI)
                {
                    // Residential building; check that either the supplier prefab name is null or it matches this building's prefab.
                    if ((prefabName == null || thisBuilding.Info.name.Equals(prefabName)) && ((service != ItemClass.Service.None && thisBuilding.Info.GetService() == service) || (subService != ItemClass.SubService.None && thisBuilding.Info.GetSubService() == subService)))
                    {
                        // Got one!  Log initial status.
                        Logging.Message("Identified building ", i, " (", thisBuilding.Info.name, ") with ", CountCitizenUnits(ref thisBuilding), " CitizenUnits");

                        // Recalculate home and visit counts.
                        privateAI.CalculateWorkplaceCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length, out int level0, out int level1, out int level2, out int level3);
                        int workCount = level0 + level1 + level2 + level3;
                        int homeCount = privateAI.CalculateHomeCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length);
                        int visitCount = privateAI.CalculateVisitplaceCount((ItemClass.Level)thisBuilding.m_level, new Randomizer(i), thisBuilding.Width, thisBuilding.Length);

                        // Local references for passing to SimulationManager.
                        PrivateBuildingAI thisAI = privateAI;
                        ushort buildingID = i;
                        bool localPreserve = preserveOccupied;

                        // Apply changes via call to EnsureCitizenUnits reverse patch in SimulationManager.
                        Singleton<SimulationManager>.instance.AddAction(delegate { EnsureCitizenUnits(thisAI, buildingID, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], homeCount, workCount, visitCount, 0); });

                        // Remove any extra CitizenUnits in SimulationManager.
                        Singleton<SimulationManager>.instance.AddAction(delegate { RemoveCitizenUnits(ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[buildingID], homeCount, workCount, visitCount, 0, localPreserve); });

                        // Log changes.
                        Logging.Message("Reset CitizenUnits for building ", i, " (", thisBuilding.Info.name, "); building now has ", CountCitizenUnits(ref thisBuilding), " CitizenUnits, and total CitizenUnit count is now ", citizenManager.m_unitCount);
                    }
                }
            }

            Logging.Message("CitizenUnit count is now ", citizenManager.m_unitCount);
        }


        /// <summary>
        /// Removes CitizenUnits that are surplus to requirements from the specified building.
        /// </summary>
        /// <param name="building">Building reference</param>
        /// <param name="homeCount">Number of households to apply</param>
        /// <param name="workCount">Number of workplaces to apply</param>
        /// <param name="visitCount">Number of visitplaces to apply</param>
        /// <param name="studentCount">Number of student places to apply</param>
        /// <param name="preserveOccupied">Preserve occupied residential households</param>
        internal static void RemoveCitizenUnits(ref Building building, int homeCount, int workCount, int visitCount, int studentCount, bool preserveOccupied)
        {

            // Local references.
            CitizenManager citizenManager = Singleton<CitizenManager>.instance;
            CitizenUnit[] citizenUnits = citizenManager.m_units.m_buffer;
            Citizen[] citizens = citizenManager.m_citizens.m_buffer;


            uint previousUnit = building.m_citizenUnits;
            uint currentUnit = building.m_citizenUnits;

            // Keep looping through all CitizenUnits in this building until the end.
            while (currentUnit != 0)
            {
                // Get reference to next unit and flags of this unit.
                CitizenUnit.Flags unitFlags = citizenUnits[currentUnit].m_flags;
                uint nextUnit = citizenUnits[currentUnit].m_nextUnit;

                // Status flag.
                int removingFlag = (int)RemovingType.NotRemoving;

                // Is this a residential unit?
                if ((ushort)(unitFlags & CitizenUnit.Flags.Home) != 0)
                {
                    // Residential unit; are we still allocating homes, and if we're preserving occupied units, is it empty?
                    if (homeCount <= 0 && (!preserveOccupied || (citizenUnits[currentUnit].m_citizen0 + citizenUnits[currentUnit].m_citizen1 + citizenUnits[currentUnit].m_citizen2 + citizenUnits[currentUnit].m_citizen3 + citizenUnits[currentUnit].m_citizen4 == 0)))
                    {
                        // Already have the maximum number of households, and iw.
                        removingFlag = (int)RemovingType.Household;
                    }
                    else
                    {
                        // Still allocating - reduce unallocated homeCount by 1.
                        --homeCount;
                    }
                }
                // Is this a workplace unit?
                if ((ushort)(unitFlags & CitizenUnit.Flags.Work) != 0)
                {
                    // Workplace unit; are we still allocating to workplaces?
                    if (workCount <= 0)
                    {
                        // Not allocating any more, therefore this workplace unit is surplus to requirements - remove it.
                        removingFlag = (int)RemovingType.Workplace;
                    }
                    else
                    {
                        // Still allocating - reduce unallocated workCount by 5.
                        workCount -= 5;
                    }
                }
                if ((ushort)(unitFlags & CitizenUnit.Flags.Visit) != 0)
                {
                    // VisitPlace unit; are we still allocating to visitCount?
                    if (visitCount <= 0)
                    {
                        // Not allocating any more, therefore this workplace unit is surplus to requirements - remove it.
                        removingFlag = (int)RemovingType.Visitplace;
                    }
                    else
                    {
                        // Still allocating - reduce unallocated visitCount by 5.
                        visitCount -= 5;
                    }
                }
                if ((ushort)(unitFlags & CitizenUnit.Flags.Student) != 0)
                {
                    // Student unit; are we still allocating to students?
                    if (studentCount <= 0)
                    {
                        // Not allocating any more, therefore this workplace unit is surplus to requirements - remove it.
                        // Student buildings are set as workplace.
                        removingFlag = (int)RemovingType.Workplace;
                    }
                    else
                    {
                        // Still allocating - reduce unallocated visitCount by 5.
                        studentCount -= 5;
                    }
                }

                // Are we removing this unit?
                if (removingFlag != (int)RemovingType.NotRemoving)
                {
                    // Yes - remove any occupying citizens.
                    for (int i = 0; i < 5; ++i)
                    {
                        // Remove relevant citizen unit reference from citizen.
                        uint citizen = citizenUnits[currentUnit].GetCitizen(i);
                        switch (removingFlag)
                        {
                            case (int)RemovingType.Household:
                                citizens[citizen].m_homeBuilding = 0;
                                break;
                            case (int)RemovingType.Workplace:
                                citizens[citizen].m_workBuilding = 0;
                                break;
                            case (int)RemovingType.Visitplace:
                                citizens[citizen].m_visitBuilding = 0;
                                break;
                        }
                    }

                    // Unlink this unit from building CitizenUnit list.
                    citizenUnits[previousUnit].m_nextUnit = nextUnit;

                    // Release unit.
                    citizenUnits[currentUnit] = default(CitizenUnit);
                    ReleaseUnitImplementation(citizenManager, currentUnit, ref citizenUnits[currentUnit]);
                    citizenManager.m_unitCount = (int)(citizenManager.m_units.ItemCount() - 1);
                }
                else
                {
                    // Not removing - therefore previous unit reference needs to be updated.
                    previousUnit = currentUnit;
                }


                // Move on to next unit.
                currentUnit = nextUnit;
            }
        }


        /// <summary>
        /// Counts the number of CitizenUnits attached to the given building.
        /// </summary>
        /// <param name="building"></param>
        /// <returns></returns>
        private static uint CountCitizenUnits(ref Building building)
        {
            uint unitCount = 0;

            // Local reference.
            CitizenUnit[] citizenUnts = Singleton<CitizenManager>.instance.m_units.m_buffer;

            // Follow m_nextUnit chain of linked CitizenUnits.
            uint currentUnit = building.m_citizenUnits;
            while (currentUnit != 0)
            {
                ++unitCount;
                currentUnit = citizenUnts[currentUnit].m_nextUnit;
            }

            return unitCount;
        }
    }
}
