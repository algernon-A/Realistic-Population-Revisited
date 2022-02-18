namespace RealPop2
{
    /// <summary>
    /// Household cache data structure.
    /// </summary>
    public struct HouseholdCache
    {
        public ushort level0;
        public ushort level1;
        public ushort level2;
        public ushort level3;
        public ushort level4;
    }


    /// <summary>
    /// Workplace cache data structure.
    /// </summary>
    public struct WorkplaceCache
    {
        public WorkplaceLevels level0;
        public WorkplaceLevels level1;
        public WorkplaceLevels level2;
    }


    /// <summary>
    /// Workplace education levels data structure.
    /// </summary>
    public struct WorkplaceLevels
    {
        public ushort level0;
        public ushort level1;
        public ushort level2;
        public ushort level3;
    }


    /// <summary>
    /// Visitplace cache data structure.
    /// </summary>
    public struct VisitplaceCache
    {
        public ushort level0;
        public ushort level1;
        public ushort level2;
    }
}