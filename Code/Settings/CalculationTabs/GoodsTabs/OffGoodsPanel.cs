// <copyright file="OffGoodsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting office goods calculations.
    /// </summary>
    internal class OffGoodsPanel : GoodsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_OFF"),
            Translations.Translate("RPR_CAT_ITC"),
        };

        private readonly ItemClass.Service[] _services =
        {
            ItemClass.Service.Office,
            ItemClass.Service.Office,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeHightech,
        };

        private readonly string[] _iconNames =
        {
            "ZoningOffice",
            "IconPolicyHightech",
        };

        private readonly string[] _atlasNames =
        {
            "Thumbnails",
            "Ingame",
        };

        // Panel components.
        private UISlider[] _prodMultSliders;

        /// <summary>
        /// Initializes a new instance of the <see cref="OffGoodsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal OffGoodsPanel(UITabstrip tabStrip, int tabIndex)
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
        protected override string TitleKey => "RPR_TIT_OGO";

        /// <summary>
        /// Gets or sets a value indicating whether legacy calcuations should be used by default for this save.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyOff; set => ModSettings.ThisSaveLegacyOff = value; }

        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < _prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider values.
                _prodMultSliders[i].value = OfficeProduction.GetProdMult(_subServices[i]);
            }
        }

        /// <summary>
        /// Adds controls for each sub-service.
        /// </summary>
        /// <param name="yPos">Relative Y position at top of row items.</param>
        /// <param name="index">Index number of this row.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected override float SubServiceControls(float yPos, int index)
        {
            // TODO: Attach controls to floor menu, so visibility will follow same state (i.e. hidden when legacy calculations are selected, shown otherwise).
            float currentY = yPos;

            // Header label.
            UILabels.AddLabel(m_panel, LeftColumn, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (_prodMultSliders == null)
            {
                _prodMultSliders = new UISlider[_subServices.Length];
            }

            // Production multiplication slider.
            _prodMultSliders[index] = AddSlider(m_panel, LeftColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            _prodMultSliders[index].objectUserData = index;
            _prodMultSliders[index].maxValue = OfficeProduction.MaxProdMult;
            _prodMultSliders[index].value = OfficeProduction.GetProdMult(_subServices[index]);
            PercentSliderText(_prodMultSliders[index], _prodMultSliders[index].value);

            return yPos;
        }

        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event.)</param>
        protected override void Apply(UIComponent c, UIMouseEventParameter p)
        {
            // Iterate through all subservices.
            for (int i = 0; i < _subServices.Length; ++i)
            {
                // Record production mutltiplier.
                OfficeProduction.SetProdMult(_subServices[i], (int)_prodMultSliders[i].value);
            }

            base.Apply(c, p);
        }

        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void ResetDefaults(UIComponent c, UIMouseEventParameter p)
        {
            // Reset sliders.
            for (int i = 0; i < _prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                _prodMultSliders[i].value = OfficeProduction.DefaultProdMult;
            }
        }

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void ResetSaved(UIComponent c, UIMouseEventParameter p) => UpdateControls();
    }
}