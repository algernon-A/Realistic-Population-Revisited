// <copyright file="ConsumptionPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Base class for options panel consumption settings (sub-)tabs (new configuration).
    /// </summary>
    internal abstract class ConsumptionPanelBase : TextfieldPanelBase
    {
        /// <summary>
        /// Power consumption column relative X-position.
        /// </summary>
        protected const float PowerX = 275f;

        // Layout constants - private.
        private const float ColumnWidth = 45f;
        private const float WideColumnWidth = 60f;
        private const float WaterX = PowerX + ColumnWidth + Margin;
        private const float GarbageX = WaterX + ColumnWidth + (Margin * 4);
        private const float SewageX = GarbageX + ColumnWidth + Margin;
        private const float PollutionX = SewageX + ColumnWidth + Margin;
        private const float NoiseX = PollutionX + ColumnWidth + Margin;
        private const float MailX = NoiseX + ColumnWidth + (Margin * 4);
        private const float IncomeX = MailX + ColumnWidth + Margin;
        private const float FinalX = MailX + WideColumnWidth;

        // Tab icons.
        private readonly string[] _tabIconNames =
        {
            "ToolbarIconElectricity",
            "ToolbarIconWaterAndSewage",
            "InfoIconGarbage",
            "InfoIconNoisePollution",
            "ToolbarIconMoney",
        };

        // Tab atlases.
        private readonly string[] _tabAtlasNames =
        {
            "ingame",
            "ingame",
            "ingame",
            "ingame",
            "ingame",
        };

        // Textfield array.
        private UITextField[][] _pollutionFields;
        private UITextField[][] _noiseFields;
        private UITextField[][] _mailFields;

        // Column labels.
        private string _pollutionLabel;
        private string _noiseLabel;
        private string _mailLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumptionPanelBase"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ConsumptionPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            m_panel = UITabstrips.AddIconTab(tabStrip, Translations.Translate("RPR_OPT_CON"), tabIndex, _tabIconNames, _tabAtlasNames);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Initialises array structure (first dimension - sub-services).
        /// </summary>
        /// <param name="numSubServices">Number of sub-services to initialise for.</param>
        protected void SubServiceArrays(int numSubServices)
        {
            // Initialise textfield array.
            m_powerFields = new UITextField[numSubServices][];
            m_waterFields = new UITextField[numSubServices][];
            m_garbageFields = new UITextField[numSubServices][];
            m_sewageFields = new UITextField[numSubServices][];
            m_incomeFields = new UITextField[numSubServices][];
            _pollutionFields = new UITextField[numSubServices][];
            _noiseFields = new UITextField[numSubServices][];
            _mailFields = new UITextField[numSubServices][];
        }

        /// <summary>
        /// Initialises array structure (second dimension - levels).
        /// </summary>
        /// <param name="service">Sub-service index to initialise.</param>
        /// <param name="numLevels">Number of levels to initialise for.</param>
        protected void LevelArrays(int service, int numLevels)
        {
            m_powerFields[service] = new UITextField[numLevels];
            m_waterFields[service] = new UITextField[numLevels];
            m_garbageFields[service] = new UITextField[numLevels];
            m_sewageFields[service] = new UITextField[numLevels];
            _pollutionFields[service] = new UITextField[numLevels];
            _noiseFields[service] = new UITextField[numLevels];
            _mailFields[service] = new UITextField[numLevels];
            m_incomeFields[service] = new UITextField[numLevels];
        }

        /// <summary>
        /// Adds column headings.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        protected void AddHeadings(UIPanel panel)
        {
            // Set string references (we'll reference these multiple times with the textfields, so this saves calling translate each time).
            m_powerLabel = Translations.Translate("RPR_OPT_POW");
            m_waterLabel = Translations.Translate("RPR_OPT_WAT");
            m_garbageLabel = Translations.Translate("RPR_OPT_GAR");
            m_sewageLabel = Translations.Translate("RPR_OPT_SEW");
            _pollutionLabel = Translations.Translate("RPR_OPT_POL");
            _noiseLabel = Translations.Translate("RPR_OPT_NOI");
            _mailLabel = Translations.Translate("RPR_OPT_MAI");
            m_incomeLabel = Translations.Translate("RPR_OPT_WEA");

            // Headings.
            ColumnIcon(panel, PowerX, ColumnWidth, m_powerLabel, "ToolbarIconElectricity");
            ColumnIcon(panel, WaterX, ColumnWidth, m_waterLabel, "ToolbarIconWaterAndSewage");
            ColumnIcon(panel, GarbageX, ColumnWidth, m_garbageLabel, "InfoIconGarbage");
            ColumnIcon(panel, SewageX, ColumnWidth, m_sewageLabel, "IconPolicyFilterIndustrialWaste");
            ColumnIcon(panel, PollutionX, ColumnWidth, _pollutionLabel, "InfoIconPollution");
            ColumnIcon(panel, NoiseX, ColumnWidth, _noiseLabel, "InfoIconNoisePollution");
            ColumnIcon(panel, MailX, ColumnWidth, _mailLabel, "InfoIconPost");
            ColumnIcon(panel, IncomeX, WideColumnWidth, m_incomeLabel, "ToolbarIconMoney");
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

            // Revert button.
            UIButton revertToSaveButton = UIButtons.AddButton(panel, Button2X, m_currentY, Translations.Translate("RPR_OPT_RTS"), ButtonWidth);
            revertToSaveButton.eventClicked += (component, clickEvent) =>
            {
                ConfigurationUtils.LoadSettings();
                PopulateFields();
            };

            // Save button.
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
        protected void AddSubService(UIComponent panel, int subService, bool isExtract = false, string label = null)
        {
            // Add a row for each level within this subservice.
            for (int i = 0; i < m_powerFields[subService].Length; ++i)
            {
                // Row label.
                RowLabel(panel, m_currentY, label ?? (isExtract ? Translations.Translate(i == 0 ? "RPR_CAT_EXT" : "RPR_CAT_PRO") : Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString()));

                // Textfields.
                m_powerFields[subService][i] = AddTextField(panel, ColumnWidth, PowerX, m_currentY, m_powerLabel);
                m_waterFields[subService][i] = AddTextField(panel, ColumnWidth, WaterX, m_currentY, m_waterLabel);
                m_garbageFields[subService][i] = AddTextField(panel, ColumnWidth, GarbageX, m_currentY, m_garbageLabel);
                m_sewageFields[subService][i] = AddTextField(panel, ColumnWidth, SewageX, m_currentY, m_sewageLabel);
                _pollutionFields[subService][i] = AddTextField(panel, ColumnWidth, PollutionX, m_currentY, _pollutionLabel);
                _noiseFields[subService][i] = AddTextField(panel, ColumnWidth, NoiseX, m_currentY, _noiseLabel);
                _mailFields[subService][i] = AddTextField(panel, ColumnWidth, MailX, m_currentY, _mailLabel);
                m_incomeFields[subService][i] = AddTextField(panel, WideColumnWidth, IncomeX, m_currentY, m_incomeLabel);

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
            for (int i = 0; i < m_powerFields[subService].Length; ++i)
            {
                m_powerFields[subService][i].text = dataArray[i][DataStore.POWER].ToString();
                m_waterFields[subService][i].text = dataArray[i][DataStore.WATER].ToString();
                m_garbageFields[subService][i].text = dataArray[i][DataStore.GARBAGE].ToString();
                m_sewageFields[subService][i].text = dataArray[i][DataStore.SEWAGE].ToString();
                _pollutionFields[subService][i].text = dataArray[i][DataStore.GROUND_POLLUTION].ToString();
                _noiseFields[subService][i].text = dataArray[i][DataStore.NOISE_POLLUTION].ToString();
                _mailFields[subService][i].text = dataArray[i][DataStore.MAIL].ToString();
                m_incomeFields[subService][i].text = dataArray[i][DataStore.INCOME].ToString();
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
            for (int i = 0; i < m_powerFields[subService].Length; ++i)
            {
                PanelUtils.ParseInt(ref dataArray[i][DataStore.POWER], m_powerFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.WATER], m_waterFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.GARBAGE], m_garbageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.SEWAGE], m_sewageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.GROUND_POLLUTION], _pollutionFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.NOISE_POLLUTION], _noiseFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.MAIL], _mailFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.INCOME], m_incomeFields[subService][i].text);
            }
        }
    }
}