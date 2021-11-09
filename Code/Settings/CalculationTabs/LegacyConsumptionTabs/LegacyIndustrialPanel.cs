using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel (sub)-tab for legacy industrial building consumption configuration.
    /// </summary>
    internal class LegacyIndustrialPanel : LegacyPanelBase
    {
        // Array reference constants.
        private const int Generic = 0;
        private const int Farming = 1;
        private const int Forestry = 2;
        private const int Oil = 3;
        private const int Ore = 4;
        private const int NumSubServices = 5;
        private const int NumLevels = 3;


        // Label constants.
        private readonly string[] subServiceLables =
        {
            "RPR_CAT_IND",
            "RPR_CAT_FAR",
            "RPR_CAT_FOR",
            "RPR_CAT_OIL",
            "RPR_CAT_ORE"
        };


        // Tab title.
        protected override string TabNameKey => "RPR_CAT_IND";

        /// <summary>
        /// Adds industrial options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal LegacyIndustrialPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Initialise textfield array.
                SetupArrays(NumSubServices);

                for (int i = 0; i < NumSubServices; i++)
                {
                    int levels = i == 0 ? NumLevels : 2;

                    areaFields[i] = new UITextField[levels];
                    floorFields[i] = new UITextField[levels];
                    extraFloorFields[i] = new UITextField[levels];
                    powerFields[i] = new UITextField[levels];
                    waterFields[i] = new UITextField[levels];
                    sewageFields[i] = new UITextField[levels];
                    garbageFields[i] = new UITextField[levels];
                    incomeFields[i] = new UITextField[levels];
                    productionFields[i] = new UITextField[NumLevels];
                }

                // Headings.
                AddHeadings(panel);

                // Create residential per-person area textfields and labels.
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[Generic]), "ZoningIndustrial", "Thumbnails");
                AddSubService(panel, Generic);
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[Farming]), "IconPolicyFarming", "Ingame");
                AddSubService(panel, Farming, true);
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[Forestry]), "IconPolicyForest", "Ingame");
                AddSubService(panel, Forestry, true);
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[Oil]), "IconPolicyOil", "Ingame");
                AddSubService(panel, Oil, true);
                PanelUtils.RowHeaderIcon(panel, ref currentY, Translations.Translate(subServiceLables[Ore]), "IconPolicyOre", "Ingame");
                AddSubService(panel, Ore, true);

                // Populate initial values.
                PopulateFields();

                // Add command buttons.
                AddButtons(panel);
            }
        }


        /// <summary>
        /// Populates the text fields with information from the DataStore.
        /// </summary>
        protected override void PopulateFields()
        {
            // Populate each subservice.
            PopulateSubService(DataStore.industry, Generic);
            PopulateSubService(DataStore.industry_farm, Farming);
            PopulateSubService(DataStore.industry_forest, Forestry);
            PopulateSubService(DataStore.industry_oil, Oil);
            PopulateSubService(DataStore.industry_ore, Ore);
        }


        /// <summary>
        /// Updates the DataStore with the information from the text fields.
        /// </summary>
        protected override void ApplyFields()
        {
            // Apply each subservice.
            ApplySubService(DataStore.industry, Generic);
            ApplySubService(DataStore.industry_farm, Farming);
            ApplySubService(DataStore.industry_forest, Forestry);
            ApplySubService(DataStore.industry_oil, Oil);
            ApplySubService(DataStore.industry_ore, Ore);

            // Clear cached values.
            PopData.instance.workplaceCache.Clear();

            // Save new settings.
            SaveLegacy();

            // Refresh settings.
            PopulateFields();
        }


        /// <summary>
        /// Resets all textfields to mod default values.
        /// </summary>
        protected override void ResetToDefaults()
        {
            // Defaults copied from Datastore.
            int[][] industry = { new int [] {38, 50, 0, 0, -1,   70, 20, 10,  0,   28,  90, 100, 20, 220,   300, 300,   100, 10},
                                           new int [] {35, 50, 0, 0, -1,   20, 45, 25, 10,   30, 100, 110, 18, 235,   150, 150,   140, 37},
                                           new int [] {32, 50, 0, 0, -1,    5, 20, 45, 30,   32, 110, 120, 16, 250,    25,  50,   160, 50} };

            int[][] industry_farm = { new int [] {250, 50, 0, 0, -1,   90, 10,  0, 0,   10,  80, 100, 20, 180,   0, 175,    50, 10},
                                                new int [] { 55, 25, 0, 0, -1,   30, 60, 10, 0,   40, 100, 150, 25, 220,   0, 180,   100, 25} };

            // The bounding box for a forest plantation is small
            int[][] industry_forest = { new int [] {160, 50, 0, 0, -1,   90, 10,  0, 0,   20, 25, 35, 20, 180,   0, 210,    50, 10},
                                                  new int [] { 45, 20, 0, 0, -1,   30, 60, 10, 0,   60, 70, 80, 30, 240,   0, 200,   100, 25} };

            int[][] industry_ore = { new int [] {80, 50, 0, 0, -1,   18, 60, 20,  2,    50, 100, 100, 50, 250,   400, 500,    75, 10},
                                               new int [] {40, 30, 0, 0, -1,   15, 40, 35, 10,   120, 160, 170, 40, 320,   300, 475,   100, 25} };

            int[][] industry_oil = { new int [] {80, 50, 0, 0, -1,   15, 60, 23,  2,    90, 180, 220, 40, 300,   450, 375,    75, 10},
                                               new int [] {38, 30, 0, 0, -1,   10, 35, 45, 10,   180, 200, 240, 50, 400,   300, 400,   100, 25} };

            // Populate text fields with these.
            PopulateSubService(industry, Generic);
            PopulateSubService(industry_farm, Farming);
            PopulateSubService(industry_forest, Forestry);
            PopulateSubService(industry_oil, Oil);
            PopulateSubService(industry_ore, Ore);
        }
    }
}