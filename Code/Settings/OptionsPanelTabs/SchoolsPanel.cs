// <copyright file="SchoolsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting school options.
    /// </summary>
    internal class EducationPanel : OptionsPanelTab
    {
        /// <summary>
        /// Adds school options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal EducationPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            panel = PanelUtils.AddTextTab(tabStrip, Translations.Translate("RPR_OPT_SCH"), tabIndex, out UIButton _, autoLayout: true);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
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

                UIHelper helper = new UIHelper(panel);

                // Enable realistic schools checkbox.
                UICheckBox schoolCapacityCheck = UICheckBoxes.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_SEN"));
                schoolCapacityCheck.isChecked = ModSettings.EnableSchoolPop;
                schoolCapacityCheck.eventCheckChanged += (control, isChecked) => ModSettings.EnableSchoolPop = isChecked;

                // Enable realistic schools checkbox.
                UICheckBox schoolPropertyCheck = UICheckBoxes.AddPlainCheckBox(panel, Translations.Translate("RPR_OPT_SEJ"));
                schoolPropertyCheck.isChecked = ModSettings.enableSchoolProperties;
                schoolPropertyCheck.eventCheckChanged += (control, isChecked) => ModSettings.enableSchoolProperties = isChecked;

                // School default multiplier.  Simple integer.
                UISlider schoolMult = UISliders.AddPlainSliderWithValue(panel, 0f, 0f, Translations.Translate("RPR_OPT_SDM"), 1f, 5f, 0.5f, ModSettings.DefaultSchoolMult);
                schoolMult.eventValueChanged += (c, value) => { ModSettings.DefaultSchoolMult = value; };
            }
        }
    }
}