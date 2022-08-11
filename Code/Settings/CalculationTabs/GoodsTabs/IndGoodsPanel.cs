// <copyright file="IndGoodsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using Realistic_Population_Revisited.Code.Patches.Production;

    /// <summary>
    /// Options panel for setting industry goods calculations.
    /// </summary>
    internal class IndGoodsPanel : GoodsPanelBase
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_IND"),
            Translations.Translate("RPR_CAT_FAR"),
            Translations.Translate("RPR_CAT_FOR"),
            Translations.Translate("RPR_CAT_OIL"),
            Translations.Translate("RPR_CAT_ORE")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial,
            ItemClass.Service.Industrial
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.IndustrialGeneric,
            ItemClass.SubService.IndustrialFarming,
            ItemClass.SubService.IndustrialForestry,
            ItemClass.SubService.IndustrialOil,
            ItemClass.SubService.IndustrialOre
        };

        private readonly string[] iconNames =
        {
            "ZoningIndustrial",
            "IconPolicyFarming",
            "IconPolicyForest",
            "IconPolicyOil",
            "IconPolicyOre"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Ingame",
            "Ingame",
            "Ingame",
            "Ingame"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;


        // Title key.
        protected override string TitleKey => "RPR_TIT_IGO";

        // Panel components.
        private UISlider[] procProdMultSliders, extProdMultSliders;
        private UIDropDown[] procProdModeMenus, extProdModeMenus;

        /// <summary>
        /// Legacy settings link.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyInd; set => ModSettings.ThisSaveLegacyInd = value; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal IndGoodsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < procProdMultSliders.Length; ++i)
            {
                // Reset visit multiplier slider values.
                procProdMultSliders[i].value = IndustrialProduction.GetProdMult(subServices[i]);
                extProdMultSliders[i].value = ExtractorProduction.GetProdMult(subServices[i]);

                // Reset visit mode menu selections.
                procProdModeMenus[i].selectedIndex = IndustrialProduction.GetProdMode(subServices[i]);
                extProdModeMenus[i].selectedIndex = ExtractorProduction.GetProdMode(subServices[i]);
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
            UILabels.AddLabel(panel, LeftColumn, currentY - 19f, Translations.Translate("RPR_DEF_PMD"), -1, 0.8f);
            UILabels.AddLabel(panel, RightColumn, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (procProdMultSliders == null)
            {
                procProdModeMenus = new UIDropDown[subServices.Length];
                extProdModeMenus = new UIDropDown[subServices.Length];
                procProdMultSliders = new UISlider[subServices.Length];
                extProdMultSliders = new UISlider[subServices.Length];
            }

            // Processor production mode menus.
            procProdModeMenus[index] = UIDropDowns.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_PRO"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            procProdModeMenus[index].tooltipBox = UIToolTips.WordWrapToolTip;
            procProdModeMenus[index].objectUserData = index;
            procProdModeMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Processor production multiplication sliders.
            procProdMultSliders[index] = AddSlider(panel, RightColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            procProdMultSliders[index].objectUserData = index;
            procProdMultSliders[index].maxValue = IndustrialProduction.MaxProdMult;
            procProdMultSliders[index].value = IndustrialProduction.GetProdMult(subServices[index]);
            PercentSliderText(procProdMultSliders[index], procProdMultSliders[index].value);

            // Extractor production mode menus.
            currentY += RowHeight;
            extProdModeMenus[index] = UIDropDowns.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAT_EXT"), ControlWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false, tooltip: Translations.Translate("RPR_DEF_PMD_TIP"));
            extProdModeMenus[index].tooltipBox = UIToolTips.WordWrapToolTip;
            extProdModeMenus[index].objectUserData = index;
            extProdModeMenus[index].items = new string[]
            {
                Translations.Translate("RPR_DEF_VNE"),
                Translations.Translate("RPR_DEF_VOL")
            };

            // Extractor production multiplication sliders.
            extProdMultSliders[index] = AddSlider(panel, RightColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            extProdMultSliders[index].objectUserData = index;
            extProdMultSliders[index].maxValue = ExtractorProduction.MaxProdMult;
            extProdMultSliders[index].value = ExtractorProduction.GetProdMult(subServices[index]);

            // Always hide generic industrial extractor (index 0) controls.
            if (index == 0)
            {
                extProdModeMenus[0].Hide();
                extProdMultSliders[0].parent.Hide();
            }
            else
            {
                PercentSliderText(extProdMultSliders[index], extProdMultSliders[index].value);
            }

            // Production calculation mode default event handlers to show/hide multiplier slider.
            procProdModeMenus[index].eventSelectedIndexChanged += ProcProdDefaultIndexChanged;
            extProdModeMenus[index].eventSelectedIndexChanged += ExtProdDefaultIndexChanged;

            // Set prodution calculation mode initial selection.
            procProdModeMenus[index].selectedIndex = IndustrialProduction.GetProdMode(subServices[index]);
            extProdModeMenus[index].selectedIndex = ExtractorProduction.GetProdMode(subServices[index]);

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
            for (int i = 0; i < subServices.Length; ++i)
            {
                // Record production calculation modes.
                IndustrialProduction.SetProdMode(subServices[i], procProdModeMenus[i].selectedIndex);
                ExtractorProduction.SetProdMode(subServices[i], extProdModeMenus[i].selectedIndex);

                // Record production multipliers.
                IndustrialProduction.SetProdMult(subServices[i], (int)procProdMultSliders[i].value);
                ExtractorProduction.SetProdMult(subServices[i], (int)extProdMultSliders[i].value);
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
            for (int i = 0; i < procProdMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                extProdMultSliders[i].value = ExtractorProduction.DefaultProdMult;
                procProdMultSliders[i].value = IndustrialProduction.DefaultProdMult;

                // Reset visit mode menu selection.
                procProdModeMenus[i].selectedIndex = ThisLegacyCategory ? (int)IndustrialProduction.ProdModes.legacy : (int)IndustrialProduction.ProdModes.popCalcs;
                extProdModeMenus[i].selectedIndex = ThisLegacyCategory ? (int)ExtractorProduction.ProdModes.legacy : (int)ExtractorProduction.ProdModes.popCalcs;
            }
        }

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused)</param>
        /// <param name="p">Mouse event (unused)</param>
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
                extProdMultSliders[subServiceIndex].parent.isVisible = index == (int)ExtractorProduction.ProdModes.popCalcs;
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
                procProdMultSliders[subServiceIndex].parent.isVisible = index == (int)IndustrialProduction.ProdModes.popCalcs;
            }
        }
    }
}