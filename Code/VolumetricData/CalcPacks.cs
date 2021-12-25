namespace RealPop2
{
    public enum DataVersion
    {
        legacy = 0,
        one,
        customOne,
        overrideOne
    }


    /// <summary>
    /// Calculation data pack - provides parameters for calculating building populations for given services and (optional) subservices.
    /// </summary>
    public class DataPack
    {
        public int version;
        public string name;
        public string displayName;
        public string description;
    }


    /// <summary>
    /// Floor calculation data pack - provides parameters for calculating building floors.
    /// </summary>
    public class FloorDataPack : DataPack
    {
        // Height per floor in metres.
        public float floorHeight;

        // Height needs to be at least this high, in metres, for any floor to exist.
        public float firstFloorMin;

        // Extend first floor height by this additional amount, in metres.
        public float firstFloorExtra;

        // True if the first floor should be excluded from calculations (e.g. for foyers/lobbies).
        public bool firstFloorEmpty;
    }


    /// <summary>
    /// Population clculation data pack - provides parameters for calculating building populations.
    /// </summary>
    public class PopDataPack : DataPack
    {
        // Service restrictions.
        public ItemClass.Service service;


        /// <summary>
        /// Returns the population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <param name="multiplier">Population multiplier</param>
        /// <returns>Population</returns>
        public virtual ushort Population(BuildingInfo buildingPrefab, int level, float multiplier) => 0;


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public virtual WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => new WorkplaceLevels {  level0 = 1, level1 = 0, level2 = 0, level3 = 0};
    }


    /// <summary>
    /// Population clculation data pack - provides parameters for calculating building populations.
    /// </summary>
    public class SchoolDataPack : DataPack
    {
        // Education ItemClass.Level.
        public ItemClass.Level level;

        // Employment calculation arrays.
        public int[] baseWorkers;
        public int[] perWorker;

        // Cost and maintenance fields.
        public int baseCost;
        public int costPer;
        public int baseMaint;
        public int maintPer;
    }



    /// <summary>
    /// Struct holding data for volumetric calculations for a given building level.
    /// </summary>
    public struct LevelData
    {
        // Empty area (if any) to be subtracted from building before any calculations are made.
        public float emptyArea;

        // Empty area (if any) as a percentage to to be subtracted from each floor area AFTER fixed empty area (above) is subtracted.
        public int emptyPercent;

        // Area per unit (household/worker), in square metres.
        public float areaPer;

        // True if unit areas should be calculated as though they extend through all levels (ground to roof - e.g. detached housing, rowhouses, etc.),
        // false if units should only be treated as single-floor entities (e.g. apartment blocks).
        // Generally speaking, true for low-density, false for high-density.
        public bool multiFloorUnits;
    }


    /// <summary>
    /// Volumetric calculation data pack.
    /// </summary>
    public class VolumetricPopPack : PopDataPack
    {
        // Building level records.
        public LevelData[] levels;

        /// <summary>
        /// Returns the volumetric population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <param name="multiplier">Population multiplier</param>
        /// <returns>Population</returns>
        public override ushort Population(BuildingInfo buildingPrefab, int level, float multiplier)
        {
            // Bounds check for building level (zero-based).
            int thisLevel = level;
            if (thisLevel >= levels.Length)
            {
                Logging.Error("Building level out of range: ", level);
                
                // Set level to maximum (zero-based, so subtract one from levels.Length).
                thisLevel = levels.Length - 1;
            }
            return PopData.instance.VolumetricPopulation(buildingPrefab.m_generatedInfo, levels[thisLevel], (FloorDataPack)FloorData.instance.ActivePack(buildingPrefab), multiplier);
        }


        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level) => EmploymentData.CalculateWorkplaces(buildingPrefab, level);
    }


    /// <summary>
    /// Legacy WG residential calculation pack.
    /// </summary>
    public class LegacyResPack : PopDataPack
    {
        /// <summary>
        /// Returns the volumetric population of the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <param name="multiplier">Ignored</param>
        /// <returns>Population</returns>
        public override ushort Population(BuildingInfo buildingPrefab, int level, float multiplier)
        {
            // First, check for volumetric population override - that trumps everything else.
            ushort value = PopData.instance.GetOverride(buildingPrefab.name);
            if (value == 0)
            {
                // No volumetric override - use legacy calcs.
                int[] array = LegacyAIUtils.GetResidentialArray(buildingPrefab, (int)level);
                return LegacyAIUtils.CalculatePrefabHousehold(buildingPrefab.GetWidth(), buildingPrefab.GetWidth(), ref buildingPrefab, ref array);
            }

            return value;
        }
    }


    /// <summary>
    /// Legacy WG commercial calculation pack.
    /// </summary>
    public class LegacyComPack : PopDataPack
    {
        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array = LegacyAIUtils.GetCommercialArray(buildingPrefab, level);
            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 4, ref array);
        }
    }


    /// <summary>
    /// Legacy WG industrial calculation pack.
    /// </summary>
    public class LegacyIndPack : PopDataPack
    {
        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array;
            int minWorkers;

            // Need to test if we're an extractor or not for this one.
            if (buildingPrefab.GetAI() is IndustrialExtractorAI)
            {
                array = LegacyAIUtils.GetExtractorArray(buildingPrefab);
                minWorkers = 3;
            }
            else
            {
                array = LegacyAIUtils.GetIndustryArray(buildingPrefab, level);
                minWorkers = 4;
            }

            return  LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, minWorkers, ref array);
        }
    }


    /// <summary>
    /// Legacy WG office calculation pack.
    /// </summary>
    public class LegacyOffPack : PopDataPack
    {
        /// <summary>
        /// Returns the workplace breakdowns and visitor count for the given building prefab and level.
        /// </summary>
        /// <param name="buildingPrefab">Building prefab record</param>
        /// <param name="level">Building level</param>
        /// <returns>Workplace breakdowns and visitor count </returns>
        public override WorkplaceLevels Workplaces(BuildingInfo buildingPrefab, int level)
        {
            int[] array = LegacyAIUtils.GetOfficeArray(buildingPrefab, level);
            return LegacyAIUtils.CalculatePrefabWorkers(buildingPrefab.GetWidth(), buildingPrefab.GetLength(), ref buildingPrefab, 10, ref array);
        }
    }
}