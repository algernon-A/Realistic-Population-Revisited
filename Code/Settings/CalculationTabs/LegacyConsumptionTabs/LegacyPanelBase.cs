using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Base class for options panel consumption settings (sub-)tabs (legacy configuration).
    /// </summary>
    internal abstract class LegacyPanelBase : TextfieldPanelBase
    {
        // UI layout constants.
        protected const float LeftTitle = 50f;
        protected const float ColumnWidth = 50f;
        protected const float Column1Width = 100f;
        protected const float Column8Width = 55f;
        protected const float Column2 = Column1 + Column1Width + Margin;
        protected const float Column3 = Column2 + ColumnWidth + Margin;
        protected const float Column4 = Column3 + ColumnWidth + Margin + Margin;
        protected const float Column5 = Column4 + ColumnWidth + Margin;
        protected const float Column6 = Column5 + ColumnWidth + Margin;
        protected const float Column7 = Column6 + ColumnWidth + Margin;
        protected const float Column8 = Column7 + Column8Width + Margin;
        protected const float Column9 = Column8 + Column8Width + Margin;


        // Textfield array.
        protected UITextField[][] areaFields, floorFields, extraFloorFields, productionFields;

        // Column labels.
        protected string areaLabel;
        protected string floorLabel;
        protected string extraFloorLabel;
        protected string productionLabel;


        // Tab title translation key.
        protected abstract string TabNameKey { get; }


        /// <summary>
        /// Constructor - adds tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal LegacyPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab.
            panel = PanelUtils.AddTextTab(tabStrip, Translations.Translate(TabNameKey), tabIndex, out UIButton _);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Saves the current legacy config.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void SaveLegacy()
        {
            // Set flag to show that user has instructed to save legacy file.
            XMLUtilsWG.writeToLegacy = true;

            // Save legacy data using WG serialization.
            XMLUtilsWG.WriteToXML();

            // Save new configuration file with updated consumption info.
            ConfigUtils.SaveSettings();
        }


        /// <summary>
        /// Initialises array structure.
        /// </summary>
        /// <param name="numSubServices">Number of sub-services to initialise for</param>
        protected void SetupArrays(int numSubServices)
        {
            // Initialise textfield array.
            areaFields = new UITextField[numSubServices][];
            floorFields = new UITextField[numSubServices][];
            extraFloorFields = new UITextField[numSubServices][];
            powerFields = new UITextField[numSubServices][];
            waterFields = new UITextField[numSubServices][];
            sewageFields = new UITextField[numSubServices][];
            garbageFields = new UITextField[numSubServices][];
            incomeFields = new UITextField[numSubServices][];
            productionFields = new UITextField[numSubServices][];
        }


        /// <summary>
        /// Adds column headings.
        /// </summary>
        /// <param name="panel">UI panel instance</param>
        protected void AddHeadings(UIPanel panel)
        {
            // Set string references (we'll reference these multiple times with the textfields, so this saves calling translate each time).
            areaLabel = Translations.Translate(notResidential ? "RPR_OPT_APW" : "RPR_OPT_APH");
            floorLabel = Translations.Translate("RPR_OPT_FLR");
            extraFloorLabel = Translations.Translate("RPR_CAL_FLR_M");
            powerLabel = Translations.Translate("RPR_OPT_POW");
            waterLabel = Translations.Translate("RPR_OPT_WAT");
            sewageLabel = Translations.Translate("RPR_OPT_SEW");
            garbageLabel = Translations.Translate("RPR_OPT_GAR");
            wealthLabel = Translations.Translate("RPR_OPT_WEA");
            productionLabel = Translations.Translate("RPR_OPT_PRO");

            // Headings.
            PanelUtils.ColumnLabel(panel, Column1, TitleHeight, Column1Width + Margin, areaLabel, areaLabel, 1.0f);
            PanelUtils.ColumnLabel(panel, Column2, TitleHeight, ColumnWidth + Margin, floorLabel, floorLabel, 1.0f);
            ColumnIcon(panel, Column4, ColumnWidth, powerLabel, "ToolbarIconElectricity");
            ColumnIcon(panel, Column5, ColumnWidth, waterLabel, "ToolbarIconWaterAndSewage");
            ColumnIcon(panel, Column6, ColumnWidth, sewageLabel, "ToolbarIconWaterAndSewageDisabled");
            ColumnIcon(panel, Column7, ColumnWidth, garbageLabel, "InfoIconGarbage");
            ColumnIcon(panel, Column8, Column8Width, wealthLabel, "ToolbarIconMoney");
            ColumnIcon(panel, Column9, ColumnWidth, productionLabel, "IconPolicyAutomatedSorting");

            // Bonus floors.
            if (notResidential)
            {
                PanelUtils.ColumnLabel(panel, Column3, TitleHeight, ColumnWidth + Margin, extraFloorLabel, extraFloorLabel, 0.8f);
            }
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

            UIButton revertToSaveButton = UIControls.AddButton(panel, Button2X, currentY, Translations.Translate("RPR_OPT_RTS"), ButtonWidth);
            revertToSaveButton.eventClicked += (component, clickEvent) => { XMLUtilsWG.ReadFromXML(); PopulateFields(); };

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
        protected void AddSubService(UIPanel panel, int subService, bool isExtract = false, string label = null)
        {
            // Add a row for each level within this subservice.
            for (int i = 0; i < areaFields[subService].Length; ++i)
            {
                // Row label.
                RowLabel(panel, currentY, label ?? (isExtract ? Translations.Translate(i == 0 ? "RPR_CAT_EXT" : "RPR_CAT_PRO") : Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString()));

                // Textfields.
                areaFields[subService][i] = AddTextField(panel, Column1Width, Column1, currentY, areaLabel);
                floorFields[subService][i] = AddTextField(panel, ColumnWidth, Column2, currentY, floorLabel);
                powerFields[subService][i] = AddTextField(panel, ColumnWidth, Column4, currentY, powerLabel);
                waterFields[subService][i] = AddTextField(panel, ColumnWidth, Column5, currentY, waterLabel);
                sewageFields[subService][i] = AddTextField(panel, ColumnWidth, Column6, currentY, sewageLabel);
                garbageFields[subService][i] = AddTextField(panel, ColumnWidth, Column7, currentY, garbageLabel);
                incomeFields[subService][i] = AddTextField(panel, Column8Width, Column8, currentY, wealthLabel);
                productionFields[subService][i] = AddTextField(panel, ColumnWidth, Column9, currentY, productionLabel);

                // Bonus levels.
                if (notResidential)
                {
                    extraFloorFields[subService][i] = AddTextField(panel, ColumnWidth, Column3, currentY, extraFloorLabel);
                }

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
            for (int i = 0; i < areaFields[subService].Length; ++i)
            {
                areaFields[subService][i].text = dataArray[i][DataStore.PEOPLE].ToString();
                floorFields[subService][i].text = dataArray[i][DataStore.LEVEL_HEIGHT].ToString();
                powerFields[subService][i].text = dataArray[i][DataStore.POWER].ToString();
                waterFields[subService][i].text = dataArray[i][DataStore.WATER].ToString();
                sewageFields[subService][i].text = dataArray[i][DataStore.SEWAGE].ToString();
                garbageFields[subService][i].text = dataArray[i][DataStore.GARBAGE].ToString();
                incomeFields[subService][i].text = dataArray[i][DataStore.INCOME].ToString();
                productionFields[subService][i].text = dataArray[i][DataStore.PRODUCTION].ToString();

                // Extra floor field, if applicable.
                if (!(this is LegacyResidentialPanel))
                {
                    extraFloorFields[subService][i].text = dataArray[i][DataStore.DENSIFICATION].ToString();
                }
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
            for (int i = 0; i < areaFields[subService].Length; ++i)
            {
                PanelUtils.ParseInt(ref dataArray[i][DataStore.PEOPLE], areaFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.LEVEL_HEIGHT], floorFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.POWER], powerFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.WATER], waterFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.SEWAGE], sewageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.GARBAGE], garbageFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.INCOME], incomeFields[subService][i].text);
                PanelUtils.ParseInt(ref dataArray[i][DataStore.PRODUCTION], productionFields[subService][i].text);

                // Extra floor field, if applicable.
                if (!(this is LegacyResidentialPanel))
                {
                    PanelUtils.ParseInt(ref dataArray[i][DataStore.DENSIFICATION], extraFloorFields[subService][i].text);
                }
            }
        }
    }
}