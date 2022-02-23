using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class PopulationPanel : PackPanelBase
    {
        // Constants.
        private const float PopCheckX = FirstItem;
        private const float FixedPopX = PopCheckX + ColumnWidth;
        private const float EmptyPercentX = FixedPopX + ColumnWidth;
        private const float EmptyAreaX = EmptyPercentX + ColumnWidth;
        private const float AreaPerX = EmptyAreaX + ColumnWidth;
        private const float MultiFloorX = AreaPerX + ColumnWidth;

        private readonly string[] serviceNames = { Translations.Translate("RPR_CAT_RES"), Translations.Translate("RPR_CAT_IND"), Translations.Translate("RPR_CAT_COM"), Translations.Translate("RPR_CAT_OFF"), Translations.Translate("RPR_CAT_SCH") };
        private readonly ItemClass.Service[] services = { ItemClass.Service.Residential, ItemClass.Service.Industrial, ItemClass.Service.Commercial, ItemClass.Service.Office, ItemClass.Service.Education };
        private readonly int[] maxLevels = { 5, 3, 3, 3, 2 };

        // Textfield arrays.
        private UITextField[] emptyAreaFields, emptyPercentFields, fixedPopFields, areaPerFields;
        private UICheckBox[] fixedPopChecks, multiFloorChecks;
        private UILabel[] rowLabels;

        // Panel components.
        private UIDropDown serviceDropDown;


        // Tab sprite name and tooltip key.
        protected override string TabSprite => "SubBarMonumentModderPackFocused";
        protected override string TabTooltipKey => "RPR_OPT_POP";
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal PopulationPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
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

                // Add title.
                float currentY = PanelUtils.TitleLabel(panel, TabTooltipKey);

                // Initialise arrays
                emptyAreaFields = new UITextField[5];
                emptyPercentFields = new UITextField[5];
                fixedPopChecks = new UICheckBox[5];
                fixedPopFields = new UITextField[5];
                areaPerFields = new UITextField[5];
                multiFloorChecks = new UICheckBox[5];
                rowLabels = new UILabel[5];

                // Service selection dropdown.
                serviceDropDown = UIControls.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_SVC"), serviceNames, -1);
                serviceDropDown.parent.relativePosition = new Vector3(20f, currentY);
                serviceDropDown.eventSelectedIndexChanged += ServiceChanged;

                // Pack selection dropdown.
                packDropDown = UIControls.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
                currentY += 70f;
                packDropDown.parent.relativePosition = new Vector3(20f, currentY);
                packDropDown.eventSelectedIndexChanged += PackChanged;

                // Label strings - cached to avoid calling Translations.Translate each time (for the tooltips, anwyay, including the others makes code more readable).
                string areaSuffix = Environment.NewLine + "(" + Measures.AreaMeasure + ")";
                string emptyArea = Translations.Translate("RPR_CAL_VOL_EMP") + areaSuffix;
                string emptyAreaTip = Translations.Translate("RPR_CAL_VOL_EMP_TIP");
                string emptyPercent = Translations.Translate("RPR_CAL_VOL_EPC");
                string emptyPercentTip = Translations.Translate("RPR_CAL_VOL_EPC_TIP");
                string useFixedPop = Translations.Translate("RPR_CAL_VOL_FXP");
                string useFixedPopTip = Translations.Translate("RPR_CAL_VOL_FXP_TIP");
                string fixedPop = Translations.Translate("RPR_CAL_VOL_UNI");
                string fixedPopTip = Translations.Translate("RPR_CAL_VOL_UNI_TIP");
                string areaPer = Translations.Translate("RPR_CAL_VOL_APU") + areaSuffix;
                string areaPerTip = Translations.Translate("RPR_CAL_VOL_APU_TIP");
                string multiFloor = Translations.Translate("RPR_CAL_VOL_MFU");
                string multiFloorTip = Translations.Translate("RPR_CAL_VOL_MFU_TIP");

                // Headings.
                currentY += 140f;
                PanelUtils.ColumnLabel(panel, EmptyAreaX, currentY, ColumnWidth, emptyArea, emptyAreaTip, 1.0f);
                PanelUtils.ColumnLabel(panel, EmptyPercentX, currentY, ColumnWidth, emptyPercent, emptyPercentTip, 1.0f);
                PanelUtils.ColumnLabel(panel, PopCheckX, currentY, ColumnWidth, useFixedPop, useFixedPopTip, 1.0f);
                PanelUtils.ColumnLabel(panel, FixedPopX, currentY, ColumnWidth, fixedPop, fixedPopTip, 1.0f);
                PanelUtils.ColumnLabel(panel, AreaPerX, currentY, ColumnWidth, areaPer, areaPerTip, 1.0f);
                PanelUtils.ColumnLabel(panel, MultiFloorX, currentY, ColumnWidth, multiFloor, multiFloorTip, 1.0f);

                // Add level textfields.
                for (int i = 0; i < 5; ++i)
                {
                    // Row label.
                    rowLabels[i] = RowLabel(panel, currentY, Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString());

                    emptyPercentFields[i] = UIControls.AddTextField(panel, EmptyPercentX + Margin, currentY, width: TextFieldWidth, tooltip: emptyPercentTip);
                    emptyPercentFields[i].eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);
                    emptyPercentFields[i].tooltipBox = TooltipUtils.TooltipBox;

                    emptyAreaFields[i] = UIControls.AddTextField(panel, EmptyAreaX + Margin, currentY, width: TextFieldWidth, tooltip: emptyAreaTip);
                    emptyAreaFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                    emptyAreaFields[i].tooltipBox = TooltipUtils.TooltipBox;

                    // Fixed pop checkboxes - ensure i is saved as objectUserData for use by event handler.  Starts unchecked by default.
                    fixedPopChecks[i] = UIControls.AddCheckBox(panel, PopCheckX + (ColumnWidth / 2), currentY, tooltip: useFixedPopTip);
                    fixedPopChecks[i].objectUserData = i;
                    fixedPopChecks[i].eventCheckChanged += FixedPopCheckChanged;
                    fixedPopChecks[i].tooltipBox = TooltipUtils.TooltipBox;

                    // Fixed population fields start hidden by default.
                    fixedPopFields[i] = UIControls.AddTextField(panel, FixedPopX + Margin, currentY, width: TextFieldWidth, tooltip: fixedPopTip);
                    fixedPopFields[i].eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);
                    fixedPopFields[i].tooltipBox = TooltipUtils.TooltipBox;
                    fixedPopFields[i].Hide();

                    areaPerFields[i] = UIControls.AddTextField(panel, AreaPerX + Margin, currentY, width: TextFieldWidth, tooltip: areaPerTip);
                    areaPerFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                    areaPerFields[i].tooltipBox = TooltipUtils.TooltipBox;

                    multiFloorChecks[i] = UIControls.AddCheckBox(panel, MultiFloorX + (ColumnWidth / 2), currentY, tooltip: multiFloorTip);
                    multiFloorChecks[i].tooltipBox = TooltipUtils.TooltipBox;

                    // Move to next row.
                    currentY += RowHeight;
                }
                
                // Add footer controls.
                PanelFooter(currentY);

                // Set service menu to initial state (residential), which will also update textfield visibility via event handler.
                serviceDropDown.selectedIndex = 0;
            }
        }


        /// <summary>
        /// Save button event handler.
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        /// </summary>
        protected override void Save(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Basic sanity check - need a valid name to proceed.
            if (!PackNameField.text.IsNullOrWhiteSpace())
            {
                base.Save(control, mouseEvent);

                // Apply update.
                FloorData.instance.CalcPackChanged(packList[packDropDown.selectedIndex]);
            }
        }


        /// <summary>
        /// 'Add new pack' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void AddPack(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Initial pack name.
            string newPackName = PackNameField.text;

            // Integer suffix for when the above name already exists (starts with 2).
            int packNum = 2;

            // Current service.
            ItemClass.Service currentService = services[serviceDropDown.selectedIndex];

            // Starting with our default new pack name, check to see if we already have a pack with this name for the currently selected service.
            while (PopData.instance.calcPacks.Find(pack => ((PopDataPack)pack).service == currentService && (pack.name.Equals(newPackName) || pack.DisplayName.Equals(newPackName))) != null)
            {
                // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                newPackName = PackNameField.text + " " + packNum++;
            }

            // We now have a unique name; set the textfield.
            PackNameField.text = newPackName;

            // Add new pack with basic values (deails will be populated later).
            VolumetricPopPack newPack = new VolumetricPopPack
            {
                version = DataVersion.customOne,
                service = services[serviceDropDown.selectedIndex],
                levels = new LevelData[maxLevels[serviceDropDown.selectedIndex]]
            };

            // Update pack with information from the panel.
            UpdatePack(newPack);

            // Add our new pack to our list of packs and update defaults panel menus.
            PopData.instance.AddCalculationPack(newPack);
            CalculationsPanel.Instance.UpdateDefaultMenus();

            // Update pack menu.
            packDropDown.items = PackList(currentService);

            // Set pack selection by iterating through each pack in the menu and looking for a match.
            for (int i = 0; i < packDropDown.items.Length; ++i)
            {
                if (packDropDown.items[i].Equals(newPack.DisplayName))
                {
                    // Got a match; apply selected index and stop looping.
                    packDropDown.selectedIndex = i;
                    break;
                }
            }

            // Save configuration file. 
            ConfigUtils.SaveSettings();
        }


        /// <summary>
        /// 'Delete pack' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void DeletePack(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Make sure it's not an inbuilt pack before proceeding.
            if (packList[packDropDown.selectedIndex].version == DataVersion.customOne)
            {
                // Remove from list of packs.
                PopData.instance.calcPacks.Remove(packList[packDropDown.selectedIndex]);

                // Regenerate pack menu.
                packDropDown.items = PackList(services[serviceDropDown.selectedIndex]);

                // Reset pack menu index.
                packDropDown.selectedIndex = 0;
            }
        }


        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update</param>
        protected override void UpdatePack(DataPack pack)
        {
            if (pack is VolumetricPopPack popPack)
            {
                // Basic pack attributes.
                pack.name = PackNameField.text;

                // Iterate through each level, parsing input fields.
                for (int i = 0; i < maxLevels[serviceDropDown.selectedIndex]; ++i)
                {
                    // Textfields.
                    PanelUtils.ParseFloat(ref popPack.levels[i].emptyArea, emptyAreaFields[i].text, true);
                    PanelUtils.ParseInt(ref popPack.levels[i].emptyPercent, emptyPercentFields[i].text);

                    // Look at fixed population checkbox state to work out if we're doing fixed population or area per.
                    if (fixedPopChecks[i].isChecked)
                    {
                        // Using fixed pop: negate the 'area per' number to denote fixed population.
                        int pop = 0;
                        PanelUtils.ParseInt(ref pop, fixedPopFields[i].text);
                        popPack.levels[i].areaPer = 0 - pop;
                    }
                    else
                    {
                        // Area per unit.
                        PanelUtils.ParseFloat(ref popPack.levels[i].areaPer, areaPerFields[i].text, true);
                    }

                    // Checkboxes.
                    popPack.levels[i].multiFloorUnits = multiFloorChecks[i].isChecked;
                }
            }
        }


        /// <summary>
        /// Service dropdown change handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="index">New selected index (unused)</param>
        private void ServiceChanged(UIComponent control, int index)
        {
            // Set textfield visibility depending on level.
            TextfieldVisibility(maxLevels[index]);

            // Reset pack menu items.
            packDropDown.items = PackList(services[index]);

            // Reset pack selection and force update of fields and button states.
            packDropDown.selectedIndex = 0;
            PopulateTextFields(0);
            ButtonStates(0);
        }


        /// <summary>
        /// Calculation pack dropdown change handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="index">New selected index (unused)</param>
        private void PackChanged(UIComponent control, int index)
        {
            // Populate text fields.
            PopulateTextFields(index);

            // Set service menu by iterating through list of services looking for a match.
            for (int i = 0; i < services.Length; ++i)
            {
                if (services[i] == ((VolumetricPopPack)packList[index]).service)
                {
                    // Found a service match; select it and stop looping.
                    serviceDropDown.selectedIndex = i;
                    break;
                }
            }

            // Update button states.
            ButtonStates(index);
        }


        /// <summary>
        /// Shows/hides textfields according to the provided maximum level to show.
        /// </summary>
        /// <param name="maxLevel">Maximum number of levels to show</param>
        private void TextfieldVisibility(int maxLevel)
        {
            // Iterate through all fields.
            for (int i = 0; i < 5; ++i)
            {
                // If this level is less than the maximum given, show.
                if (i < maxLevel)
                {
                    rowLabels[i].Show();
                    fixedPopChecks[i].Show();

                    // Arear per or fixed population, depending on fixed pop check state.
                    if (fixedPopChecks[i].isChecked)
                    {
                        fixedPopFields[i].Show();
                    }
                    else
                    {
                        emptyAreaFields[i].Show();
                        emptyPercentFields[i].Show();
                        areaPerFields[i].Show();
                        multiFloorChecks[i].Show();
                    }
                }
                else
                {
                    // Otherwise, hide.
                    rowLabels[i].Hide();
                    fixedPopChecks[i].Hide();

                    fixedPopFields[i].Hide();
                    emptyAreaFields[i].Hide();
                    emptyPercentFields[i].Hide();
                    areaPerFields[i].Hide();
                    multiFloorChecks[i].Hide();
                }
            }
        }


        /// <summary>
        /// Event handler for fixed populaton checkboxes.
        /// Updates fixed population/area per textfield visibility based on state.
        /// </summary>
        /// <param name="control">Calling UIComponent</param>
        /// <param name="isChecked">New isChecked state</param>
        private void FixedPopCheckChanged(UIComponent control, bool isChecked)
        {
            // Get stored index of calling checkbox.
            int index = (int)control.objectUserData;

            fixedPopFields[index].isVisible = isChecked;

            emptyAreaFields[index].isVisible = !isChecked;
            emptyPercentFields[index].isVisible = !isChecked;
            areaPerFields[index].isVisible = !isChecked;
            multiFloorChecks[index].isVisible = !isChecked;
        }


        /// <summary>
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            VolumetricPopPack volPack = (VolumetricPopPack)packList[index];

            // Set name field.
            PackNameField.text = volPack.DisplayName;

            // Set service selection menu by iterating through each service and looking for a match.
            for (int i = 0; i < services.Length; ++i)
            {
                if (services[i] == volPack.service)
                {
                    // Got a match; apply selected index and stop looping.
                    // This also applies text field visibility via the service menue event handler.
                    serviceDropDown.selectedIndex = i;
                    break;
                }    
            }

            // Iterate through each level in the pack and populate the relevant row.
            for (int i = 0; i < volPack.levels.Length; ++i)
            {
                // Local reference.
                LevelData level = volPack.levels[i];

                // Populate controls.
                emptyAreaFields[i].text = Measures.AreaFromMetric(level.emptyArea).ToString("N1");
                emptyPercentFields[i].text = level.emptyPercent.ToString();
                fixedPopChecks[i].isChecked = level.areaPer < 0;
                areaPerFields[i].text = Measures.AreaFromMetric(Math.Abs(level.areaPer)).ToString("N1");
                fixedPopFields[i].text = Math.Abs(level.areaPer).ToString();
                multiFloorChecks[i].isChecked = level.multiFloorUnits;
            }
        }


        /// <summary>
        /// (Re)builds the list of available packs.
        /// </summary>
        /// <param name="service">Service index</param>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items)</returns>
        private string[] PackList(ItemClass.Service service)
        {
            // Re-initialise pack list.
            packList = new List<DataPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (PopDataPack calcPack in PopData.instance.calcPacks)
            {
                // Check for custom packs.
                if (calcPack is VolumetricPopPack volPack && volPack.service == service)
                {
                    // Found one - add to our lists.
                    packList.Add(volPack);
                    packNames.Add(volPack.DisplayName);
                }
            }

            return packNames.ToArray();
        }
    }
}