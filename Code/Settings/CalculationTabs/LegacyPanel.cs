using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class LegacyPanel : OptionsPanelTab
    {
        /// <summary>
        /// Adds education options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal LegacyPanel(UITabstrip parentTabStrip, int tabIndex)
        {
            // Add tab and helper.
            panel = PanelUtils.AddTextTab(parentTabStrip, Translations.Translate("RPR_OPT_LEG"), tabIndex, out UIButton tabButton, 100f);

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
                LegacyResidentialPanel resPanel = new LegacyResidentialPanel(childTabStrip, 0);
                new LegacyIndustrialPanel(childTabStrip, 1);
                new LegacyCommercialPanel(childTabStrip, 2);
                new LegacyOfficePanel(childTabStrip, 3);

                // Change tab size and text scale (to differentiate from 'main' tabstrip).
                foreach (UIButton button in childTabStrip.components)
                {
                    button.textScale = 0.8f;
                    button.width = 100f;
                }

                // Event handler for tab index change; setup the selected tab.
                childTabStrip.eventSelectedIndexChanged += (control, index) =>
                {
                    if (childTabStrip.tabs[index].objectUserData is OptionsPanelTab tab)
                    {
                        tab.Setup();
                    }
                };

                // Perform setup of residential tab (default selection).
                resPanel.Setup();
                childTabStrip.selectedIndex = 0;
            }
        }
    }
}