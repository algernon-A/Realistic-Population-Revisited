﻿using System;
using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting crime-related options.
    /// </summary>
    internal class CrimePanel
    {
        /// <summary>
        /// Adds crime options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal CrimePanel(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            UIPanel panel = PanelUtils.AddTextTab(tabStrip, Translations.Translate("RPR_OPT_CRI"), tabIndex, out UIButton _);
            UIHelper helper = new UIHelper(panel);
            panel.autoLayout = true;

            // Add slider component.
            UISlider newSlider = UIControls.AddSlider(panel, Translations.Translate("RPR_OPT_CML"), 1f, 200f, 1f, ModSettings.crimeMultiplier);
            newSlider.tooltipBox = TooltipUtils.TooltipBox;
            newSlider.tooltip = Translations.Translate("RPR_OPT_CML_TIP");

            // Value label.
            UIPanel sliderPanel = (UIPanel)newSlider.parent;
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.relativePosition = UIControls.PositionRightOf(newSlider, 8f, 1f);

            // Set initial text.
            PercentSliderText(newSlider, newSlider.value);

            // Slider change event.
            newSlider.eventValueChanged += (control, value) =>
            {
                // Update value label.
                PercentSliderText(control, value);

                // Update setting.
                ModSettings.crimeMultiplier = value;
            };
        }


        /// <summary>
        /// Updates the displayed percentage value on a multiplier slider.
        /// </summary>
        /// <param name="control">Calling component</param>
        /// <param name="value">New valie</param>
        protected void PercentSliderText(UIComponent control, float value)
        {
            if (control?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                decimal decimalNumber = new Decimal(Mathf.RoundToInt(value));
                valueLabel.text = "x" + Decimal.Divide(decimalNumber, 100).ToString("0.00");
            }
        }
    }
}