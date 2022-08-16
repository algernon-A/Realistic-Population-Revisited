// <copyright file="CalculationsTabBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel tab for calculations for a particular service group.
    /// </summary>
    internal abstract class CalculationsTabBase : OptionsPanelTab
    {
        /// <summary>
        /// Defaults panel instance.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected internal field")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Protected internal field")]
        protected internal DefaultsPanelBase m_defaultsPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationsTabBase"/> class.
        /// </summary>
        /// <param name="parentTabStrip">Parent tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CalculationsTabBase(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            m_panel = PanelUtils.AddIconTab(parentTabStrip, Tooltip, tabIndex, IconNames, AtlasNames, TabWidth);

            // Set tab object reference.
            parentTabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Gets the tab width.
        /// </summary>
        protected virtual float TabWidth => 100f;

        /// <summary>
        /// Gets the array of icon sprite names for this tab.
        /// </summary>
        protected abstract string[] IconNames { get; }

        /// <summary>
        /// Gets the array of icon atlas names for this tab.
        /// </summary>
        protected abstract string[] AtlasNames { get; }

        /// <summary>
        /// Gets the tooltip for this tab.
        /// </summary>
        protected abstract string Tooltip { get; }

        /// <summary>
        /// Updates control values for relevant defaults panel.
        /// </summary>
        internal void UpdateControls() => m_defaultsPanel?.UpdateControls();

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
                childTabStrip.relativePosition = new Vector2(0, 0);
                childTabStrip.size = new Vector2(744f, 725f);

                // Tab container (the panels underneath each tab).
                UITabContainer tabContainer = m_panel.AddUIComponent<UITabContainer>();
                tabContainer.relativePosition = new Vector2(0, 30f);
                tabContainer.size = new Vector2(744f, 720);
                childTabStrip.tabPages = tabContainer;

                // Set up child tabs and make sure first one is selected (doing a 'quickstep' via the second tab to ensure proper events are triggered).
                AddTabs(childTabStrip);
                childTabStrip.selectedIndex = 1;
                childTabStrip.selectedIndex = 0;
                (childTabStrip.tabs[0].objectUserData as OptionsPanelTab)?.Setup();

                // Event handler for tab index change; setup the selected tab.
                childTabStrip.eventSelectedIndexChanged += (c, index) =>
                {
                    if (childTabStrip.tabs[index].objectUserData is OptionsPanelTab tab)
                    {
                        tab.Setup();
                    }
                };
            }
        }

        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference.</param>
        protected abstract void AddTabs(UITabstrip tabStrip);
    }
}