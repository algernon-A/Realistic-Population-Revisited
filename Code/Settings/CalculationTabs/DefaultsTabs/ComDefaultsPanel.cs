// <copyright file="ComDefaultsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
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
    internal class ComDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_CLO"),
            Translations.Translate("RPR_CAT_CHI"),
            Translations.Translate("RPR_CAT_ORG"),
            Translations.Translate("RPR_CAT_LEI"),
            Translations.Translate("RPR_CAT_TOU"),
        };

        private readonly ItemClass.Service[] _services =
        {
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
        };

        private readonly string[] _iconNames =
        {
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "IconPolicyOrganic",
            "IconPolicyLeisure",
            "IconPolicyTourist",
        };

        private readonly string[] _atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ComDefaultsPanel"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ComDefaultsPanel(UITabstrip tabStrip, int tabIndex)
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
        protected override string TitleKey => "RPR_TIT_CDF";

        /// <summary>
        /// Gets or sets the default calculation mode for new saves for this tab.
        /// </summary>
        protected override DefaultMode NewDefaultMode { get => ModSettings.NewSaveDefaultCom; set => ModSettings.NewSaveDefaultCom = value; }

        /// <summary>
        /// Gets or sets the default calculation mode for this save for this tab.
        /// </summary>
        protected override DefaultMode ThisDefaultMode { get => ModSettings.ThisSaveDefaultCom; set => ModSettings.ThisSaveDefaultCom = value; }

        /// <summary>
        /// Gets the translation key for the legacy settings label for this tab.
        /// </summary>
        protected override string DefaultModeLabel => "RPR_DEF_DMC";
    }
}