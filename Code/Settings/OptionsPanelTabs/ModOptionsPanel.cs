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
            UIDropDown languageDrop = UIControls.AddPlainDropDown(panel, Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index);
            languageDrop.eventSelectedIndexChanged += (control, index) =>
            {
                Translations.Index = index;
                SettingsUtils.SaveSettings();
            };

            // Hotkey control.
            panel.gameObject.AddComponent<OptionsKeymapping>();

            // Detail logging option.
            UICheckBox logCheckBox = UIControls.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_LDT"));
            logCheckBox.isChecked = Logging.detailLogging;
            logCheckBox.eventCheckChanged += (control, isChecked) =>
            {
                // Update mod settings.
                Logging.detailLogging = isChecked;

                // Update configuration file.
                SettingsUtils.SaveSettings();

                Logging.KeyMessage("detailed logging ", Logging.detailLogging ? "enabled" : "disabled");
            };
        }
    }
}