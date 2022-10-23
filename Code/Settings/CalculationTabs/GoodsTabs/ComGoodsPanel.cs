// <copyright file="ComGoodsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting commercial goods calculations.
    /// </summary>
    internal class ComGoodsPanel : GoodsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_CLO"),
            Translations.Translate("RPR_CAT_CHI"),
            Translations.Translate("RPR_CAT_CW2"),
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
            ItemClass.Service.Commercial,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialWallToWall,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialTourist,
        };

        private readonly string[] _iconNames =
        {
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "DistrictSpecializationCommercialWallToWall",
            "IconPolicyOrganic",
            "IconPolicyLeisure",
            "IconPolicyTourist",
        };

        private readonly string[] _atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
        };

        // Panel components.
        private UIDropDown[] _visitDefaultMenus;
        private UISlider[] _visitMultSliders;
        private UISlider[] _goodsMultSliders;
        private UISlider[] _inventorySliders;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComGoodsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ComGoodsPanel(UITabstrip tabStrip, int tabIndex)
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
        protected override string TitleKey => "RPR_TIT_CGO";

        /// <summary>
        /// Gets or sets a value indicating whether the legacy 'use legacy by default for this save' setting is in effect.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyCom; set => ModSettings.ThisSaveLegacyCom = value; }

        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < _visitMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                _visitMultSliders[i].value = Visitors.GetVisitMult(_subServices[i]);

                // Reset visit mode menu selections.
                _visitDefaultMenus[i].selectedIndex = Visitors.GetVisitMode(_subServices[i]);

                // Reset inventory cap slider value.
                _inventorySliders[i].value = GoodsUtils.GetInventoryCap(_subServices[i]);
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
            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (_visitDefaultMenus == null)
            {
                _visitDefaultMenus = new UIDropDown[_subServices.Length];
                _visitMultSliders = new UISlider[_subServices.Length];
                _goodsMultSliders = new UISlider[_subServices.Length];
                _inventorySliders = new UISlider[_subServices.Length];
            }

            // Sales multiplier slider.
            float currentY = yPos;
            _goodsMultSliders[index] = AddSlider(m_panel, LeftColumn, currentY, ControlWidth, "RPR_DEF_CGM_TIP");
            _goodsMultSliders[index].objectUserData = index;
            _goodsMultSliders[index].value = (int)GoodsUtils.GetComMult(_subServices[index]);
            PercentSliderText(_goodsMultSliders[index], _goodsMultSliders[index].value);

            // Sales multiplier label.
            UILabel goodsLabel = UILabels.AddLabel(m_panel, 0f, 0f, Translations.Translate("RPR_DEF_CGM"), textScale: 0.8f);
            goodsLabel.relativePosition = new Vector2(LeftColumn - 10f - goodsLabel.width, currentY + ((_goodsMultSliders[index].parent.height - goodsLabel.height) / 2f));

            // Vist mode header label.
            UILabels.AddLabel(m_panel, RightColumn, currentY - 19f, Translations.Translate("RPR_DEF_VIS"), -1, 0.8f);

            // Visit mode menu.
            _visitDefaultMenus[index] = UIDropDowns.AddDropDown(m_panel, RightColumn, currentY, ControlWidth, height: 20f, itemVertPadding: 6, tooltip: Translations.Translate("RPR_DEF_VIS_TIP"));
            _visitDefaultMenus[index].tooltipBox = UIToolTips.WordWrapToolTip;
            _visitDefaultMenus[index].objectUserData = index;
            _visitDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL"),
            };

            // Inventory cap slider.
            currentY += RowHeight;
            _inventorySliders[index] = AddSlider(m_panel, LeftColumn, currentY, ControlWidth, "RPR_DEF_IDC_TIP", false);
            _inventorySliders[index].objectUserData = index;
            _inventorySliders[index].minValue = GoodsUtils.MinInventory;
            _inventorySliders[index].maxValue = GoodsUtils.MaxInventory;
            _inventorySliders[index].stepSize = 1000f;
            _inventorySliders[index].value = GoodsUtils.GetInventoryCap(_subServices[index]);
            AbsSliderText(_inventorySliders[index], _inventorySliders[index].value);

            // Inventory cap label.
            UILabel invLabel = UILabels.AddLabel(m_panel, 0f, 0f, Translations.Translate("RPR_DEF_IDC"), textScale: 0.8f);
            invLabel.relativePosition = new Vector2(LeftColumn - 10f - invLabel.width, currentY + ((_inventorySliders[index].parent.height - invLabel.height) / 2f));

            // Visitor multiplication slider.
            _visitMultSliders[index] = AddSlider(m_panel, RightColumn, currentY, ControlWidth, "RPR_DEF_VMU_TIP");
            _visitMultSliders[index].maxValue = 200f;
            _visitMultSliders[index].objectUserData = index;
            _visitMultSliders[index].value = Visitors.GetVisitMult(_subServices[index]);
            PercentSliderText(_visitMultSliders[index], _visitMultSliders[index].value);

            // Visit mode default event handler to show/hide multiplier slider.
            _visitDefaultMenus[index].eventSelectedIndexChanged += VisitDefaultIndexChanged;

            // Set visit mode initial selection.
            _visitDefaultMenus[index].selectedIndex = Visitors.GetVisitMode(_subServices[index]);

            return currentY;
        }

        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event.</param>
        protected override void Apply(UIComponent c, UIMouseEventParameter p)
        {
            // Iterate through all subservices.
            for (int i = 0; i < _subServices.Length; ++i)
            {
                // Record vist calculation modes.
                Visitors.SetVisitMode(_subServices[i], _visitDefaultMenus[i].selectedIndex);

                // Record visitor multiplier.
                Visitors.SetVisitMult(_subServices[i], (int)_visitMultSliders[i].value);

                // Record goods multiplier.
                GoodsUtils.SetComMult(_subServices[i], (int)_goodsMultSliders[i].value);

                // Record inventory cap.
                GoodsUtils.SetInventoryCap(_subServices[i], (int)_inventorySliders[i].value);

                // Recalculate citizen units.
                CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.None, _subServices[i], false);
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
            for (int i = 0; i < _visitMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider value.s
                _goodsMultSliders[i].value = GoodsUtils.DefaultSalesMult;

                // Reset visit mode menu selection.
                _visitDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)Visitors.ComVisitModes.Legacy : (int)Visitors.ComVisitModes.PopCalcs;

                // Reset goods multiplier slider value.
                _visitMultSliders[i].value = Visitors.DefaultVisitMult(_subServices[i]);

                // Reset inventory cap slider value.
                _inventorySliders[i].value = GoodsUtils.DefaultInventory;
            }
        }

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected override void ResetSaved(UIComponent c, UIMouseEventParameter p) => UpdateControls();

        /// <summary>
        /// Visit default menu index changed event handler.
        /// <param name="c">Calling component.</param>
        /// <param name="index">New selected index.</param>
        /// </summary>
        private void VisitDefaultIndexChanged(UIComponent c, int index)
        {
            // Extract subservice index from this control's object user data.
            if (c.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                _visitMultSliders[subServiceIndex].parent.isVisible = index == (int)Visitors.ComVisitModes.PopCalcs;
            }
        }
    }
}