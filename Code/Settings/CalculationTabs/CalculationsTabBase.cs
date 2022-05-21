using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel tab for calculations for a particular service group.
    /// </summary>
    internal abstract class CalculationsTabBase : OptionsPanelTab
    {
        // Defaults panel reference.
        protected DefaultsPanelBase defaultsPanel;

        // Tab icons.
        protected abstract string[] IconNames { get; }
        protected abstract string[] AtlasNames { get; }
        protected abstract string Tooltip { get; }

        // Tab width.
        protected virtual float TabWidth => 100f;



        /// <summary>
        /// Adds options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsTabBase(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            panel = PanelUtils.AddIconTab(parentTabStrip, Tooltip, tabIndex, IconNames, AtlasNames, TabWidth);

            // Set tab object reference.
            parentTabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Updates control values for relevant defaults panel.
        /// </summary>
        internal void UpdateControls() => defaultsPanel?.UpdateControls();


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected abstract void AddTabs(UITabstrip tabStrip);


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

                // Add tabstrip.
                UITabstrip childTabStrip = panel.AddUIComponent<UITabstrip>();
                childTabStrip.relativePosition = new Vector2(0, 0);
                childTabStrip.size = new Vector2(744f, 725f);

                // Tab container (the panels underneath each tab).
                UITabContainer tabContainer = panel.AddUIComponent<UITabContainer>();
                tabContainer.relativePosition = new Vector2(0, 30f);
                tabContainer.size = new Vector2(744f, 720);
                childTabStrip.tabPages = tabContainer;

                // Set up child tabs and make sure first one is selected (doing a 'quickstep' via the second tab to ensure proper events are triggered).
                AddTabs(childTabStrip);
                childTabStrip.selectedIndex = 1;
                childTabStrip.selectedIndex = 0;
                (childTabStrip.tabs[0].objectUserData as OptionsPanelTab)?.Setup();

                // Event handler for tab index change; setup the selected tab.
                childTabStrip.eventSelectedIndexChanged += (control, index) =>
                {
                    if (childTabStrip.tabs[index].objectUserData is OptionsPanelTab tab)
                    {
                        tab.Setup();
                    }
                };
            }
        }
    }
}