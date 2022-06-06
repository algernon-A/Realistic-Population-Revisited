using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class ModOptionsPanel
    {
        /// <summary>
        /// Adds mod options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ModOptionsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTextTab(tabStrip, Translations.Translate("RPR_OPT_MOD"), tabIndex, out UIButton _, 210f);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Language dropdown.
            UIDropDown languageDrop = UIControls.AddPlainDropDown(panel, 0f, 0f, Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDrop.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                OptionsPanel.LocaleChanged();
            };

            // Hotkey control.
            panel.gameObject.AddComponent<OptionsKeymapping>();


            UICheckBox usMeasureCheck = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_MEA"));
            usMeasureCheck.isChecked = !Measures.UsingMetric;
            usMeasureCheck.eventCheckChanged += (control, isChecked) =>
            {
                Measures.UsingMetric = !isChecked;
                OptionsPanel.LocaleChanged();
            };

            // Detail logging option.
            UICheckBox logCheckBox = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_LDT"));
            logCheckBox.isChecked = Logging.detailLogging;
            logCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Logging.detailLogging = isChecked;
                Logging.KeyMessage("detailed logging ", Logging.detailLogging ? "enabled" : "disabled");
            };

            // Don't rebuild CitizenUnit array option.
            UICheckBox dontRebuildCheck = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_DRC"));
            dontRebuildCheck.tooltipBox = TooltipUtils.TooltipBox;
            dontRebuildCheck.tooltip = Translations.Translate("RPR_OPT_DRC_TIP");
            dontRebuildCheck.isChecked = ModSettings.dontRebuildUnits;
            dontRebuildCheck.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                ModSettings.dontRebuildUnits = isChecked;
                Logging.KeyMessage("don't rebuild CitizenUnit array on first load ", ModSettings.dontRebuildUnits ? "enabled" : "disabled");
            };
        }
    }
}