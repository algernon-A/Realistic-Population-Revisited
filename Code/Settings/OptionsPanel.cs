// <copyright file="OptionsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// The mod's settings options panel.
    /// </summary>
    public class OptionsPanel : UIPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        internal OptionsPanel()
        {
            // Add tabstrip.
            UITabstrip tabstrip = UITabstrips.AddTabStrip(this, 0f, 0f, OptionsPanelManager<OptionsPanel>.PanelWidth, OptionsPanelManager<OptionsPanel>.PanelHeight, out _);

            // Initialize data.
            CalcData.Setup();

            // Add tabs and panels.
            new ModOptionsPanel(tabstrip, 0);
            new CalculationsPanel(tabstrip, 1);
            new SchoolsPanel(tabstrip, 2);
            new CrimePanel(tabstrip, 3);

            // Event handler for tab index change; setup the selected tab.
            tabstrip.eventSelectedIndexChanged += (c, index) =>
            {
                if (tabstrip.tabs[index].objectUserData is OptionsPanelTab tab)
                {
                    tab.Setup();
                }
            };

            // Ensure initial selected tab (doing a 'quickstep' to ensure proper events are triggered).
            tabstrip.selectedIndex = 1;
            tabstrip.selectedIndex = 0;
        }
    }
}