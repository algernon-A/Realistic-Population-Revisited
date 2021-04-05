﻿using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting residential calculation options.
    /// </summary>
    internal class SchoolTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] tabIconNames =
        {
            "ToolbarIconEducation"
        };

        private readonly string[] tabAtlasNames =
        {
            "Ingame"
        };

        protected override string[] IconNames => tabIconNames;
        protected override string[] AtlasNames => tabAtlasNames;
        protected override string Tooltip => Translations.Translate("RPR_CAT_SCH");

        // Tab width.
        protected override float TabWidth => 50f;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal SchoolTab(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            defaultsPanel = new SchDefaultsPanel(tabStrip, 0);
        }
    }
}