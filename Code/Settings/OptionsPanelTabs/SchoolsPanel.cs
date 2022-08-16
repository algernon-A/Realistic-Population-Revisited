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
    internal class SchoolsPanel : OptionsPanelTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchoolsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal SchoolsPanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            m_panel = PanelUtils.AddTextTab(tabStrip, Translations.Translate("RPR_OPT_SCH"), tabIndex, out UIButton _, autoLayout: true);

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

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

                UIHelper helper = new UIHelper(m_panel);

                // Enable realistic schools checkbox.
                UICheckBox schoolCapacityCheck = UICheckBoxes.AddPlainCheckBox(m_panel, Translations.Translate("RPR_OPT_SEN"));
                schoolCapacityCheck.isChecked = ModSettings.EnableSchoolPop;
                schoolCapacityCheck.eventCheckChanged += (c, isChecked) => ModSettings.EnableSchoolPop = isChecked;

                // Enable realistic schools checkbox.
                UICheckBox schoolPropertyCheck = UICheckBoxes.AddPlainCheckBox(m_panel, Translations.Translate("RPR_OPT_SEJ"));
                schoolPropertyCheck.isChecked = ModSettings.EnableSchoolProperties;
                schoolPropertyCheck.eventCheckChanged += (c, isChecked) => ModSettings.EnableSchoolProperties = isChecked;

                // School default multiplier.  Simple integer.
                UISlider schoolMult = UISliders.AddPlainSliderWithValue(m_panel, 0f, 0f, Translations.Translate("RPR_OPT_SDM"), 1f, 5f, 0.5f, ModSettings.DefaultSchoolMult);
                schoolMult.eventValueChanged += (c, value) => { ModSettings.DefaultSchoolMult = value; };
            }
        }
    }
}