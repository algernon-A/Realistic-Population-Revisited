// <copyright file="CommercialTab.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting basic mod options.
    /// </summary>
    internal class CommercialTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] iconNames =
        {
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "IconPolicyOrganic",
            "IconPolicyLeisure",
            "IconPolicyTourist"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
        };

        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;
        protected override string Tooltip => Translations.Translate("RPR_CAT_COM");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CommercialTab(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            defaultsPanel = new ComDefaultsPanel(tabStrip, 0);
            new ComGoodsPanel(tabStrip, 1);
            new ComConsumptionPanel(tabStrip, 2);
        }
    }
}