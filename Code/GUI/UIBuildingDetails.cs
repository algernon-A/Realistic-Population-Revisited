// <copyright file="UIBuildingDetails.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AlgernonCommons;
    using AlgernonCommons.UI;
    using AlgernonCommons.Translation;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Buidling details panel manager static class.
    /// </summary>
    public static class BuildingDetailsPanel
    {
        // Instance references.
        private static GameObject s_uiGameObject;
        private static UIBuildingDetails s_panel;

        // Previous selection.
        private static BuildingInfo lastSelection;
        private static bool[] lastFilter;
        private static int lastPostion = 0;
        private static int lastIndex = -1;

        // Info panel buttons.
        private static UIButton zonedButton, serviceButton;

        /// <summary>
        /// Gets the current panel instance.
        /// </summary>
        public static UIBuildingDetails Panel => s_panel;

        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        internal static void Open(BuildingInfo selected = null)
        {
            try
            {
                // If no instance already set, create one.
                if (s_uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    s_uiGameObject = new GameObject("RealPopBuildingDetails");
                    s_uiGameObject.transform.parent = UIView.GetAView().transform;

                    s_panel = s_uiGameObject.AddComponent<UIBuildingDetails>();

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
            lastSelection = Panel?._currentSelection;
            lastFilter = Panel?.GetFilter();
            Panel?.GetListPosition(out lastIndex, out lastPostion);

            // Destroy objects and nullify for GC.
            GameObject.Destroy(s_panel);
            GameObject.Destroy(s_uiGameObject);
            s_panel = null;
            s_uiGameObject = null;
        }

        /// <summary>
        /// Adds button to access building details from building info panels.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            // Zoned building panel - get parent panel and add button.
            ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
            zonedButton = UIButtons.AddButton(infoPanel.component, infoPanel.component.width - 133f - 10, 120, Translations.Translate("RPR_REALPOP"), 133f, 19.5f, 0.65f);
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
            serviceButton = UIButtons.AddButton(servicePanel.component, servicePanel.component.width - 133f - 10, 120, Translations.Translate("RPR_REALPOP"), 133f, 19.5f, 0.65f);
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
        private UITitleBar _titleBar;
        private UIBuildingFilter _filterBar;
        private UIList _buildingSelection;
        private UIPreviewPanel _previewPanel;
        private UIEditPanel _editPanel;
        private UIModCalcs _calcsPanel;

        // Current selections.
        internal BuildingInfo _currentSelection;


        /// <summary>
        /// Refreshes the building selection list.
        /// </summary>
        internal void RefreshList() => _buildingSelection.Refresh();


        /// <summary>
        /// Gets the current filter state as a boolean array.
        /// </summary>
        /// <returns>Current filter toggle settings</returns>
        internal bool[] GetFilter() => _filterBar.GetFilter();


        /// <summary>
        /// Sets the filter state to match a boolean array.
        /// </summary>
        internal void SetFilter(bool[] filterState) => _filterBar.SetFilter(filterState);


        /// <summary>
        /// Gets the current index and list positions of the building selection list.
        /// </summary>
        /// <param name="selectedIndex">Index of currently selected item</param>
        /// <param name="listPosition">Current list position</param>
        internal void GetListPosition(out int selectedIndex, out int listPosition)
        {
            listPosition = _buildingSelection.CurrentPosition;
            selectedIndex = _buildingSelection.SelectedIndex;
        }


        /// <summary>
        /// Sets the current index and list positions of the building selection list.
        /// </summary>
        /// <param name="selectedIndex">Selected item index to set</param>
        /// <param name="listPosition">List position to set</param>
        internal void SetListPosition(int selectedIndex, int listPosition)
        {
            _buildingSelection.CurrentPosition = listPosition;
            _buildingSelection.SelectedIndex = selectedIndex;
        }


        /// <summary>
        /// Communicates floor calculation pack changes to previewer and edit panel.
        /// </summary>
        internal FloorDataPack FloorDataPack { set => _previewPanel.FloorPack = value; }


        /// <summary>
        /// Suppresses floor preview rendering (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors { set => _previewPanel.HideFloors = value; }


        /// <summary>
        /// Communicates floor calculation pack changes to previewer.
        /// </summary>
        internal FloorDataPack OverrideFloors
        {
            set
            {
                _previewPanel.OverrideFloors = value;
                _calcsPanel.OverrideFloors = value;
            }
        }

        /// <summary>
        /// Called when the building selection changes to update other panels.
        /// </summary>
        /// <param name="building">Newly selected building.</param>
        public void UpdateSelectedBuilding(BuildingInfo building)
        {
            if (building != null)
            {
                // Update building preview.
                _currentSelection = building;
                _previewPanel.Show(_currentSelection);
            }

            // Update mod calculations and edit panels.
            _calcsPanel.SelectionChanged(building);
            _editPanel.SelectionChanged(building);
        }

        /// <summary>
        /// Refreshes the building selection list.
        /// Used to update custom settings checkboxes.
        /// </summary>
        public void Refresh()
        {
            // Refresh the building list.
            _buildingSelection.Refresh();

            // Update mod calculations and edit panels.
            UpdateSelectedBuilding(_currentSelection);
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
                _titleBar = AddUIComponent<UITitleBar>();
                _titleBar.Setup();

                // Filter.
                _filterBar = AddUIComponent<UIBuildingFilter>();
                _filterBar.width = width - (Spacing * 2);
                _filterBar.height = FilterHeight;
                _filterBar.relativePosition = new Vector2(Spacing, TitleHeight);

                _filterBar.EventFilteringChanged += (c, i) =>
                {
                    if (i == -1) return;

                    int listCount = _buildingSelection.Data.m_size;
                    float position = _buildingSelection.CurrentPosition;

                    _buildingSelection.SelectedIndex = -1;

                    _buildingSelection.Data = GenerateFastList();
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

                _previewPanel = middlePanel.AddUIComponent<UIPreviewPanel>();
                _previewPanel.width = middlePanel.width;
                _previewPanel.height = (PanelHeight - Spacing) / 2;
                _previewPanel.relativePosition = Vector2.zero;
                _previewPanel.Setup();

                _editPanel = middlePanel.AddUIComponent<UIEditPanel>();
                _editPanel.width = middlePanel.width;
                _editPanel.height = (PanelHeight - Spacing) / 2;
                _editPanel.relativePosition = new Vector2(0, _previewPanel.height + Spacing);
                _editPanel.Setup();

                // Right panel - mod calculations.
                _calcsPanel = this.AddUIComponent<UIModCalcs>();
                _calcsPanel.width = RightWidth;
                _calcsPanel.height = PanelHeight;
                _calcsPanel.relativePosition = new Vector2(LeftWidth + MiddleWidth + (Spacing * 3), TitleHeight + FilterHeight + Spacing);
                _calcsPanel.Setup();

                // Building selection list.
                _buildingSelection = UIList.AddUIList<UIBuildingRow>(leftPanel);
                _buildingSelection.width = leftPanel.width;
                _buildingSelection.height = leftPanel.height;
                _buildingSelection.RowHeight = 30f;
                _buildingSelection.relativePosition = Vector2.zero;
                _buildingSelection.Data = new FastList<object>();
                _buildingSelection.SelectedIndex = -1;

                _buildingSelection.EventSelectionChanged += (c, item) => UpdateSelectedBuilding(item as BuildingInfo);

                // Set up filterBar to make sure selection filters are properly initialised before calling GenerateFastList.
                _filterBar.Setup();

                // Populate the list.
                _buildingSelection.Data = GenerateFastList();
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
        /// <param name="building">Selected building.</param>
        internal void SelectBuilding(BuildingInfo building)
        {
            // Ensure the fastlist is filtered to include this building.
            _filterBar.SelectBuildingCategory(building.m_class);
            _buildingSelection.Data = GenerateFastList();

            // Clear the name filter.
            _filterBar.nameFilter.text = String.Empty;

            // Find and select the building in the fastlist.
            _buildingSelection.FindItem(building);

            // Update the selected building to the current.
            UpdateSelectedBuilding(building);
        }

        /// <summary>
        /// Generates the list of buildings depending on current filter settings.
        /// </summary>
        /// <returns>List of buildings.</returns>
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
                if (subService == ItemClass.SubService.ResidentialLow && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.ResidentialLow].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.ResidentialHigh && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.ResidentialHigh].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialLow && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.CommercialLow].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialHigh && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.CommercialHigh].isChecked)
                {
                }
                else if (service == ItemClass.Service.Office && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Office].isChecked)
                {
                }
                else if (service == ItemClass.Service.Industrial && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Industrial].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialTourist && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Tourism].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialLeisure && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Leisure].isChecked)
                {
                }
                else if (subService == ItemClass.SubService.CommercialEco && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Organic].isChecked)
                {
                }
                else if ((subService == ItemClass.SubService.ResidentialLowEco || subService == ItemClass.SubService.ResidentialHighEco) && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Selfsufficient].isChecked)
                {
                }
                else if (service == ItemClass.Service.Education && _filterBar.categoryToggles[(int)UIBuildingFilter.BuildingCategories.Education].isChecked && item.GetClassLevel() < ItemClass.Level.Level3)
                {
                }
                else
                {
                    // If we've gotten here, then we've matched no categories; move on to next item.
                    continue;
                }

                // Filter by name.
                if (!_filterBar.nameFilter.text.Trim().IsNullOrWhiteSpace() && !GetDisplayName(item.name).ToLower().Contains(_filterBar.nameFilter.text.Trim().ToLower()))
                {
                    continue;
                }

                // Filter by settings.
                bool hasOverride = OverrideUtils.GetResidential(item) != 0 || OverrideUtils.GetWorker(item) != 0 || FloorData.instance.HasOverride(itemName) != null;
                bool hasNonDefault = PopData.instance.HasPackOverride(itemName) != null || FloorData.instance.HasPackOverride(itemName) != null || SchoolData.instance.HasPackOverride(itemName) != null || Multipliers.instance.HasOverride(itemName);

                if (_filterBar.SettingsFilter[(int)UIBuildingFilter.FilterCategories.HasOverride].isChecked && !hasOverride) continue;
                if (_filterBar.SettingsFilter[(int)UIBuildingFilter.FilterCategories.HasNonDefault].isChecked && !hasNonDefault) continue;
                if (_filterBar.SettingsFilter[(int)UIBuildingFilter.FilterCategories.Any].isChecked && !(hasOverride || hasNonDefault)) continue;

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
        /// <param name="fullName">Raw prefab name.</param>
        /// <returns>Cleaned-up display name.</returns>
        public static string GetDisplayName(string fullName)
        {
            // Filter out leading package number and trailing '_Data'.
            return fullName.Substring(fullName.IndexOf('.') + 1).Replace("_Data", "");
        }
    }
}