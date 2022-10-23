// <copyright file="CalculationsPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.Globalization;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class CalculationsPanelBase : OptionsPanelTab
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        /// <summary>
        /// Row Height.
        /// </summary>
        protected const float RowHeight = 25f;

        /// <summary>
        /// Left column relative X-position.
        /// </summary>
        protected const float LeftColumn = 200f;

        /// <summary>
        /// Footer button width.
        /// </summary>
        protected const float ButtonWidth = 240f;

        /// <summary>
        /// Footer button 1 relative X-position.
        /// </summary>
        protected const float Button1X = Margin;

        /// <summary>
        /// Footer button 2 relative X-position.
        /// </summary>
        protected const float Button2X = Button1X + ButtonWidth + Margin;

        /// <summary>
        /// Footer button 3 relative X-position.
        /// </summary>
        protected const float Button3X = Button2X + ButtonWidth + Margin;

        // Instance reference.
        private static CalculationsPanelBase s_instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationsPanelBase"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal CalculationsPanelBase(UITabstrip tabStrip, int tabIndex)
        {
            // Add tab and helper.
            m_panel = UITabstrips.AddIconTab(tabStrip, TabName, tabIndex, TabIconNames, TabAtlasNames, TabWidth);

            // Set instance.
            s_instance = this;

            // Set tab object reference.
            tabStrip.tabs[tabIndex].objectUserData = this;
        }

        /// <summary>
        /// Gets the tab width.
        /// </summary>
        protected virtual float TabWidth => 100f;

        /// <summary>
        /// Gets the tab name.
        /// </summary>
        protected abstract string TabName { get; }

        /// <summary>
        /// Gets the array of icon sprite names for this tab.
        /// </summary>
        protected abstract string[] TabIconNames { get; }

        /// <summary>
        /// Gets the array of icon atlas names for this tab.
        /// </summary>
        protected abstract string[] TabAtlasNames { get; }

        /// <summary>
        /// Gets the array of sub-service display names for this tab.
        /// </summary>
        protected abstract string[] SubServiceNames { get; }

        /// <summary>
        /// Gets the array of relevant building services for this tab.
        /// </summary>
        protected abstract ItemClass.Service[] Services { get; }

        /// <summary>
        /// Gets the array of relevant building sub-services for this tab.
        /// </summary>
        protected abstract ItemClass.SubService[] SubServices { get; }

        /// <summary>
        /// Gets the array of building type icon sprite names for this tab.
        /// </summary>
        protected abstract string[] IconNames { get; }

        /// <summary>
        /// Gets the array of building type icon atlas names for this tab.
        /// </summary>
        protected abstract string[] AtlasNames { get; }

        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected abstract void ResetDefaults(UIComponent c, UIMouseEventParameter p);

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        protected abstract void ResetSaved(UIComponent c, UIMouseEventParameter p);

        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        protected virtual void FooterButtons(float yPos)
        {
            // Reset button.
            UIButton resetButton = UIButtons.AddButton(m_panel, Button1X, yPos, Translations.Translate("RPR_OPT_RTD"), ButtonWidth);
            resetButton.eventClicked += ResetDefaults;

            // Revert button.
            UIButton revertToSaveButton = UIButtons.AddButton(m_panel, Button2X, yPos, Translations.Translate("RPR_OPT_RTS"), ButtonWidth);
            revertToSaveButton.eventClicked += ResetSaved;
        }

        /// <summary>
        /// 'Save and apply' button.
        /// </summary>
        /// <param name="parent">Parent UIComponent.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <returns>New UIButton.</returns>
        protected UIButton AddSaveButton(UIComponent parent, float yPos) => UIButtons.AddButton(parent, Button3X, yPos, Translations.Translate("RPR_OPT_SAA"), ButtonWidth);

        /// <summary>
        /// Adds a slider.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Relative X position.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <param name="width">Slider width.</param>
        /// <param name="tooltipKey">Tooltip translation key.</param>
        /// <param name="isPercent">True if this slider should display percentage values, false to display absolute values.</param>
        /// <returns>New slider.</returns>
        protected UISlider AddSlider(UIComponent parent, float xPos, float yPos, float width, string tooltipKey, bool isPercent = true)
        {
            // Layout constants.
            const float SliderPanelHeight = 20f;
            const float SliderHeight = 6f;
            const float ValueLabelWidth = 45f;
            const float OffsetX = (SliderPanelHeight - SliderHeight) / 2f;

            // Mutiplier slider panel.
            UIPanel sliderPanel = parent.AddUIComponent<UIPanel>();
            sliderPanel.autoSize = false;
            sliderPanel.autoLayout = false;
            sliderPanel.size = new Vector2(width, SliderPanelHeight);
            sliderPanel.relativePosition = new Vector2(xPos, yPos);

            // Mutiplier slider value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.verticalAlignment = UIVerticalAlignment.Middle;
            valueLabel.textAlignment = UIHorizontalAlignment.Center;
            valueLabel.textScale = 0.7f;
            valueLabel.autoSize = false;
            valueLabel.color = new Color32(91, 97, 106, 255);
            valueLabel.size = new Vector2(ValueLabelWidth, 15f);
            valueLabel.relativePosition = new Vector2(sliderPanel.width - ValueLabelWidth - Margin, (SliderPanelHeight - valueLabel.height) / 2f);

            // Mutiplier slider control.
            UISlider newSlider = sliderPanel.AddUIComponent<UISlider>();
            newSlider.size = new Vector2(sliderPanel.width - ValueLabelWidth - (Margin * 3), SliderHeight);
            newSlider.relativePosition = new Vector2(0f, OffsetX);

            // Mutiplier slider track.
            UISlicedSprite sliderSprite = newSlider.AddUIComponent<UISlicedSprite>();
            sliderSprite.autoSize = false;
            sliderSprite.size = new Vector2(newSlider.width, newSlider.height);
            sliderSprite.relativePosition = new Vector2(0f, 0f);
            sliderSprite.atlas = UITextures.InGameAtlas;
            sliderSprite.spriteName = "ScrollbarTrack";

            // Mutiplier slider thumb.
            UISlicedSprite sliderThumb = newSlider.AddUIComponent<UISlicedSprite>();
            sliderThumb.atlas = UITextures.InGameAtlas;
            sliderThumb.spriteName = "ScrollbarThumb";
            sliderThumb.height = 20f;
            sliderThumb.width = 10f;
            sliderThumb.relativePosition = new Vector2(0f, -OffsetX);
            newSlider.thumbObject = sliderThumb;

            // Tooltip.
            newSlider.tooltipBox = UIToolTips.WordWrapToolTip;
            newSlider.tooltip = Translations.Translate(tooltipKey);

            // Mutiplier slider value range.
            newSlider.stepSize = 1f;
            newSlider.minValue = 1f;
            newSlider.maxValue = 100f;

            // Event handler to update text.
            if (isPercent)
            {
                newSlider.eventValueChanged += PercentSliderText;
            }
            else
            {
                newSlider.eventValueChanged += AbsSliderText;
            }

            return newSlider;
        }

        /// <summary>
        /// Updates the displayed percentage value on a multiplier slider.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="value">New value.</param>
        protected void PercentSliderText(UIComponent c, float value)
        {
            if (c?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                decimal decimalNumber = new decimal(Mathf.RoundToInt(value));
                valueLabel.text = 'x' + decimal.Divide(decimalNumber, 100).ToString("0.00");
            }
        }

        /// <summary>
        /// Updates the displayed absolute value on a multiplier slider.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="value">New value.</param>
        protected void AbsSliderText(UIComponent c, float value)
        {
            if (c?.parent?.Find<UILabel>("ValueLabel") is UILabel valueLabel)
            {
                valueLabel.text = Mathf.RoundToInt(value).ToString("N0", LocaleManager.cultureInfo);
            }
        }
    }
}