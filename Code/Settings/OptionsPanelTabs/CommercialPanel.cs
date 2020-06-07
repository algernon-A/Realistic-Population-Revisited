﻿using ColossalFramework.UI;


namespace RealisticPopulationRevisited
{
    /// <summary>
    /// Options panel for setting commercial calculation options.
    /// </summary>
    internal class CommercialPanel : PanelBase
    {
        // Array reference constants.
        private const int LowCom = 0;
        private const int HighCom = 1;
        private const int EcoCom = 2;
        private const int Leisure = 3;
        private const int Tourist = 4;
        private const int NumSubServices = 5;
        private const int NumLevels = 3;


        // Label constants.
        private string[] subServiceLables =
        {
            "Low com level ",
            "High com level ",
            "Eco commercial",
            "Leisure",
            "Tourism"
        };

        /// <summary>
        /// Adds commercial options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        public CommercialPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            UIHelper commercialTab = PanelUtils.AddTab(tabStrip, "Commercial", tabIndex);
            UIPanel panel = commercialTab.self as UIPanel;

            panel.autoLayout = false;

            // Initialise textfield array.
            SetupArrays(NumSubServices);

            for (int i = 0; i < NumSubServices; i++)
            {
                int levels = i < 2 ? NumLevels : 1;

                areaFields[i] = new UITextField[levels];
                floorFields[i] = new UITextField[levels];
                extraFloorFields[i] = new UITextField[levels];
                powerFields[i] = new UITextField[levels];
                waterFields[i] = new UITextField[levels];
                sewageFields[i] = new UITextField[levels];
                garbageFields[i] = new UITextField[levels];
                incomeFields[i] = new UITextField[levels];
            }

            // Headings.
            AddHeadings(panel);

            // Create residential per-person area textfields and labels.
            AddSubService(panel, subServiceLables[LowCom], true, LowCom);
            AddSubService(panel, subServiceLables[HighCom], true, HighCom);
            AddSubService(panel, subServiceLables[EcoCom], false, EcoCom);
            AddSubService(panel, subServiceLables[Leisure], false, Leisure);
            AddSubService(panel, subServiceLables[Tourist], false, Tourist);

            // Populate initial values.
            PopulateFields();

            // Add command buttons.
            AddButtons(panel);
        }


        /// <summary>
        /// Populates the text fields with information from the DataStore.
        /// </summary>
        protected override void PopulateFields()
        {
            // Populate each subservice.
            PopulateSubService(DataStore.commercialLow, LowCom);
            PopulateSubService(DataStore.commercialHigh, HighCom);
            PopulateSubService(DataStore.commercialEco, EcoCom);
            PopulateSubService(DataStore.commercialLeisure, Leisure);
            PopulateSubService(DataStore.commercialTourist, Tourist);
        }


        /// <summary>
        /// Updates the DataStore with the information from the text fields.
        /// </summary>
        protected override void ApplyFields()
        {
            // Apply each subservice.
            ApplySubService(DataStore.commercialLow, LowCom);
            ApplySubService(DataStore.commercialHigh, HighCom);
            ApplySubService(DataStore.commercialEco, EcoCom);
            ApplySubService(DataStore.commercialLeisure, Leisure);
            ApplySubService(DataStore.commercialTourist, Tourist);

            // Clear cached values.
            DataStore.prefabWorkerVisit.Clear();

            // Save new settings.
            XMLUtils.WriteToXML();

            // Refresh settings.
            PopulateFields();
        }


        /// <summary>
        /// Resets all textfields to mod default values.
        /// </summary>
        protected override void ResetToDefaults()
        {
            // Defaults copied from Datastore.
            int[][] commercialLow = { new int [] {100, 6, 1, 0,  90,   70, 20, 10,  0,    9, 30, 30, 9, 700,   0, 100,   -1, 30},
                                                new int [] {105, 6, 1, 0, 100,   30, 45, 20,  5,   10, 35, 35, 8, 750,   0,  90,   -1, 20},
                                                new int [] {110, 6, 1, 0, 110,    5, 30, 55, 10,   11, 40, 40, 7, 800,   0,  75,   -1, 10} };

            int[][] commercialHigh = { new int [] {115, 5, 1, 0, 220,   10, 45, 40,  5,   10, 28, 28, 9, 750,   0, 80,   -1, 20},
                                                 new int [] {120, 5, 1, 0, 310,    7, 32, 43, 18,   11, 32, 32, 8, 800,   0, 70,   -1, 14},
                                                 new int [] {125, 5, 1, 0, 400,    5, 25, 45, 25,   13, 36, 36, 7, 850,   0, 60,   -1,  8} };

            int[][] commercialEco = { new int[] { 120, 6, 1, 0, 100, 50, 40, 10, 0, 11, 30, 30, 7, 800, 0, 2, 50, 20 } };

            int[][] commercialTourist = { new int[] { 1000, 10, 50, 0, 250, 15, 35, 35, 15, 30, 50, 55, 30, 900, 0, 150, -1, 50 } };

            int[][] commercialLeisure = { new int[] { 60, 10, 0, 0, 250, 15, 40, 35, 10, 30, 36, 40, 25, 750, 0, 300, -1, 30 } };

            // Populate text fields with these.
            PopulateSubService(commercialLow, LowCom);
            PopulateSubService(commercialHigh, HighCom);
            PopulateSubService(commercialEco, EcoCom);
            PopulateSubService(commercialLeisure, Leisure);
            PopulateSubService(commercialTourist, Tourist);
        }
    }
}