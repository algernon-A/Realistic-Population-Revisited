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
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_OFF"),
            Translations.Translate("RPR_CAT_ITC")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Office,
            ItemClass.Service.Office
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeHightech
        };

        private readonly string[] iconNames =
        {
            "ZoningOffice",
            "IconPolicyHightech"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Ingame"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;

        // Title key.
        protected override string TitleKey => "RPR_TIT_ODF";

        // Default mode references.
        protected override DefaultMode ThisDefaultMode { get => ModSettings.ThisSaveDefaultOff; set => ModSettings.ThisSaveDefaultOff = value; }
        protected override DefaultMode NewDefaultMode { get => ModSettings.newSaveDefaultOff; set => ModSettings.newSaveDefaultOff = value; }
        protected override string DefaultModeLabel => "RPR_DEF_DMO";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal OffDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }
    }
}