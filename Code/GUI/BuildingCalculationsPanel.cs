// <copyright file="BuildingCalculationsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// Showd mod calculations to the user.
    /// </summary>
    public class BuildingCalculationsPanel : UIPanel
    {
        // Layout constants - private.
        private const float Margin = 5f;
        private const float ColumnWidth = BuildingDetailsPanel.RightWidth / 2f;
        private const float ComponentWidth = ColumnWidth - (Margin * 2f);
        private const float RightColumnX = ColumnWidth + Margin;
        private const float LabelHeight = 20f;
        private const float MenuHeight = 30f;
        private const float DescriptionHeight = 40f;
        private const float ColumnLabelY = 30f;
        private const float MenuY = ColumnLabelY + LabelHeight;
        private const float DescriptionY = MenuY + MenuHeight;
        private const float BaseSaveY = DescriptionY + DescriptionHeight;
        private const float BaseCalcY = BaseSaveY + 35f;
        private const float SchoolSaveY = BaseSaveY + LabelHeight + MenuHeight + DescriptionHeight;
        private const float SchoolCalcY = SchoolSaveY + 35f;
        private const float ButtonWidth = 200f;
        private const float ApplyX = ColumnWidth - (ButtonWidth / 2);
        private const float Row2LabelY = DescriptionY + DescriptionHeight;
        private const float BaseCalcPanelHeight = BuildingDetailsPanel.InternalPanelHeight - BaseCalcY;
        private const float SchoolCalcPanelHeight = BuildingDetailsPanel.InternalPanelHeight - SchoolCalcY;

        // Panel components.
        private UILabel _title;
        private UIPanel _floorPanel;
        private UIPanel _schoolPanel;
        private LegacyCalculationPreview _legacyPanel;
        private VanillaCalculationPreview _vanillaPanel;
        private VolumetricCalculationPreview _volumetricPanel;
        private UIDropDown _popMenu;
        private UIDropDown _floorMenu;
        private UIDropDown _schoolMenu;
        private UICheckBox _multCheck;
        private UISlider _multSlider;
        private UILabel _multDefaultLabel;
        private UILabel _popDescription;
        private UILabel _floorDescription;
        private UILabel _schoolDescription;
        private UILabel _floorOverrideLabel;
        private UIButton _applyButton;

        // Data arrays.
        private PopDataPack[] _popPacks;
        private DataPack[] _floorPacks;
        private SchoolDataPack[] _schoolPacks;

        // Current selections.
        private BuildingInfo _currentBuilding;
        private PopDataPack _currentPopPack;
        private FloorDataPack _currentFloorPack;
        private FloorDataPack _currentFloorOverride;
        private SchoolDataPack _currentSchoolPack;

        // Flags.
        private bool usingLegacyOrVanilla;

        // Pop multiplier.
        private float currentMult;

        /// <summary>
        /// Sets the a floor data manual override for previewing.
        /// </summary>
        internal FloorDataPack OverrideFloors
        {
            set
            {
                // Store override.
                _currentFloorOverride = value;

                // Don't do anything else if we're using legacy or vanilla calculations.
                if (usingLegacyOrVanilla)
                {
                    return;
                }

                // Floor data pack to display.
                FloorDataPack displayPack;

                // If value is null (no override), show floor panel and display current floor pack data; otherwise, hide the floor panel and show the provided override data.
                if (value == null)
                {
                    displayPack = _currentFloorPack;
                    _floorOverrideLabel.Hide();
                    _floorPanel.Show();
                }
                else
                {
                    // Valid override - hide floor panel.
                    _floorPanel.Hide();

                    // Set override text label and show it.
                    _floorOverrideLabel.text = Translations.Translate("RPR_CAL_FOV");
                    _floorOverrideLabel.Show();

                    // Display figures for override, not current floor pack.
                    displayPack = value;
                }

                // Update panel with new calculations.
                _volumetricPanel.UpdateFloorText(displayPack);
                _volumetricPanel.CalculateVolumetric(_currentBuilding, CurrentLevelData, displayPack, _currentSchoolPack, currentMult);
            }
        }

        /// <summary>
        /// Gets the level data record from the current floor pack that's relevant to the selected building's level.
        /// </summary>
        private VolumetricPopPack.LevelData CurrentLevelData
        {
            get
            {
                // Bounds check on provided level, to handle misconfigured prefabs.
                VolumetricPopPack.LevelData[] levels = ((VolumetricPopPack)_currentPopPack).Levels;
                int level = (int)_currentBuilding.GetClassLevel();
                int maxLevel = levels.Length - 1;
                if (level > maxLevel)
                {
                    Logging.Error("building ", _currentBuilding.name, " has ClassLevel ", level, " but maximum configured level count is ", maxLevel);
                    level = maxLevel;
                }

                return levels[level];
            }
        }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Basic setup.
            clipChildren = true;
            width = BuildingDetailsPanel.RightWidth;
            height = BuildingDetailsPanel.InternalPanelHeight;

            // Title.
            _title = this.AddUIComponent<UILabel>();
            _title.relativePosition = Vector2.zero;
            _title.textAlignment = UIHorizontalAlignment.Center;
            _title.text = Translations.Translate("RPR_CAL_MOD");
            _title.textScale = 1.2f;
            _title.autoSize = false;
            _title.width = BuildingDetailsPanel.RightWidth;

            // Column titles.
            UILabel densityTitle = ColumnLabel(this, Translations.Translate("RPR_CAL_DEN"), Margin, ColumnLabelY);
            UILabel floorTitle = ColumnLabel(this, Translations.Translate("RPR_CAL_BFL"), RightColumnX, ColumnLabelY);

            // Volumetric calculations panel.
            _volumetricPanel = this.AddUIComponent<VolumetricCalculationPreview>();
            _volumetricPanel.height = BaseCalcPanelHeight;
            _volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);

            // Legacy calculations panel - copy volumetric calculations panel.
            _legacyPanel = this.AddUIComponent<LegacyCalculationPreview>();
            _legacyPanel.relativePosition = _volumetricPanel.relativePosition;
            _legacyPanel.height = _volumetricPanel.height;
            _legacyPanel.width = _volumetricPanel.width;
            _legacyPanel.Setup();
            _legacyPanel.Hide();

            // Vanilla calculations panel - copy volumetric calculations panel.
            _vanillaPanel = this.AddUIComponent<VanillaCalculationPreview>();
            _vanillaPanel.relativePosition = _volumetricPanel.relativePosition;
            _vanillaPanel.height = _volumetricPanel.height;
            _vanillaPanel.width = _volumetricPanel.width;
            _vanillaPanel.Setup();
            _vanillaPanel.Hide();

            // Floor dropdown panel - set size manually to avoid invisible overlap of calculations panel (preventing e.g. tooltips).
            _floorPanel = this.AddUIComponent<UIPanel>();
            _floorPanel.relativePosition = new Vector2(RightColumnX, MenuY);
            _floorPanel.autoSize = false;
            _floorPanel.width = RightColumnX - this.width;
            _floorPanel.height = BaseCalcY - MenuY;
            _floorPanel.autoLayout = false;
            _floorPanel.clipChildren = false;
            _floorPanel.Show();

            // Floor override label (for when floor dropdown menu is hidden).
            _floorOverrideLabel = UILabels.AddLabel(this, RightColumnX, MenuY, Translations.Translate("RPR_CAL_FOV"), this.width - RightColumnX, 0.7f);
            _floorOverrideLabel.Hide();

            // Pack dropdowns.
            _popMenu = UIDropDowns.AddDropDown(this, Margin, MenuY, ComponentWidth);
            _floorMenu = UIDropDowns.AddDropDown(_floorPanel, 0f, 0f, ComponentWidth);

            // School dropdown panel.
            _schoolPanel = this.AddUIComponent<UIPanel>();
            _schoolPanel.relativePosition = new Vector2(Margin, Row2LabelY);
            _schoolPanel.autoSize = false;
            _schoolPanel.autoLayout = false;
            _schoolPanel.clipChildren = false;
            _schoolPanel.height = ApplyX - Row2LabelY;
            _schoolPanel.width = this.width - (Margin * 2);

            // School panel title and dropdown menu.
            UILabel schoolTitle = ColumnLabel(_schoolPanel, Translations.Translate("RPR_CAL_SCH_PRO"), 0, 0);
            _schoolMenu = UIDropDowns.AddDropDown(_schoolPanel, 0f, LabelHeight, ComponentWidth);
            _schoolPanel.Hide();

            // Pack descriptions.
            _popDescription = Description(this, Margin, DescriptionY);
            _floorDescription = Description(_floorPanel, 0f, DescriptionY - MenuY);
            _schoolDescription = Description(_schoolPanel, 0f, LabelHeight + DescriptionY - MenuY);

            // Apply button.
            _applyButton = UIButtons.AddButton(this, ApplyX, BaseSaveY, Translations.Translate("RPR_OPT_SAA"), ButtonWidth);
            _applyButton.eventClicked += (c, p) => ApplySettings();

            // Dropdown event handlers.
            _popMenu.eventSelectedIndexChanged += (component, index) => UpdatePopSelection(index);
            _floorMenu.eventSelectedIndexChanged += (component, index) => UpdateFloorSelection(index);
            _schoolMenu.eventSelectedIndexChanged += (component, index) => UpdateSchoolSelection(index);

            // Add school multiplier slider (starts hidden).
            _multSlider = AddSliderWithMultipler(_schoolPanel, string.Empty, 1f, 5f, 0.5f, ModSettings.DefaultSchoolMult, (value) => UpdateMultiplier(value), ComponentWidth);
            _multSlider.parent.relativePosition = new Vector2(RightColumnX, 10f);
            _multSlider.parent.Hide();

            // Muliplier checkbox.
            _multCheck = UICheckBoxes.AddLabelledCheckBox(_schoolPanel, RightColumnX, 18f, Translations.Translate("RPR_CAL_CAP_OVR"));

            // Multiplier default label.
            _multDefaultLabel = UILabels.AddLabel(_schoolPanel, RightColumnX + 21f, 40f, Translations.Translate("RPR_CAL_CAP_DEF") + " x" + ModSettings.DefaultSchoolMult, textScale: 0.8f);

            // Multplier checkbox event handler.
            _multCheck.eventCheckChanged += (c, isChecked) => MultiplierCheckChanged(isChecked);
        }

        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building.</param>
        internal void SelectionChanged(BuildingInfo building)
        {
            // Set current building.
            _currentBuilding = building;

            // Safety first!
            if (_currentBuilding != null)
            {
                string buildingName = building.name;

                // Get available calculation packs for this building.
                _popPacks = PopData.Instance.GetPacks(building);
                _floorPacks = FloorData.Instance.Packs;

                // Get current and default packs for this item.
                _currentPopPack = (PopDataPack)PopData.Instance.ActivePack(building);
                _currentFloorPack = (FloorDataPack)FloorData.Instance.ActivePack(building);
                PopDataPack defaultPopPack = (PopDataPack)PopData.Instance.CurrentDefaultPack(building);
                FloorDataPack defaultFloorPack = (FloorDataPack)FloorData.Instance.CurrentDefaultPack(building);

                // Update multiplier before we do any other calcs.
                _multCheck.isChecked = Multipliers.Instance.HasOverride(buildingName);
                currentMult = Multipliers.Instance.ActiveMultiplier(building);

                // Build pop pack menu.
                _popMenu.items = new string[_popPacks.Length];
                for (int i = 0; i < _popMenu.items.Length; ++i)
                {
                    _popMenu.items[i] = _popPacks[i].DisplayName;

                    // Check for default name match,
                    if (_popPacks[i].Name.Equals(defaultPopPack.Name))
                    {
                        _popMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (_popPacks[i].Name.Equals(_currentPopPack.Name))
                    {
                        _popMenu.selectedIndex = i;
                    }
                }

                // Set population pack to current pack.
                UpdatePopSelection(_currentPopPack);

                // Build floor pack menu.
                _floorMenu.items = new string[_floorPacks.Length];
                for (int i = 0; i < _floorPacks.Length; ++i)
                {
                    _floorMenu.items[i] = _floorPacks[i].DisplayName;

                    // Check for default name match,
                    if (_floorPacks[i].Name.Equals(defaultFloorPack.Name))
                    {
                        _floorMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (_floorPacks[i].Name.Equals(_currentFloorPack.Name))
                    {
                        _floorMenu.selectedIndex = i;

                        // Force pack selection update.
                        UpdateFloorSelection(i);
                    }
                }

                // Update legacy and vanilla panel for private building AIs (volumetric panel is updated by menu selection change above).
                if (building.GetAI() is PrivateBuildingAI)
                {
                    _legacyPanel.SelectionChanged(building);
                    _vanillaPanel.SelectionChanged(building);
                }

                // Is this a school building (need to do school building after pop and floor packs are updated)?
                if (building.GetAI() is SchoolAI)
                {
                    // Yes - school building.  Set current pack.
                    _currentSchoolPack = (SchoolDataPack)SchoolData.Instance.ActivePack(building);

                    // Hide vanilla panel.
                    _vanillaPanel.Hide();

                    // Are we using custom school settings?
                    if (ModSettings.EnableSchoolProperties)
                    {
                        // Yes - extend panel height and show school panel.
                        _volumetricPanel.relativePosition = new Vector2(0f, SchoolCalcY);
                        _volumetricPanel.height = SchoolCalcPanelHeight;
                        _applyButton.relativePosition = new Vector2(ApplyX, SchoolSaveY);

                        // Get available school packs for this building.
                        _schoolPacks = SchoolData.Instance.GetPacks(building);

                        // Get current and default packs for this item.
                        _currentSchoolPack = (SchoolDataPack)SchoolData.Instance.ActivePack(building);
                        SchoolDataPack defaultSchoolPack = (SchoolDataPack)SchoolData.Instance.CurrentDefaultPack(building);

                        // Build school pack menu.
                        _schoolMenu.items = new string[_schoolPacks.Length];
                        for (int i = 0; i < _schoolMenu.items.Length; ++i)
                        {
                            _schoolMenu.items[i] = _schoolPacks[i].DisplayName;

                            // Check for default name match,
                            if (_schoolPacks[i].Name.Equals(defaultSchoolPack.Name))
                            {
                                _schoolMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                            }

                            // Set menu selection to current pack if it matches.
                            if (_schoolPacks[i].Name.Equals(_currentSchoolPack.Name))
                            {
                                _schoolMenu.selectedIndex = i;

                                // Force pack selection update.
                                UpdateSchoolSelection(i);
                            }
                        }

                        // Set multiplier value.
                        _multSlider.value = currentMult;

                        _schoolPanel.Show();
                    }
                    else
                    {
                        // It's a school, but we're not using custom school settings, so use the non-school layout.
                        _volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
                        _applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
                        _volumetricPanel.height = BaseCalcPanelHeight;
                        _schoolPanel.Hide();
                    }
                }
                else
                {
                    // Not a school building - use non-school layout.
                    _currentSchoolPack = null;
                    _volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
                    _applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
                    _volumetricPanel.height = BaseCalcPanelHeight;
                    _schoolPanel.Hide();
                }
            }
        }

        /// <summary>
        /// Updates the population calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack.</param>
        private void UpdatePopSelection(int index) => UpdatePopSelection(_popPacks[index]);

        /// <summary>
        /// Updates the population calculation pack selection to the selected pack.
        /// </summary>
        /// <param name="popPack">New population data pack.</param>
        private void UpdatePopSelection(PopDataPack popPack)
        {
            // Update selected pack.
            _currentPopPack = popPack;

            // Update description.
            _popDescription.text = _currentPopPack.Description;

            // Check if we're using legacy or volumetric data.
            if (_currentPopPack is VolumetricPopPack)
            {
                // Volumetric pack.  Are we coming from a legacy or vanilla setting?
                if (usingLegacyOrVanilla)
                {
                    // Reset flag.
                    usingLegacyOrVanilla = false;

                    // Restore floor rendering.
                    BuildingDetailsPanelManager.Panel.HideFloors = false;

                    // Update override label text.
                    _floorOverrideLabel.text = Translations.Translate("RPR_CAL_FOV");

                    // Set visibility.
                    _legacyPanel.Hide();
                    _vanillaPanel.Hide();
                    _volumetricPanel.Show();
                }

                // Update panel with new calculations.
                VolumetricPopPack.LevelData thisLevel = CurrentLevelData;
                _volumetricPanel.UpdatePopText(thisLevel);
                _volumetricPanel.CalculateVolumetric(_currentBuilding, thisLevel, _currentFloorOverride ?? _currentFloorPack, _currentSchoolPack, currentMult);

                // Set visibility.
                if (_currentFloorOverride == null)
                {
                    _floorOverrideLabel.Hide();
                    _floorMenu.Show();
                }
                else
                {
                    _floorOverrideLabel.Show();
                    _floorMenu.Hide();
                }

                _floorPanel.Show();
            }
            else if (_currentPopPack is VanillaPack)
            {
                // Using vanilla calcs.
                usingLegacyOrVanilla = true;

                // Set visibility.
                _volumetricPanel.Hide();
                _floorPanel.Hide();
                _legacyPanel.Hide();
                _vanillaPanel.isVisible = !(_currentBuilding.m_buildingAI is SchoolAI);

                // Set override label and show.
                _floorOverrideLabel.text = Translations.Translate("RPR_CAL_FLG");
                _floorOverrideLabel.Show();

                // Cancel any floor rendering.
                BuildingDetailsPanelManager.Panel.HideFloors = true;
            }
            else
            {
                // Using legacy calcs - set flag.
                usingLegacyOrVanilla = true;

                // Set visibility.
                _volumetricPanel.Hide();
                _floorPanel.Hide();
                _vanillaPanel.Hide();
                _legacyPanel.Show();

                // Set override label and show.
                _floorOverrideLabel.text = Translations.Translate("RPR_CAL_FLG");
                _floorOverrideLabel.Show();

                // Cancel any floor rendering.
                BuildingDetailsPanelManager.Panel.HideFloors = true;
            }
        }

        /// <summary>
        /// Updates the floor calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack.</param>
        private void UpdateFloorSelection(int index)
        {
            // Update selected pack.
            _currentFloorPack = (FloorDataPack)_floorPacks[index];

            // Update description.
            _floorDescription.text = _currentFloorPack.Description;

            // Update panel with new calculations, assuming that we're not using legacy or vanilla popultion calcs.
            _volumetricPanel.UpdateFloorText(_currentFloorPack);
            if (_currentPopPack.Version != DataPack.DataVersion.Legacy && _currentPopPack.Version != DataPack.DataVersion.Vanilla)
            {
                _volumetricPanel.CalculateVolumetric(_currentBuilding, CurrentLevelData, _currentFloorPack, _currentSchoolPack, currentMult);
            }

            // Communicate change with to rest of panel.
            BuildingDetailsPanelManager.Panel.FloorPack = _currentFloorPack;
        }

        /// <summary>
        /// Updates the school calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack.</param>
        private void UpdateSchoolSelection(int index)
        {
            // Update selected pack.
            _currentSchoolPack = _schoolPacks[index];

            // Update description.
            _schoolDescription.text = _currentSchoolPack.Description;

            // Update volumetric panel with new calculations.
            if (!usingLegacyOrVanilla)
            {
                _volumetricPanel.CalculateVolumetric(_currentBuilding, CurrentLevelData, _currentFloorPack, _currentSchoolPack, currentMult);
            }

            // School selections aren't used anywhere else, so no need to communicate change to rest of panel.
        }

        /// <summary>
        /// Updates the current multiplier and regenerates calculations if necesssary when the multiplier slider is changed.
        /// Should only be called from multSlider onValueChanged.
        /// </summary>
        /// <param name="multiplier">New multiplier.</param>
        private void UpdateMultiplier(float multiplier)
        {
            // Set multiplier.
            currentMult = multiplier;

            // Recalculte values if we're not using legacy or vanilla calcs.
            if (!usingLegacyOrVanilla)
            {
                _volumetricPanel.CalculateVolumetric(_currentBuilding, CurrentLevelData, _currentFloorPack, _currentSchoolPack, currentMult);
            }
        }

        /// <summary>
        /// Updates the current multiplier and regenerates calculations if necessary when the custom multiplier check changes.
        /// Should only be called from multCheck onCheckChanged.
        /// </summary>
        /// <param name="isCustom">Custom multiplier enabled state.</param>
        private void MultiplierCheckChanged(bool isCustom)
        {
            // Toggle slider and default label visibility.
            if (isCustom)
            {
                _multDefaultLabel.Hide();
                _multSlider.parent.Show();

                // Set multiplier value to whatever is currently active for that building.
                currentMult = Multipliers.Instance.ActiveMultiplier(_currentBuilding);
                _multSlider.value = currentMult;
            }
            else
            {
                // Set default multiplier.
                currentMult = ModSettings.DefaultSchoolMult;

                _multSlider.parent.Hide();
                _multDefaultLabel.Show();
            }

            // In either case, recalculate as necessary.
            if (!usingLegacyOrVanilla)
            {
                _volumetricPanel.CalculateVolumetric(_currentBuilding, CurrentLevelData, _currentFloorPack, _currentSchoolPack, currentMult);
            }
        }

        /// <summary>
        /// Applies current settings and saves the updated configuration to file.
        /// </summary>
        private void ApplySettings()
        {
            // Update building setting and save - multiplier first!
            Multipliers.Instance.UpdateMultiplier(_currentBuilding, currentMult);
            PopData.Instance.UpdateBuildingPack(_currentBuilding, _currentPopPack);
            FloorData.Instance.UpdateBuildingPack(_currentBuilding, _currentFloorPack);

            // Update multiplier.
            if (_multCheck.isChecked)
            {
                // If the multiplier override checkbox is selected, update the multiplier with the slider value.
                Multipliers.Instance.UpdateMultiplier(_currentBuilding, currentMult);
            }
            else
            {
                // Otherwise, delete any multiplier override.
                Multipliers.Instance.DeleteMultiplier(_currentBuilding.name);
            }

            // Make sure SchoolData is called AFTER student count is settled via Pop and Floor packs, so it can work from updated data.
            if (_currentBuilding.m_buildingAI is SchoolAI)
            {
                SchoolData.Instance.UpdateBuildingPack(_currentBuilding, _currentSchoolPack);
            }

            ConfigurationUtils.SaveSettings();

            // Update all existing buildings, force-evicting residents as necessary.
            CitizenUnitUtils.UpdateCitizenUnits(_currentBuilding.name, ItemClass.Service.None, _currentBuilding.GetSubService(), false);

            // Refresh the selection list (to make sure settings checkboxes reflect new state).
            BuildingDetailsPanelManager.Panel.RefreshList();
        }

        /// <summary>
        /// Adds a column header label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="text">Label text.</param>
        /// <param name="xPos">Label x-position.</param>
        /// <param name="yPos">Label y-position.</param>
        /// <returns>New column label.</returns>
        private UILabel ColumnLabel(UIComponent parent, string text, float xPos, float yPos)
        {
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector2(xPos, yPos);
            newLabel.textAlignment = UIHorizontalAlignment.Center;
            newLabel.text = text;
            newLabel.textScale = 1f;
            newLabel.autoSize = false;
            newLabel.width = ComponentWidth;

            return newLabel;
        }

        /// <summary>
        /// Adds a pack description text label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="xPos">Label x-position.</param>
        /// <param name="yPos">Label y-position.</param>
        /// <returns>New description label.</returns>
        private UILabel Description(UIComponent parent, float xPos, float yPos)
        {
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector2(xPos, yPos);
            newLabel.autoSize = false;
            newLabel.autoHeight = true;
            newLabel.wordWrap = true;
            newLabel.textScale = 0.7f;
            newLabel.width = ComponentWidth;

            return newLabel;
        }

        /// <summary>
        /// Adds a slider with a multiplier label below.
        /// </summary>
        /// <param name="parent">Panel to add the control to.</param>
        /// <param name="text">Descriptive label text.</param>
        /// <param name="min">Slider minimum value.</param>
        /// <param name="max">Slider maximum value.</param>
        /// <param name="step">Slider minimum step.</param>
        /// <param name="defaultValue">Slider initial value.</param>
        /// <param name="eventCallback">Slider event handler.</param>
        /// <param name="width">Slider width (excluding value label to right) (default 600).</param>
        /// <returns>New UI slider with attached labels.</returns>
        private UISlider AddSliderWithMultipler(UIComponent parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback, float width = 600f)
        {
            // Add slider component.
            UISlider newSlider = UISliders.AddPlainSlider(parent, 0f, 0f, text, min, max, step, defaultValue, width);
            UIPanel sliderPanel = (UIPanel)newSlider.parent;

            // Value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.relativePosition = UILayout.PositionUnder(newSlider, 2, 0f);
            valueLabel.text = "x" + newSlider.value.ToString();

            // Event handler to update value label.
            newSlider.eventValueChanged += (component, value) =>
            {
                valueLabel.text = "x" + value.ToString();

                // Execute provided callback.
                eventCallback(value);
            };

            return newSlider;
        }
    }
}
