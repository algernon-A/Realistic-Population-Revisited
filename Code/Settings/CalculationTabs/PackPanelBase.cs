// <copyright file="PackPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for creating and editing calculation packs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected fields")]
    internal abstract class PackPanelBase : OptionsPanelTab
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        /// <summary>
        /// Texfield width.
        /// </summary>
        protected const float TextFieldWidth = 85f;

        /// <summary>
        /// Column width.
        /// </summary>
        protected const float ColumnWidth = TextFieldWidth + (Margin * 2);

        /// <summary>
        /// First item relative X-position.
        /// </summary>
        protected const float FirstItem = 110f;

        /// <summary>
        /// First item relative X-position.
        /// </summary>
        protected const float RowHeight = 27f;

        /// <summary>
        /// Pack selection dropdown.
        /// </summary>
        protected UIDropDown m_packDropDown;

        /// <summary>
        /// List of calculation packs.
        /// </summary>
        protected List<DataPack> m_packList;

        // Layout constants - private.
        private const float LeftItem = 75f;

        // Panel components.
        private UIButton _saveButton;
        private UIButton _deleteButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackPanelBase"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal PackPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Layout constants.
            const float TabWidth = 50f;

            // Add tab and helper.
            m_panel = PanelUtils.AddIconTab(tabStrip, Translations.Translate(TabTooltipKey), tabIndex, new string[] { TabSprite }, new string[] { "ingame" }, TabWidth);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Gets the icon sprite name for this tab.
        /// </summary>
        protected abstract string TabSprite { get; }

        /// <summary>
        /// Gets the tooltip translation key for this tab.
        /// </summary>
        protected abstract string TabTooltipKey { get; }

        /// <summary>
        /// Gets the pack name field for this tab.
        /// </summary>
        protected UITextField PackNameField { get; private set; }

        /// <summary>
        /// 'Add new pack' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected abstract void AddPack(UIComponent c, UIMouseEventParameter p);

        /// <summary>
        /// 'Delete pack' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected abstract void DeletePack(UIComponent c, UIMouseEventParameter p);

        /// <summary>
        /// Updates the given calculation pack with data from the panel.
        /// </summary>
        /// <param name="pack">Pack to update.</param>
        protected abstract void UpdatePack(DataPack pack);

        /// <summary>
        /// Adds panel footer controls (pack name textfield and buttons).
        /// </summary>
        /// <param name="yPos">Reference Y position.</param>
        protected void PanelFooter(float yPos)
        {
            // Additional space before name textfield.
            float currentY = yPos + RowHeight;

            // Pack name textfield.
            PackNameField = UITextFields.AddBigTextField(m_panel, 200f, currentY);
            UILabel packNameLabel = UILabels.AddLabel(PackNameField, -100f, (PackNameField.height - 18f) / 2, Translations.Translate("RPR_OPT_EDT_NAM"));

            // Adjsut pack name textfield position to accomodate longer translation strings.
            float excessWidth = packNameLabel.width - 95f;
            if (excessWidth > 0f)
            {
                Vector3 adjustment = new Vector3(excessWidth, 0f);
                packNameLabel.relativePosition -= adjustment;
                PackNameField.relativePosition += adjustment;
            }

            // Space for buttons.
            currentY += 50f;

            // 'Add new' button.
            UIButton addNewButton = UIButtons.AddButton(m_panel, 20f, currentY, Translations.Translate("RPR_OPT_NEW"));
            addNewButton.eventClicked += AddPack;

            // Save pack button.
            _saveButton = UIButtons.AddButton(m_panel, 250f, currentY, Translations.Translate("RPR_OPT_SAA"));
            _saveButton.eventClicked += Save;

            // Delete pack button.
            _deleteButton = UIButtons.AddButton(m_panel, 480f, currentY, Translations.Translate("RPR_OPT_DEL"));
            _deleteButton.eventClicked += DeletePack;
        }

        /// <summary>
        /// Sets button and textfield enabled/disabled states.
        /// </summary>
        /// <param name="index">Selected pack list index.</param>
        protected void ButtonStates(int index)
        {
            // Enable save and delete buttons and name textfield if this is a custom pack, otherwise disable.
            if (m_packList[index].Version == DataPack.DataVersion.CustomOne)
            {
                _saveButton.Enable();
                _deleteButton.Enable();
            }
            else
            {
                _saveButton.Disable();
                _deleteButton.Disable();
            }
        }

        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        /// <param name="yPos">Reference Y position.</param>
        /// <param name="text">Label text.</param>
        /// <returns>New row label.</returns>
        protected UILabel RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (FirstItem - Margin) - lineLabel.width);

            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;
            }

            lineLabel.relativePosition = new Vector2(xPos, yPos + 2);

            return lineLabel;
        }

        /// <summary>
        /// Save button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected virtual void Save(UIComponent c, UIMouseEventParameter p)
        {
            // Update currently selected pack with information from the panel.
            UpdatePack(m_packList[m_packDropDown.selectedIndex]);

            // Update selected menu item in case the name has changed.
            m_packDropDown.items[m_packDropDown.selectedIndex] = m_packList[m_packDropDown.selectedIndex].DisplayName;

            // Update defaults panel menus.
            CalculationsPanel.Instance.UpdateDefaultMenus();

            // Save configuration file.
            ConfigurationUtils.SaveSettings();

            // Apply update.
            FloorData.Instance.CalcPackChanged(m_packList[m_packDropDown.selectedIndex]);
        }
    }
}