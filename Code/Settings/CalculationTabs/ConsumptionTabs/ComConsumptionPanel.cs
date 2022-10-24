// <copyright file="ComConsumptionPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel (sub)-tab for commercial building consumption configuration.
    /// </summary>
    internal class ComConsumptionPanel : ConsumptionPanelBase
    {
        // Array reference constants.
        private const int LowCom = 0;
        private const int HighCom = 1;
        private const int Wall2Wall = 2;
        private const int EcoCom = 3;
        private const int Leisure = 4;
        private const int Tourist = 5;
        private const int NumSubServices = 7;
        private const int NumLevels = 3;

        // Label constants.
        private readonly string[] _subServiceLables =
        {
            "RPR_CAT_CLO",
            "RPR_CAT_CHI",
            "RPR_CAT_CW2",
            "RPR_CAT_ORG",
            "RPR_CAT_LEI",
            "RPR_CAT_TOU",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ComConsumptionPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ComConsumptionPanel(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!m_isSetup)
            {
                // Perform initial setup.
                m_isSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Initialise textfield arrays (first dimension, sub-services).
                SubServiceArrays(NumSubServices);

                // Initialise textfield arrays (second dimension, levels).
                for (int i = 0; i < NumSubServices; i++)
                {
                    // Number of levels is either 3 (for the first three categories, low, high, and W2W), or 1 for the remainder.
                    int levels = i < 3 ? NumLevels : 1;

                    LevelArrays(i, levels);
                }

                // Headings.
                AddHeadings(m_panel);

                // Create residential per-person area textfields and labels.
                PanelUtils.RowHeaderIcon(m_panel, ref m_currentY, Translations.Translate(_subServiceLables[LowCom]), "ZoningCommercialLow", "Thumbnails");
                AddSubService(m_panel, LowCom);
                PanelUtils.RowHeaderIcon(m_panel, ref m_currentY, Translations.Translate(_subServiceLables[HighCom]), "ZoningCommercialHigh", "Thumbnails");
                AddSubService(m_panel, HighCom);
                PanelUtils.RowHeaderIcon(m_panel, ref m_currentY, Translations.Translate(_subServiceLables[Wall2Wall]), "DistrictSpecializationCommercialWallToWall", "Thumbnails");
                AddSubService(m_panel, Wall2Wall);
                PanelUtils.RowHeaderIcon(m_panel, ref m_currentY, Translations.Translate(_subServiceLables[EcoCom]), "IconPolicyOrganic", "Ingame");
                AddSubService(m_panel, EcoCom, label: Translations.Translate("RPR_CAT_ECO"));
                PanelUtils.RowHeaderIcon(m_panel, ref m_currentY, Translations.Translate(_subServiceLables[Leisure]), "IconPolicyLeisure", "Ingame");
                AddSubService(m_panel, Leisure, label: Translations.Translate(_subServiceLables[Leisure]));
                PanelUtils.RowHeaderIcon(m_panel, ref m_currentY, Translations.Translate(_subServiceLables[Tourist]), "IconPolicyTourist", "Ingame");
                AddSubService(m_panel, Tourist, label: Translations.Translate(_subServiceLables[Tourist]));

                // Populate initial values.
                PopulateFields();

                // Add command buttons.
                AddButtons(m_panel);
            }
        }

        /// <summary>
        /// Populates the text fields with information from the DataStore.
        /// </summary>
        protected override void PopulateFields()
        {
            // Populate each subservice.
            PopulateSubService(DataStore.commercialLow, LowCom);
            PopulateSubService(DataStore.commercialHigh, HighCom);
            PopulateSubService(DataStore.commercialW2W, Wall2Wall);
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
            ApplySubService(DataStore.commercialW2W, Wall2Wall);
            ApplySubService(DataStore.commercialEco, EcoCom);
            ApplySubService(DataStore.commercialLeisure, Leisure);
            ApplySubService(DataStore.commercialTourist, Tourist);

            // Clear cached values.
            PopData.Instance.ClearWorkplaceCache();

            // Save new settings.
            ConfigurationUtils.SaveSettings();

            // Refresh settings.
            PopulateFields();
        }

        /// <summary>
        /// Resets all textfields to mod default values.
        /// </summary>
        protected override void ResetToDefaults()
        {
            // Defaults copied from Datastore.
            int[][] commercialLow =
            {
                new int[] { 100, 6, 1, 0,  90,   70, 20, 10,  0,    9, 30, 30, 9, 700,   0, 100,   -1, 30 },
                new int[] { 105, 6, 1, 0, 100,   30, 45, 20,  5,   10, 35, 35, 8, 750,   0,  90,   -1, 20 },
                new int[] { 110, 6, 1, 0, 110,    5, 30, 55, 10,   11, 40, 40, 7, 800,   0,  75,   -1, 10 },
            };

            int[][] commercialHigh =
            {
                new int[] { 115, 5, 1, 0, 220,   10, 45, 40,  5,   10, 28, 28, 9, 750,   0, 80,   -1, 20 },
                new int[] { 120, 5, 1, 0, 310,    7, 32, 43, 18,   11, 32, 32, 8, 800,   0, 70,   -1, 14 },
                new int[] { 125, 5, 1, 0, 400,    5, 25, 45, 25,   13, 36, 36, 7, 850,   0, 60,   -1,  8 },
            };

            int[][] commercialWall2Wall =
            {
                new int[] { 115, 5, 1, 0, 220,   10, 45, 40,  5,   10, 28, 28, 9, 750,   0, 80,   -1, 20 },
                new int[] { 120, 5, 1, 0, 310,    7, 32, 43, 18,   11, 32, 32, 8, 800,   0, 70,   -1, 14 },
                new int[] { 125, 5, 1, 0, 400,    5, 25, 45, 25,   13, 36, 36, 7, 850,   0, 60,   -1,  8 },
            };

            int[][] commercialEco = { new int[] { 120, 6, 1, 0, 100, 50, 40, 10, 0, 11, 30, 30, 7, 800, 0, 2, 50, 20 } };

            int[][] commercialTourist = { new int[] { 1000, 10, 50, 0, 250, 15, 35, 35, 15, 30, 50, 55, 30, 900, 0, 150, -1, 50 } };

            int[][] commercialLeisure = { new int[] { 60, 10, 0, 0, 250, 15, 40, 35, 10, 30, 36, 40, 25, 750, 0, 300, -1, 30 } };

            // Populate text fields with these.
            PopulateSubService(commercialLow, LowCom);
            PopulateSubService(commercialHigh, HighCom);
            PopulateSubService(commercialWall2Wall, Wall2Wall);
            PopulateSubService(commercialEco, EcoCom);
            PopulateSubService(commercialLeisure, Leisure);
            PopulateSubService(commercialTourist, Tourist);
        }
    }
}
