using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Buidling details panel manager static class.
    /// </summary>
    public static class BuildingDetailsPanel
    {
        // Instance references.
        private static GameObject uiGameObject;
        private static UIBuildingDetails _panel;
        public static UIBuildingDetails Panel => _panel;

        // Previous selection.
        private static BuildingInfo lastSelection;
        private static bool[] lastFilter;
        private static float lastPostion;
        private static int lastIndex = -1;

        // Info panel buttons.
        private static UIButton zonedButton, serviceButton;


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Open(BuildingInfo selected = null)
        {
            try
            {
                // If no instance already set, create one.
                if (uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    uiGameObject = new GameObject("RealPopBuildingDetails");
                    uiGameObject.transform.parent = UIView.GetAView().transform;

                    _panel = uiGameObject.AddComponent<UIBuildingDetails>();

                    // Set up panel.
                    Panel.Setup();
                }

                // Select appropriate building if there's a preselection.
                if (selected != null)
                {
                    Panel.SelectBuilding(selected);
                }
                else if (lastSelection != null)
                {
                    // Restore previous filter state.
                    if (lastFilter != null)
                    {
                        Panel.SetFilter(lastFilter);
                    }

                    // Restore previous building selection list postion and selected item (specifically in that order to ensure correct item is selected).
                    Panel.SetListPosition(lastIndex, lastPostion);
                    Panel.SelectBuilding(lastSelection);
                }


                Panel.Show();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception opening building panel");
                return;
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void Close()
        {
            // Save current selection for next time.
            lastSelection = Panel?.currentSelection;
            lastFilter = Panel?.GetFilter();
            Panel?.GetListPosition(out lastIndex, out lastPostion);

            // Destroy objects and nullify for GC.
            GameObject.Destroy(_panel);
            GameObject.Destroy(uiGameObject);
            _panel = null;
            uiGameObject = null;
        }


        /// <summary>
        /// Adds button to access building details from building info panels.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            // Zoned building panel - get parent panel and add button.
            ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
            zonedButton = UIControls.AddButton(infoPanel.component, infoPanel.component.width - 133f - 10, 120, Translations.Translate("RPR_REALPOP"), 133f, 19.5f, 0.65f);
            zonedButton.textPadding = new RectOffset(2, 2, 4, 0);

            // Just in case other mods are interfering.
            zonedButton.Enable();

            // Event handler.
            zonedButton.eventClick += (control, clickEvent) =>
            {
                // Select current building in the building details panel and show.
                Open(InstanceManager.GetPrefabInfo(WorldInfoPanel.GetCurrentInstanceID()) as BuildingInfo);
            };

            // Service building panel - get parent panel and add button.
            CityServiceWorldInfoPanel servicePanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name);
            serviceButton = UIControls.AddButton(servicePanel.component, servicePanel.component.width - 133f - 10, 120, Translations.Translate("RPR_REALPOP"), 133f, 19.5f, 0.65f);
            serviceButton.textPadding = new RectOffset(2, 2, 4, 0);

            // Event handler.
            serviceButton.eventClick += (control, clickEvent) =>
            {
                // Select current building in the building details panel and show.
                Open(InstanceManager.GetPrefabInfo(WorldInfoPanel.GetCurrentInstanceID()) as BuildingInfo);
            };
        }


        /// <summary>
        /// Updates the state of the service panel Realistic Population button - should only be visible and enabled when looking at elementary and high schools.
        /// </summary>
        internal static void UpdateServicePanelButton()
        {
            // Get current building instance.
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Ensure valid building before proceeding.
            if (building > 0)
            {
                // Check for eduction service and level 1 or 2.
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info;
                if (info.GetService() == ItemClass.Service.Education && info.GetClassLevel() < ItemClass.Level.Level3)
                {
                    // It's a school!  Enable and show the button, and return.
                    serviceButton.Enable();
                    serviceButton.Show();
                    return;
                }
            }

            // If we got here, it's not a valid school building; disable and hide the button.
            serviceButton.Disable();
            serviceButton.Hide();
        }
    }


    /// <summary>
    /// Base class of the building details screen.  Based (via AJ3D's Ploppable RICO) ultimately on SamsamTS's Building Themes panel; many thanks to him for his work.
    /// </summary>
    public class UIBuildingDetails : UIPanel
    {
        // Constants.
        private const float LeftWidth = 430f;
        internal const float MiddleWidth = 280f;
        internal const float RightWidth = 650f;
        private const float FilterHeight = 40f;
        private const float PanelHeight = 550f;
        internal const float BottomMargin = 10f;
        private const float Spacing = 5f;
        internal const float TitleHeight = 40f;
        private const float CheckFilterHeight = 30f;

        // Panel components.
        private UITitleBar titleBar;
        private UIBuildingFilter filterBar;
        private UIFastList buildingSelection;
        private UIPreviewPanel previewPanel;
        private UIEditPanel editPanel;
        private UIModCalcs calcsPanel;

        // Current selections.
        internal BuildingInfo currentSelection;


        /// <summary>
        /// Refreshes the building selection list.
        /// </summary>
        internal void RefreshList() => buildingSelection.Refresh();


        /// <summary>
        /// Gets the current filter state as a boolean array.
        /// </summary>
        /// <returns>Current filter toggle settings</returns>
        internal bool[] GetFilter() => filterBar.GetFilter();


        /// <summary>
        /// Sets the filter state to match a boolean array.
        /// </summary>
        internal void SetFilter(bool[] filterState) => filterBar.SetFilter(filterState);


        /// <summary>
        /// Gets the current index and list positions of the building selection list.
        /// </summary>
        /// <param name="selectedIndex">Index of currently selected item</param>
        /// <param name="listPosition">Current list position</param>
        internal void GetListPosition(out int selectedIndex, out float listPosition)
        {
            listPosition = buildingSelection.listPosition;
            selectedIndex = buildingSelection.selectedIndex;
        }


        /// <summary>
        /// Sets the current index and list positions of the building selection list.
        /// </summary>
        /// <param name="selectedIndex">Selected item index to set</param>
        /// <param name="listPosition">List position to set</param>
        internal void SetListPosition(int selectedIndex, float listPosition)
        {
            buildingSelection.listPosition = listPosition;
            buildingSelection.selectedIndex = selectedIndex;
        }


        /// <summary>
        /// Communicates floor calculation pack changes to previewer and edit panel.
        /// </summary>
        internal FloorDataPack FloorDataPack { set => previewPanel.FloorPack = value; }


        /// <summary>
        /// Suppresses floor preview rendering (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors { set => previewPanel.HideFloors = value; }


        /// <summary>
        /// Communicates floor calculation pack changes to previewer.
        /// </summary>
        internal FloorDataPack OverrideFloors
        {
            set
            {
                previewPanel.OverrideFloors = value;
                calcsPanel.OverrideFloors = value;
            }
        }


        /// <summary>
        /// Called when the building selection changes to update other panels.
        /// </summary>
        /// <param name="building">Newly selected building</param>
        public void UpdateSelectedBuilding(BuildingInfo building)
        {
            if (building != null)
            {
                // Update building preview.
                currentSelection = building;
                previewPanel.Show(currentSelection);
            }

            // Update mod calculations and edit panels.
            calcsPanel.SelectionChanged(building);
            editPanel.SelectionChanged(building);
        }


        /// <summary>
        /// Refreshes the building selection list.
        /// Used to update custom settings checkboxes.
        /// </summary>
        public void Refresh()
        {
            // Refresh the building list.
            buildingSelection.Refresh();

            // Update mod calculations and edit panels.
            UpdateSelectedBuilding(currentSelection);
        }


        /// <summary>
        /// Create the building editor panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        internal void Setup()
        {
            try
            {
                // Basic setup.
                isVisible = false;
                canFocus = true;
                isInteractive = true;
                width = LeftWidth + MiddleWidth + RightWidth + (Spacing * 4) + Spacing;
                height = PanelHeight + TitleHeight + FilterHeight + (Spacing * 2) + BottomMargin;
                relativePosition = new Vector2(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
                backgroundSprite = "UnlockingPanel2";

                // Titlebar.
                titleBar = AddUIComponent<UITitleBar>();
                titleBar.Setup();

                // Filter.
                filterBar = AddUIComponent<UIBuildingFilter>();
                filterBar.width = width - (Spacing * 2);
                filterBar.height = FilterHeight;
                filterBar.relativePosition = new Vector2(Spacing, TitleHeight);

                filterBar.EventFilteringChanged += (c, i) =>
                {
                    if (i == -1) return;

                    int listCount = buildingSelection.rowsData.m_size;
                    float position = buildingSelection.listPosition;

                    buildingSelection.selectedIndex = -1;

                    buildingSelection.rowsData = GenerateFastList();
                };

                // Set up panels.
                // Left panel - list of buildings.
                UIPanel leftPanel = AddUIComponent<UIPanel>();
                leftPanel.width = LeftWidth;
                leftPanel.height = PanelHeight - CheckFilterHeight;
                leftPanel.relativePosition = new Vector2(Spacing, TitleHeight + FilterHeight + CheckFilterHeight + Spacing);

                // Middle panel - building preview and edit panels.
                UIPanel middlePanel = AddUIComponent<UIPanel>();
                middlePanel.width = MiddleWidth;
                middlePanel.height = PanelHeight;
                middlePanel.relativePosition = new Vector2(LeftWidth + (Spacing * 2), TitleHeight + FilterHeight + Spacing);

                previewPanel = middlePanel.AddUIComponent<UIPreviewPanel>();
                previewPanel.width = middlePanel.width;
                previewPanel.height = (PanelHeight - Spacing) / 2;
                previewPanel.relativePosition = Vector2.zero;
                previewPanel.Setup();

                editPanel = middlePanel.AddUIComponent<UIEditPanel>();
                editPanel.width = middlePanel.width;
                editPanel.height = (PanelHeight - Spacing) / 2;
                editPanel.relativePosition = new Vector2(0, previewPanel.height + Spacing);
                editPanel.Setup();

                // Right panel - mod calculations.
                calcsPanel = this.AddUIComponent<UIModCalcs>();
                calcsPanel.width = RightWidth;
                calcsPanel.height = PanelHeight;
                calcsPanel.relativePosition = new Vector2(LeftWidth + MiddleWidth + (Spacing * 3), TitleHeight + FilterHeight + Spacing);
                calcsPanel.Setup();

                // Building selection list.
                buildingSelection = UIFastList.Create<UIBuildingRow>(leftPanel);
                buildingSelection.backgroundSprite = "UnlockingPanel";
                buildingSelection.width = leftPanel.width;
                buildingSelection.height = leftPanel.height;
                buildingSelection.canSelect = true;
                buildingSelection.rowHeight = 30f;
                buildingSelection.autoHideScrollbar = true;
                buildingSelection.relativePosition = Vector2.zero;
                buildingSelection.rowsData = new FastList<object>();
                buildingSelection.selectedIndex = -1;

                // Set up filterBar to make sure selection filters are properly initialised before calling GenerateFastList.
                filterBar.Setup();

                // Populate the list.
                buildingSelection.rowsData = GenerateFastList();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception setting up building panel");
            }
        }


        /// <summary>
        /// Called to select a building from 'outside' the building details editor (e.g. by button on building info panel).
        /// Sets the filter to only display the relevant category for the relevant building, and makes that building selected in the list.
        /// </summary>
        /// <param name="building"></param>
        internal void SelectBuilding(BuildingInfo building)
        {
            // Ensure the fastlist is filtered to include this building.
            filterBar.SelectBuildingCategory(building.m_class);
            buildingSelection.rowsData = GenerateFastList();

            // Clear the name filter.
            filterBar.nameFilter.text = String.Empty;

            // Find and select the building in the fastlist.
            buildingSelection.FindBuilding(building.name);

            // Update the selected building to the current.
            UpdateSelectedBuilding(building);
        }


        /// <summary>
        /// Generates the list of buildings depending on current filter settings.
        /// </summary>
        /// <returns>List of buildings</returns>
        private FastList<object> GenerateFastList()
        {
            // List to store all building prefabs that pass the filter.
            List<BuildingInfo> filteredList = new List<BuildingInfo>();

            // Iterate through all loaded building prefabs and add them to the list if they meet the filter conditions.
            for (uint i = 0; i < PrefabCollection<BuildingInfo>.LoadedCount(); i++)
            {
                BuildingInfo item = PrefabCollection<BuildingInfo>.GetLoaded(i);
                string itemName = item?.name;

                // Skip any null or invalid prefabs.
                if (itemName == null)
                {
                    continue;
                }

                // Apply zone type filters.
                ItemClass.Service service = item.GetService();
                ItemClass.SubService subService = item.GetSubService();

                // Laid out this way for clear visibility.
                if (subService == ItemClass.SubService.ResidentialLow && filterBar.categoryToggles[(int)BuildingCategories.ResidentialLow].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.ResidentialHigh && filterBar.categoryToggles[(int)BuildingCategories.ResidentialHigh].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialLow && filterBar.categoryToggles[(int)BuildingCategories.CommercialLow].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialHigh && filterBar.categoryToggles[(int)BuildingCategories.CommercialHigh].isChecked)
                {
                }
                else if (service == ItemClass.Service.Office && filterBar.categoryToggles[(int)BuildingCategories.Office].isChecked)
                {
                }
                else if (service == ItemClass.Service.Industrial && filterBar.categoryToggles[(int)BuildingCategories.Industrial].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialTourist && filterBar.categoryToggles[(int)BuildingCategories.Tourism].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialLeisure && filterBar.categoryToggles[(int)BuildingCategories.Leisure].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialEco && filterBar.categoryToggles[(int)BuildingCategories.Organic].isChecked)
                {
                }
                else if ((subService == ItemClass.SubService.ResidentialLowEco || subService == ItemClass.SubService.ResidentialHighEco) && filterBar.categoryToggles[(int)BuildingCategories.Selfsufficient].isChecked)
                {
                }
                else if (service == ItemClass.Service.Education && filterBar.categoryToggles[(int)BuildingCategories.Education].isChecked && item.GetClassLevel() < ItemClass.Level.Level3)
                {
                }
                else
                {
                    // If we've gotten here, then we've matched no categories; move on to next item.
                    continue;
                }

                // Filter by name.
                if (!filterBar.nameFilter.text.Trim().IsNullOrWhiteSpace() && !GetDisplayName(item.name).ToLower().Contains(filterBar.nameFilter.text.Trim().ToLower()))
                {
                    continue;
                }

                // Filter by settings.
                bool hasOverride = OverrideUtils.GetResidential(item) != 0 || OverrideUtils.GetWorker(item) != 0 || FloorData.instance.HasOverride(itemName) != null;
                bool hasNonDefault = PopData.instance.HasPackOverride(itemName) != null || FloorData.instance.HasPackOverride(itemName) != null || SchoolData.instance.HasPackOverride(itemName) != null || Multipliers.instance.HasOverride(itemName);

                if (filterBar.SettingsFilter[(int)FilterCategories.HasOverride].isChecked && !hasOverride) continue;
                if (filterBar.SettingsFilter[(int)FilterCategories.HasNonDefault].isChecked && !hasNonDefault) continue;
                if (filterBar.SettingsFilter[(int)FilterCategories.Any].isChecked && !(hasOverride || hasNonDefault)) continue;

                // Finally!  We've got an item that's passed all filters; add it to the list.
                filteredList.Add(item);
            }

            // Create return list with our filtered list, sorted alphabetically.
            FastList<object> fastList = new FastList<object>
            {
                m_buffer = filteredList.OrderBy(x => UIBuildingDetails.GetDisplayName(x.name)).ToArray(),
                m_size = filteredList.Count
            };
            return fastList;
        }


        /// <summary>
        /// Returns the name of the building prefab cleaned up for display.
        /// </summary>
        /// <param name="fullName">Raw prefab name</param>
        /// <returns></returns>
        public static string GetDisplayName(string fullName)
        {
            // Filter out leading package number and trailing '_Data'.
            return fullName.Substring(fullName.IndexOf('.') + 1).Replace("_Data", "");
        }
    }
}