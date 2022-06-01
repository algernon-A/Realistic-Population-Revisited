using System.Linq;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Utilities for Options Panel UI.
    /// </summary>
    internal static class PanelUtils
    {
        /// <summary>
        /// Event handler filter for text fields to ensure only integer values are entered.
        /// </summary>
        /// <param name="control">Relevant control</param>
        /// <param name="value">Text value</param>
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
        /// <param name="control">Relevant control</param>
        /// <param name="value">Text value</param>
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
        /// <param name="intVar">Integer variable to store result (left unchanged if parse fails)</param>
        /// <param name="text">Text to parse</param>
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
        /// <param name="floatVer">Float variable to store result (left unchanged if parse fails)</param>
        /// <param name="text">Text to parse</param>
        /// <param name="isArea">True if this is an area calculation, false if a length calculation</param>
        internal static void ParseFloat(ref float floatVar, string text, bool isArea)
        {
            if (float.TryParse(text, out float result))
            {
                floatVar = isArea ? Measures.AreaToMetric(result) : Measures.LengthToMetric(result);
            }
        }


        /// <summary>
        /// Adds a text-based tab to a UI tabstrip.
        /// </summary>
        /// <param name="tabStrip">UIT tabstrip to add to</param>
        /// <param name="tabName">Name of this tab</param>
        /// <param name="tabIndex">Index number of this tab</param>
        /// <param name="button">Tab button instance reference</param>
        /// <param name="width">Tab width</param>
        /// <param name="autoLayout">Default autoLayout setting</param>
        /// <returns>UIHelper instance for the new tab panel</returns>
        internal static UIPanel AddTextTab(UITabstrip tabStrip, string tabName, int tabIndex, out UIButton button, float width = 170f, bool autoLayout = false)
        {
            // Create tab.
            UIButton tabButton = tabStrip.AddTab(tabName);

            // Sprites.
            tabButton.normalBgSprite = "SubBarButtonBase";
            tabButton.disabledBgSprite = "SubBarButtonBaseDisabled";
            tabButton.focusedBgSprite = "SubBarButtonBaseFocused";
            tabButton.hoveredBgSprite = "SubBarButtonBaseHovered";
            tabButton.pressedBgSprite = "SubBarButtonBasePressed";

            // Tooltip.
            tabButton.tooltip = tabName;

            // Force width.
            tabButton.width = width;

            // Get tab root panel.
            UIPanel rootPanel = tabStrip.tabContainer.components[tabIndex] as UIPanel;

            // Panel setup.
            rootPanel.autoLayout = autoLayout;
            rootPanel.autoLayoutDirection = LayoutDirection.Vertical;
            rootPanel.autoLayoutPadding.top = 5;
            rootPanel.autoLayoutPadding.left = 10;

            button = tabButton;

            return rootPanel;
        }


        /// <summary>
        /// Adds an icon-based tab to a UI tabstrip.
        /// </summary>
        /// <param name="tabStrip">UIT tabstrip to add to</param>
        /// <param name="tabName">Name of this tab</param>
        /// <param name="tabIndex">Index number of this tab</param>
        /// <param name="iconNames">Icon sprite names</param>
        /// <param name="atlasNames">Icon atlas names</param>
        /// <param name="width">Tab width</param>
        /// <param name="autoLayout">Default autoLayout setting</param>
        /// <returns>UIHelper instance for the new tab panel</returns>
        internal static UIPanel AddIconTab(UITabstrip tabStrip, string tabName, int tabIndex, string[] iconNames, string[] atlasNames, float width = 120f, bool autoLayout = false)
        {
            // Layout constants.
            const float TabIconSize = 23f;


            // Create tab.
            UIPanel rootPanel = AddTextTab(tabStrip, tabName, tabIndex, out UIButton button, width, autoLayout);

            // Clear button text.
            button.text = "";

            // Add tab sprites.
            float spriteBase = (width - 2f) / iconNames.Length;
            float spriteOffset = (spriteBase - TabIconSize) / 2f;
            for (int i = 0; i < iconNames.Length; ++i)
            {
                UISprite thumbSprite = button.AddUIComponent<UISprite>();
                thumbSprite.relativePosition = new Vector2(1f + (spriteBase * i) + spriteOffset, 1f);
                thumbSprite.width = TabIconSize;
                thumbSprite.height = TabIconSize;
                thumbSprite.atlas = TextureUtils.GetTextureAtlas(atlasNames[i]);
                thumbSprite.spriteName = iconNames[i];

                // Put later sprites behind earlier sprites, for clarity.
                thumbSprite.SendToBack();
            }

            return rootPanel;
        }


        /// <summary>
        /// Adds a row header icon label at the current Y position.
        /// </summary>
        /// <param name="panel">UI panel</param>
        /// <param name="yPos">Reference Y positions</param>
        /// <param name="text">Tooltip text</param>
        /// <param name="icon">Icon name</param>
        /// <param name="maX">Maximum label X-position (wrap text to fit); 0 (default) to ignore</param>
        internal static void RowHeaderIcon(UIPanel panel, ref float yPos, string text, string icon, string atlas, float maxX = 0f)
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
            thumbSprite.atlas = TextureUtils.GetTextureAtlas(atlas);
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
        /// <param name="panel">UI panel</param>
        /// <param name="xPos">Reference X position</param>
        /// <param name="baseY">Y position of base of label/param>
        /// <param name="width">Width of reference item (for centering)</param>
        /// <param name="text">Label text</param>
        /// <param name="tooltip">Tooltip text</param>
        /// <param name="scale">Label text size (default 0.7)</param>
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
            columnLabel.tooltipBox = TooltipUtils.TooltipBox;

            // Set the relative position at the end so we can adjust for the final post-wrap autoheight.
            columnLabel.relativePosition = new Vector2(xPos + ((width - columnLabel.width) / 2), baseY - columnLabel.height);
        }


        /// <summary>
        /// Adds a title label across the top of the specified UIComponent.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="titleKey">Title translation key</param>
        /// <returns>Y position below title</returns>
        internal static float TitleLabel(UIComponent parent, string titleKey)
        {
            // Margin.
            const float Margin = 5f;

            // Add title.
            UILabel titleLabel = UIControls.AddLabel(parent, 0f, Margin, Translations.Translate(titleKey), parent.width, 1.5f);
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Semibold");

            UIControls.OptionsSpacer(parent, Margin, titleLabel.height + (Margin * 2f), parent.width - (Margin * 2f));

            return Margin + titleLabel.height + Margin + 5f + Margin;
        }
    }
}