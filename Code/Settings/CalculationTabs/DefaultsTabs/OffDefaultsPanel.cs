// <copyright file="OffDefaultsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting office goods calculations.
    /// </summary>
    internal class OffDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_OFF"),
            Translations.Translate("RPR_CAT_OW2"),
            Translations.Translate("RPR_CAT_ITC"),
        };

        private readonly ItemClass.Service[] _services =
        {
            ItemClass.Service.Office,
            ItemClass.Service.Office,
            ItemClass.Service.Office,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeWallToWall,
            ItemClass.SubService.OfficeHightech,
        };

        private readonly string[] _iconNames =
        {
            "ZoningOffice",
            "DistrictSpecializationOfficeWallToWall",
            "IconPolicyHightech",
        };

        private readonly string[] _atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="OffDefaultsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal OffDefaultsPanel(UITabstrip tabStrip, int tabIndex)
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
        protected override string TitleKey => "RPR_TIT_ODF";

        /// <summary>
        /// Gets or sets the default calculation mode for new saves for this tab.
        /// </summary>
        protected override DefaultMode NewDefaultMode { get => ModSettings.NewSaveDefaultOff; set => ModSettings.NewSaveDefaultOff = value; }

        /// <summary>
        /// Gets or sets the default calculation mode for this save for this tab.
        /// </summary>
        protected override DefaultMode ThisDefaultMode { get => ModSettings.ThisSaveDefaultOff; set => ModSettings.ThisSaveDefaultOff = value; }

        /// <summary>
        /// Gets the translation key for the legacy settings label for this tab.
        /// </summary>
        protected override string DefaultModeLabel => "RPR_DEF_DMO";
    }
}