// <copyright file="RICODefaultsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class RICODefaultsPanel : DefaultsPanelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RICODefaultsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal RICODefaultsPanel(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets or sets the default calculation mode for new saves for this tab.
        /// </summary>
        protected abstract DefaultMode NewDefaultMode { get; set; }

        /// <summary>
        /// Gets or sets the default calculation mode for this save for this tab.
        /// </summary>
        protected abstract DefaultMode ThisDefaultMode { get; set; }

        /// <summary>
        /// Gets the translation key for the legacy settings label for this tab.
        /// </summary>
        protected abstract string DefaultModeLabel { get; }

        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected override float PanelHeader(float yPos)
        {
            // Y position reference.
            float currentY = yPos + Margin;

            // Add 'Use legacy by default' header.

            // Label.
            UILabel legacyLabel = UILabels.AddLabel(m_panel, Margin, currentY, Translations.Translate(DefaultModeLabel), m_panel.width - Margin, textScale: 0.9f);
            currentY += legacyLabel.height + 5f;

            // Mode dropdown items.
            string[] modeMenuItems = new string[]
            {
                "New",
                "Vanilla",
                "Legacy",
            };

            UIDropDown thisSaveModeDrop = UIDropDowns.AddLabelledDropDown(m_panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LTS"));
            thisSaveModeDrop.items = modeMenuItems;
            thisSaveModeDrop.selectedIndex = (int)ThisDefaultMode;
            thisSaveModeDrop.eventSelectedIndexChanged += (control, index) =>
            {
                ThisDefaultMode = (DefaultMode)index;
                UpdateControls();
            };
            currentY += 30f;

            UIDropDown newSaveModeDrop = UIDropDowns.AddLabelledDropDown(m_panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LAS"));
            newSaveModeDrop.items = modeMenuItems;
            newSaveModeDrop.selectedIndex = (int)NewDefaultMode;
            newSaveModeDrop.eventSelectedIndexChanged += (control, index) =>
            {
                NewDefaultMode = (DefaultMode)index;
                UpdateControls();
            };

            // Align menus horizontally.
            float menuOffset = thisSaveModeDrop.relativePosition.x - newSaveModeDrop.relativePosition.x;
            if (menuOffset < 0)
            {
                thisSaveModeDrop.relativePosition -= new Vector3(menuOffset, 0);
            }
            else
            {
                newSaveModeDrop.relativePosition += new Vector3(menuOffset, 0);
            }

            // Spacer bar.
            currentY += 35f;
            UISpacers.AddOptionsSpacer(m_panel, Margin, currentY, m_panel.width - (Margin * 2f));

            return currentY + 10f;
        }
    }
}