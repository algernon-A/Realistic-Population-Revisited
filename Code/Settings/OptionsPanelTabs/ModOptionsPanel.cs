// <copyright file="ModOptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class ModOptionsPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModOptionsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ModOptionsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = UITabstrips.AddTextTab(tabStrip, Translations.Translate("RPR_OPT_MOD"), tabIndex, out UIButton _, 210f);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Language dropdown.
            UIDropDown languageDrop = UIDropDowns.AddPlainDropDown(panel, 0f, 0f, Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDrop.eventSelectedIndexChanged += (c, index) =>
            {
                Translations.Index = index;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };

            // Hotkey control.
            OptionsKeymapping keyMapping = panel.gameObject.AddComponent<OptionsKeymapping>();
            keyMapping.Label = Translations.Translate("RPR_OPT_KEY");
            keyMapping.Binding = HotkeyThreading.HotKey;

            UICheckBox usMeasureCheck = UICheckBoxes.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_MEA"));
            usMeasureCheck.isChecked = !Measures.UsingMetric;
            usMeasureCheck.eventCheckChanged += (c, isChecked) =>
            {
                Measures.UsingMetric = !isChecked;
                OptionsPanelManager<OptionsPanel>.LocaleChanged();
            };

            // Detail logging option.
            UICheckBox logCheckBox = UICheckBoxes.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_LDT"));
            logCheckBox.isChecked = Logging.DetailLogging;
            logCheckBox.eventCheckChanged += (c, isChecked) =>
            {
                // Update mod settings.
                Logging.DetailLogging = isChecked;
                Logging.KeyMessage("detailed logging ", Logging.DetailLogging ? "enabled" : "disabled");
            };
        }
    }
}