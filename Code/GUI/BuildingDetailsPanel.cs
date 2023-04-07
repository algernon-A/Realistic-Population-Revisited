// <copyright file="BuildingDetailsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using System.Linq;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Base class of the building details screen.  Based (via AJ3D's Ploppable RICO) ultimately on SamsamTS's Building Themes panel; many thanks to him for his work.
    /// </summary>
    public class BuildingDetailsPanel : StandalonePanel
    {
        /// <summary>
        /// Title height.
        /// </summary>
        internal const float TitleHeight = 40f;

        /// <summary>
        /// Middle layout column width.
        /// </summary>
        internal const float MiddleWidth = 280f;

        /// <summary>
        /// Rght layout column width.
        /// </summary>
        internal const float RightWidth = 650f;

        /// <summary>
        /// Panel layout bottom margin.
        /// </summary>
        internal const float BottomMargin = 10f;

        /// <summary>
        /// Panel width.
        /// </summary>
        internal const float CalculatedWidth = Margin + FilterWidth + Margin;

        /// <summary>
        /// Filter bar width.
        /// </summary>
        internal const float FilterWidth = LeftWidth + Margin + MiddleWidth + Margin + RightWidth;

        /// <summary>
        /// Middle sub-panel height.
        /// </summary>
        internal const float MiddlePanelHeight = InternalPanelHeight / 2;

        /// <summary>
        /// Filter bar height.
        /// </summary>
        internal const float FilterHeight = 40f;

        /// <summary>
        /// Component panel height.
        /// </summary>
        internal const float InternalPanelHeight = ListHeight + CheckFilterHeight;

        // Layout constants - private.
        private const float LeftWidth = 430f;
        private const float InternalPanelY = TitleHeight + FilterHeight + Margin;
        private const float MiddlePanelX = Margin + LeftWidth + Margin;
        private const float CheckFilterHeight = 30f;
        private const float ListHeight = BuildingRow.CustomRowHeight * 18f;

        // Panel components.
        private BuildingPanelFilter _filterBar;
        private UIList _buildingSelection;
        private BuildingPreviewPanel _previewPanel;
        private BuildingEditPanel _editPanel;
        private BuildingCalculationsPanel _calcsPanel;

        // Current selections.
        private BuildingInfo _currentSelection;

        /// <summary>
        /// Gets the panel width.
        /// </summary>
        public override float PanelWidth => CalculatedWidth;

        /// <summary>
        /// Gets the panel height.
        /// </summary>
        public override float PanelHeight => InternalPanelY + InternalPanelHeight + BottomMargin;

        /// <summary>
        /// Gets the current building selection.
        /// </summary>
        internal BuildingInfo CurrentSelection => _currentSelection;

        /// <summary>
        /// Sets the floor data pack for previewing.
        /// </summary>
        internal FloorDataPack FloorPack { set => _previewPanel.FloorPack = value; }

        /// <summary>
        /// Sets a value indicating whether floor floor preview rendering should be suppressed regardless of user setting (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors { set => _previewPanel.HideFloors = value; }

        /// <summary>
        /// Sets a manual floor override for previewing.
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
        /// Gets the panel's title.
        /// </summary>
        protected override string PanelTitle => Mod.Instance.BaseName;

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Decorative icon (top-left).
            SetIcon(UITextures.InGameAtlas, "ToolbarIconZoomOutCity");

            // Filter.
            _filterBar = AddUIComponent<BuildingPanelFilter>();
            _filterBar.relativePosition = new Vector2(Margin, TitleHeight);

            // Building preview.
            _previewPanel = AddUIComponent<BuildingPreviewPanel>();
            _previewPanel.relativePosition = new Vector2(MiddlePanelX, InternalPanelY);

            _editPanel = AddUIComponent<BuildingEditPanel>();
            _editPanel.relativePosition = new Vector2(MiddlePanelX, InternalPanelY + MiddlePanelHeight);

            // Right panel - mod calculations.
            _calcsPanel = AddUIComponent<BuildingCalculationsPanel>();
            _calcsPanel.relativePosition = new Vector2(LeftWidth + MiddleWidth + (Margin * 3), TitleHeight + FilterHeight + Margin);

            // Building selection list.
            _buildingSelection = UIList.AddUIList<BuildingRow>(this, Margin, TitleHeight + FilterHeight + CheckFilterHeight + Margin, LeftWidth, InternalPanelHeight - CheckFilterHeight, BuildingRow.CustomRowHeight);
            _buildingSelection.EventSelectionChanged += (c, item) => UpdateSelectedBuilding(item as BuildingInfo);

            _filterBar.EventFilteringChanged += (c, i) =>
            {
                if (i == -1)
                {
                    return;
                }

                int listCount = _buildingSelection.Data.m_size;
                float position = _buildingSelection.CurrentPosition;

                _buildingSelection.SelectedIndex = -1;

                _buildingSelection.Data = GenerateFastList();
            };

            // Populate the list.
            _buildingSelection.Data = GenerateFastList();
        }

        /// <summary>
        /// Returns the name of the building prefab cleaned up for display.
        /// </summary>
        /// <param name="fullName">Raw prefab name.</param>
        /// <returns>Cleaned-up display name.</returns>
        internal static string GetDisplayName(string fullName)
        {
            // Filter out leading package number and trailing '_Data'.
            return fullName.Substring(fullName.IndexOf('.') + 1).Replace("_Data", string.Empty);
        }

        /// <summary>
        /// Refreshes the building selection list.
        /// </summary>
        internal void RefreshList() => _buildingSelection.Refresh();

        /// <summary>
        /// Gets the current filter state as a boolean array.
        /// </summary>
        /// <returns>Current filter toggle settings.</returns>
        internal bool[] GetFilter() => _filterBar.GetFilter();

        /// <summary>
        /// Sets the filter state to match a boolean array.
        /// </summary>
        /// <param name="filterState">Boolean arry of filter states.</param>
        internal void SetFilter(bool[] filterState) => _filterBar.SetFilter(filterState);

        /// <summary>
        /// Gets the current index and list positions of the building selection list.
        /// </summary>
        /// <param name="selectedIndex">Index of currently selected item.</param>
        /// <param name="listPosition">Current list position.</param>
        internal void GetListPosition(out int selectedIndex, out int listPosition)
        {
            listPosition = _buildingSelection.CurrentPosition;
            selectedIndex = _buildingSelection.SelectedIndex;
        }

        /// <summary>
        /// Sets the current index and list positions of the building selection list.
        /// </summary>
        /// <param name="selectedIndex">Selected item index to set.</param>
        /// <param name="listPosition">List position to set.</param>
        internal void SetListPosition(int selectedIndex, int listPosition)
        {
            _buildingSelection.CurrentPosition = listPosition;
            _buildingSelection.SelectedIndex = selectedIndex;
        }

        /// <summary>
        /// Called when the building selection changes to update other panels.
        /// </summary>
        /// <param name="building">Newly selected building.</param>
        internal void UpdateSelectedBuilding(BuildingInfo building)
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
        internal void Refresh()
        {
            // Refresh the building list.
            _buildingSelection.Refresh();

            // Update mod calculations and edit panels.
            UpdateSelectedBuilding(_currentSelection);
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
            _filterBar.NameFilterText = string.Empty;

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

                switch (service)
                {
                    case ItemClass.Service.Residential:
                        switch (subService)
                        {
                            case ItemClass.SubService.ResidentialLow:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.ResidentialLow].isChecked)
                                {
                                    continue;
                                }

                                break;

                            default:
                            case ItemClass.SubService.ResidentialHigh:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.ResidentialHigh].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.ResidentialWallToWall:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.ResidentialWallToWall].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.ResidentialLowEco:
                            case ItemClass.SubService.ResidentialHighEco:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.Selfsufficient].isChecked)
                                {
                                    continue;
                                }

                                break;
                        }

                        break;

                    case ItemClass.Service.Commercial:
                        switch (subService)
                        {
                            case ItemClass.SubService.CommercialLow:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.CommercialLow].isChecked)
                                {
                                    continue;
                                }

                                break;

                            default:
                            case ItemClass.SubService.CommercialHigh:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.CommercialHigh].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.CommercialWallToWall:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.CommercialWallToWall].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.CommercialTourist:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.Tourism].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.CommercialLeisure:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.Leisure].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.CommercialEco:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.Organic].isChecked)
                                {
                                    continue;
                                }

                                break;
                        }

                        break;

                    case ItemClass.Service.Office:
                        switch (subService)
                        {
                            default:
                            case ItemClass.SubService.OfficeGeneric:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.OfficeGeneric].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.OfficeWallToWall:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.OfficeWallToWall].isChecked)
                                {
                                    continue;
                                }

                                break;

                            case ItemClass.SubService.OfficeHightech:
                                if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.OfficeHighTech].isChecked)
                                {
                                    continue;
                                }

                                break;
                        }

                        break;

                    case ItemClass.Service.Industrial:
                        if (!_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.Industrial].isChecked)
                        {
                            continue;
                        }

                        break;

                    case ItemClass.Service.Education:
                        if (!(_filterBar.CategoryToggles[(int)BuildingPanelFilter.BuildingCategories.Education].isChecked && item.GetClassLevel() < ItemClass.Level.Level3))
                        {
                            continue;
                        }

                        break;

                    default:
                        // No service match - move on to next item.
                        continue;
                }

                // Filter by name.
                if (!_filterBar.NameFilterText.Trim().IsNullOrWhiteSpace() && !GetDisplayName(item.name).ToLower().Contains(_filterBar.NameFilterText.Trim().ToLower()))
                {
                    continue;
                }

                // Filter by settings.
                bool hasOverride = OverrideUtils.GetResidential(item) != 0 || OverrideUtils.GetWorker(item) != 0 || FloorData.Instance.HasOverride(itemName) != null;
                bool hasNonDefault = PopData.Instance.HasPackOverride(itemName) != null || FloorData.Instance.HasPackOverride(itemName) != null || SchoolData.Instance.HasPackOverride(itemName) != null || Multipliers.Instance.HasOverride(itemName);

                if (_filterBar.SettingsFilter[(int)BuildingPanelFilter.FilterCategories.HasOverride].isChecked && !hasOverride)
                {
                    continue;
                }

                if (_filterBar.SettingsFilter[(int)BuildingPanelFilter.FilterCategories.HasNonDefault].isChecked && !hasNonDefault)
                {
                    continue;
                }

                if (_filterBar.SettingsFilter[(int)BuildingPanelFilter.FilterCategories.Any].isChecked && !(hasOverride || hasNonDefault))
                {
                    continue;
                }

                // Finally!  We've got an item that's passed all filters; add it to the list.
                filteredList.Add(item);
            }

            // Create return list with our filtered list, sorted alphabetically.
            FastList<object> fastList = new FastList<object>
            {
                m_buffer = filteredList.OrderBy(x => GetDisplayName(x.name)).ToArray(),
                m_size = filteredList.Count,
            };
            return fastList;
        }
    }
}