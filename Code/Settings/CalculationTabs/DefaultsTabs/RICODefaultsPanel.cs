﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class RICODefaultsPanel : DefaultsPanelBase
    {

        // Default mode links.
        protected abstract DefaultMode NewDefaultMode { get; set; }
        protected abstract DefaultMode ThisDefaultMode { get; set; }


        // Translation key for legacy settings label.
        protected abstract string DefaultModeLabel { get; }


        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal RICODefaultsPanel(UITabstrip tabStrip, int tabIndex) :  base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected override float PanelHeader(float yPos)
        {
            // Y position reference.
            float currentY = yPos + Margin;

            // Add 'Use legacy by default' header.

            // Label.
            UILabel legacyLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate(DefaultModeLabel), panel.width - Margin, textScale: 0.9f);
            currentY += legacyLabel.height + 5f;

            // Mode dropdown items.
            string[] modeMenuItems = new string[]
            {
                "New",
                "Vanilla",
                "Legacy"
            };


            UIDropDown thisSaveModeDrop = UIControls.AddLabelledDropDown(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LTS"));
            thisSaveModeDrop.items = modeMenuItems;
            thisSaveModeDrop.selectedIndex = (int)ThisDefaultMode;
            thisSaveModeDrop.eventSelectedIndexChanged += (control, index) =>
            {
                ThisDefaultMode = (DefaultMode)index;
                UpdateControls();
            };
            currentY += 30f;

            UIDropDown newSaveModeDrop = UIControls.AddLabelledDropDown(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LAS"));
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
            UIControls.OptionsSpacer(panel, Margin, currentY, panel.width - (Margin * 2f));

            return currentY + 10f;
        }
    }
}