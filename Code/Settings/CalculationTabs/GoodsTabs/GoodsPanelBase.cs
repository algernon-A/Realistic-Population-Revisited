// <copyright file="GoodsPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class GoodsPanelBase : CalculationsPanelBase
    {
        // Layout constants.
        protected const float ControlWidth = 250f;
        protected const float RightColumn = LeftColumn + ControlWidth + (Margin * 2f);

        // Tab icons.
        private readonly string[] tabIconNames =
        {
            "IconPolicyAutomatedSorting"
        };

        private readonly string[] tabAtlasNames =
        {
            "Ingame"
        };

        // Tab settings.
        protected override string TabName => Translations.Translate(TitleKey);
        protected override string[] TabIconNames => tabIconNames;
        protected override string[] TabAtlasNames => tabAtlasNames;

        // Tab width.
        protected override float TabWidth => 40f;

        // Title key.
        protected abstract string TitleKey { get; }

        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal GoodsPanelBase(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Add title.
                float currentY = PanelUtils.TitleLabel(panel, TitleKey);

                // Add menus.
                currentY = SetUpMenus(panel, currentY);

                // Add buttons- add extra space.
                FooterButtons(currentY + Margin);
            }
        }

        /// <summary>
        /// Updates controls.
        /// </summary>
        internal virtual void UpdateControls()
        {
        }

        /// <summary>
        /// Adds footer buttons to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        protected override void FooterButtons(float yPos)
        {
            base.FooterButtons(yPos);

            // Save button.
            UIButton saveButton = AddSaveButton(panel, yPos);
            saveButton.eventClicked += Apply;
        }

        /// <summary>
        /// Adds controls for each sub-service.
        /// </summary>
        /// <param name="yPos">Relative Y position at top of row items.</param>
        /// <param name="index">Index number of this row.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected virtual float SubServiceControls(float yPos, int index)
        {
            return yPos;
        }

        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused).</param>
        /// <param name="p">Mouse event (unused).</param>
        protected virtual void Apply(UIComponent c, UIMouseEventParameter p)
        {
            // Save settings.
            ConfigUtils.SaveSettings();
        }

        /// <summary>
        /// Sets up the defaults dropdown menus.
        /// </summary>
        /// <param name="panel">Panel reference.</param>
        /// <param name="yPos">Relative Y position for buttons.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        private float SetUpMenus(UIPanel panel, float yPos)
        {
            // Starting y position.
            float currentY = yPos + Margin;

            for (int i = 0; i < SubServiceNames.Length; ++i)
            {

                // Row icon and label.
                PanelUtils.RowHeaderIcon(panel, ref currentY, SubServiceNames[i], IconNames[i], AtlasNames[i]);

                // Add any additional controls.
                currentY = SubServiceControls(currentY, i);

                // Next row.
                currentY += RowHeight + Margin;
            }

            // Return finishing Y position.
            return currentY;
        }
    }
}