using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class FloorPanel : PackPanelBase
    {
        // Constants.
        protected const float FloorHeightX = FirstItem;
        protected const float FirstMinX = FloorHeightX + ColumnWidth;
        protected const float FirstMaxX = FirstMinX + ColumnWidth;
        protected const float FirstEmptyX = FirstMaxX + ColumnWidth;
        protected const float MultiFloorX = FirstEmptyX + ColumnWidth;

        // Textfield arrays.
        protected UITextField floorHeightField, firstMinField, firstExtraField;
        protected UICheckBox firstEmptyCheck;


        // Tab sprite name and tooltip key.
        protected override string TabSprite => "ToolbarIconZoomOutCity";
        protected override string TabTooltipKey => "RPR_OPT_STO";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal FloorPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
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
                floorHeightField = new UITextField();
                firstMinField = new UITextField();
                firstExtraField = new UITextField();
                firstEmptyCheck = new UICheckBox();

                // Pack selection dropdown.
                packDropDown = UIControls.AddPlainDropDown(panel, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
                packDropDown.parent.relativePosition = new Vector3(20f, currentY);
                packDropDown.eventSelectedIndexChanged += PackChanged;

                // Headings.
                currentY += 140f;
                PanelUtils.ColumnLabel(panel, FloorHeightX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FLH"), Translations.Translate("RPR_CAL_VOL_FLH_TIP"), 1.0f);
                PanelUtils.ColumnLabel(panel, FirstMinX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMN"), Translations.Translate("RPR_CAL_VOL_FMN_TIP"), 1.0f);
                PanelUtils.ColumnLabel(panel, FirstMaxX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMX"), Translations.Translate("RPR_CAL_VOL_FMX_TIP"), 1.0f);
                PanelUtils.ColumnLabel(panel, FirstEmptyX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_IGF"), Translations.Translate("RPR_CAL_VOL_IGF_TIP"), 1.0f);

                // Add level textfields.
                currentY += RowHeight;
                floorHeightField = UIControls.AddTextField(panel, FloorHeightX + Margin, currentY, width: TextFieldWidth, tooltip: Translations.Translate("RPR_CAL_VOL_FLH_TIP"));
                floorHeightField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                floorHeightField.tooltipBox = TooltipUtils.TooltipBox;

                firstMinField = UIControls.AddTextField(panel, FirstMinX + Margin, currentY, width: TextFieldWidth, tooltip: Translations.Translate("RPR_CAL_VOL_FMN_TIP"));
                firstMinField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                firstMinField.tooltipBox = TooltipUtils.TooltipBox;

                firstExtraField = UIControls.AddTextField(panel, FirstMaxX + Margin, currentY, width: TextFieldWidth, tooltip: Translations.Translate("RPR_CAL_VOL_FMX_TIP"));
                firstExtraField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                firstExtraField.tooltipBox = TooltipUtils.TooltipBox;

                firstEmptyCheck = UIControls.AddCheckBox(panel, FirstEmptyX + (ColumnWidth / 2), currentY, tooltip: Translations.Translate("RPR_CAL_VOL_IGF_TIP"));
                firstEmptyCheck.tooltipBox = TooltipUtils.TooltipBox;

                // Move to next row.
                currentY += RowHeight;

                // Add footer controls.
                PanelFooter(currentY);

                // Populate pack menu and set onitial pack selection.
                packDropDown.items = PackList();
                packDropDown.selectedIndex = 0;
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

            // Starting with our default new pack name, check to see if we already have a pack with this name for the currently selected service.
            while (FloorData.instance.calcPacks.Find(pack => pack.name.Equals(newPackName)) != null)
            {
                // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                newPackName = "New pack " + packNum++;
            }

            // We now have a unique name; set the textfield.
            PackNameField.text = newPackName;

            // Add new pack with basic values (deails will be populated later).
            FloorDataPack newPack = new FloorDataPack
            {
                version = (int)DataVersion.customOne
            };

            // Update pack with information from the panel.
            UpdatePack(newPack);

            // Add our new pack to our list of packs and update defaults panel menus.
            FloorData.instance.AddCalculationPack(newPack);
            CalculationsPanel.Instance.UpdateDefaultMenus();

            // Update pack menu.
            packDropDown.items = PackList();

            // Set pack selection by iterating through each pack in the menu and looking for a match.
            for (int i = 0; i < packDropDown.items.Length; ++i)
            {
                if (packDropDown.items[i].Equals(newPack.displayName))
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
            if (packList[packDropDown.selectedIndex].version == (int)DataVersion.customOne)
            {
                // Remove from list of packs.
                FloorData.instance.calcPacks.Remove(packList[packDropDown.selectedIndex]);

                // Regenerate pack menu.
                packDropDown.items = PackList();

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
            if (pack is FloorDataPack floorPack)
            {
                // Basic pack attributes.
                floorPack.name = PackNameField.text;
                floorPack.displayName = PackNameField.text;
                floorPack.version = (int)DataVersion.customOne;

                // Textfields.
                PanelUtils.ParseFloat(ref floorPack.floorHeight, floorHeightField.text);
                PanelUtils.ParseFloat(ref floorPack.firstFloorMin, firstMinField.text);
                PanelUtils.ParseFloat(ref floorPack.firstFloorExtra, firstExtraField.text);

                // Checkboxes.
                floorPack.firstFloorEmpty = firstEmptyCheck.isChecked;
            }
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

            // Update button states.
            ButtonStates(index);
        }


        /// <summary>
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            FloorDataPack floorPack = (FloorDataPack)packList[index];

            // Set name field.
            PackNameField.text = floorPack.displayName ?? floorPack.name;

            // Populate controls.
            floorHeightField.text = floorPack.floorHeight.ToString();
            firstMinField.text = floorPack.firstFloorMin.ToString();
            firstExtraField.text = floorPack.firstFloorExtra.ToString();
            firstEmptyCheck.isChecked = floorPack.firstFloorEmpty;
        }


        /// <summary>
        /// (Re)builds the list of available packs.
        /// </summary>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items)</returns>
        private string[] PackList()
        {
            // Re-initialise pack list.
            packList = new List<DataPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (DataPack calcPack in FloorData.instance.calcPacks)
            {
                // Found one - add to our lists.
                packList.Add((FloorDataPack)calcPack);
                packNames.Add(calcPack.displayName ?? calcPack.name);
            }

            return packNames.ToArray();
        }
    }
}