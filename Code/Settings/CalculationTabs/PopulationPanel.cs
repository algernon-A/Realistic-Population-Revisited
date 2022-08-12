// <copyright file="PopulationPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;

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
        private UITextField[] _emptyAreaFields;
        private UITextField[] _emptyPercentFields;
        private UITextField[] _fixedPopFields;
        private UITextField[] _areaPerFields;
        private UICheckBox[] _fixedPopChecks;
        private UICheckBox[] _multiFloorChecks;
        private UILabel[] rowLabels;

        // Panel components.
        private UIDropDown _serviceDropDown;

        /// <summary>
        /// Initializes a new instance of the <see cref="PopulationPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal PopulationPanel(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets the icon sprite name for this tab.
        /// </summary>
        protected override string TabSprite => "SubBarMonumentModderPackFocused";

        /// <summary>
        /// Gets the tooltip translation key for this tab.
        /// </summary>
        protected override string TabTooltipKey => "RPR_OPT_POP";

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
                _emptyAreaFields = new UITextField[5];
                _emptyPercentFields = new UITextField[5];
                _fixedPopChecks = new UICheckBox[5];
                _fixedPopFields = new UITextField[5];
                _areaPerFields = new UITextField[5];
                _multiFloorChecks = new UICheckBox[5];
                rowLabels = new UILabel[5];

                // Service selection dropdown.
                _serviceDropDown = UIDropDowns.AddPlainDropDown(m_panel, 20f, currentY, Translations.Translate("RPR_OPT_SVC"), serviceNames, -1);
                _serviceDropDown.eventSelectedIndexChanged += ServiceChanged;
                currentY += _serviceDropDown.parent.height;

                // Pack selection dropdown.
                m_packDropDown = UIDropDowns.AddPlainDropDown(m_panel, 20f, currentY, Translations.Translate("RPR_OPT_CPK"), new string[0], -1);
                m_packDropDown.eventSelectedIndexChanged += PackChanged;
                currentY += m_packDropDown.parent.height;

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
                currentY += 70f;
                PanelUtils.ColumnLabel(m_panel, EmptyAreaX, currentY, ColumnWidth, emptyArea, emptyAreaTip, 1.0f);
                PanelUtils.ColumnLabel(m_panel, EmptyPercentX, currentY, ColumnWidth, emptyPercent, emptyPercentTip, 1.0f);
                PanelUtils.ColumnLabel(m_panel, PopCheckX, currentY, ColumnWidth, useFixedPop, useFixedPopTip, 1.0f);
                PanelUtils.ColumnLabel(m_panel, FixedPopX, currentY, ColumnWidth, fixedPop, fixedPopTip, 1.0f);
                PanelUtils.ColumnLabel(m_panel, AreaPerX, currentY, ColumnWidth, areaPer, areaPerTip, 1.0f);
                PanelUtils.ColumnLabel(m_panel, MultiFloorX, currentY, ColumnWidth, multiFloor, multiFloorTip, 1.0f);

                // Add level textfields.
                for (int i = 0; i < 5; ++i)
                {
                    // Row label.
                    rowLabels[i] = RowLabel(m_panel, currentY, Translations.Translate("RPR_OPT_LVL") + " " + (i + 1).ToString());

                    _emptyPercentFields[i] = UITextFields.AddTextField(m_panel, EmptyPercentX + Margin, currentY, width: TextFieldWidth, tooltip: emptyPercentTip);
                    _emptyPercentFields[i].eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);
                    _emptyPercentFields[i].tooltipBox = UIToolTips.WordWrapToolTip;

                    _emptyAreaFields[i] = UITextFields.AddTextField(m_panel, EmptyAreaX + Margin, currentY, width: TextFieldWidth, tooltip: emptyAreaTip);
                    _emptyAreaFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                    _emptyAreaFields[i].tooltipBox = UIToolTips.WordWrapToolTip;

                    // Fixed pop checkboxes - ensure i is saved as objectUserData for use by event handler.  Starts unchecked by default.
                    _fixedPopChecks[i] = UICheckBoxes.AddCheckBox(m_panel, PopCheckX + (ColumnWidth / 2), currentY, tooltip: useFixedPopTip);
                    _fixedPopChecks[i].objectUserData = i;
                    _fixedPopChecks[i].eventCheckChanged += FixedPopCheckChanged;
                    _fixedPopChecks[i].tooltipBox = UIToolTips.WordWrapToolTip;

                    // Fixed population fields start hidden by default.
                    _fixedPopFields[i] = UITextFields.AddTextField(m_panel, FixedPopX + Margin, currentY, width: TextFieldWidth, tooltip: fixedPopTip);
                    _fixedPopFields[i].eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);
                    _fixedPopFields[i].tooltipBox = UIToolTips.WordWrapToolTip;
                    _fixedPopFields[i].Hide();

                    _areaPerFields[i] = UITextFields.AddTextField(m_panel, AreaPerX + Margin, currentY, width: TextFieldWidth, tooltip: areaPerTip);
                    _areaPerFields[i].eventTextChanged += (control, value) => PanelUtils.FloatTextFilter((UITextField)control, value);
                    _areaPerFields[i].tooltipBox = UIToolTips.WordWrapToolTip;

                    _multiFloorChecks[i] = UICheckBoxes.AddCheckBox(m_panel, MultiFloorX + (ColumnWidth / 2), currentY, tooltip: multiFloorTip);
                    _multiFloorChecks[i].tooltipBox = UIToolTips.WordWrapToolTip;

                    // Move to next row.
                    currentY += RowHeight;
                }

                // Add footer controls.
                PanelFooter(currentY);

                // Set service menu to initial state (residential), which will also update textfield visibility via event handler.
                _serviceDropDown.selectedIndex = 0;
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
        /// <param name="c">Calling component (unused).</param>
        /// <param name="p">Mouse event (unused).</param>
        protected override void AddPack(UIComponent c, UIMouseEventParameter p)
        {
            // Initial pack name.
            string newPackName = PackNameField.text;

            // Integer suffix for when the above name already exists (starts with 2).
            int packNum = 2;

            // Current service.
            ItemClass.Service currentService = services[_serviceDropDown.selectedIndex];

            // Starting with our default new pack name, check to see if we already have a pack with this name for the currently selected service.
            while (PopData.Instance.CalcPacks.Find(pack => ((PopDataPack)pack).Service == currentService && (pack.Name.Equals(newPackName) || pack.DisplayName.Equals(newPackName))) != null)
            {
                // We already have a match for this name; append the current integer suffix to the base name and try again, incementing the integer suffix for the next attempt (if required).
                newPackName = PackNameField.text + " " + packNum++;
            }

            // We now have a unique name; set the textfield.
            PackNameField.text = newPackName;

            // Add new pack with basic values (deails will be populated later).
            VolumetricPopPack newPack = new VolumetricPopPack(DataPack.DataVersion.CustomOne, services[_serviceDropDown.selectedIndex]);

            // Update pack with information from the panel.
            UpdatePack(newPack);

            // Add our new pack to our list of packs and update defaults panel menus.
            PopData.Instance.AddCalculationPack(newPack);
            CalculationsPanel.Instance.UpdateDefaultMenus();

            // Update pack menu.
            m_packDropDown.items = PackList(currentService);

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
                PopData.Instance.CalcPacks.Remove(m_packList[m_packDropDown.selectedIndex]);

                // Regenerate pack menu.
                m_packDropDown.items = PackList(services[_serviceDropDown.selectedIndex]);

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
            if (pack is VolumetricPopPack popPack)
            {
                // Basic pack attributes.
                pack.Name = PackNameField.text;

                // Iterate through each level, parsing input fields.
                for (int i = 0; i < maxLevels[_serviceDropDown.selectedIndex]; ++i)
                {
                    // Textfields.
                    PanelUtils.ParseFloat(ref popPack.Levels[i].EmptyArea, _emptyAreaFields[i].text, true);
                    PanelUtils.ParseInt(ref popPack.Levels[i].EmptyPercent, _emptyPercentFields[i].text);

                    // Look at fixed population checkbox state to work out if we're doing fixed population or area per.
                    if (_fixedPopChecks[i].isChecked)
                    {
                        // Using fixed pop: negate the 'area per' number to denote fixed population.
                        int pop = 0;
                        PanelUtils.ParseInt(ref pop, _fixedPopFields[i].text);
                        popPack.Levels[i].AreaPer = 0 - pop;
                    }
                    else
                    {
                        // Area per unit.
                        PanelUtils.ParseFloat(ref popPack.Levels[i].AreaPer, _areaPerFields[i].text, true);
                    }

                    // Checkboxes.
                    popPack.Levels[i].MultiFloorUnits = _multiFloorChecks[i].isChecked;
                }
            }
        }

        /// <summary>
        /// Service dropdown change handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="index">New selected index.</param>
        private void ServiceChanged(UIComponent c, int index)
        {
            // Set textfield visibility depending on level.
            TextfieldVisibility(maxLevels[index]);

            // Reset pack menu items.
            m_packDropDown.items = PackList(services[index]);

            // Reset pack selection and force update of fields and button states.
            m_packDropDown.selectedIndex = 0;
            PopulateTextFields(0);
            ButtonStates(0);
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

            // Set service menu by iterating through list of services looking for a match.
            for (int i = 0; i < services.Length; ++i)
            {
                if (services[i] == ((VolumetricPopPack)m_packList[index]).Service)
                {
                    // Found a service match; select it and stop looping.
                    _serviceDropDown.selectedIndex = i;
                    break;
                }
            }

            // Update button states.
            ButtonStates(index);
        }

        /// <summary>
        /// Shows/hides textfields according to the provided maximum level to show.
        /// </summary>
        /// <param name="maxLevel">Maximum number of levels to show.</param>
        private void TextfieldVisibility(int maxLevel)
        {
            // Iterate through all fields.
            for (int i = 0; i < 5; ++i)
            {
                // If this level is less than the maximum given, show.
                if (i < maxLevel)
                {
                    rowLabels[i].Show();
                    _fixedPopChecks[i].Show();

                    // Arear per or fixed population, depending on fixed pop check state.
                    if (_fixedPopChecks[i].isChecked)
                    {
                        _fixedPopFields[i].Show();
                    }
                    else
                    {
                        _emptyAreaFields[i].Show();
                        _emptyPercentFields[i].Show();
                        _areaPerFields[i].Show();
                        _multiFloorChecks[i].Show();
                    }
                }
                else
                {
                    // Otherwise, hide.
                    rowLabels[i].Hide();
                    _fixedPopChecks[i].Hide();

                    _fixedPopFields[i].Hide();
                    _emptyAreaFields[i].Hide();
                    _emptyPercentFields[i].Hide();
                    _areaPerFields[i].Hide();
                    _multiFloorChecks[i].Hide();
                }
            }
        }

        /// <summary>
        /// Event handler for fixed populaton checkboxes.
        /// Updates fixed population/area per textfield visibility based on state.
        /// </summary>
        /// <param name="c">Calling UIComponent.</param>
        /// <param name="isChecked">New isChecked state.</param>
        private void FixedPopCheckChanged(UIComponent c, bool isChecked)
        {
            // Get stored index of calling checkbox.
            int index = (int)c.objectUserData;

            _fixedPopFields[index].isVisible = isChecked;

            _emptyAreaFields[index].isVisible = !isChecked;
            _emptyPercentFields[index].isVisible = !isChecked;
            _areaPerFields[index].isVisible = !isChecked;
            _multiFloorChecks[index].isVisible = !isChecked;
        }

        /// <summary>
        /// Populates the textfields with data from the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number of calculation pack.</param>
        private void PopulateTextFields(int index)
        {
            // Get local reference.
            VolumetricPopPack volPack = (VolumetricPopPack)m_packList[index];

            // Set name field.
            PackNameField.text = volPack.DisplayName;

            // Set service selection menu by iterating through each service and looking for a match.
            for (int i = 0; i < services.Length; ++i)
            {
                if (services[i] == volPack.Service)
                {
                    // Got a match; apply selected index and stop looping.
                    // This also applies text field visibility via the service menue event handler.
                    _serviceDropDown.selectedIndex = i;
                    break;
                }
            }

            // Iterate through each level in the pack and populate the relevant row.
            for (int i = 0; i < volPack.Levels.Length; ++i)
            {
                // Local reference.
                VolumetricPopPack.LevelData level = volPack.Levels[i];

                // Populate controls.
                _emptyAreaFields[i].text = Measures.AreaFromMetric(level.EmptyArea).ToString("N1");
                _emptyPercentFields[i].text = level.EmptyPercent.ToString();
                _fixedPopChecks[i].isChecked = level.AreaPer < 0;
                _areaPerFields[i].text = Measures.AreaFromMetric(Math.Abs(level.AreaPer)).ToString("N1");
                _fixedPopFields[i].text = Math.Abs(level.AreaPer).ToString();
                _multiFloorChecks[i].isChecked = level.MultiFloorUnits;
            }
        }

        /// <summary>
        /// (Re)builds the list of available packs.
        /// </summary>
        /// <param name="service">Service index.</param>
        /// <returns>String array of custom pack names, in order (suitable for use as dropdown menu items).</returns>
        private string[] PackList(ItemClass.Service service)
        {
            // Re-initialise pack list.
            m_packList = new List<DataPack>();
            List<string> packNames = new List<string>();

            // Iterate through all available packs.
            foreach (PopDataPack calcPack in PopData.Instance.CalcPacks)
            {
                // Check for custom packs.
                if (calcPack is VolumetricPopPack volPack && volPack.Service == service)
                {
                    // Found one - add to our lists.
                    m_packList.Add(volPack);
                    packNames.Add(volPack.DisplayName);
                }
            }

            return packNames.ToArray();
        }
    }
}