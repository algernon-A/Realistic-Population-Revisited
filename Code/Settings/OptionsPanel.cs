namespace RealPop2
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    internal class OptionsPanel : UIPanel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsPanel"/> class.
        /// </summary>
        internal OptionsPanel()
        {
            // Add tabstrip.
            UITabstrip tabstrip = UITabstrips.AddTabStrip(this, 0f, 0f, OptionsPanelManager<OptionsPanel>.PanelWidth, OptionsPanelManager<OptionsPanel>.PanelHeight, out _);

            // Initialize data.
            DataUtils.Setup();

            // Add tabs and panels.
            new ModOptionsPanel(tabstrip, 0);
            new CalculationsPanel(tabstrip, 1);
            new EducationPanel(tabstrip, 2);
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
            tabstrip.selectedIndex = -1;
            tabstrip.selectedIndex = 0;
        }
    }
}