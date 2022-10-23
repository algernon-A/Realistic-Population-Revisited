// <copyright file="DefaultsPanelBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class DefaultsPanelBase : CalculationsPanelBase
    {
        // Tab icons.
        private readonly string[] _tabIconNames =
        {
            "SubBarMonumentModderPackFocused",
            "ToolbarIconZoomOutCity",
        };

        private readonly string[] _tabAtlasNames =
        {
            "ingame",
            "ingame",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultsPanelBase"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal DefaultsPanelBase(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets the tab name.
        /// </summary>
        protected override string TabName => Translations.Translate(TitleKey);

        /// <summary>
        /// Gets the array of icon sprite names for this tab.
        /// </summary>
        protected override string[] TabIconNames => _tabIconNames;

        /// <summary>
        /// Gets the array of icon atlas names for this tab.
        /// </summary>
        protected override string[] TabAtlasNames => _tabAtlasNames;

        /// <summary>
        /// Gets the tab title translation key for this tab.
        /// </summary>
        protected abstract string TitleKey { get; }

        /// <summary>
        /// Gets the array of population pack selection menus for this tab.
        /// </summary>
        protected UIDropDown[] PopMenus { get; private set; }

        /// <summary>
        /// Gets the array of floor pack selection menus for this tab.
        /// </summary>
        protected UIDropDown[] FloorMenus { get; private set; }

        /// <summary>
        /// Gets the array of available population packs for this tab.
        /// </summary>
        protected PopDataPack[][] AvailablePopPacks { get; private set; }

        /// <summary>
        /// Gets the array of available floor packs for this tab.
        /// </summary>
        protected DataPack[] AvailableFloorPacks { get; private set; }

        /// <summary>
        /// Updates control values.
        /// </summary>
        internal void UpdateControls()
        {
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Save current index in object user data.
                PopMenus[i].objectUserData = i;
                FloorMenus[i].objectUserData = i;

                // Get available packs for this service/subservice combination.
                AvailablePopPacks[i] = PopData.Instance.GetPacks(Services[i]);
                AvailableFloorPacks = FloorData.Instance.Packs;

                // Get current and default packs for this item.
                DataPack currentPopPack = PopData.Instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultPopPack = PopData.Instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack currentFloorPack = FloorData.Instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.Instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Build preset menus.
                PopMenus[i].items = new string[AvailablePopPacks[i].Length];
                FloorMenus[i].items = new string[AvailableFloorPacks.Length];

                // Iterate through each item in pop menu.
                for (int j = 0; j < PopMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    PopMenus[i].items[j] = AvailablePopPacks[i][j].DisplayName;

                    // Check for default name match.
                    if (AvailablePopPacks[i][j].Name.Equals(defaultPopPack.Name))
                    {
                        // Match - add default postscript.
                        PopMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (AvailablePopPacks[i][j].Name.Equals(currentPopPack.Name))
                    {
                        PopMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < FloorMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    FloorMenus[i].items[j] = AvailableFloorPacks[j].DisplayName;

                    // Check for deefault name match.
                    if (AvailableFloorPacks[j].Name.Equals(defaultFloorPack.Name))
                    {
                        // Match - add default postscript.
                        FloorMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (AvailableFloorPacks[j].Name.Equals(currentFloorPack.Name))
                    {
                        FloorMenus[i].selectedIndex = j;
                    }
                }
            }
        }

        /// <summary>
        /// Performs initial setup; called via event when tab is first selected.
        /// </summary>
        internal override void Setup()
        {
            // Don't do anything if already set up.
            if (!m_isSetup)
            {
                // Perform initial setup.
                m_isSetup = true;
                Logging.Message("setting up ", this.GetType());

                // Initialise arrays.
                AvailablePopPacks = new PopDataPack[SubServiceNames.Length][];
                AvailableFloorPacks = FloorData.Instance.Packs;
                PopMenus = new UIDropDown[SubServiceNames.Length];
                FloorMenus = new UIDropDown[SubServiceNames.Length];

                // Add title.
                float currentY = PanelUtils.TitleLabel(m_panel, TitleKey);

                // Add header controls.
                currentY = PanelHeader(currentY);

                // Add menus.
                currentY = SetUpMenus(currentY);

                // Add buttons- add extra space.
                FooterButtons(currentY + Margin);

                // Set control values.
                UpdateControls();
            }
        }

        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected virtual float PanelHeader(float yPos)
        {
            return yPos;
        }

        /// <summary>
        /// Adds any additional controls to each row.
        /// </summary>
        /// <param name="yPos">Relative Y position at top of row items.</param>
        /// <param name="index">Index number of this row.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected virtual float RowAdditions(float yPos, int index)
        {
            return yPos;
        }

        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused).</param>
        /// <param name="p">Mouse event (unused).</param>
        protected override void ResetDefaults(UIComponent c, UIMouseEventParameter p)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get current and default packs for this item.
                DataPack defaultPopPack = PopData.Instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.Instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Iterate through each item in pop menu.
                for (int j = 0; j < PopMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (AvailablePopPacks[i][j].Name.Equals(defaultPopPack.Name))
                    {
                        // Match - set selection to this one.
                        PopMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < FloorMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (AvailableFloorPacks[j].Name.Equals(defaultFloorPack.Name))
                    {
                        // Match - set selection to this one.
                        FloorMenus[i].selectedIndex = j;
                    }
                }
            }
        }

        /// <summary>
        /// 'Save and apply' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused).</param>
        /// <param name="p">Mouse event (unused).</param>
        protected virtual void Apply(UIComponent c, UIMouseEventParameter p)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get population pack menu selected index.
                int popIndex = PopMenus[i].selectedIndex;

                // Check to see if this is a change from the current default.
                if (!PopData.Instance.CurrentDefaultPack(Services[i], SubServices[i]).Name.Equals(AvailablePopPacks[i][popIndex].Name))
                {
                    // Default has changed - update default population dictionary for this subservice.
                    PopData.Instance.ChangeDefault(Services[i], SubServices[i], AvailablePopPacks[i][popIndex]);
                }

                // Update floor data pack if we're not using legacy or vanilla calculations.
                DataPack.DataVersion version = AvailablePopPacks[i][popIndex].Version;
                if (version != DataPack.DataVersion.Legacy && version != DataPack.DataVersion.Vanilla)
                {
                    // Check to see if this is a change from the current default.
                    if (!FloorData.Instance.CurrentDefaultPack(Services[i], SubServices[i]).Name.Equals(AvailableFloorPacks[FloorMenus[i].selectedIndex]))
                    {
                        // Default has changed - update default floor dictionary for this subservice.
                        FloorData.Instance.ChangeDefault(Services[i], SubServices[i], AvailableFloorPacks[FloorMenus[i].selectedIndex]);
                    }
                }
            }

            // Save settings.
            ConfigurationUtils.SaveSettings();
        }

        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused).</param>
        /// <param name="p">Mouse event (unused).</param>
        protected override void ResetSaved(UIComponent c, UIMouseEventParameter p) => UpdateControls();

        /// <summary>
        /// Population pack menu changed event handler.
        /// <param name="c">Calling component.</param>
        /// <param name="index">New selected index.</param>
        /// </summary>
        private void PopMenuChanged(UIComponent c, int index)
        {
            // Retrieve stored index.
            int serviceIndex = (int)c.objectUserData;

            // Hide floor menu if we've selected legacy or vanilla calcs, otherwise show it.
            DataPack.DataVersion version = AvailablePopPacks[serviceIndex][index].Version;
            if (version == DataPack.DataVersion.Legacy || version == DataPack.DataVersion.Vanilla)
            {
                FloorMenus[serviceIndex].Hide();
            }
            else
            {
                FloorMenus[serviceIndex].Show();
            }
        }

        /// <summary>
        /// Sets up the defaults dropdown menus.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        private float SetUpMenus(float yPos)
        {
            // Layout constants.
            const float LeftColumn = 200f;
            const float MenuWidth = 300f;

            // Starting y position.
            float currentY = yPos + Margin;

            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Row icon and label.
                PanelUtils.RowHeaderIcon(m_panel, ref currentY, SubServiceNames[i], IconNames[i], AtlasNames[i]);

                // Pop pack dropdown.
                PopMenus[i] = UIDropDowns.AddLabelledDropDown(m_panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_DEN"), MenuWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false);

                // Save current index in object user data.
                PopMenus[i].objectUserData = i;

                // Event handler.
                PopMenus[i].eventSelectedIndexChanged += PopMenuChanged;

                // Floor pack on next row.
                currentY += RowHeight;

                // Floor pack dropdown.
                FloorMenus[i] = UIDropDowns.AddLabelledDropDown(m_panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_BFL"), MenuWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false);

                // Add any additional controls.
                currentY = RowAdditions(currentY, i);

                // Next row.
                currentY += RowHeight;
            }

            // Return finishing Y position.
            return currentY;
        }
    }
}