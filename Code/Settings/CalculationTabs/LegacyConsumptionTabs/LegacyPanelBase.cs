// <copyright file="LegacyPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Base class for options panel consumption settings (sub-)tabs (legacy configuration).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected fields")]
    internal abstract class LegacyPanelBase : TextfieldPanelBase
    {
        /// <summary>
        /// Array of building area textfields.
        /// </summary>
        protected UITextField[][] m_areaFields;

        /// <summary>
        /// Array of floor height textfields.
        /// </summary>
        protected UITextField[][] m_floorFields;

        /// <summary>
        /// Array of extra floor textfields.
        /// </summary>
        protected UITextField[][] m_extraFloorFields;

        /// <summary>
        /// Array of production rate textfields.
        /// </summary>
        protected UITextField[][] m_productionFields;

        // UI layout constants.
        private const float LeftTitle = 50f;
        private const float ColumnWidth = 50f;
        private const float Column1Width = 100f;
        private const float Column8Width = 55f;
        private const float Column2 = Column1 + Column1Width + Margin;
        private const float Column3 = Column2 + ColumnWidth + Margin;
        private const float Column4 = Column3 + ColumnWidth + Margin + Margin;
        private const float Column5 = Column4 + ColumnWidth + Margin;
        private const float Column6 = Column5 + ColumnWidth + Margin;
        private const float Column7 = Column6 + ColumnWidth + Margin;
        private const float Column8 = Column7 + Column8Width + Margin;
        private const float Column9 = Column8 + Column8Width + Margin;

        // Column labels.
        private string _areaLabel;
        private string _floorLabel;
        private string _extraFloorLabel;
        private string _productionLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyPanelBase"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal LegacyPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            m_panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate(TabNameKey), tabIndex, out UIButton _);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Gets the tab's tile translation key.
        /// </summary>
        protected abstract string TabNameKey { get; }

        /// <summary>
        /// Saves the current legacy config.
        /// </summary>
        protected void SaveLegacy()
        {
            // Set flag to show that user has instructed to save legacy file.
            XMLUtilsWG.WriteToLegacy = true;

            // Save legacy data using WG serialization.
            XMLUtilsWG.WriteToXML();

            // Save new configuration file with updated consumption info.
            ConfigurationUtils.SaveSettings();
        }

        /// <summary>
        /// Initialises array structure.
        /// </summary>
        /// <param name="numSubServices">Number of sub-services to initialise for.</param>
        protected void SetupArrays(int numSubServices)
        {
            // Initialise textfield array.
            m_areaFields = new UITextField[numSubServices][];
            m_floorFields = new UITextField[numSubServices][];
            m_extraFloorFields = new UITextField[numSubServices][];
            m_powerFields = new UITextField[numSubServices][];
            m_waterFields = new UITextField[numSubServices][];
            m_sewageFields = new UITextField[numSubServices][];
            m_garbageFields = new UITextField[numSubServices][];
            m_incomeFields = new UITextField[numSubServices][];
            m_productionFields = new UITextField[numSubServices][];
        }

        /// <summary>
        /// Adds column headings.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        protected void AddHeadings(UIPanel panel)
        {
            // Set string references (we'll reference these multiple times with the textfields, so this saves calling translate each time).
            _areaLabel = Translations.Translate(m_notResidential ? "RPR_OPT_APW" : "RPR_OPT_APH");
            _floorLabel = Translations.Translate("RPR_OPT_FLR");
            _extraFloorLabel = Translations.Translate("RPR_CAL_FLR_M");
            m_powerLabel = Translations.Translate("RPR_OPT_POW");
            m_waterLabel = Translations.Translate("RPR_OPT_WAT");
            m_sewageLabel = Translations.Translate("RPR_OPT_SEW");
            m_garbageLabel = Translations.Translate("RPR_OPT_GAR");
            m_incomeLabel = Translations.Translate("RPR_OPT_WEA");
            _productionLabel = Translations.Translate("RPR_OPT_PRO");

            // Headings.
            PanelUtils.ColumnLabel(panel, Column1, TitleHeight, Column1Width + Margin, _areaLabel, _areaLabel, 1.0f);
            PanelUtils.ColumnLabel(panel, Column2, TitleHeight, ColumnWidth + Margin, _floorLabel, _floorLabel, 1.0f);
            ColumnIcon(panel, Column4, ColumnWidth, m_powerLabel, "ToolbarIconElectricity");
            ColumnIcon(panel, Column5, ColumnWidth, m_waterLabel, "ToolbarIconWaterAndSewage");
            ColumnIcon(panel, Column6, ColumnWidth, m_sewageLabel, "ToolbarIconWaterAndSewageDisabled");
            ColumnIcon(panel, Column7, ColumnWidth, m_garbageLabel, "InfoIconGarbage");
            ColumnIcon(panel, Column8, Column8Width, m_incomeLabel, "ToolbarIconMoney");
            ColumnIcon(panel, Column9, ColumnWidth, _productionLabel, "IconPolicyAutomatedSorting");

            // Bonus floors.
            if (m_notResidential)
            {
                PanelUtils.ColumnLabel(panel, Column3, TitleHeight, ColumnWidth + Margin, _extraFloorLabel, _extraFloorLabel, 0.8f);
            }
        }

        /// <summary>
        /// Adds control buttons to the bottom of the panel.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        protected void AddButtons(UIPanel panel)
        {
            // Add extra space.
            m_currentY += Margin;

            // Reset button.
            UIButton resetButton = UIButtons.AddButton(panel, Button1X, m_currentY, Translations.Translate("RPR_OPT_RTD"), ButtonWidth);
            resetButton.eventClicked += (component, clickEvent) => ResetToDefaults();

            UIButton revertToSaveButton = UIButtons.AddButton(panel, Button2X, m_currentY, Translations.Translate("RPR_OPT_RTS"), ButtonWidth);
            revertToSaveButton.eventClicked += (component, clickEvent) =>
            {
                XMLUtilsWG.ReadFromXML();
                PopulateFields();
            };

            UIButton saveButton = UIButtons.AddButton(panel, Button3X, m_currentY, Translations.Translate("RPR_OPT_SAA"), ButtonWidth);
            saveButton.eventClicked += (component, clickEvent) => ApplyFields();
        }

        /// <summary>
        /// Adds a sub-service field group to the panel.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        /// <param name="subService">Subservice reference number.</param>
        /// <param name="isExtract">Set this to true (and label to null) to add extractor/processor labels (default false, which is plain level labels).</param>
        /// <param name="label">Text label base for each row; null (default) to use level numbers or extractor/prcessor.</param>
        protected void AddSubService(UIPanel panel, int subService, bool isExtract = false, string label = null)
        {
            // Add a row for each level within this subservice.
            for (int i = 0; i < m_areaFields[subService].Length; ++i)
            {
                // Row label.
                RowLabel(panel, m_currentY, label ?? (isExtract ? Translations.Translate(i == 0 ? "RPR_CAT_EXT" : "RPR_CAT_PRO") : Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString()));

                // Textfields.
                m_areaFields[subService][i] = AddTextField(panel, Column1Width, Column1, m_currentY, _areaLabel);
                m_floorFields[subService][i] = AddTextField(panel, ColumnWidth, Column2, m_currentY, _floorLabel);
                m_powerFields[subService][i] = AddTextField(panel, ColumnWidth, Column4, m_currentY, m_powerLabel);
                m_waterFields[subService][i] = AddTextField(panel, ColumnWidth, Column5, m_currentY, m_waterLabel);
                m_sewageFields[subService][i] = AddTextField(panel, ColumnWidth, Column6, m_currentY, m_sewageLabel);
                m_garbageFields[subService][i] = AddTextField(panel, ColumnWidth, Column7, m_currentY, m_garbageLabel);
                m_incomeFields[subService][i] = AddTextField(panel, Column8Width, Column8, m_currentY, m_incomeLabel);
                m_productionFields[subService][i] = AddTextField(panel, ColumnWidth, Column9, m_currentY, _productionLabel);

                // Bonus levels.
                if (m_notResidential)
                {
                    m_extraFloorFields[subService][i] = AddTextField(panel, ColumnWidth, Column3, m_currentY, _extraFloorLabel);
                }

                // Increment Y position.
                m_currentY += RowHeight;
            }

            // Add an extra bit of space at the end.
            m_currentY += Margin;
        }

        /// <summary>
        /// Populates the text fields for a given subservice with information from the DataStore.
        /// </summary>
        /// <param name="dataArray">DataStore data array for the SubService.</param>
        /// <param name="subService">SubService reference number.</param>
        protected void PopulateSubService(int[][] dataArray, int subService)
        {
            // Iterate though each level, populating each row as we go.
            for (int i = 0; i < m_areaFields[subService].Length; ++i)
            {
                m_areaFields[subService][i].text = dataArray[i][DataStore.PEOPLE].ToString();
                m_floorFields[subService][i].text = dataArray[i][DataStore.LEVEL_HEIGHT].ToString();
                m_powerFields[subService][i].text = dataArray[i][DataStore.POWER].ToString();
                m_waterFields[subService][i].text = dataArray[i][DataStore.WATER].ToString();
                m_sewageFields[subService][i].text = dataArray[i][DataStore.SEWAGE].ToString();
                m_garbageFields[subService][i].text = dataArray[i][DataStore.GARBAGE].ToString();
                m_incomeFields[subService][i].text = dataArray[i][DataStore.INCOME].ToString();
                m_productionFields[subService][i].text = dataArray[i][DataStore.PRODUCTION].ToString();

                // Extra floor field, if applicable.
                if (!(this is LegacyResidentialPanel))
                {
                    m_extraFloorFields[subService][i].text = dataArray[i][DataStore.DENSIFICATION].ToString();
                }
            }
        }

        /// <summary>
        /// Updates the DataStore for a given SubService with information from text fields.
        /// </summary>
        /// <param name="dataArray">DataStore data array for the SubService.</param>
        /// <param name="subService">SubService reference number.</param>
        protected void ApplySubService(int[][] dataArray, int subService)
        {
            // Iterate though each level, populating each row as we go.
            for (int i = 0; i < m_areaFields[subService].Length; ++i)
            {
                PanelUtils.ParseInt(ref dataArray[i][DataStore.PEOPLE], m_areaFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.LEVEL_HEIGHT], m_floorFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.POWER], m_powerFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.WATER], m_waterFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.SEWAGE], m_sewageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.GARBAGE], m_garbageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.INCOME], m_incomeFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.PRODUCTION], m_productionFields[subService][i].text);

                // Extra floor field, if applicable.
                if (!(this is LegacyResidentialPanel))
                {
                    PanelUtils.ParseInt(ref dataArray[i][DataStore.DENSIFICATION], m_extraFloorFields[subService][i].text);
                }
            }
        }
    }
}