using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Base class for options panel consumption settings (sub-)tabs (new configuration).
    /// </summary>
    internal abstract class ConsumptionPanelBase : TextfieldPanelBase
    {
        // Constants.
        protected const float PowerX = 275f;
        protected const float ColumnWidth = 45f;
        protected const float WideColumnWidth = 60f;
        protected const float WaterX = PowerX + ColumnWidth + Margin;
        protected const float GarbageX = WaterX + ColumnWidth + (Margin * 4);
        protected const float SewageX = GarbageX + ColumnWidth + Margin;
        protected const float PollutionX = SewageX + ColumnWidth + Margin;
        protected const float NoiseX = PollutionX + ColumnWidth + Margin;
        protected const float MailX = NoiseX + ColumnWidth + (Margin * 4);
        protected const float IncomeX = MailX + ColumnWidth + Margin;
        protected const float FinalX = MailX + WideColumnWidth;


        // Textfield array.
        protected UITextField[][] pollutionFields;
        protected UITextField[][] noiseFields;
        protected UITextField[][] mailFields;

        // Column labels.
        protected string pollutionLabel;
        protected string noiseLabel;
        protected string mailLabel;

        // Tab icons.
        protected readonly string[] tabIconNames =
        {
            "ToolbarIconElectricity",
            "ToolbarIconWaterAndSewage",
            "InfoIconGarbage",
            "InfoIconNoisePollution",
            "ToolbarIconMoney"
        };
        protected readonly string[] tabAtlasNames =
        {
            "ingame",
            "ingame",
            "ingame",
            "ingame",
            "ingame"
        };


        /// <summary>
        /// Constructor - adds tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ConsumptionPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = PanelUtils.AddIconTab(tabStrip, Translations.Translate("RPR_OPT_CON"), tabIndex, tabIconNames, tabAtlasNames);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Initialises array structure (first dimension - sub-services).
        /// </summary>
        /// <param name="numSubServices">Number of sub-services to initialise for</param>
        protected void SubServiceArrays(int numSubServices)
        {
            // Initialise textfield array.
            powerFields = new UITextField[numSubServices][];
            waterFields = new UITextField[numSubServices][];
            garbageFields = new UITextField[numSubServices][];
            sewageFields = new UITextField[numSubServices][];
            incomeFields = new UITextField[numSubServices][];
            pollutionFields = new UITextField[numSubServices][];
            noiseFields = new UITextField[numSubServices][];
            mailFields = new UITextField[numSubServices][];
        }


        /// <summary>
        /// Initialises array structure (second dimension - levels).
        /// </summary>
        /// <param name="service">Sub-service index to initialise</param>
        /// <param name="numLevels">Number of levels to initialise for</param>
        protected void LevelArrays(int service, int numLevels)
        {
            powerFields[service] = new UITextField[numLevels];
            waterFields[service] = new UITextField[numLevels];
            garbageFields[service] = new UITextField[numLevels];
            sewageFields[service] = new UITextField[numLevels];
            pollutionFields[service] = new UITextField[numLevels];
            noiseFields[service] = new UITextField[numLevels];
            mailFields[service] = new UITextField[numLevels];
            incomeFields[service] = new UITextField[numLevels];
        }


        /// <summary>
        /// Adds column headings.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void AddHeadings(UIPanel panel)
        {
            // Set string references (we'll reference these multiple times with the textfields, so this saves calling translate each time).
            powerLabel = Translations.Translate("RPR_OPT_POW");
            waterLabel = Translations.Translate("RPR_OPT_WAT");
            garbageLabel = Translations.Translate("RPR_OPT_GAR");
            sewageLabel = Translations.Translate("RPR_OPT_SEW");
            pollutionLabel = Translations.Translate("RPR_OPT_POL");
            noiseLabel = Translations.Translate("RPR_OPT_NOI");
            mailLabel = Translations.Translate("RPR_OPT_MAI");
            wealthLabel = Translations.Translate("RPR_OPT_WEA");

            // Headings.
            ColumnIcon(panel, PowerX, ColumnWidth, powerLabel, "ToolbarIconElectricity");
            ColumnIcon(panel, WaterX, ColumnWidth, waterLabel, "ToolbarIconWaterAndSewage");
            ColumnIcon(panel, GarbageX, ColumnWidth, garbageLabel, "InfoIconGarbage");
            ColumnIcon(panel, SewageX, ColumnWidth, sewageLabel, "IconPolicyFilterIndustrialWaste");
            ColumnIcon(panel, PollutionX, ColumnWidth, pollutionLabel, "InfoIconPollution");
            ColumnIcon(panel, NoiseX, ColumnWidth, noiseLabel, "InfoIconNoisePollution");
            ColumnIcon(panel, MailX, ColumnWidth, mailLabel, "InfoIconPost");
            ColumnIcon(panel, IncomeX, WideColumnWidth, wealthLabel, "ToolbarIconMoney");
        }


        /// <summary>
        /// Adds control buttons to the bottom of the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void AddButtons(UIPanel panel)
        {
            // Add extra space.
            currentY += Margin;

            // Reset button.
            UIButton resetButton = UIControls.AddButton(panel, Button1X, currentY, Translations.Translate("RPR_OPT_RTD"), ButtonWidth);
            resetButton.eventClicked += (component, clickEvent) => ResetToDefaults();

            // Revert button.
            UIButton revertToSaveButton = UIControls.AddButton(panel, Button2X, currentY, Translations.Translate("RPR_OPT_RTS"), ButtonWidth);
            revertToSaveButton.eventClicked += (component, clickEvent) => { ConfigUtils.LoadSettings(); PopulateFields(); };

            // Save button.
            UIButton saveButton = UIControls.AddButton(panel, Button3X, currentY, Translations.Translate("RPR_OPT_SAA"), ButtonWidth);
            saveButton.eventClicked += (component, clickEvent) => ApplyFields();
        }


        /// <summary>
        /// Adds a sub-service field group to the panel.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        /// <param name="subService">Subservice reference number</param>
        /// <param name="isExtract">Set this to true (and label to null) to add extractor/processor labels (default false, which is plain level labels)</param>
        /// <param name="label">Text label base for each row; null (default) to use level numbers or extractor/prcessor</param>
        protected void AddSubService(UIPanel panel, bool _, int subService, bool isExtract = false, string label = null)
        {
            // Add a row for each level within this subservice.
            for (int i = 0; i < powerFields[subService].Length; ++i)
            {
                // Row label.
                RowLabel(panel, currentY, label ?? (isExtract ? Translations.Translate(i == 0 ? "RPR_CAT_EXT" : "RPR_CAT_PRO") : Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString()));
                
                // Textfields.
                powerFields[subService][i] = AddTextField(panel, ColumnWidth, PowerX, currentY, powerLabel);
                waterFields[subService][i] = AddTextField(panel, ColumnWidth, WaterX, currentY, waterLabel);
                garbageFields[subService][i] = AddTextField(panel, ColumnWidth, GarbageX, currentY, garbageLabel);
                sewageFields[subService][i] = AddTextField(panel, ColumnWidth, SewageX, currentY, sewageLabel);
                pollutionFields[subService][i] = AddTextField(panel, ColumnWidth, PollutionX, currentY, pollutionLabel);
                noiseFields[subService][i] = AddTextField(panel, ColumnWidth, NoiseX, currentY, noiseLabel);
                mailFields[subService][i] = AddTextField(panel, ColumnWidth, MailX, currentY, mailLabel);
                incomeFields[subService][i] = AddTextField(panel, WideColumnWidth, IncomeX, currentY, wealthLabel);

                // Increment Y position.
                currentY += RowHeight;
            }

            // Add an extra bit of space at the end.
            currentY += Margin;
        }


        /// <summary>
        /// Populates the text fields for a given subservice with information from the DataStore.
        /// </summary>
        /// <param name="dataArray">DataStore data array for the SubService</param>
        /// <param name="subService">SubService reference number</param>
        protected void PopulateSubService(int[][] dataArray, int subService)
        {
            // Iterate though each level, populating each row as we go.
            for (int i = 0; i < powerFields[subService].Length; ++i)
            {
                powerFields[subService][i].text = dataArray[i][DataStore.POWER].ToString();
                waterFields[subService][i].text = dataArray[i][DataStore.WATER].ToString();
                garbageFields[subService][i].text = dataArray[i][DataStore.GARBAGE].ToString();
                sewageFields[subService][i].text = dataArray[i][DataStore.SEWAGE].ToString();
                pollutionFields[subService][i].text = dataArray[i][DataStore.GROUND_POLLUTION].ToString();
                noiseFields[subService][i].text = dataArray[i][DataStore.NOISE_POLLUTION].ToString();
                mailFields[subService][i].text = dataArray[i][DataStore.MAIL].ToString();
                incomeFields[subService][i].text = dataArray[i][DataStore.INCOME].ToString();
            }
        }


        /// <summary>
        /// Updates the DataStore for a given SubService with information from text fields. 
        /// </summary>
        /// <param name="dataArray">DataStore data array for the SubService</param>
        /// <param name="subService">SubService reference number</param>
        protected void ApplySubService(int[][] dataArray, int subService)
        {
            // Iterate though each level, populating each row as we go.
            for (int i = 0; i < powerFields[subService].Length; ++i)
            {
                PanelUtils.ParseInt(ref dataArray[i][DataStore.POWER], powerFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.WATER], waterFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.GARBAGE], garbageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.SEWAGE], sewageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.GROUND_POLLUTION], pollutionFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.NOISE_POLLUTION], noiseFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.MAIL], mailFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.INCOME], incomeFields[subService][i].text);
            }
        }
    }
}