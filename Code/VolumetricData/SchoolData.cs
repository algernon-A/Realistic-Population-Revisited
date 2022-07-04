using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Simple class to store original school stats (for reversion to if needed).
    /// </summary>
    public class OriginalSchoolStats
    {
        public int students;
        public int jobs0, jobs1, jobs2, jobs3;
        public int cost, maintenance;
    }


    /// <summary>
    /// Centralised store and management of school calculation data.
    /// </summary>
    internal class SchoolData : CalcData
    {
        // Instance reference.
        internal static SchoolData instance;

        // Dictionary of original settings.
        private Dictionary<string, OriginalSchoolStats> originalStats;


        /// <summary>
        /// Constructor - initializes inbuilt default calculation packs and performs other setup tasks.
        /// </summary>
        public SchoolData()
        {
            // Vanilla elementary.
            SchoolDataPack newPack = new SchoolDataPack
            {
                name = "vanelem",
                nameKey = "RPR_PCK_SVE_NAM",
                descriptionKey = "RPR_PCK_SVE_DES",
                version = DataVersion.one,
                level = ItemClass.Level.Level1,
                baseWorkers = new int[] { 1, 2, 1, 0 },
                perWorker = new int[] { 20, 50, 300, 0 },
                baseCost = 1000,
                costPer = 30,
                baseMaint = 100,
                maintPer = 3
            };
            calcPacks.Add(newPack);

            // Vanilla community school.
            newPack = new SchoolDataPack
            {
                name = "vancom",
                nameKey = "RPR_PCK_SVC_NAM",
                descriptionKey = "RPR_PCK_SVC_DES",
                version = DataVersion.one,
                level = ItemClass.Level.Level1,
                baseWorkers = new int[] { 2, 2, 1, 1 },
                perWorker = new int[] { 25, 25, 50, 0 },
                baseCost = 2000,
                costPer = 40,
                baseMaint = 250,
                maintPer = 5
            };
            calcPacks.Add(newPack);

            // Vanilla high school.
            newPack = new SchoolDataPack
            {
                name = "vanhigh",
                nameKey = "RPR_PCK_SVH_NAM",
                descriptionKey = "RPR_PCK_SVH_DES",
                version = DataVersion.one,
                level = ItemClass.Level.Level2,
                baseWorkers = new int[] { 9, 11, 5, 1 },
                perWorker = new int[] { 100, 20, 100, 250 },
                baseCost = 4000,
                costPer = 20,
                baseMaint = 500,
                maintPer = 3
            };
            calcPacks.Add(newPack);

            // Vanilla art school.
            newPack = new SchoolDataPack
            {
                name = "vanart",
                nameKey = "RPR_PCK_SVA_NAM",
                descriptionKey = "RPR_PCK_SVA_DES",
                version = DataVersion.one,
                level = ItemClass.Level.Level2,
                baseWorkers = new int[] { 10, 20, 5, 1 },
                perWorker = new int[] { 80, 20, 80, 200 },
                baseCost = 6000,
                costPer = 30,
                baseMaint = 500,
                maintPer = 5
            };
            calcPacks.Add(newPack);
        }


        /// <summary>
        /// Returns the original student count for the given school prefab (overriding with any manual figure).
        /// </summary>
        /// <param name="building">Building prefab</param>
        /// <returns>Original student count, if available, overridden by any manual figure (300 if no record available)</returns>
        internal int OriginalStudentCount(BuildingInfo prefab)
        {
            // Has the original stats dictionary been initialized yet (i.e. are we still loading)?
            if (originalStats == null)
            {
                // Not yet initialized - use raw prefab value.
                if (prefab.m_buildingAI is SchoolAI schoolAI)
                {
                    return schoolAI.m_studentCount;
                }
            }
            else if (originalStats.ContainsKey(prefab.name))
            {
                // Original stats dictionary initialized - check for any override.
                ushort value = PopData.instance.GetOverride(prefab.name);
                if (value > 0)
                {
                    // Manual override present - use that value.
                    return value;
                }
                // No override - retrieve stored value.
                return originalStats[prefab.name].students;
            }

            // If we got here, no record was found; return 300 (vanilla elementary).
            return 300;
        }


        /// <summary>
        /// Updates our building setting dictionary for the selected building prefab to the indicated calculation pack.
        /// IMPORTANT: make sure student count is called before calling this.
        /// </summary>
        /// <param name="building">Building prefab to update</param>
        /// <param name="pack">New data pack to apply</param>
        /// <param name="refresh">True to force a CitizenUnit refresh of all existing buildings, false to leave intact</param>
        internal override void UpdateBuildingPack(BuildingInfo prefab, DataPack pack, bool refresh)
        {
            // Check for volumetric school pack.
            if (pack is SchoolDataPack schoolPack)
            {
                // Apply settings to prefab.
                ApplyPack(prefab, schoolPack);
            }
            else if (pack is VanillaPack vanillaPack && prefab?.m_buildingAI is SchoolAI schoolAI && originalStats.TryGetValue(prefab.name, out OriginalSchoolStats vanillaStats))
            {
                // Vanilla school - restore worker counts.
                schoolAI.m_workPlaceCount0 = vanillaStats.jobs0;
                schoolAI.m_workPlaceCount1 = vanillaStats.jobs1;
                schoolAI.m_workPlaceCount2 = vanillaStats.jobs2;
                schoolAI.m_workPlaceCount3 = vanillaStats.jobs3;

                // Restore costs and maintenance.
                schoolAI.m_constructionCost = vanillaStats.cost;
                schoolAI.m_maintenanceCost = vanillaStats.maintenance;

                // Update prefab and tooltip.
                UpdateSchoolPrefab(prefab, schoolAI);
            }
            else
            {
                // Something went wong - return without doing anything.
                Logging.Error("invalid parameter passed to SchoolData.UpdateBuildingPack");
                return;
            }

            // Call base to update dictionary.
            base.UpdateBuildingPack(prefab, pack, refresh);

            // Update existing school buildings.
            BuildingInfo thisPrefab = prefab;
            bool thisRefresh = refresh;
            Singleton<SimulationManager>.instance.AddAction(delegate { UpdateSchools(thisPrefab, refresh); });
        }


        /// <summary>
        /// Returns the currently set default calculation pack for the given prefab.
        /// </summary>
        /// <param name="building">Building prefab</param>
        /// <returns>Default calculation data pack</returns>
        internal override DataPack CurrentDefaultPack(BuildingInfo building) => BaseDefaultPack(building.GetClassLevel(), building);


        /// <summary>
        /// Returns the inbuilt default calculation pack for the given school AI level and prefab.
        /// </summary>
        /// <param name="service">School level to check</param
        /// <param name="prefab">Building prefab to check (null if none)</param>
        /// <returns>Default calculation data pack</returns>
        internal DataPack BaseDefaultPack(ItemClass.Level level, BuildingInfo prefab)
        {
            string defaultName;

            // Is it high school?
            if (level == ItemClass.Level.Level2)
            {
                if (prefab?.name != null && prefab.name.Equals("University of Creative Arts"))
                {
                    // Art school.
                    defaultName = "vanart";
                }
                else
                {
                    // Plain high school.
                    defaultName = "vanhigh";
                }
            }
            else
            // If not high school, default to elementary school.
            {
                if (prefab?.name != null && prefab.name.Equals("Community School"))
                {
                    // Community school.
                    defaultName = "vancom";
                }
                else
                {
                    // Plain elementary school.
                    defaultName = "vanelem";
                }
            }

            // Match name to floorpack.
            return calcPacks.Find(pack => pack.name.Equals(defaultName));
        }


        /// <summary>
        /// Returns a list of calculation packs available for the given prefab.
        /// </summary>
        /// <param name="prefab">BuildingInfo prefab</param>
        /// <returns>Array of available calculation packs</returns>
        internal SchoolDataPack[] GetPacks(BuildingInfo prefab)
        {
            // Return list.
            List<SchoolDataPack> list = new List<SchoolDataPack>();

            ItemClass.Level level = prefab.GetClassLevel();

            // Iterate through each floor pack and see if it applies.
            foreach (SchoolDataPack pack in calcPacks)
            {
                // Check for matching service.
                if (pack.level == level)
                {
                    // Service matches; add pack.
                    list.Add(pack);
                }
            }

            return list.ToArray();
        }


        /// <summary>
        /// Serializes building pack settings to XML.
        /// </summary>
        /// <param name="existingList">Existing list to modify, from population pack serialization (null if none)</param>
        /// <returns>New sorted list with building pack settings</returns>
        internal SortedList<string, BuildingRecord> SerializeBuildings(SortedList<string, BuildingRecord> existingList)
        {
            // Return list.
            SortedList<string, BuildingRecord> returnList = existingList ?? new SortedList<string, BuildingRecord>();

            // Iterate through each key (BuildingInfo) in our dictionary and serialise it into a BuildingRecord.
            foreach (string prefabName in buildingDict.Keys)
            {
                string packName = buildingDict[prefabName]?.name;

                // Check to see if our existing list already contains this building.
                if (returnList.ContainsKey(prefabName))
                {
                    // Yes; update that record to include this floor pack.
                    returnList[prefabName].schoolPack = packName;
                }
                else
                {
                    // No; add a new record with this floor pack.
                    BuildingRecord newRecord = new BuildingRecord { prefab = prefabName, schoolPack = packName };
                    returnList.Add(prefabName, newRecord);
                }
            }

            return returnList;
        }


        /// <summary>
        /// Performs task on completion of level loading - recording of school default properties and application of our settings.
        /// Should be called OnLevelLoaded, after prefabs have been loaded but before gameplay commences.
        /// </summary>
        internal void OnLoad()
        {
            // Initialise original properties dictionary.
            originalStats = new Dictionary<string, OriginalSchoolStats>();

            // Iterate through all loaded building prefabs.
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); ++i)
            {
                BuildingInfo building = PrefabCollection<BuildingInfo>.GetLoaded(i);

                // Check for schools.
                if (building?.name != null && building.GetAI() is SchoolAI schoolAI && (building.GetClassLevel() == ItemClass.Level.Level1 || building.GetClassLevel() == ItemClass.Level.Level2))
                {
                    // Found a school; add it to our dictionary.
                    originalStats.Add(building.name, new OriginalSchoolStats
                    {
                        students = schoolAI.m_studentCount,
                        jobs0 = schoolAI.m_workPlaceCount0,
                        jobs1 = schoolAI.m_workPlaceCount1,
                        jobs2 = schoolAI.m_workPlaceCount2,
                        jobs3 = schoolAI.m_workPlaceCount3,
                        cost = schoolAI.m_constructionCost,
                        maintenance = schoolAI.m_maintenanceCost
                    });

                    Logging.KeyMessage("found school prefab ", building.name, " with student count ", schoolAI.m_studentCount);

                    // If setting is set, get currently active pack and apply it.
                    if (ModSettings.enableSchoolProperties)
                    {
                        ApplyPack(building, ActivePack(building) as SchoolDataPack);

                        // ApplyPack includes a call to UpdateSchoolPrefab, so no need to do it again here.
                        continue;
                    }

                    // Update school record and tooltip.
                    UpdateSchoolPrefab(building, schoolAI);
                }
            }
        }


        /// <summary>
        /// Updates all school prefabs (e.g. when the global multiplier has changed).
        /// Should only be called via simulation thread.
        /// </summary>
        internal void UpdateSchoolPrefabs()
        {
            // Iterate through all loaded building prefabs.
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); ++i)
            {
                BuildingInfo building = PrefabCollection<BuildingInfo>.GetLoaded(i);

                // Check for schools.
                if (building?.name != null && building.GetAI() is SchoolAI schoolAI && (building.GetClassLevel() == ItemClass.Level.Level1 || building.GetClassLevel() == ItemClass.Level.Level2))
                {
                    // Found a school; update school record and tooltip.
                    UpdateSchoolPrefab(building, schoolAI);
                }
            }

            // Update school buildings.
            UpdateSchools(null, true);
        }


        /// <summary>
        /// Calculates school worker totals by education level, given a school calculation pack and a total student count.
        /// </summary>
        /// <param name="schoolPack">School calculation pack to use</param>
        /// <param name="students">Student count to use</param>
        /// <returns>Array (length 4) of workers by education level</returns>
        internal int[] CalcWorkers(SchoolDataPack schoolPack, int students)
        {
            const int WorkerLevels = 4;

            int[] workers = new int[WorkerLevels];

            // Basic checks.  If we fail we just return the zeroed array.
            if (schoolPack != null)
            {
                // Local references.
                int[] baseWorkers = schoolPack.baseWorkers;
                int[] perWorker = schoolPack.perWorker;

                // Calculate workers: base jobs plus extra jobs for X number of students (ensuring divisor is greater than zero).
                for (int i = 0; i < WorkerLevels; ++i)
                {
                    workers[i] = baseWorkers[i] + (perWorker[i] > 0 ? students / perWorker[i] : 0);
                }
            }

            return workers;
        }


        /// <summary>
        /// Calculates school building placement cost, given a school calculation pack and a total student count.
        /// Placement cost is base cost plus extra cost per X students.
        /// </summary>
        /// <param name="schoolPack">School calculation pack to use</param>
        /// <param name="students">Student count to use</param>
        /// <returns>Placement cost</returns>
        internal int CalcCost(SchoolDataPack schoolPack, int students) => schoolPack.baseCost + (schoolPack.costPer * students);


        /// <summary>
        /// Calculates school building maintenance cost, given a school calculation pack and a total student count.
        /// Maintenance cost is base maintenance plus extra maintenance per X students.
        /// </summary>
        /// <param name="schoolPack">School calculation pack to use</param>
        /// <param name="students">Student count to use</param>
        /// <returns>Maintenance cost</returns>
        internal int CalcMaint(SchoolDataPack schoolPack, int students) => schoolPack.baseMaint + (schoolPack.maintPer * students);


        /// <summary>
        /// Updates a school prefab record (and associated tooltip) with updated population.
        /// </summary>
        /// <param name="prefab">Prefab to update</param>
        internal void UpdateSchoolPrefab(BuildingInfo prefab)
        {
            // Update prefab.
            UpdateSchoolPrefab(prefab, prefab.GetAI() as SchoolAI);

            // Update existing school buildings via SimulationManager.
            Singleton<SimulationManager>.instance.AddAction(delegate { UpdateSchools(prefab, true); });
        }


        /// <summary>
        /// Updates all school buildings matching the given prefab, or all school buildings if no prefab is specified, to current settings.
        /// Should only be called via simulation thread.
        /// <param name="schoolPrefab">Building prefab to update (null to update all schools)</param>
        /// <param name="allowRemoval">True to force eviction of existing students if required to reduce the CitizenUnit count of all existing buildings, false to leave intact</param>
        /// </summary>
        internal void UpdateSchools(BuildingInfo schoolPrefab, bool allowRemoval)
        {
            // Iterate through all buildings looking for schools.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            for (int i = 0; i < buildingBuffer.Length; ++i)
            {
                // Is this a valid building?
                if ((buildingBuffer[i].m_flags & Building.Flags.Created) != Building.Flags.None)
                {
                    // Valid building - check for a prefab match (if applicable), and school AI type and level.
                    BuildingInfo thisInfo = buildingBuffer[i].Info;
                    if (thisInfo != null && (schoolPrefab == null || schoolPrefab == thisInfo) && thisInfo.m_buildingAI is SchoolAI schoolAI && thisInfo.m_class.m_level <= ItemClass.Level.Level2)
                    {
                        // Found a school - apply changes to citizen units.
                        int workCount = schoolAI.m_workPlaceCount0 + schoolAI.m_workPlaceCount1 + schoolAI.m_workPlaceCount2 + schoolAI.m_workPlaceCount3;
                        int studentCount = schoolAI.StudentCount * 5 / 4; // Game ratio.
                        CitizenUnitUtils.EnsureCitizenUnits(schoolAI, (ushort)i, ref buildingBuffer[i], 0, workCount, 0, studentCount);
                        CitizenUnitUtils.RemoveCitizenUnits(ref buildingBuffer[i], 0, workCount, 0, studentCount, allowRemoval);
                    }
                }
            }
        }


        /// <summary>
        /// Applies a school data pack to a school prefab.
        /// </summary>
        /// <param name="prefab">School prefab to apply to</param>
        /// <param name="schoolPack">School data pack to apply</param>
        private void ApplyPack(BuildingInfo prefab, SchoolDataPack schoolPack)
        {
            // Null checks first.
            if (prefab?.name == null)
            {
                Logging.Error("No prefab found for SchoolPack ", schoolPack.name);
                return;
            }

            if (schoolPack == null)
            {
                Logging.Error("No SchoolPack found for prefab ", prefab.name);
            }

            // Apply settings to prefab.
            SchoolAI schoolAI = prefab.GetAI() as SchoolAI;
            if (prefab != null && schoolPack != null)
            {
                // Calculate workers and breakdowns.
                int[] workers = CalcWorkers(schoolPack, schoolAI.StudentCount);

                // Update prefab AI worker count with results (base + extras) per education level.
                schoolAI.m_workPlaceCount0 = workers[0];
                schoolAI.m_workPlaceCount1 = workers[1];
                schoolAI.m_workPlaceCount2 = workers[2];
                schoolAI.m_workPlaceCount3 = workers[3];

                // Calculate and update costs and maintenance.
                schoolAI.m_constructionCost = CalcCost(schoolPack, schoolAI.StudentCount);
                schoolAI.m_maintenanceCost = CalcMaint(schoolPack, schoolAI.StudentCount);

                // Update prefab and tooltip.
                UpdateSchoolPrefab(prefab, schoolAI);
            }
        }


        /// <summary>
        /// Updates a school prefab record (and associated tooltip) with updated population.
        /// </summary>
        /// <param name="prefab">Prefab to update</param>
        /// <param name="ai">Prefab AI</param>
        private void UpdateSchoolPrefab(BuildingInfo prefab, SchoolAI schoolAI)
        {
            if (prefab == null || schoolAI == null)
            {
                Logging.Error("null parameter passed to UpdateSchoolPrefab");
                return;
            }

            Logging.KeyMessage("updating school prefab ", prefab.name, " with studentCount ", schoolAI.m_studentCount);
            Logging.KeyMessage("applying calculation pack ", PopData.instance.ActivePack(prefab).DisplayName, " with multiplier ", Multipliers.instance.ActiveMultiplier(prefab));

            // Update prefab population record.
            schoolAI.m_studentCount = schoolAI.StudentCount;

            Logging.KeyMessage("new student count is ", schoolAI.m_studentCount);

            // Update tooltip.
            UpdateSchoolTooltip(prefab);
        }


        /// <summary>
        /// Updates a school building's tooltip (in the education tool panel).
        /// </summary>
        /// <param name="prefab">School prefab to update</param>
        private void UpdateSchoolTooltip(BuildingInfo prefab)
        {
            // Find education panel game object.
            GameObject educationPanelObject = GameObject.Find("EducationDefaultPanel");
            if (educationPanelObject == null)
            {
                Logging.Message("couldn't find education panel object (tooltip won't be updated)");
            }
            else
            {
                // Find education panel scrollable panel.
                UIScrollablePanel edScrollPanel = educationPanelObject.GetComponentInChildren<UIScrollablePanel>();
                if (edScrollPanel == null)
                {
                    Logging.Message("couldn't find education panel scrollable panel (tooltip won't be updated)");
                }
                else
                {
                    // Find buttons in panel.
                    UIButton[] schoolButtons = edScrollPanel.GetComponentsInChildren<UIButton>();

                    if (schoolButtons == null)
                    {
                        Logging.Message("couldn't find school buttons (tooltip won't be updated)");
                    }
                    else
                    {
                        // Iterate through list of buttons, looking for a match for our prefab.
                        foreach (UIButton schoolButton in schoolButtons)
                        {
                            if (schoolButton.name.Equals(prefab.name))
                            {
                                // Match!  Update tooltip.
                                schoolButton.tooltip = prefab.GetLocalizedTooltip();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Extracts the relevant school pack name from a building line record.
        /// </summary>
        /// <param name="buildingRecord">Building record to extract from</param>
        /// <returns>School pack name (if any)</returns>
        protected override string BuildingPack(BuildingRecord buildingRecord) => buildingRecord.schoolPack;
    }
}