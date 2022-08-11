

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using Realistic_Population_Revisited.Code.Patches.Production;

    /// <summary>
    /// Options panel for setting office goods calculations.
    /// </summary>
    internal class OffGoodsPanel : GoodsPanelBase
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
        protected override string TitleKey => "RPR_TIT_OGO";

        // Panel components.
        private UISlider[] prodMultSliders;

        /// <summary>
        /// Legacy settings link.
        /// </summary>
        protected bool ThisLegacyCategory { get => ModSettings.ThisSaveLegacyOff; set => ModSettings.ThisSaveLegacyOff = value; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal OffGoodsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Updates pack selection menu items.
        /// </summary>
        internal override void UpdateControls()
        {
            base.UpdateControls();

            // Reset sliders and menus.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider values.
                prodMultSliders[i].value = OfficeProduction.GetProdMult(subServices[i]);
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
            UILabels.AddLabel(panel, LeftColumn, currentY - 19f, Translations.Translate("RPR_DEF_PRD"), -1, 0.8f);

            // SubServiceControls is called as part of parent constructor, so we need to initialise them here if they aren't already.
            if (prodMultSliders == null)
            {
                prodMultSliders = new UISlider[subServices.Length];
            }

            // Production multiplication slider.
            prodMultSliders[index] = AddSlider(panel, LeftColumn, currentY, ControlWidth, "RPR_DEF_PRD_TIP");
            prodMultSliders[index].objectUserData = index;
            prodMultSliders[index].maxValue = OfficeProduction.MaxProdMult;
            prodMultSliders[index].value = OfficeProduction.GetProdMult(subServices[index]);
            PercentSliderText(prodMultSliders[index], prodMultSliders[index].value);

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
            for (int i = 0; i < subServices.Length; ++i)
            {
                // Record production mutltiplier.
                OfficeProduction.SetProdMult(subServices[i], (int)prodMultSliders[i].value);
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
            // Reset sliders.
            for (int i = 0; i < prodMultSliders.Length; ++i)
            {
                // Reset production multiplier slider value.
                prodMultSliders[i].value = OfficeProduction.DefaultProdMult;
            }
        }

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused)</param>
        /// <param name="p">Mouse event (unused)</param>
        protected override void ResetSaved(UIComponent c, UIMouseEventParameter p) => UpdateControls();
    }
}