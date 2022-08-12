// <copyright file="CalculationsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class CalculationsPanel : OptionsPanelTab
    {
        // Components.
        private ResidentialTab _resTab;
        private CommercialTab _comTab;
        private OfficeTab _offTab;
        private IndustrialTab _indTab;
        private SchoolTab _schTab;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationsPanel"/> class.
        /// </summary>
        /// <param name="parentTabStrip">Parent tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CalculationsPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Instance reference.
            Instance = this;

            // Add tab and helper.
            m_panel = PanelUtils.AddTextTab(parentTabStrip, Translations.Translate("RPR_PCK_NAM"), tabIndex, out UIButton _);

            // Set tab object reference.
            parentTabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Gets the current instance.
        /// </summary>
        internal static CalculationsPanel Instance { get; private set; }

        /// <summary>
        /// Updates default calculation pack selection menu options.
        /// </summary>
        internal void UpdateDefaultMenus()
        {
            // Update for each defaults panel.
            _resTab.UpdateControls();
            _comTab.UpdateControls();
            _offTab.UpdateControls();
            _indTab.UpdateControls();
            _schTab.UpdateControls();
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

                // Add tabstrip.
                UITabstrip childTabStrip = m_panel.AddUIComponent<UITabstrip>();
                childTabStrip.relativePosition = Vector2.zero;
                childTabStrip.size = new Vector2(744f, 725f);

                // Tab container (the panels underneath each tab).
                UITabContainer tabContainer = m_panel.AddUIComponent<UITabContainer>();
                tabContainer.relativePosition = new Vector2(0, 30f);
                tabContainer.size = new Vector2(744f, 720);
                childTabStrip.tabPages = tabContainer;

                // Add child tabs.
                int tab = 0;
                _resTab = new ResidentialTab(childTabStrip, tab++);
                _comTab = new CommercialTab(childTabStrip, tab++);
                _offTab = new OfficeTab(childTabStrip, tab++);
                _indTab = new IndustrialTab(childTabStrip, tab++);
                _schTab = new SchoolTab(childTabStrip, tab++);
                new PopulationPanel(childTabStrip, tab++);
                new FloorPanel(childTabStrip, tab++);
                new LegacyPanel(childTabStrip, tab);

                // Event handler for tab index change; setup the selected tab.
                childTabStrip.eventSelectedIndexChanged += (control, index) =>
                {
                    if (childTabStrip.tabs[index].objectUserData is OptionsPanelTab childTab)
                    {
                        childTab.Setup();
                    }
                };

                // Perform setup of residential tab (default selection) and make sure first one is selected (doing a 'quickstep' to ensure proper events are triggered).
                childTabStrip.selectedIndex = -1;
                childTabStrip.selectedIndex = 0;
            }
        }
    }
}