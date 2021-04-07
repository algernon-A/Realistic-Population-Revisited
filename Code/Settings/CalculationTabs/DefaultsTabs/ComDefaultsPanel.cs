﻿using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class ComDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_CLO"),
            Translations.Translate("RPR_CAT_CHI"),
            Translations.Translate("RPR_CAT_ORG"),
            Translations.Translate("RPR_CAT_LEI"),
            Translations.Translate("RPR_CAT_TOU")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist
        };

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

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Title key.
        protected override string TitleKey => "RPR_TIT_CDF";


        // Legacy settings references.
        protected override bool NewLegacyCategory { get => ModSettings.newSaveLegacyCom; set => ModSettings.newSaveLegacyCom = value; }
        protected override bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyCom; set => ModSettings.ThisSaveLegacyCom = value; }
        protected override string LegacyCheckLabel => "RPR_DEF_LGC";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal ComDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }
    }
}