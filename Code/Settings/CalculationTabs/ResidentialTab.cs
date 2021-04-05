﻿using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting residential calculation options.
    /// </summary>
    internal class ResidentialTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] iconNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "IconPolicySelfsufficient",
            "IconPolicySelfsufficient"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame"
        };

        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override string Tooltip => Translations.Translate("RPR_CAT_RES");


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ResidentialTab(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            defaultsPanel = new ResDefaultsPanel(tabStrip, 0);
            new ResConsumptionPanel(tabStrip, 1);
        }
    }
}