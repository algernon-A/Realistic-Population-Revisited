// <copyright file="IndGoodsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting industry goods calculations.
    /// </summary>
    internal class IndGoodsPanel : GoodsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_IND"),
            Translations.Translate("RPR_CAT_FAR"),
            Translations.Translate("RPR_CAT_FOR"),
            Translations.Translate("RPR_CAT_OIL"),
            Translations.Translate("RPR_CAT_ORE"),
        };

        private readonly ItemClass.Service[] _services =
        {
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.IndustrialGeneric,
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre,
        };

        private readonly string[] _iconNames =
        {
            "ZoningIndustrial",
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre",
        };

        private readonly string[] _atlasNames =
        {
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame",
        };

        // Panel components.
        private UISlider[] _procProdMultSliders;
        private UISlider[] _extProdMultSliders;
        private UIDropDown[] _procProdModeMenus;
        private UIDropDown[] _extProdModeMenus;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndGoodsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal IndGoodsPanel(UITabstrip tabStrip, int tabIndex)
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
        protected override string TitleKey => "RPR_TIT_IGO";

        /// <summary>
        /// Gets or sets a value indicating whether legacy calcuations should be used by default for this save.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyInd; set => ModSettings.ThisSaveLegacyInd = value; }

        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < _procProdMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                _procProdMultSliders[i].value = IndustrialProduction.GetProdMult(_subServices[i]);
                _extProdMultSliders[i].value = ExtractorProduction.GetProdMult(_subServices[i]);

                // Reset visit mode menu selections.
                _procProdModeMenus[i].selectedIndex = IndustrialProduction.GetProdMode(_subServices[i]);
                _extProdModeMenus[i].selectedIndex = ExtractorProduction.GetProdMode(_subServices[i]);
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
            float currentY = yPos;

            // Header labels.
            UILabels.AddLabel(m_panel, LeftColumn, currentY - 19f, Translations.Translate("RPR_DEF_PMD"), -1, 0.8f);
            UILabels.AddLabel(m_panel, RightColumn, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (_procProdMultSliders == null)
            {
                _procProdModeMenus = new UIDropDown[_subServices.Length];
                _extProdModeMenus = new UIDropDown[_subServices.Length];
                _procProdMultSliders = new UISlider[_subServices.Length];
                _extProdMultSliders = new UISlider[_subServices.Length];
            }

            // Processor production mode menus.
            _procProdModeMenus[index] = UIDropDowns.AddLabelledDropDown(m_panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_PRO"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            _procProdModeMenus[index].tooltipBox = UIToolTips.WordWrapToolTip;
            _procProdModeMenus[index].objectUserData = index;
            _procProdModeMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL"),
            };

            // Processor production multiplication sliders.
            _procProdMultSliders[index] = AddSlider(m_panel, RightColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            _procProdMultSliders[index].objectUserData = index;
            _procProdMultSliders[index].maxValue = IndustrialProduction.MaxProdMult;
            _procProdMultSliders[index].value = IndustrialProduction.GetProdMult(_subServices[index]);
            PercentSliderText(_procProdMultSliders[index], _procProdMultSliders[index].value);

            // Extractor production mode menus.
            currentY += RowHeight;
            _extProdModeMenus[index] = UIDropDowns.AddLabelledDropDown(m_panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_EXT"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            _extProdModeMenus[index].tooltipBox = UIToolTips.WordWrapToolTip;
            _extProdModeMenus[index].objectUserData = index;
            _extProdModeMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL"),
            };

            // Extractor production multiplication sliders.
            _extProdMultSliders[index] = AddSlider(m_panel, RightColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            _extProdMultSliders[index].objectUserData = index;
            _extProdMultSliders[index].maxValue = ExtractorProduction.MaxProdMult;
            _extProdMultSliders[index].value = ExtractorProduction.GetProdMult(_subServices[index]);

            // Always hide generic industrial extractor (index 0) controls.
            if (index == 0)
            {
                _extProdModeMenus[0].Hide();
                _extProdMultSliders[0].parent.Hide();
            }
            else
            {
                PercentSliderText(_extProdMultSliders[index], _extProdMultSliders[index].value);
            }

            // Production calculation mode default event handlers to show/hide multiplier slider.
            _procProdModeMenus[index].eventSelectedIndexChanged += ProcProdDefaultIndexChanged;
            _extProdModeMenus[index].eventSelectedIndexChanged += ExtProdDefaultIndexChanged;

            // Set prodution calculation mode initial selection.
            _procProdModeMenus[index].selectedIndex = IndustrialProduction.GetProdMode(_subServices[index]);
            _extProdModeMenus[index].selectedIndex = ExtractorProduction.GetProdMode(_subServices[index]);

            return currentY;
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
                // Record production calculation modes.
                IndustrialProduction.SetProdMode(_subServices[i], _procProdModeMenus[i].selectedIndex);
                ExtractorProduction.SetProdMode(_subServices[i], _extProdModeMenus[i].selectedIndex);

                // Record production multipliers.
                IndustrialProduction.SetProdMult(_subServices[i], (int)_procProdMultSliders[i].value);
                ExtractorProduction.SetProdMult(_subServices[i], (int)_extProdMultSliders[i].value);
            }

            base.Apply(c, p);
        }

        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event.</param>
        protected override void ResetDefaults(UIComponent c, UIMouseEventParameter p)
        {
            // Reset sliders and menus.
            for (int i = 0; i < _procProdMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                _extProdMultSliders[i].value = ExtractorProduction.DefaultProdMult;
                _procProdMultSliders[i].value = IndustrialProduction.DefaultProdMult;

                // Reset visit mode menu selection.
                _procProdModeMenus[i].selectedIndex = ThisLegacyCategory ? (int)IndustrialProduction.ProdModes.Legacy : (int)IndustrialProduction.ProdModes.PopCalcs;
                _extProdModeMenus[i].selectedIndex = ThisLegacyCategory ? (int)ExtractorProduction.ProdModes.Legacy : (int)ExtractorProduction.ProdModes.PopCalcs;
            }
        }

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void ResetSaved(UIComponent c, UIMouseEventParameter p) => UpdateControls();

        /// <summary>
        /// Extractor production mode menu index changed event handler.
        /// <param name="c">Calling component.</param>
        /// <param name="index">New selected index.</param>
        /// </summary>
        private void ExtProdDefaultIndexChanged(UIComponent c, int index)
        {
            // Extract subservice index from this control's object user data - 0 is generic industrial, for which the extractor controls are always hidden.
            if (c.objectUserData is int subServiceIndex && subServiceIndex != 0)
            {
                // Toggle multiplier slider visibility based on current state.
                _extProdMultSliders[subServiceIndex].parent.isVisible = index == (int)ExtractorProduction.ProdModes.PopCalcs;
            }
        }

        /// <summary>
        /// Processor production mode menu index changed event handler.
        /// <param name="c">Calling component.</param>
        /// <param name="index">New selected index.</param>
        /// </summary>
        private void ProcProdDefaultIndexChanged(UIComponent c, int index)
        {
            // Extract subservice index from this control's object user data.
            if (c.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                _procProdMultSliders[subServiceIndex].parent.isVisible = index == (int)IndustrialProduction.ProdModes.PopCalcs;
            }
        }
    }
}