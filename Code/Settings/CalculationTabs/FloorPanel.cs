// <copyright file="FloorPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    internal class FloorPanel : PackPanelBase
    {
        // Constants.
        private const float FloorHeightX = FirstItem;
        private const float FirstMinX = FloorHeightX + ColumnWidth;
        private const float FirstMaxX = FirstMinX + ColumnWidth;
        private const float FirstEmptyX = FirstMaxX + ColumnWidth;
        private const float MultiFloorX = FirstEmptyX + ColumnWidth;

        // Panel components.
        private UITextField _floorHeightField;
        private UITextField _firstMinField;
        private UITextField _firstExtraField;
        private UICheckBox _firstEmptyCheck;

        /// <summary>
        /// Initializes a new instance of the <see cref="FloorPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal FloorPanel(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets the icon sprite name for this tab.
        /// </summary>
        protected override string TabSprite => "ToolbarIconZoomOutCity";

        /// <summary>
        /// Gets the tooltip translation key for this tab.
        /// </summary>
        protected override string TabTooltipKey => "RPR_OPT_STO";

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

                // Add title.
                float currentY = PanelUtils.TitleLabel(m_panel, TabTooltipKey);

                // Initialise arrays
                _floorHeightField = new UITextField();
                _firstMinField = new UITextField();
                _firstExtraField = new UITextField();
                _firstEmptyCheck = new UICheckBox();

                // Pack selection dropdown.
                m_packDropDown = UIDropDowns.AddPlainDropDown(m_panel, 20f, currentY, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
                m_packDropDown.eventSelectedIndexChanged += PackChanged;

                // Headings.
                currentY += 160f;
                string lengthSuffix = System.Environment.NewLine + "(" + Measures.LengthMeasure + ")";
                PanelUtils.ColumnLabel(m_panel, FloorHeightX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FLH") + lengthSuffix, Translations.Translate("RPR_CAL_VOL_FLH_TIP"), 1.0f);
                PanelUtils.ColumnLabel(m_panel, FirstMinX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMN") + lengthSuffix, Translations.Translate("RPR_CAL_VOL_FMN_TIP"), 1.0f);
                PanelUtils.ColumnLabel(m_panel, FirstMaxX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_FMX") + lengthSuffix, Translations.Translate("RPR_CAL_VOL_FMX_TIP"), 1.0f);
                PanelUtils.ColumnLabel(m_panel, FirstEmptyX, currentY, ColumnWidth, Translations.Translate("RPR_CAL_VOL_IGF"), Translations.Translate("RPR_CAL_VOL_IGF_TIP"), 1.0f);

                // Add level textfields.
                _floorHeightField = UITextFields.AddTextField(m_panel, FloorHeightX + Margin, currentY, width: TextFieldWidth, tooltip: Translations.Translate("RPR_CAL_VOL_FLH_TIP"));
                _floorHeightField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);

                _firstMinField = UITextFields.AddTextField(m_panel, FirstMinX + Margin, currentY, width: TextFieldWidth, tooltip: Translations.Translate("RPR_CAL_VOL_FMN_TIP"));
                _firstMinField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                _firstMinField.tooltipBox = UIToolTips.WordWrapToolTip;

                _firstExtraField = UITextFields.AddTextField(m_panel, FirstMaxX + Margin, currentY, width: TextFieldWidth, tooltip: Translations.Translate("RPR_CAL_VOL_FMX_TIP"));
                _firstExtraField.eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                _firstExtraField.tooltipBox = UIToolTips.WordWrapToolTip;
                _firstEmptyCheck = UICheckBoxes.AddCheckBox(m_panel, FirstEmptyX + (ColumnWidth / 2), currentY, tooltip: Translations.Translate("RPR_CAL_VOL_IGF_TIP"));
                _firstEmptyCheck.tooltipBox = UIToolTips.WordWrapToolTip;

                // Add space before footer.
                currentY += RowHeight;

                // Add footer controls.
                PanelFooter(currentY);

                // Populate pack menu and set onitial pack selection.
                m_packDropDown.items = PackList();
                m_packDropDown.selectedIndex = 0;
            }
        }

        /// <summary>
        /// Save button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void Save(UIComponent c, UIMouseEventParameter p)
        {
            // Basic sanity check - need a valid name to proceed.
            if (!PackNameField.text.IsNullOrWhiteSpace())
            {
                base.Save(c, p);

                // Apply update.
                FloorData.Instance.CalcPackChanged(m_packList[m_packDropDown.selectedIndex]);
            }
        }

        /// <summary>
        /// 'Add new pack' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void AddPack(UIComponent c, UIMouseEventParameter p)
        {
            // Initial pack name.
            string newPackName = PackNameField.text;

            // Integer suffix for when the above name already exists (starts with 2).
            int packNum = 2;

            // Starting with our default new pack name, check to see if we already have a pack with this name for the currently selected service.
            while (FloorData.Instance.CalcPacks.Find(pack => pack.Name.Equals(newPackName) || pack.DisplayName.Equals(newPackName)) != null)
            {
                // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                newPackName = PackNameField.text + " " + packNum++;
            }

            // We now have a unique name; set the textfield.
            PackNameField.text = newPackName;

            // Add new pack with basic values (deails will be populated later).
            FloorDataPack newPack = new FloorDataPack(DataPack.DataVersion.CustomOne);

            // Update pack with information from the panel.
            UpdatePack(newPack);

            // Add our new pack to our list of packs and update defaults panel menus.
            FloorData.Instance.AddCalculationPack(newPack);
            CalculationsPanel.Instance.UpdateDefaultMenus();

            // Update pack menu.
            m_packDropDown.items = PackList();

            // Set pack selection by iterating through each pack in the menu and looking for a match.
            for (int i = 0; i < m_packDropDown.items.Length; ++i)
            {
                if (m_packDropDown.items[i].Equals(newPack.DisplayName))
                {
                    // Got a match; apply selected index and stop looping.
                    m_packDropDown.selectedIndex = i;
                    break;
                }
            }

            // Save configuration file.
            ConfigurationUtils.SaveSettings();
        }

        /// <summary>
        /// 'Delete pack' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void DeletePack(UIComponent c, UIMouseEventParameter p)
        {
            // Make sure it's not an inbuilt pack before proceeding.
            if (m_packList[m_packDropDown.selectedIndex].Version == DataPack.DataVersion.CustomOne)
            {
                // Remove from list of packs.
                FloorData.Instance.CalcPacks.Remove(m_packList[m_packDropDown.selectedIndex]);

                // Regenerate pack menu.
                m_packDropDown.items = PackList();

                // Reset pack menu index.
                m_packDropDown.selectedIndex = 0;
            }
        }

        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update.</param>
        protected override void UpdatePack(DataPack pack)
        {
            if (pack is FloorDataPack floorPack)
            {
                // Basic pack attributes.
                floorPack.Name = PackNameField.text;

                // Textfields.
                PanelUtils.ParseFloat(ref floorPack.m_floorHeight, _floorHeightField.text, false);
                PanelUtils.ParseFloat(ref floorPack.m_firstFloorMin, _firstMinField.text, false);
                PanelUtils.ParseFloat(ref floorPack.m_firstFloorExtra, _firstExtraField.text, false);

                // Checkboxes.
                floorPack.m_firstFloorEmpty = _firstEmptyCheck.isChecked;
            }
        }

        /// <summary>
        /// Calculation pack dropdown change handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="index">New selected index.</param>
        private void PackChanged(UIComponent c, int index)
        {
            // Populate text fields.
            PopulateTextFields(index);

            // Update button states.
            ButtonStates(index);
        }

        /// <summary>
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack.</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            FloorDataPack floorPack = (FloorDataPack)m_packList[index];

            // Set name field.
            PackNameField.text = floorPack.DisplayName;

            // Populate controls.
            _floorHeightField.text = Measures.LengthFromMetric(floorPack.m_floorHeight).ToString("N1");
            _firstMinField.text = Measures.LengthFromMetric(floorPack.m_firstFloorMin).ToString("N1");
            _firstExtraField.text = Measures.LengthFromMetric(floorPack.m_firstFloorExtra).ToString("N1");
            _firstEmptyCheck.isChecked = floorPack.m_firstFloorEmpty;
        }

        /// <summary>
        /// (Re)builds the list of available packs.
        /// </summary>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items).</returns>
        private string[] PackList()
        {
            // Re-initialise pack list.
            m_packList = new List<DataPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (DataPack calcPack in FloorData.Instance.CalcPacks)
            {
                // Found one - add to our lists.
                m_packList.Add((FloorDataPack)calcPack);
                packNames.Add(calcPack.DisplayName);
            }

            return packNames.ToArray();
        }
    }
}