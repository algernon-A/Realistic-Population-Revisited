﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class RICODefaultsPanel : DefaultsPanel
    {

        // Legacy settings links.
        protected abstract bool NewLegacyCategory { get; set; }
        protected abstract bool ThisLegacyCategory { get; set; }

        // Translation key for legacy settings label.
        protected abstract string LegacyCheckLabel { get; }


        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal RICODefaultsPanel(UITabstrip tabStrip, int tabIndex) :  base(tabStrip, tabIndex)
        {
            // Add 'Use legacy by default' header.

            // Label.
            float currentY = Margin;
            UILabel legacyLabel = UIControls.AddLabel(panel, Margin, currentY, Translations.Translate(LegacyCheckLabel), textScale: 0.9f);
            currentY += legacyLabel.height + 5f;

            // Use legacy by default for this save check.
            UICheckBox legacyThisSaveCheck = UIControls.LabelledCheckBox(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LTS"));
            legacyThisSaveCheck.label.wordWrap = true;
            legacyThisSaveCheck.label.autoSize = false;
            legacyThisSaveCheck.label.width = 710f;
            legacyThisSaveCheck.label.autoHeight = true;
            legacyThisSaveCheck.isChecked = ThisLegacyCategory;
            legacyThisSaveCheck.eventCheckChanged += (control, isChecked) =>
            {
                ThisLegacyCategory = isChecked;
                UpdateMenus();
                SettingsUtils.SaveSettings();
            };
            currentY += 20f;

            // Use legacy by default for new saves check.
            UICheckBox legacyNewSaveCheck = UIControls.LabelledCheckBox(panel, Margin * 2, currentY, Translations.Translate("RPR_DEF_LAS"));
            legacyNewSaveCheck.label.wordWrap = true;
            legacyNewSaveCheck.label.autoSize = false;
            legacyNewSaveCheck.label.width = 710f;
            legacyNewSaveCheck.label.autoHeight = true;
            legacyNewSaveCheck.isChecked = NewLegacyCategory;
            legacyNewSaveCheck.eventCheckChanged += (control, isChecked) =>
            {
                NewLegacyCategory = isChecked;
                UpdateMenus();
                SettingsUtils.SaveSettings();
            };
        }
    }
}