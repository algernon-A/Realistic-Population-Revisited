// <copyright file="TextfieldPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Base class for options panel textfield-based (sub-)tabs.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Protected fields")]
    internal abstract class TextfieldPanelBase : OptionsPanelTab
    {
        /// <summary>
        /// Layout margin.
        /// </summary>
        protected const float Margin = 5f;

        /// <summary>
        /// Panel title height.
        /// </summary>
        protected const float TitleHeight = 60f;

        /// <summary>
        /// Row height.
        /// </summary>
        protected const float RowHeight = 23f;

        /// <summary>
        /// Left column relative X-position.
        /// </summary>
        protected const float Column1 = 180f;

        /// <summary>
        /// Footer button width.
        /// </summary>
        protected const float ButtonWidth = 230f;

        /// <summary>
        /// Footer button 1 relative X-position.
        /// </summary>
        protected const float Button1X = Margin;

        /// <summary>
        /// Footer button 2 relative X-position.
        /// </summary>
        protected const float Button2X = Button1X + ButtonWidth + (Margin * 2f);

        /// <summary>
        /// Footer button 3 relative X-position.
        /// </summary>
        protected const float Button3X = Button2X + ButtonWidth + (Margin * 2f);

        /// <summary>
        /// Array of power consumption textfields.
        /// </summary>
        protected UITextField[][] m_powerFields;

        /// <summary>
        /// Array of water consumption textfields.
        /// </summary>
        protected UITextField[][] m_waterFields;

        /// <summary>
        /// Array of sewage production textfields.
        /// </summary>
        protected UITextField[][] m_sewageFields;

        /// <summary>
        /// Array of garbage production textfields.
        /// </summary>
        protected UITextField[][] m_garbageFields;

        /// <summary>
        /// Array of income generation textfields.
        /// </summary>
        protected UITextField[][] m_incomeFields;

        /// <summary>
        /// Power consumption label tooltip.
        /// </summary>
        protected string m_powerLabel;

        /// <summary>
        /// Water consumption label tooltip.
        /// </summary>
        protected string m_waterLabel;

        /// <summary>
        /// Sewage production label tooltip.
        /// </summary>
        protected string m_sewageLabel;

        /// <summary>
        /// Garbage production label tooltip.
        /// </summary>
        protected string m_garbageLabel;

        /// <summary>
        /// Income generation label tooltip.
        /// </summary>
        protected string m_incomeLabel;

        /// <summary>
        /// Layout current Y-position indicator.
        /// </summary>
        protected float m_currentY = TitleHeight;

        /// <summary>
        /// Indicates whether or not this panel is displaying values for residential buildings.
        /// </summary>
        protected bool m_notResidential = true;

        // Layout constants - private.
        private const float LeftItem = 75f;

        /// <summary>
        /// Event handler for button events - populate fields with values.
        /// </summary>
        protected virtual void PopulateFields()
        {
        }

        /// <summary>
        /// Event handler for button events - apply field values.
        /// </summary>
        protected virtual void ApplyFields()
        {
        }

        /// <summary>
        /// Event handler for button events - reset fields to defaults.
        /// </summary>
        protected virtual void ResetToDefaults()
        {
        }

        /// <summary>
        /// Adds a column header icon label.
        /// </summary>
        /// <param name="panel">UI panel.</param>
        /// <param name="xPos">Reference X position.</param>
        /// <param name="width">Width of reference item (for centering).</param>
        /// <param name="text">Tooltip text.</param>
        /// <param name="icon">Icon name.</param>
        protected void ColumnIcon(UIPanel panel, float xPos, float width, string text, string icon)
        {
            // Create mini-panel for the icon background.
            UIPanel thumbPanel = panel.AddUIComponent<UIPanel>();
            thumbPanel.width = 35f;
            thumbPanel.height = 35f;
            thumbPanel.relativePosition = new Vector2(xPos + ((width - 35f) / 2), TitleHeight - 40f);
            thumbPanel.clipChildren = true;
            thumbPanel.backgroundSprite = "IconPolicyBaseRect";
            thumbPanel.tooltip = text;

            // Actual icon.
            UISprite thumbSprite = thumbPanel.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.size = thumbPanel.size;
            thumbSprite.atlas = UITextures.InGameAtlas;
            thumbSprite.spriteName = icon;
        }

        /// <summary>
        /// Adds a row text label.
        /// </summary>
        /// <param name="panel">UI panel instance.</param>
        /// <param name="yPos">Reference Y position.</param>
        /// <param name="text">Label text.</param>
        protected void RowLabel(UIPanel panel, float yPos, string text)
        {
            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 0.9f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;
            lineLabel.text = text;

            // X position: by default it's LeftItem, but we move it further left if the label is too long to fit (e.g. long translation strings).
            float xPos = Mathf.Min(LeftItem, (Column1 - Margin) - lineLabel.width);

            // But never further left than the edge of the screen.
            if (xPos < 0)
            {
                xPos = LeftItem;

                // Too long to fit in the given space, so we'll let this wrap across and just move the textfields down an extra line.
                m_currentY += RowHeight;
            }

            lineLabel.relativePosition = new Vector2(xPos, yPos + 2);
        }

        /// <summary>
        /// Adds an input text field at the specified coordinates.
        /// </summary>
        /// <param name="panel">Panel to add to.</param>
        /// <param name="width">Textfield width.</param>
        /// <param name="posX">Relative X postion.</param>
        /// <param name="posY">Relative Y position.</param>
        /// <param name="tooltip">Tooltip, if any.</param>
        /// <returns>New input textfield.</returns>
        protected UITextField AddTextField(UIPanel panel, float width, float posX, float posY, string tooltip = null)
        {
            UITextField textField = UITextFields.AddSmallTextField(panel, posX, posY, width);
            textField.eventTextChanged += (control, value) => PanelUtils.IntTextFilter((UITextField)control, value);

            // Add tooltip.
            if (tooltip != null)
            {
                textField.tooltip = tooltip;
            }

            return textField;
        }
    }
}