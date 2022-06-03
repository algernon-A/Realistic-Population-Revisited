using UnityEngine;
using ColossalFramework;
using ColossalFramework.Math;


namespace RealPop2
{
	/// <summary>
	/// Utilities class for dealing with CitizenUnits.
	/// </summary>
	internal static class UnitUtils
	{
		/// <summary>
		/// Copies CitizenUnits from a deserialized array to a new clean array.
		/// </summary>
		internal static void ResetUnits()
		{
			// Manager references.
			CitizenManager citizenManager = Singleton<CitizenManager>.instance;
			BuildingManager buildingManager = Singleton<BuildingManager>.instance;
			VehicleManager vehicleManager = Singleton<VehicleManager>.instance;

			// Store original units.
			Array32<CitizenUnit> originalUnits = Singleton<CitizenManager>.instance.m_units;
			if (originalUnits == null || originalUnits.m_buffer.Length == 0)
			{
				Logging.Error("no original units reference set; aborting copy");
				return;
			}

			// Reset array.
			uint bufferLength = (uint)originalUnits.m_buffer.Length;
			Logging.Message("creating new CitizenUnit array with length ", bufferLength);
			citizenManager.m_units = new Array32<CitizenUnit>(bufferLength);
			Logging.Message("finished creating array");

			// Array buffer references.
			Building[] buildingBuffer = buildingManager.m_buildings.m_buffer;
			Vehicle[] vehicleBuffer = vehicleManager.m_vehicles.m_buffer;
			CitizenUnit[] newUnitBuffer = citizenManager.m_units.m_buffer;
			CitizenUnit[] oldUnitBuffer = originalUnits.m_buffer;

			// Reset buildings.
			ResetBuildings(citizenManager, buildingManager, buildingBuffer);

			// Reset vehicles.
			ResetVehicles(citizenManager, vehicleManager, vehicleBuffer);

			// Iterate through each unit in the old buffer.
			Logging.Message("copying CitizenUnit data");
			for (uint i = 0; i < bufferLength; ++i)
			{
				// Local references.
				CitizenUnit oldUnit = oldUnitBuffer[i];

				// Count citizens in old unit.
				int citizenCount = 0;
				if (oldUnit.m_citizen0 != 0)
				{
					++citizenCount;
				}
				if (oldUnit.m_citizen1 != 0)
				{
					++citizenCount;
				}
				if (oldUnit.m_citizen2 != 0)
				{
					++citizenCount;
				}
				if (oldUnit.m_citizen3 != 0)
				{
					++citizenCount;
				}
				if (oldUnit.m_citizen4 != 0)
				{
					++citizenCount;
				}

				// Skip any units with no citizens.
				if (citizenCount == 0)
				{
					continue;
				}

				// Get unit building and vehicle IDs.
				ushort buildingID = oldUnit.m_building;
				ushort vehicleID = oldUnit.m_vehicle;

				if (buildingID > 0)
				{
					// Found building unit.
					Logging.Message("found old building unit ", i, " with building ", buildingID);

					// Skip non-existent, abandoned, or collapsed buildings.
					Building.Flags buildingFlags = buildingBuffer[buildingID].m_flags;
					if ((buildingFlags & Building.Flags.Created) == 0 || (buildingFlags & (Building.Flags.Abandoned | Building.Flags.Collapsed)) != 0)
					{
						continue;
					}

					// Attempt to copy unit from old record into building.
					Logging.Message("attempting to copy building unit ", i, " with citizen count of ", citizenCount);
					if (CopyUnit(buildingBuffer[buildingID].m_citizenUnits, ref oldUnit, newUnitBuffer))
					{
						// Succesful copy - clear old unit so we don't end up double-assigning citizens due to corrupted unit chains.
						oldUnitBuffer[i] = default(CitizenUnit);
					}
				}
				else if (vehicleID > 0)
				{
					// Found vehicle unit.
					Logging.Message("found old vehicle unit ", i, " with vehicle ", vehicleID);

					// Skip non-existent vehicles, or vehicles with null infos.
					Vehicle.Flags vehicleFlags = vehicleBuffer[vehicleID].m_flags;
					if ((vehicleFlags & Vehicle.Flags.Created) == 0 || vehicleBuffer[vehicleID].Info == null)
					{
						continue;
					}

					// Attempt to copy unit from old record into vehicle.
					Logging.Message("attempting to copy vehicle unit ", i, " with citizen count of ", citizenCount);
					if (CopyUnit(vehicleBuffer[vehicleID].m_citizenUnits, ref oldUnit, newUnitBuffer))
					{
						// Succesful copy - clear old unit so we don't end up double-assigning citizens due to corrupted unit chains.
						oldUnitBuffer[i] = default(CitizenUnit);
					}
				}
				else
				{
					// No building or vehicle assigned; clear this record and continue.
					oldUnitBuffer[i] = default(CitizenUnit);
					continue;
				}
			}

			// Reset citizen array too.
			CitizenUtils.ResetCitizens();

			// Calculate new residential population.
			uint population = 0;
			for (uint i = 0; i < bufferLength; ++i)
			{
				CitizenUnit.Flags flags = newUnitBuffer[i].m_flags;
				if ((flags & CitizenUnit.Flags.Created) != CitizenUnit.Flags.None && (flags & CitizenUnit.Flags.Home) != CitizenUnit.Flags.None)
				{
					if (newUnitBuffer[i].m_citizen0 != 0)
					{
						++population;
					}
					if (newUnitBuffer[i].m_citizen1 != 0)
					{
						++population;
					}
					if (newUnitBuffer[i].m_citizen2 != 0)
					{
						++population;
					}
					if (newUnitBuffer[i].m_citizen3 != 0)
					{
						++population;
					}
					if (newUnitBuffer[i].m_citizen4 != 0)
					{
						++population;
					}
				}
			}
			Logging.Message("current residential population is ", population);
		}


		/// <summary>
		/// Tries to copy a CitizenUnit into the given CitizenUnit chain.
		/// </summary>
		/// <param name="startUnit">Starting CitizenUnit of (new) chain</param>
		/// <param name="oldUnit">Old CitizenUnit to copy</param>
		/// <param name="newUnitBuffer">New CitizenUnit buffer to copy into</param>
		/// <returns>True if copying was succesful, false otherwise (if no empty unit in the chain was found)</returns>
		private static bool CopyUnit(uint startUnit, ref CitizenUnit oldUnit, CitizenUnit[] newUnitBuffer)
		{
			uint newUnitID = startUnit;

			while (newUnitID != 0)
			{
				// Bounds check.
				if (newUnitID > newUnitBuffer.Length)
				{
					Logging.Error("invalid unit id ", newUnitID);
					return false;
				}

				// If this unit doesn't have any citizens, it's a valid target; copy it over.
				if (newUnitBuffer[newUnitID].m_flags == oldUnit.m_flags && newUnitBuffer[newUnitID].m_citizen0 == 0 && newUnitBuffer[newUnitID].m_citizen1 == 0 && newUnitBuffer[newUnitID].m_citizen2 == 0 && newUnitBuffer[newUnitID].m_citizen3 == 0 && newUnitBuffer[newUnitID].m_citizen4 == 0)
				{
					newUnitBuffer[newUnitID].m_citizen0 = oldUnit.m_citizen0;
					newUnitBuffer[newUnitID].m_citizen1 = oldUnit.m_citizen1;
					newUnitBuffer[newUnitID].m_citizen2 = oldUnit.m_citizen2;
					newUnitBuffer[newUnitID].m_citizen3 = oldUnit.m_citizen3;
					newUnitBuffer[newUnitID].m_citizen4 = oldUnit.m_citizen4;
					newUnitBuffer[newUnitID].m_goods = oldUnit.m_goods;

					// Done here.
					Logging.Message("copied unit to new unit ", newUnitID);
					return true;
				}

				// No empty unit found; try next unit in chain.
				uint nextUnitID = newUnitBuffer[newUnitID].m_nextUnit;

				// If we've exhausted the chain, try to add a new unit to this building to accomodate the excess old one.
				if (nextUnitID == 0)
				{
					// Attempt unit creation.
					Singleton<CitizenManager>.instance.m_units.CreateItem(out uint newUnit, ref Singleton<SimulationManager>.instance.m_randomizer);
					if (newUnit != 0)
					{
						// Creation successful; set new unit flags to match the old one and add to chain.
						newUnitBuffer[newUnit].m_flags = oldUnit.m_flags;
						newUnitBuffer[newUnitID].m_nextUnit = newUnit;

						// Use this new unit in next pass.
						nextUnitID = newUnit;
					}
				}

				// Next unit to check.
				newUnitID = nextUnitID;
			}

			// If we got here, we didn't have space to copy the old unit over.
			Logging.Message("no empty unit to copy to");
			return false;
		}


		/// <summary>
		/// Resets the CitizenUnits for all buildings on the map.
		/// </summary>
		/// <param name="citizenManager">Citizen manager reference</param>
		/// <param name="buildingManager">Building manager reference</param>
		/// <param name="buildingBuffer">Building buffer reference</param>
		private static void ResetBuildings(CitizenManager citizenManager, BuildingManager buildingManager, Building[] buildingBuffer)
		{
			Logging.Message("resetting buildings");

			// Iterate through each building in map.
			for (uint buildingID = 0; buildingID < buildingBuffer.Length; ++buildingID)
			{
				// Skip buildings that haven't been created, that are abandoned or collapsed, or have null infos.
				BuildingInfo info = buildingBuffer[buildingID].Info;
				Building.Flags buildingFlags = buildingBuffer[buildingID].m_flags;
				if ((buildingFlags & Building.Flags.Created) == Building.Flags.None || (buildingFlags & (Building.Flags.Abandoned | Building.Flags.Collapsed)) != Building.Flags.None || info == null)
				{
					continue;
				}

				// Required household and citizen counts.
				int homeCount = 0, visitCount = 0, workCount = 0, passengerCount = 0, studentCount = 0;

				// Assign home, visit, work, and/or student counts based on building AI.
				switch (info.m_buildingAI)
				{
					case PrivateBuildingAI privateAI:
						homeCount = privateAI.CalculateHomeCount(info.GetClassLevel(), new Randomizer(buildingID), info.m_cellWidth, info.m_cellLength);
						visitCount = privateAI.CalculateVisitplaceCount(info.GetClassLevel(), new Randomizer(buildingID), info.m_cellWidth, info.m_cellLength);
						privateAI.CalculateWorkplaceCount(info.GetClassLevel(), new Randomizer(buildingID), info.m_cellWidth, info.m_cellLength, out int level0, out int level1, out int level2, out int level3);
						workCount = level0 + level1 + level2 + level3;
						break;

					case CargoStationAI cargoAI:
						workCount = cargoAI.m_workPlaceCount0 + cargoAI.m_workPlaceCount1 + cargoAI.m_workPlaceCount2 + cargoAI.m_workPlaceCount3;
						break;

					case CemeteryAI cemeteryAI:
						workCount = cemeteryAI.m_workPlaceCount0 + cemeteryAI.m_workPlaceCount1 + cemeteryAI.m_workPlaceCount2 + cemeteryAI.m_workPlaceCount3;
						visitCount = cemeteryAI.m_corpseCapacity;
						break;

					case ChildcareAI childcareAI:
						workCount = childcareAI.m_workPlaceCount0 + childcareAI.m_workPlaceCount1 + childcareAI.m_workPlaceCount2 + childcareAI.m_workPlaceCount3;
						visitCount = childcareAI.PatientCapacity;
						break;

					case DepotAI depotAI:
						workCount = depotAI.m_workPlaceCount0 + depotAI.m_workPlaceCount1 + depotAI.m_workPlaceCount2 + depotAI.m_workPlaceCount3;
						break;

					case DisasterResponseBuildingAI disasterAI:
						workCount = disasterAI.m_workPlaceCount0 + disasterAI.m_workPlaceCount1 + disasterAI.m_workPlaceCount2 + disasterAI.m_workPlaceCount3;
						break;

					case DoomsdayVaultAI doomsdayAI:
						workCount = doomsdayAI.m_workPlaceCount0 + doomsdayAI.m_workPlaceCount1 + doomsdayAI.m_workPlaceCount2 + doomsdayAI.m_workPlaceCount3;
						break;

					case EarthquakeSensorAI earthquakeSensorAI:
						workCount = earthquakeSensorAI.m_workPlaceCount0 + earthquakeSensorAI.m_workPlaceCount1 + earthquakeSensorAI.m_workPlaceCount2 + earthquakeSensorAI.m_workPlaceCount3;
						break;

					case EldercareAI eldercareAI:
						workCount = eldercareAI.m_workPlaceCount0 + eldercareAI.m_workPlaceCount1 + eldercareAI.m_workPlaceCount2 + eldercareAI.m_workPlaceCount3;
						visitCount = eldercareAI.PatientCapacity;
						break;

					case FireStationAI fireStationAI:
						workCount = fireStationAI.m_workPlaceCount0 + fireStationAI.m_workPlaceCount1 + fireStationAI.m_workPlaceCount2 + fireStationAI.m_workPlaceCount3;
						break;

					case FirewatchTowerAI fireTowerAI:
						workCount = fireTowerAI.m_workPlaceCount0 + fireTowerAI.m_workPlaceCount1 + fireTowerAI.m_workPlaceCount2 + fireTowerAI.m_workPlaceCount3;
						break;

					case FishFarmAI fishFarmAI:
						workCount = fishFarmAI.m_workPlaceCount0 + fishFarmAI.m_workPlaceCount1 + fishFarmAI.m_workPlaceCount2 + fishFarmAI.m_workPlaceCount3;
						break;

					case FishingHarborAI fishingHarborAI:
						workCount = fishingHarborAI.m_workPlaceCount0 + fishingHarborAI.m_workPlaceCount1 + fishingHarborAI.m_workPlaceCount2 + fishingHarborAI.m_workPlaceCount3;
						break;

					case HadronColliderAI hadronAI:
						workCount = hadronAI.m_workPlaceCount0 + hadronAI.m_workPlaceCount1 + hadronAI.m_workPlaceCount2 + hadronAI.m_workPlaceCount3;
						break;

					case HeatingPlantAI heatingAI:
						workCount = heatingAI.m_workPlaceCount0 + heatingAI.m_workPlaceCount1 + heatingAI.m_workPlaceCount2 + heatingAI.m_workPlaceCount3;
						break;

					case HelicopterDepotAI helicopterAI:
						workCount = helicopterAI.m_workPlaceCount0 + helicopterAI.m_workPlaceCount1 + helicopterAI.m_workPlaceCount2 + helicopterAI.m_workPlaceCount3;
						break;

					case HospitalAI hospitalAI:
						workCount = hospitalAI.m_workPlaceCount0 + hospitalAI.m_workPlaceCount1 + hospitalAI.m_workPlaceCount2 + hospitalAI.m_workPlaceCount3;
						visitCount = hospitalAI.PatientCapacity;
						break;

					case IndustryBuildingAI industryAI:
						workCount = industryAI.m_workPlaceCount0 + industryAI.m_workPlaceCount1 + industryAI.m_workPlaceCount2 + industryAI.m_workPlaceCount3;
						break;

					case LandfillSiteAI landfillAI:
						workCount = landfillAI.m_workPlaceCount0 + landfillAI.m_workPlaceCount1 + landfillAI.m_workPlaceCount2 + landfillAI.m_workPlaceCount3;
						break;

					case LibraryAI libraryAI:
						workCount = libraryAI.m_workPlaceCount0 + libraryAI.m_workPlaceCount1 + libraryAI.m_workPlaceCount2 + libraryAI.m_workPlaceCount3;
						// 5-4 ratio is from game code.
						visitCount = libraryAI.VisitorCount * 5 / 4;
						break;

					case MainCampusBuildingAI campusBuildingAI:
						workCount = campusBuildingAI.m_workPlaceCount0 + campusBuildingAI.m_workPlaceCount1 + campusBuildingAI.m_workPlaceCount2 + campusBuildingAI.m_workPlaceCount3;
						visitCount = 100;
						break;

					case MainIndustryBuildingAI industryAI:
						workCount = industryAI.m_workPlaceCount0 + industryAI.m_workPlaceCount1 + industryAI.m_workPlaceCount2 + industryAI.m_workPlaceCount3;
						visitCount = 100;
						break;

					case MaintenanceDepotAI depotAI:
						workCount = depotAI.m_workPlaceCount0 + depotAI.m_workPlaceCount1 + depotAI.m_workPlaceCount2 + depotAI.m_workPlaceCount3;
						break;

					case MarketAI marketAI:
						workCount = marketAI.m_workPlaceCount0 + marketAI.m_workPlaceCount1 + marketAI.m_workPlaceCount2 + marketAI.m_workPlaceCount3;
						visitCount = marketAI.m_visitPlaceCount;
						break;

					case MonumentAI monumentAI:
						workCount = monumentAI.m_workPlaceCount0 + monumentAI.m_workPlaceCount1 + monumentAI.m_workPlaceCount2 + monumentAI.m_workPlaceCount3;
						// 5-4 ratio is from game code.
						visitCount = (monumentAI.m_visitPlaceCount0 + monumentAI.m_visitPlaceCount1 + monumentAI.m_visitPlaceCount2) * 5 / 4;
						break;

					case ParkAI parkAI:
						visitCount = parkAI.m_visitPlaceCount0 + parkAI.m_visitPlaceCount1 + parkAI.m_visitPlaceCount2;
						break;

					case ParkBuildingAI parkBuildingAI:
						// 3-2 ratio is from game code.
						visitCount = (parkBuildingAI.m_visitPlaceCount0 + parkBuildingAI.m_visitPlaceCount1 + parkBuildingAI.m_visitPlaceCount2) * 3 / 2;
						break;

					case ParkGateAI parkGateAI:
						visitCount = 100;
						break;

					case PoliceStationAI policeStationAI:
						workCount = policeStationAI.m_workPlaceCount0 + policeStationAI.m_workPlaceCount1 + policeStationAI.m_workPlaceCount2 + policeStationAI.m_workPlaceCount3;
						visitCount = policeStationAI.JailCapacity;
						break;

					case PostOfficeAI postOfficeAI:
						workCount = postOfficeAI.m_workPlaceCount0 + postOfficeAI.m_workPlaceCount1 + postOfficeAI.m_workPlaceCount2 + postOfficeAI.m_workPlaceCount3;
						break;

					case PowerPlantAI powerPlantAI:
						workCount = powerPlantAI.m_workPlaceCount0 + powerPlantAI.m_workPlaceCount1 + powerPlantAI.m_workPlaceCount2 + powerPlantAI.m_workPlaceCount3;
						break;

					case RadioMastAI radioMastAI:
						workCount = radioMastAI.m_workPlaceCount0 + radioMastAI.m_workPlaceCount1 + radioMastAI.m_workPlaceCount2 + radioMastAI.m_workPlaceCount3;
						break;

					case SaunaAI saunaAI:
						workCount = saunaAI.m_workPlaceCount0 + saunaAI.m_workPlaceCount1 + saunaAI.m_workPlaceCount2 + saunaAI.m_workPlaceCount3;
						// 5-4 ratio is from game code.
						visitCount = (saunaAI.m_visitPlaceCount0 + saunaAI.m_visitPlaceCount1 + saunaAI.m_visitPlaceCount2) * 5 / 4;
						break;

					case SchoolAI schoolAI:
						workCount = schoolAI.m_workPlaceCount0 + schoolAI.m_workPlaceCount1 + schoolAI.m_workPlaceCount2 + schoolAI.m_workPlaceCount3;
						studentCount = schoolAI.m_studentCount;
						break;

					case ShelterAI shelterAI:
						workCount = shelterAI.m_workPlaceCount0 + shelterAI.m_workPlaceCount1 + shelterAI.m_workPlaceCount2 + shelterAI.m_workPlaceCount3;
						// From game code - capacity is boosted by 30% if Doomsday Vault is active.
						bool extraCapacity = false;
						FastList<ushort> serviceBuildings = buildingManager.GetServiceBuildings(info.m_class.m_service);
						for (int i = 0; i < serviceBuildings.m_size; ++i)
						{
							ushort serviceBuilding = serviceBuildings.m_buffer[i];
							if (serviceBuilding != 0)
							{
								BuildingInfo buildingInfo = buildingManager.m_buildings.m_buffer[serviceBuilding].Info;
								if (buildingInfo.m_class.m_level == info.m_class.m_level && buildingInfo.m_buildingAI is DoomsdayVaultAI)
								{
									extraCapacity = true;
									break;
								}
							}
						}
						visitCount = (extraCapacity ? (shelterAI.m_capacity * 13 / 10) : shelterAI.m_capacity);
						break;

					case SnowDumpAI snowDumpAI:
						workCount = snowDumpAI.m_workPlaceCount0 + snowDumpAI.m_workPlaceCount1 + snowDumpAI.m_workPlaceCount2 + snowDumpAI.m_workPlaceCount3;
						break;

					case SpaceElevatorAI spaceElevatorAI:
						workCount = spaceElevatorAI.m_workPlaceCount0 + spaceElevatorAI.m_workPlaceCount1 + spaceElevatorAI.m_workPlaceCount2 + spaceElevatorAI.m_workPlaceCount3;
						break;

					case SpaceRadarAI spaceRadarAI:
						workCount = spaceRadarAI.m_workPlaceCount0 + spaceRadarAI.m_workPlaceCount1 + spaceRadarAI.m_workPlaceCount2 + spaceRadarAI.m_workPlaceCount3;
						break;

					case TourBuildingAI tourBuildingAI:
						// 5-4 ratio is from game code.
						visitCount = (tourBuildingAI.m_visitPlaceCount0 + tourBuildingAI.m_visitPlaceCount1 + tourBuildingAI.m_visitPlaceCount2) * 5 / 4;
						break;

					case WarehouseAI warehouseAI:
						workCount = warehouseAI.m_workPlaceCount0 + warehouseAI.m_workPlaceCount1 + warehouseAI.m_workPlaceCount2 + warehouseAI.m_workPlaceCount3;
						break;

					case WaterCleanerAI waterCleanerAI:
						workCount = waterCleanerAI.m_workPlaceCount0 + waterCleanerAI.m_workPlaceCount1 + waterCleanerAI.m_workPlaceCount2 + waterCleanerAI.m_workPlaceCount3;
						break;

					case WaterFacilityAI waterFacilityAI:
						workCount = waterFacilityAI.m_workPlaceCount0 + waterFacilityAI.m_workPlaceCount1 + waterFacilityAI.m_workPlaceCount2 + waterFacilityAI.m_workPlaceCount3;
						break;

					case WeatherRadarAI weatherRadarAI:
						workCount = weatherRadarAI.m_workPlaceCount0 + weatherRadarAI.m_workPlaceCount1 + weatherRadarAI.m_workPlaceCount2 + weatherRadarAI.m_workPlaceCount3;
						break;

					default:
						// If not explicitly covered above, skip to next building.
						continue;
				}

				// Create CitizenUnits for this building based on above figures.
				citizenManager.CreateUnits(out buildingBuffer[buildingID].m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, (ushort)buildingID, 0, homeCount, workCount, visitCount, passengerCount, studentCount);
			}

			Logging.Message("finished resetting buildings");
		}


		/// <summary>
		/// Resets the CitizenUnits for all vehicles on the map.
		/// </summary>
		/// <param name="citizenManager">Citizen manager reference</param>
		/// <param name="vehicleManager">Vehicle manager reference</param>
		/// <param name="vehicleBuffer">Vehicle buffer reference</param>
		private static void ResetVehicles(CitizenManager citizenManager, VehicleManager vehicleManager, Vehicle[] vehicleBuffer)
		{
			Logging.Message("resetting vehicles");

			// Iterate through each building in map.
			for (uint vehicleID = 0; vehicleID < vehicleBuffer.Length; ++vehicleID)
			{
				// Skip vehicles that haven't been created or have null infos.
				VehicleInfo info = vehicleBuffer[vehicleID].Info;
				Vehicle.Flags vehicleFlags = vehicleBuffer[vehicleID].m_flags;
				if ((vehicleFlags & Vehicle.Flags.Created) == 0 || info == null)
				{
					continue;
				}

				// Required passenger count.
				int passengerCount = 0;

				// Assign home, visit, work, and/or student counts based on building AI.
				switch (info.m_vehicleAI)
				{
					case AmbulanceAI ambulanceAI:
						passengerCount = ambulanceAI.m_patientCapacity + ambulanceAI.m_paramedicCount;
						break;

					case AmbulanceCopterAI ambulanceCopterAI:
						passengerCount = ambulanceCopterAI.m_patientCapacity + ambulanceCopterAI.m_paramedicCount;
						break;

					case BicycleAI bicycle:
						passengerCount = 2;
						break;

					case BusAI busAI:
						passengerCount = busAI.m_passengerCapacity;
						break;

					case CableCarAI cableCarAI:
						passengerCount = cableCarAI.m_passengerCapacity;
						break;

					case DisasterResponseCopterAI disasterResponseCopterAI:
						passengerCount = disasterResponseCopterAI.m_workerCount;
						break;

					case DisasterResponseVehicleAI disasterResponseVehicleAI:
						passengerCount = disasterResponseVehicleAI.m_workerCount;
						break;

					case FireTruckAI fireTruckAI:
						passengerCount = fireTruckAI.m_firemanCount;
						break;

					case HearseAI hearseAI:
						passengerCount = hearseAI.m_corpseCapacity + hearseAI.m_driverCount;
						break;

					case ParkMaintenanceVehicleAI parkMaintenanceVehicleAI:
						passengerCount = parkMaintenanceVehicleAI.m_workerCount;
						break;

					case PassengerBlimpAI blimpAI:
						passengerCount = blimpAI.m_passengerCapacity;
						break;

					case PassengerCarAI carAI:
						passengerCount = 5;
						break;

					case PassengerFerryAI ferryAI:
						passengerCount = ferryAI.m_passengerCapacity;
						break;

					case PassengerHelicopterAI helicopterAI:
						passengerCount = helicopterAI.m_passengerCapacity;
						break;

					case PassengerPlaneAI planeAI:
						passengerCount = planeAI.m_passengerCapacity;
						break;

					case PassengerShipAI shipAI:
						passengerCount = shipAI.m_passengerCapacity;
						break;

					case PassengerTrainAI trainAI:
						passengerCount = trainAI.m_passengerCapacity;
						break;

					case PoliceCarAI policeCarAI:
						passengerCount = policeCarAI.m_policeCount + policeCarAI.m_criminalCapacity;
						break;

					case TaxiAI taxiAI:
						passengerCount = taxiAI.m_passengerCapacity;
						break;

					case TramAI tramAI:
						passengerCount = tramAI.m_passengerCapacity;
						break;

					case TrolleybusAI trolleybusAI:
						passengerCount = trolleybusAI.m_passengerCapacity;
						break;

					default:
						// If not explicitly covered above, skip to next vehicle.
						continue;
				}

				// Create CitizenUnits for this vehicle based on above figures.
				citizenManager.CreateUnits(out vehicleBuffer[vehicleID].m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, 0, (ushort)vehicleID, 0, 0, 0, passengerCount, 0);
			}

			Logging.Message("finished resetting vehicles");
		}


		/// <summary>
		/// Ensures correct allocation of CitizenUnits to a building.
		/// Based on game's method.
		/// </summary>
		/// <param name="buildingID">Building ID</param>
		/// <param name="data">Building data reference</param>
		/// <param name="homeCount">Number of households to allocate</param>
		/// <param name="workCount">Number of workplaces to allocate</param>
		/// <param name="visitCount">Number of visitor places to allocate</param>
		/// <param name="studentCount">Number of student places to allocate</param>
		private static void EnsureCitizenUnits(ushort buildingID, ref Building data, int homeCount, int workCount, int visitCount, int studentCount)
		{
			// Don't allocate units to abandoned or collapsed buildings.
			if ((data.m_flags & (Building.Flags.Abandoned | Building.Flags.Collapsed)) != 0)
			{
				return;
			}

			// Local referencse.
			CitizenManager citizenManager = Singleton<CitizenManager>.instance;
			CitizenUnit[] citizenUnitBuffer = citizenManager.m_units.m_buffer;

			// Get welath level.
			Citizen.Wealth wealthLevel = Citizen.GetWealthLevel((ItemClass.Level)data.m_level);


			uint previousCitizenUnit = 0u;
			uint currentCitizenUnit = data.m_citizenUnits;
			while (currentCitizenUnit != 0)
			{
				CitizenUnit.Flags flags = citizenUnitBuffer[currentCitizenUnit].m_flags;
				if ((flags & CitizenUnit.Flags.Home) != 0)
				{
					citizenUnitBuffer[currentCitizenUnit].SetWealthLevel(wealthLevel);
					homeCount--;
				}
				if ((flags & CitizenUnit.Flags.Work) != 0)
				{
					workCount -= 5;
				}
				if ((flags & CitizenUnit.Flags.Visit) != 0)
				{
					visitCount -= 5;
				}
				if ((flags & CitizenUnit.Flags.Student) != 0)
				{
					studentCount -= 5;
				}
				previousCitizenUnit = currentCitizenUnit;
				currentCitizenUnit = citizenUnitBuffer[currentCitizenUnit].m_nextUnit;
			}
			homeCount = Mathf.Max(0, homeCount);
			workCount = Mathf.Max(0, workCount);
			visitCount = Mathf.Max(0, visitCount);
			studentCount = Mathf.Max(0, studentCount);
			if (homeCount == 0 && workCount == 0 && visitCount == 0 && studentCount == 0)
			{
				return;
			}
			uint firstUnit;
			if (citizenManager.CreateUnits(out firstUnit, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, homeCount, workCount, visitCount, 0, studentCount))
			{
				if (previousCitizenUnit != 0)
				{
					citizenUnitBuffer[previousCitizenUnit].m_nextUnit = firstUnit;
				}
				else
				{
					data.m_citizenUnits = firstUnit;
				}
			}
		}
	}
}
