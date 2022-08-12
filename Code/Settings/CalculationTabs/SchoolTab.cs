// <copyright file="SchoolTab.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting residential calculation options.
    /// </summary>
    internal class SchoolTab : CalculationsTabBase
    {
        // Tab icons.
        private readonly string[] _tabIconNames =
        {
            "ToolbarIconEducation",
        };

        private readonly string[] _tabAtlasNames =
        {
            "Ingame",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SchoolTab"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal SchoolTab(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets the tab width.
        /// </summary>
        protected override float TabWidth => 50f;

        /// <summary>
        /// Gets the array of icon sprite names for this tab.
        /// </summary>
        protected override string[] IconNames => _tabIconNames;

        /// <summary>
        /// Gets the array of icon atlas names for this tab.
        /// </summary>
        protected override string[] AtlasNames => _tabAtlasNames;

        /// <summary>
        /// Gets the tooltip for this tab.
        /// </summary>
        protected override string Tooltip => Translations.Translate("RPR_CAT_SCH");

        /// <summary>
        /// Adds required sub-tabs.
        /// </summary>
        /// <param name="tabStrip">Tabstrip reference.</param>
        protected override void AddTabs(UITabstrip tabStrip)
        {
            m_defaultsPanel = new SchDefaultsPanel(tabStrip, 0);
        }
    }
}