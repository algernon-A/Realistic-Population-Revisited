// <copyright file="LegacyPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Linq;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class LegacyPanel : OptionsPanelTab
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyPanel"/> class.
        /// </summary>
        /// <param name="parentTabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal LegacyPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            m_panel = UITabstrips.AddTextTab(parentTabStrip, Translations.Translate("RPR_OPT_LEG"), tabIndex, out UIButton tabButton, 100f);

            // Button size and text scale.
            tabButton.textScale = 0.7f;

            // Set tab object reference.
            parentTabStrip.tabs[tabIndex].objectUserData = this;
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
                LegacyResidentialPanel resPanel = new LegacyResidentialPanel(childTabStrip, 0);
                new LegacyIndustrialPanel(childTabStrip, 1);
                new LegacyCommercialPanel(childTabStrip, 2);
                new LegacyOfficePanel(childTabStrip, 3);

                // Change tab size and text scale (to differentiate from 'main' tabstrip).
                foreach (UIComponent component in childTabStrip.components)
                {
                    if (component is UIButton button)
                    {
                        button.textScale = 0.8f;
                        button.width = 100f;
                    }
                }

                // Event handler for tab index change; setup the selected tab.
                childTabStrip.eventSelectedIndexChanged += (c, index) =>
                {
                    if (childTabStrip.tabs[index].objectUserData is OptionsPanelTab tab)
                    {
                        tab.Setup();
                    }
                };

                // Perform setup of residential tab (default selection) and make sure first one is selected (doing a 'quickstep' via the second tab to ensure proper events are triggered).
                childTabStrip.selectedIndex = 1;
                childTabStrip.selectedIndex = 0;
            }
        }
    }
}