﻿using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal class IndDefaultsPanel : EmpDefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_IND")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Industrial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.IndustrialGeneric
        };

        private readonly string[] iconNames =
        {
            "ZoningIndustrial"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;

        // Tab width.
        protected override float TabWidth => 40f;


        // Panel components.
        private UISlider[] prodMultSliders;
        private UIDropDown[] prodDefaultMenus;


        // Legacy settings references.
        protected override bool NewLegacyCategory { get => ModSettings.newSaveLegacyInd; set => ModSettings.newSaveLegacyInd = value; }
        protected override bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyInd; set => ModSettings.ThisSaveLegacyInd = value; }
        protected override string LegacyCheckLabel => "RPR_DEF_LGI";


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal IndDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


        // <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateMenus()
        {
            base.UpdateMenus();

            // Reset sliders and menus.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                prodMultSliders[i].value = RealisticIndustrialProduction.GetProdMult();

                // Reset visit mode menu selections.
                prodDefaultMenus[i].selectedIndex = RealisticIndustrialProduction.GetProdMode();
            }
        }


        /// <summary>
        /// Adds any additional controls to each row.
        /// </summary>
        /// <param name="panel">Panel reference</param>
        /// <param name="yPos">Relative Y position at top of row items</param>
        /// <param name="index">Index number of this row</param>
        /// <returns>Relative Y coordinate adjusted for any finished setup</returns>
        protected override float RowAdditions(UIPanel panel, float yPos, int index)
        {
            // Layout constants.
            float controlWidth = panel.width - RowAdditionX;


            float currentY = yPos - RowHeight;

            // Header label.
            UIControls.AddLabel(panel, RowAdditionX, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // RowAdditions is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (prodMultSliders == null)
            {
                prodMultSliders = new UISlider[subServices.Length];
                prodDefaultMenus = new UIDropDown[subServices.Length];
            }

            prodDefaultMenus[index] = UIControls.AddDropDown(panel, RowAdditionX, currentY, controlWidth, height: 20f, itemVertPadding: 6, tooltip: Translations.Translate("RPR_DEF_VIS_TIP"));
            prodDefaultMenus[index].tooltipBox = TooltipUtils.TooltipBox;
            prodDefaultMenus[index].objectUserData = index;
            prodDefaultMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Production multiplication slider.
            currentY = yPos;
            prodMultSliders[index] = AddSlider(panel, RowAdditionX, currentY, controlWidth);
            prodMultSliders[index].objectUserData = index;
            prodMultSliders[index].maxValue = RealisticOfficeProduction.MaxProdMult;
            prodMultSliders[index].value = RealisticOfficeProduction.GetProdMult(subServices[index]);
            prodMultSliders[index].tooltipBox = TooltipUtils.TooltipBox;
            prodMultSliders[index].tooltip = Translations.Translate("RPR_DEF_PRD_TIP");
            MultSliderText(prodMultSliders[index], prodMultSliders[index].value);

            // Production calculation mode default event handler to show/hide multiplier slider.
            prodDefaultMenus[index].eventSelectedIndexChanged += ProdDefaultIndexChanged;

            // Set prodution calculation mode initial selection.
            prodDefaultMenus[index].selectedIndex = RealisticIndustrialProduction.GetProdMode();

            return yPos;
        }


        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through all subservices.
            for (int i = 0; i < subServices.Length; ++i)
            {
                // Record production calculation modes.
                RealisticIndustrialProduction.SetProdMode(prodDefaultMenus[i].selectedIndex);

                // Record production multiplier.
                RealisticIndustrialProduction.SetProdMult((int)prodMultSliders[i].value);
            }

            base.Apply(control, mouseEvent);
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            base.ResetDefaults(control, mouseEvent);

            // Reset sliders and menus.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                prodMultSliders[i].value = RealisticIndustrialProduction.DefaultProdMult;

                // Reset visit mode menu selection.
                prodDefaultMenus[i].selectedIndex = ThisLegacyCategory ? (int)RealisticIndustrialProduction.ProdModes.legacy : (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            }
        }


        /// <summary>
        /// Production mode menu index changed event handler.
        /// <param name="control">Calling component</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void ProdDefaultIndexChanged(UIComponent control, int index)
        {
            // Extract subservice index from this control's object user data.
            if (control.objectUserData is int subServiceIndex)
            {
                // Toggle multiplier slider visibility based on current state.
                prodMultSliders[subServiceIndex].parent.isVisible = index == (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            }
        }
    }
}