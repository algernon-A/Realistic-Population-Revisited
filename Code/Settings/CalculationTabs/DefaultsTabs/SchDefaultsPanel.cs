// <copyright file="SchDefaultsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class SchDefaultsPanel : DefaultsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_SCH"),
        };

        private readonly ItemClass.Service[] _services =
        {
            ItemClass.Service.Education,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.None,
        };

        private readonly string[] _iconNames =
        {
            "ToolbarIconEducation",
        };

        private readonly string[] _atlasNames =
        {
            "Ingame",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="SchDefaultsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal SchDefaultsPanel(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets the array of sub-service display names for this tab.
        /// </summary>
        protected override string[] SubServiceNames => _subServiceNames;

        /// <summary>
        /// Gets the array of relevant building services for this tab.
        /// </summary>
        protected override ItemClass.Service[] Services => _services;

        /// <summary>
        /// Gets the array of relevant building sub-services for this tab.
        /// </summary>
        protected override ItemClass.SubService[] SubServices => _subServices;

        /// <summary>
        /// Gets the array of building type icon sprite names for this tab.
        /// </summary>
        protected override string[] IconNames => _iconNames;

        /// <summary>
        /// Gets the array of building type icon atlas names for this tab.
        /// </summary>
        protected override string[] AtlasNames => _atlasNames;

        /// <summary>
        /// Gets the tab title translation key for this tab.
        /// </summary>
        protected override string TitleKey => "RPR_TIT_SDF";

        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        protected override void FooterButtons(float yPos)
        {
            base.FooterButtons(yPos);

            // Save button.
            UIButton saveButton = AddSaveButton(m_panel, yPos);
            saveButton.eventClicked += Apply;
        }
    }
}