using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class CalculationsPanel : OptionsPanelTab
    {
        // Instance reference.
        internal static CalculationsPanel Instance { get; private set; }


        // Components.
        private ResidentialTab resTab;
        private CommercialTab comTab;
        private OfficeTab offTab;
        private IndustrialTab indTab;
        private SchoolTab schTab;


        /// <summary>
        /// Adds education options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CalculationsPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Instance reference.
            Instance = this;

            // Add tab and helper.
            panel = PanelUtils.AddTextTab(parentTabStrip, Translations.Translate("RPR_PCK_NAM"), tabIndex, out UIButton _);

            // Set tab object reference.
            parentTabStrip.tabs[tabIndex].objectUserData = this;
        }


        /// <summary>
        /// Updates default calculation pack selection menu options.
        /// </summary>
        internal void UpdateDefaultMenus()
        {
            // Update for each defaults panel.
            resTab.UpdateControls();
            comTab.UpdateControls();
            offTab.UpdateControls();
            indTab.UpdateControls();
            schTab.UpdateControls();
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

                // Add tabstrip.
                UITabstrip childTabStrip = panel.AddUIComponent<UITabstrip>();
                childTabStrip.relativePosition = new Vector3(0, 0);
                childTabStrip.size = new Vector2(744f, 725f);

                // Tab container (the panels underneath each tab).
                UITabContainer tabContainer = panel.AddUIComponent<UITabContainer>();
                tabContainer.relativePosition = new Vector3(0, 30f);
                tabContainer.size = new Vector3(744f, 720);
                childTabStrip.tabPages = tabContainer;

                // Add child tabs.
                int tab = 0;
                resTab = new ResidentialTab(childTabStrip, tab++);
                comTab = new CommercialTab(childTabStrip, tab++);
                offTab = new OfficeTab(childTabStrip, tab++);
                indTab = new IndustrialTab(childTabStrip, tab++);
                schTab = new SchoolTab(childTabStrip, tab++);
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

                // Perform setup of residential tab (default selection) and make sure first one is selected (doing a 'quickstep' via the second tab to ensure proper events are triggered).
                childTabStrip.selectedIndex = 1;
                childTabStrip.selectedIndex = 0;

            }
        }
    }
}