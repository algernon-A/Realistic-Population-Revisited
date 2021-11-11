using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Options panel for setting default employment calculation packs.
    /// </summary>
    internal abstract class DefaultsPanelBase : CalculationsPanelBase
    {
        // Layout constants.
        private const float MenuWidth = 300f;
        protected const float RowAdditionX = LeftColumn + MenuWidth + (Margin * 2);


        // Tab icons.
        protected readonly string[] tabIconNames =
        {
            "SubBarMonumentModderPackFocused",
            "ToolbarIconZoomOutCity"
        };
        protected readonly string[] tabAtlasNames =
        {
            "ingame",
            "ingame"
        };


        // Tab icons.
        protected override string TabName => Translations.Translate(TitleKey);
        protected override string[] TabIconNames => tabIconNames;
        protected override string[] TabAtlasNames => tabAtlasNames;


        // Title key.
        protected abstract string TitleKey { get; }


        // Dropdown menus.
        protected UIDropDown[] PopMenus { get; private set; }
        protected UIDropDown[] FloorMenus { get; private set; }

        // Available packs arrays.
        protected PopDataPack[][] AvailablePopPacks { get; private set; }
        protected DataPack[] AvailableFloorPacks { get; private set; }


        /// <summary>
        /// Constructor - adds default options tab to tabstrip.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to</param>
        /// <param name="tabIndex">Index number of tab</param>
        internal DefaultsPanelBase(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }


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
                AvailablePopPacks[i] = PopData.instance.GetPacks(Services[i]);
                AvailableFloorPacks = FloorData.instance.Packs;

                // Get current and default packs for this item.
                DataPack currentPopPack = PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack currentFloorPack = FloorData.instance.CurrentDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Build preset menus.
                PopMenus[i].items = new string[AvailablePopPacks[i].Length];
                FloorMenus[i].items = new string[AvailableFloorPacks.Length];

                // Iterate through each item in pop menu.
                for (int j = 0; j < PopMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    PopMenus[i].items[j] = AvailablePopPacks[i][j].displayName;

                    // Check for default name match.
                    if (AvailablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - add default postscript.
                        PopMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (AvailablePopPacks[i][j].name.Equals(currentPopPack.name))
                    {
                        PopMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < FloorMenus[i].items.Length; ++j)
                {
                    // Set menu item text.
                    FloorMenus[i].items[j] = AvailableFloorPacks[j].displayName;

                    // Check for deefault name match.
                    if (AvailableFloorPacks[j].name.Equals(defaultFloorPack.name))
                    {
                        // Match - add default postscript.
                        FloorMenus[i].items[j] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (AvailableFloorPacks[j].name.Equals(currentFloorPack.name))
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
            if (!isSetup)
            {
                // Perform initial setup.
                isSetup = true;
                Logging.Message("setting up ", this.GetType());
                // Initialise arrays.
                AvailablePopPacks = new PopDataPack[SubServiceNames.Length][];
                AvailableFloorPacks = FloorData.instance.Packs;
                PopMenus = new UIDropDown[SubServiceNames.Length];
                FloorMenus = new UIDropDown[SubServiceNames.Length];

                // Add title.
                float currentY = PanelUtils.TitleLabel(panel, TitleKey);

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
        /// Sets up the defaults dropdown menus.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
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
                PanelUtils.RowHeaderIcon(panel, ref currentY, SubServiceNames[i], IconNames[i], AtlasNames[i]);

                // Pop pack dropdown.
                PopMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_DEN"), MenuWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false);

                // Save current index in object user data.
                PopMenus[i].objectUserData = i;

                // Event handler.
                PopMenus[i].eventSelectedIndexChanged += PopMenuChanged;

                // Floor pack on next row.
                currentY += RowHeight;

                // Floor pack dropdown.
                FloorMenus[i] = UIControls.AddLabelledDropDown(panel, LeftColumn, currentY, Translations.Translate("RPR_CAL_BFL"), MenuWidth, height: 20f, itemVertPadding: 6, accomodateLabel: false);

                // Add any additional controls.
                currentY = RowAdditions(currentY, i);


                // Next row.
                currentY += RowHeight + Margin;
            }

            // Return finishing Y position.
            return currentY;
        }


        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected virtual float PanelHeader(float yPos)
        {
            return yPos;
        }


        /// <summary>
        /// Adds any additional controls to each row.
        /// </summary>
        /// <param name="yPos">Relative Y position at top of row items</param>
        /// <param name="index">Index number of this row</param>
        /// <returns>Relative Y coordinate below the finished setup</returns>
        protected virtual float RowAdditions(float yPos, int index)
        {
            return yPos;
        }


        /// <summary>
        /// 'Revert to defaults' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetDefaults(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get current and default packs for this item.
                DataPack defaultPopPack = PopData.instance.BaseDefaultPack(Services[i], SubServices[i]);
                DataPack defaultFloorPack = FloorData.instance.BaseDefaultPack(Services[i], SubServices[i]);

                // Iterate through each item in pop menu.
                for (int j = 0; j < PopMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (AvailablePopPacks[i][j].name.Equals(defaultPopPack.name))
                    {
                        // Match - set selection to this one.
                        PopMenus[i].selectedIndex = j;
                    }
                }

                // Iterate through each item in floor menu.
                for (int j = 0; j < FloorMenus[i].items.Length; ++j)
                {
                    // Check for deefault name match.
                    if (AvailableFloorPacks[j].name.Equals(defaultFloorPack.name))
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
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected virtual void Apply(UIComponent control, UIMouseEventParameter mouseEvent)
        {
            // Iterate through each sub-service menu.
            for (int i = 0; i < SubServiceNames.Length; ++i)
            {
                // Get population pack menu selected index.
                int popIndex = PopMenus[i].selectedIndex;

                // Check to see if this is a change from the current default.
                if (!PopData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(AvailablePopPacks[i][popIndex].name))
                {
                    // Default has changed - update default population dictionary for this subservice.
                    PopData.instance.ChangeDefault(Services[i], SubServices[i], AvailablePopPacks[i][popIndex]);
                }

                // Update floor data pack if we're not using legacy calculations.
                if (AvailablePopPacks[i][popIndex].version != (int)DataVersion.legacy)
                {
                    // Check to see if this is a change from the current default.
                    if (!FloorData.instance.CurrentDefaultPack(Services[i], SubServices[i]).name.Equals(AvailableFloorPacks[FloorMenus[i].selectedIndex]))
                    {
                        // Default has changed - update default floor dictionary for this subservice.
                        FloorData.instance.ChangeDefault(Services[i], SubServices[i], AvailableFloorPacks[FloorMenus[i].selectedIndex]);
                    }
                }
            }

            // Save settings.
            ConfigUtils.SaveSettings();
        }


        /// <summary>
        /// 'Revert to saved' button event handler.
        /// </summary>
        /// <param name="control">Calling component (unused)</param>
        /// <param name="mouseEvent">Mouse event (unused)</param>
        protected override void ResetSaved(UIComponent control, UIMouseEventParameter mouseEvent) => UpdateControls();


        /// <summary>
        /// Population pack menu changed event handler.
        /// <param name="control">Calling component</param>
        /// <param name="index">New selected index</param>
        /// </summary>
        private void PopMenuChanged(UIComponent control, int index)
        {
            // Retrieve stored index.
            int serviceIndex = (int)control.objectUserData;

            // Hide floor menu if we've selected legacy calcs, otherwise show it.
            if (AvailablePopPacks[serviceIndex][index].version == (int)DataVersion.legacy)
            {
                FloorMenus[serviceIndex].Hide();
            }
            else
            {
                FloorMenus[serviceIndex].Show();
            }
        }
    }
}