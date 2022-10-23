// <copyright file="PanelUtils.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Linq;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Utilities for Options Panel UI.
    /// </summary>
    internal static class PanelUtils
    {
        /// <summary>
        /// Event handler filter for text fields to ensure only integer values are entered.
        /// </summary>
        /// <param name="control">Relevant control.</param>
        /// <param name="value">Text value.</param>
        internal static void IntTextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
            if (!value.IsNullOrWhiteSpace() && !int.TryParse(value, out int _))
            {
                control.text = value.Substring(0, value.Length - 1);
                control.MoveSelectionPointRight();
            }
        }

        /// <summary>
        /// Event handler filter for text fields to ensure only floating-point values are entered.
        /// </summary>
        /// <param name="control">Relevant control.</param>
        /// <param name="value">Text value.</param>
        internal static void FloatTextFilter(UITextField control, string value)
        {
            // If it's not blank and isn't an integer, remove the last character and set selection to end.
            if (!value.IsNullOrWhiteSpace() && !float.TryParse(value, out float _))
            {
                control.text = value.Substring(0, value.Length - 1);
                control.MoveSelectionPointRight();
            }
        }

        /// <summary>
        /// Attempts to parse a string for an integer value; if the parse fails, simply does nothing (leaving the original value intact).
        /// </summary>
        /// <param name="intVar">Integer variable to store result (left unchanged if parse fails).</param>
        /// <param name="text">Text to parse.</param>
        internal static void ParseInt(ref int intVar, string text)
        {
            if (int.TryParse(text, out int result))
            {
                intVar = result;
            }
        }

        /// <summary>
        /// Attempts to parse a string for an floating-point value; if the parse fails, simply does nothing (leaving the original value intact).
        /// Parsed value is converted from display units to metric.
        /// </summary>
        /// <param name="floatVar">Float variable to store result (left unchanged if parse fails).</param>
        /// <param name="text">Text to parse.</param>
        /// <param name="isArea">True if this is an area calculation, false if a length calculation.</param>
        internal static void ParseFloat(ref float floatVar, string text, bool isArea)
        {
            if (float.TryParse(text, out float result))
            {
                floatVar = isArea ? Measures.AreaToMetric(result) : Measures.LengthToMetric(result);
            }
        }

        /// <summary>
        /// Adds a row header icon label at the current Y position.
        /// </summary>
        /// <param name="panel">UI panel.</param>
        /// <param name="yPos">Reference Y positions.</param>
        /// <param name="text">Tooltip text.</param>
        /// <param name="icon">Icon name.</param>
        /// <param name="atlas">Icon atlas.</param>
        /// <param name="maxX">Maximum label X-position (wrap text to fit); 0 (default) to ignore.</param>
        internal static void RowHeaderIcon(UIComponent panel, ref float yPos, string text, string icon, string atlas, float maxX = 0f)
        {
            // UI layout constants.
            const float Margin = 5f;
            const float SpriteSize = 35f;
            const float LeftTitle = 50f;

            // Actual icon.
            UISprite thumbSprite = panel.AddUIComponent<UISprite>();
            thumbSprite.relativePosition = new Vector2(Margin, yPos - 2.5f);
            thumbSprite.width = SpriteSize;
            thumbSprite.height = SpriteSize;
            thumbSprite.atlas = UITextures.GetTextureAtlas(atlas);
            thumbSprite.spriteName = icon;

            // Text label.
            UILabel lineLabel = panel.AddUIComponent<UILabel>();
            lineLabel.textScale = 1.0f;
            lineLabel.verticalAlignment = UIVerticalAlignment.Middle;

            // If a maximum X position has been provided, fix the label width and wrap text accordingly.
            if (maxX > 0)
            {
                lineLabel.autoSize = false;
                lineLabel.autoHeight = true;
                lineLabel.wordWrap = true;
                lineLabel.width = maxX - LeftTitle - Margin;
            }
            else
            {
                lineLabel.autoSize = true;
            }

            // Set text.
            lineLabel.text = text;

            // Set poistion.
            lineLabel.relativePosition = new Vector2(LeftTitle, yPos - 2.5f + ((SpriteSize - lineLabel.height) / 2f));

            // Increment our current height.
            yPos += 30f;
        }

        /// <summary>
        /// Adds a column header text label.
        /// </summary>
        /// <param name="panel">UI panel.</param>
        /// <param name="xPos">Reference X position.</param>
        /// <param name="baseY">Y position of base of label.</param>
        /// <param name="width">Width of reference item (for centering).</param>
        /// <param name="text">Label text.</param>
        /// <param name="tooltip">Tooltip text.</param>
        /// <param name="scale">Label text size (default 0.7).</param>
        internal static void ColumnLabel(UIPanel panel, float xPos, float baseY, float width, string text, string tooltip, float scale = 0.7f)
        {
            // Basic setup.
            UILabel columnLabel = panel.AddUIComponent<UILabel>();
            columnLabel.textScale = scale;
            columnLabel.verticalAlignment = UIVerticalAlignment.Middle;
            columnLabel.textAlignment = UIHorizontalAlignment.Center;
            columnLabel.autoSize = false;
            columnLabel.autoHeight = true;
            columnLabel.wordWrap = true;
            columnLabel.width = width;

            // Text and tooltip.
            columnLabel.text = text;
            columnLabel.tooltip = tooltip;
            columnLabel.tooltipBox = UIToolTips.WordWrapToolTip;

            // Set the relative position at the end so we can adjust for the final post-wrap autoheight.
            columnLabel.relativePosition = new Vector2(xPos + ((width - columnLabel.width) / 2), baseY - columnLabel.height);
        }

        /// <summary>
        /// Adds a title label across the top of the specified UIComponent.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="titleKey">Title translation key.</param>
        /// <returns>Y position below title.</returns>
        internal static float TitleLabel(UIComponent parent, string titleKey)
        {
            // Margin.
            const float Margin = 5f;

            // Add title.
            UILabel titleLabel = UILabels.AddLabel(parent, 0f, Margin, Translations.Translate(titleKey), parent.width, 1.5f);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");

            UISpacers.AddOptionsSpacer(parent, Margin, titleLabel.height + (Margin * 2f), parent.width - (Margin * 2f));

            return Margin + titleLabel.height + Margin + 5f + Margin;
        }
    }
}