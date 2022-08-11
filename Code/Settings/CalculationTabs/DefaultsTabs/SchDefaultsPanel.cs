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
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_SCH")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Education
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.None
        };

        private readonly string[] iconNames =
        {
            "ToolbarIconEducation"
        };

        private readonly string[] atlasNames =
        {
            "Ingame"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;

        // Title key.
        protected override string TitleKey => "RPR_TIT_SDF";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal SchDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        protected override void FooterButtons(float yPos)
        {
            base.FooterButtons(yPos);

            // Save button.
            UIButton saveButton = AddSaveButton(panel, yPos);
            saveButton.eventClicked += Apply;
        }
    }
}