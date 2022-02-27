using UnityEngine;
using ICities;
using ColossalFramework.UI;


namespace RealPop2
{
    public class UIModCalcs : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float ColumnWidth = UIBuildingDetails.RightWidth / 2f;
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

        // Panel components.
        private UILabel title;
        private UIPanel floorPanel, schoolPanel;
        private UILegacyCalcs legacyPanel;
        private UIVanillaCalcs vanillaPanel;
        private UIVolumetricPanel volumetricPanel;
        private UIDropDown popMenu, floorMenu, schoolMenu;
        private UICheckBox multCheck;
        private UISlider multSlider;
        private UILabel multDefaultLabel;
        private UILabel popDescription, floorDescription, schoolDescription, floorOverrideLabel;
        private UIButton applyButton;

        // Data arrays.
        private PopDataPack[] popPacks;
        private DataPack[] floorPacks;
        private SchoolDataPack[] schoolPacks;

        // Current selections.
        private BuildingInfo currentBuilding;
        private PopDataPack currentPopPack;
        private FloorDataPack currentFloorPack, currentFloorOverride;
        private SchoolDataPack currentSchoolPack;

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
                currentFloorOverride = value;

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
                    displayPack = currentFloorPack;
                    floorOverrideLabel.Hide();
                    floorPanel.Show();
                }
                else
                {
                    // Valid override - hide floor panel.
                    floorPanel.Hide();

                    // Set override text label and show it.
                    floorOverrideLabel.text = Translations.Translate("RPR_CAL_FOV");
                    floorOverrideLabel.Show();

                    // Display figures for override, not current floor pack.
                    displayPack = value;
                }

                // Update panel with new calculations.
                volumetricPanel.UpdateFloorText(displayPack);
                volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, displayPack, currentSchoolPack, currentMult);
            }
        }
        

        /// <summary>
        /// /// Returns the level data record from the current floor pack that's relevant to the selected building's level.
        /// /// </summary>
        private LevelData CurrentLevelData
        {
            get
            {
                // Bounds check on provided level, to handle misconfigured prefabs.
                LevelData[] levels = ((VolumetricPopPack)currentPopPack).levels;
                int level = (int)currentBuilding.GetClassLevel();
                int maxLevel = levels.Length - 1;
                if (level > maxLevel)
                {
                    Logging.Error("building ", currentBuilding.name, " has ClassLevel ", level, " but maximum configured level count is ", maxLevel);
                    level = maxLevel;
                }
                return levels[level];
            }
        }


        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        internal void Setup()
        {
            // Basic setup.
            clipChildren = true;

            // Title.
            title = this.AddUIComponent<UILabel>();
            title.relativePosition = new Vector3(0, 0);
            title.textAlignment = UIHorizontalAlignment.Center;
            title.text = Translations.Translate("RPR_CAL_MOD");
            title.textScale = 1.2f;
            title.autoSize = false;
            title.width = this.width;

            // Column titles.
            UILabel densityTitle = ColumnLabel(this, Translations.Translate("RPR_CAL_DEN"), Margin, ColumnLabelY);
            UILabel floorTitle = ColumnLabel(this, Translations.Translate("RPR_CAL_BFL"), RightColumnX, ColumnLabelY);

            // Volumetric calculations panel.
            volumetricPanel = this.AddUIComponent<UIVolumetricPanel>();
            volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
            volumetricPanel.height = this.height - title.height + 80f;
            volumetricPanel.width = this.width;
            volumetricPanel.Setup();

            // Legacy calculations panel - copy volumetric calculations panel.
            legacyPanel = this.AddUIComponent<UILegacyCalcs>();
            legacyPanel.relativePosition = volumetricPanel.relativePosition;
            legacyPanel.height = volumetricPanel.height;
            legacyPanel.width = volumetricPanel.width;
            legacyPanel.Setup();
            legacyPanel.Hide();

            // Vanilla calculations panel - copy volumetric calculations panel.
            vanillaPanel = this.AddUIComponent<UIVanillaCalcs>();
            vanillaPanel.relativePosition = volumetricPanel.relativePosition;
            vanillaPanel.height = volumetricPanel.height;
            vanillaPanel.width = volumetricPanel.width;
            vanillaPanel.Setup();
            vanillaPanel.Hide();

            // Floor dropdown panel - set size manually to avoid invisible overlap of calculations panel (preventing e.g. tooltips).
            floorPanel = this.AddUIComponent<UIPanel>();
            floorPanel.relativePosition = new Vector2(RightColumnX, MenuY);
            floorPanel.autoSize = false;
            floorPanel.width = RightColumnX - this.width;
            floorPanel.height = BaseCalcY - MenuY;
            floorPanel.autoLayout = false;
            floorPanel.clipChildren = false;
            floorPanel.Show();

            // Floor override label (for when floor dropdown menu is hidden).
            floorOverrideLabel = UIControls.AddLabel(this, RightColumnX, MenuY, Translations.Translate("RPR_CAL_FOV"), this.width - RightColumnX, 0.7f);
            floorOverrideLabel.Hide();

            // Pack dropdowns.
            popMenu = UIControls.AddDropDown(this, Margin, MenuY, ComponentWidth);
            floorMenu = UIControls.AddDropDown(floorPanel, 0f, 0f, ComponentWidth);

            // School dropdown panel.
            schoolPanel = this.AddUIComponent<UIPanel>();
            schoolPanel.relativePosition = new Vector2(Margin, Row2LabelY);
            schoolPanel.autoSize = false;
            schoolPanel.autoLayout = false;
            schoolPanel.clipChildren = false;
            schoolPanel.height = ApplyX - Row2LabelY;
            schoolPanel.width = this.width - (Margin * 2);

            // School panel title and dropdown menu.
            UILabel schoolTitle = ColumnLabel(schoolPanel, Translations.Translate("RPR_CAL_SCH_PRO"), 0, 0);
            schoolMenu = UIControls.AddDropDown(schoolPanel, 0f, LabelHeight, ComponentWidth);
            schoolPanel.Hide();

            // Pack descriptions.
            popDescription = Description(this, Margin, DescriptionY);
            floorDescription = Description(floorPanel, 0f, DescriptionY - MenuY);
            schoolDescription = Description(schoolPanel, 0f, LabelHeight + DescriptionY - MenuY);

            // Apply button.
            applyButton = UIControls.AddButton(this, ApplyX, BaseSaveY, Translations.Translate("RPR_OPT_SAA"), ButtonWidth);
            applyButton.eventClicked += (control, clickEvent) => ApplySettings();

            // Dropdown event handlers.
            popMenu.eventSelectedIndexChanged += (component, index) => UpdatePopSelection(index);
            floorMenu.eventSelectedIndexChanged += (component, index) => UpdateFloorSelection(index);
            schoolMenu.eventSelectedIndexChanged += (component, index) => UpdateSchoolSelection(index);

            // Add school multiplier slider (starts hidden).
            multSlider = AddSliderWithMultipler(schoolPanel, string.Empty, 1f, 5f, 0.5f, ModSettings.DefaultSchoolMult, (value) => UpdateMultiplier(value), ComponentWidth);
            multSlider.parent.relativePosition = new Vector2(RightColumnX, 10f);
            multSlider.parent.Hide();

            // Muliplier checkbox.
            multCheck = UIControls.LabelledCheckBox(schoolPanel, RightColumnX, 18f, Translations.Translate("RPR_CAL_CAP_OVR"));

            // Multiplier default label.
            multDefaultLabel = UIControls.AddLabel(schoolPanel, RightColumnX + 21f, 40f, Translations.Translate("RPR_CAL_CAP_DEF") + " x" + ModSettings.DefaultSchoolMult, textScale: 0.8f);

            // Multplier checkbox event handler.
            multCheck.eventCheckChanged += (control, isChecked) => MultiplierCheckChanged(isChecked);
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building</param>
        internal void SelectionChanged(BuildingInfo building)
        {
            // Set current building.
            currentBuilding = building;

            // Safety first!
            if (currentBuilding != null)
            {
                string buildingName = building.name;

                // Get available calculation packs for this building.
                popPacks = PopData.instance.GetPacks(building);
                floorPacks = FloorData.instance.Packs;

                // Get current and default packs for this item.
                currentPopPack = (PopDataPack)PopData.instance.ActivePack(building);
                currentFloorPack = (FloorDataPack)FloorData.instance.ActivePack(building);
                PopDataPack defaultPopPack = (PopDataPack)PopData.instance.CurrentDefaultPack(building);
                FloorDataPack defaultFloorPack = (FloorDataPack)FloorData.instance.CurrentDefaultPack(building);

                // Update multiplier before we do any other calcs.
                multCheck.isChecked = Multipliers.instance.HasOverride(buildingName);
                currentMult = Multipliers.instance.ActiveMultiplier(building);


                // Build pop pack menu.
                popMenu.items = new string[popPacks.Length];
                for (int i = 0; i < popMenu.items.Length; ++i)
                {
                    popMenu.items[i] = popPacks[i].DisplayName;

                    // Check for default name match,
                    if (popPacks[i].name.Equals(defaultPopPack.name))
                    {
                        popMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (popPacks[i].name.Equals(currentPopPack.name))
                    {
                        popMenu.selectedIndex = i;
                    }
                }

                // Set population pack to current pack.
                UpdatePopSelection(currentPopPack);

                // Build floor pack menu.
                floorMenu.items = new string[floorPacks.Length];
                for (int i = 0; i < floorPacks.Length; ++i)
                {
                    floorMenu.items[i] = floorPacks[i].DisplayName;

                    // Check for default name match,
                    if (floorPacks[i].name.Equals(defaultFloorPack.name))
                    {
                        floorMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                    }

                    // Set menu selection to current pack if it matches.
                    if (floorPacks[i].name.Equals(currentFloorPack.name))
                    {
                        floorMenu.selectedIndex = i;

                        // Force pack selection update.
                        UpdateFloorSelection(i);
                    }
                }

                // Update legacy and vanilla panel for private building AIs (volumetric panel is updated by menu selection change above).
                if (building.GetAI() is PrivateBuildingAI)
                {
                    legacyPanel.SelectionChanged(building);
                    vanillaPanel.SelectionChanged(building);
                }

                // Is this a school building (need to do school building after pop and floor packs are updated)?
                if (building.GetAI() is SchoolAI)
                {
                    // Yes - school building.  Set current pack.
                    currentSchoolPack = (SchoolDataPack)SchoolData.instance.ActivePack(building);

                    // Hide vanilla panel.
                    vanillaPanel.Hide();

                    // Are we using custom school settings?
                    if (ModSettings.enableSchoolProperties)
                    {
                        // Yes - extend panel height and show school panel.
                        volumetricPanel.relativePosition = new Vector2(0f, SchoolCalcY);
                        applyButton.relativePosition = new Vector2(ApplyX, SchoolSaveY);

                        // Get available school packs for this building.
                        schoolPacks = SchoolData.instance.GetPacks(building);

                        // Get current and default packs for this item.
                        currentSchoolPack = (SchoolDataPack)SchoolData.instance.ActivePack(building);
                        SchoolDataPack defaultSchoolPack = (SchoolDataPack)SchoolData.instance.CurrentDefaultPack(building);

                        // Build school pack menu.
                        schoolMenu.items = new string[schoolPacks.Length];
                        for (int i = 0; i < schoolMenu.items.Length; ++i)
                        {
                            schoolMenu.items[i] = schoolPacks[i].DisplayName;

                            // Check for default name match,
                            if (schoolPacks[i].name.Equals(defaultSchoolPack.name))
                            {
                                schoolMenu.items[i] += Translations.Translate("RPR_PCK_DEF");
                            }

                            // Set menu selection to current pack if it matches.
                            if (schoolPacks[i].name.Equals(currentSchoolPack.name))
                            {
                                schoolMenu.selectedIndex = i;

                                // Force pack selection update.
                                UpdateSchoolSelection(i);
                            }
                        }

                        // Set multiplier value.
                        multSlider.value = currentMult;

                        schoolPanel.Show();
                    }
                    else
                    {
                        // It's a school, but we're not using custom school settings, so use the non-school layout.
                        volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
                        applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
                        schoolPanel.Hide();
                    }
                }
                else
                {
                    // Not a school building - use non-school layout.
                    currentSchoolPack = null;
                    volumetricPanel.relativePosition = new Vector2(0f, BaseCalcY);
                    applyButton.relativePosition = new Vector2(ApplyX, BaseSaveY);
                    schoolPanel.Hide();
                }
            }
        }


        /// <summary>
        /// Updates the population calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        private void UpdatePopSelection(int index) => UpdatePopSelection(popPacks[index]);


        /// <summary>
        /// Updates the population calculation pack selection to the selected pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        private void UpdatePopSelection(PopDataPack popPack)
        {
            // Update selected pack.
            currentPopPack = popPack;

            // Update description.
            popDescription.text = currentPopPack.Description;

            // Check if we're using legacy or volumetric data.
            if (currentPopPack is VolumetricPopPack)
            {
                // Volumetric pack.  Are we coming from a legacy or vanilla setting?
                if (usingLegacyOrVanilla)
                {
                    // Reset flag.
                    usingLegacyOrVanilla = false;

                    // Restore floor rendering.
                    BuildingDetailsPanel.Panel.HideFloors = false;

                    // Update override label text.
                    floorOverrideLabel.text = Translations.Translate("RPR_CAL_FOV");

                    // Set visibility.
                    legacyPanel.Hide();
                    vanillaPanel.Hide();
                    volumetricPanel.Show();
                }

                // Update panel with new calculations.
                LevelData thisLevel = CurrentLevelData;
                volumetricPanel.UpdatePopText(thisLevel);
                volumetricPanel.CalculateVolumetric(currentBuilding, thisLevel, currentFloorOverride ?? currentFloorPack, currentSchoolPack, currentMult);

                // Set visibility.
                if (currentFloorOverride == null)
                {
                    floorOverrideLabel.Hide();
                    floorMenu.Show();
                }
                else
                {
                    floorOverrideLabel.Show();
                    floorMenu.Hide();
                }

                floorPanel.Show();
            }
            else if (currentPopPack is VanillaPack)
            {
                // Using vanilla calcs.
                usingLegacyOrVanilla = true;

                // Set visibility.
                volumetricPanel.Hide();
                floorPanel.Hide();
                legacyPanel.Hide();
                vanillaPanel.Show();

                // Set override label and show.
                floorOverrideLabel.text = Translations.Translate("RPR_CAL_FLG");
                floorOverrideLabel.Show();

                // Cancel any floor rendering.
                BuildingDetailsPanel.Panel.HideFloors = true;
            }
            else
            {
                // Using legacy calcs - set flag.
                usingLegacyOrVanilla = true;

                // Set visibility.
                volumetricPanel.Hide();
                floorPanel.Hide();
                vanillaPanel.Hide();
                legacyPanel.Show();

                // Set override label and show.
                floorOverrideLabel.text = Translations.Translate("RPR_CAL_FLG");
                floorOverrideLabel.Show();

                // Cancel any floor rendering.
                BuildingDetailsPanel.Panel.HideFloors = true;
            }
        }


        /// <summary>
        /// Updates the floor calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        private void UpdateFloorSelection(int index)
        {
            // Update selected pack.
            currentFloorPack = (FloorDataPack)floorPacks[index];

            // Update description.
            floorDescription.text = currentFloorPack.Description;

            // Update panel with new calculations, assuming that we're not using legacy or vanilla popultion calcs.
            volumetricPanel.UpdateFloorText(currentFloorPack);
            if (currentPopPack.version != DataVersion.legacy && currentPopPack.version != DataVersion.vanilla)
            {
                volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, currentFloorPack, currentSchoolPack, currentMult);
            }

            // Communicate change with to rest of panel.
            BuildingDetailsPanel.Panel.FloorDataPack = currentFloorPack;
        }


        /// <summary>
        /// Updates the school calculation pack selection to the selected calculation pack.
        /// </summary>
        /// <param name="index">Index number (from menu) of selection pack</param>
        private void UpdateSchoolSelection(int index)
        {
            // Update selected pack.
            currentSchoolPack = schoolPacks[index];

            // Update description.
            schoolDescription.text = currentSchoolPack.Description;

            // Update volumetric panel with new calculations.
            if (!usingLegacyOrVanilla)
            {
                volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, currentFloorPack, currentSchoolPack, currentMult);
            }

            // School selections aren't used anywhere else, so no need to communicate change to rest of panel.
        }


        /// <summary>
        /// Updates the current multiplier and regenerates calculations if necesssary when the multiplier slider is changed.
        /// Should only be called from multSlider onValueChanged.
        /// </summary>
        /// <param name="multiplier">New multiplier</param>
        private void UpdateMultiplier(float multiplier)
        {
            // Set multiplier.
            currentMult = multiplier;

            // Recalculte values if we're not using legacy or vanilla calcs.
            if (!usingLegacyOrVanilla)
            {
                volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, currentFloorPack, currentSchoolPack, currentMult);
            }
        }


        /// <summary>
        /// Updates the current multiplier and regenerates calculations if necessary when the custom multiplier check changes.
        /// Should only be called from multCheck onCheckChanged.
        /// </summary>
        /// <param name="isCustom">Custom multiplier enabled state</param>
        private void MultiplierCheckChanged(bool isCustom)
        {
            // Toggle slider and default label visibility.
            if (isCustom)
            {
                multDefaultLabel.Hide();
                multSlider.parent.Show();

                // Set multiplier value to whatever is currently active for that building.
                currentMult = Multipliers.instance.ActiveMultiplier(currentBuilding);
                multSlider.value = currentMult;
            }
            else
            {
                // Set default multiplier.
                currentMult = ModSettings.DefaultSchoolMult;

                multSlider.parent.Hide();
                multDefaultLabel.Show();
            }

            // In either case, recalculate as necessary.
            if (!usingLegacyOrVanilla)
            {
                volumetricPanel.CalculateVolumetric(currentBuilding, CurrentLevelData, currentFloorPack, currentSchoolPack, currentMult);
            }
        }


        /// <summary>
        /// Applies current settings and saves the updated configuration to file.
        /// </summary>
        private void ApplySettings()
        {
            // Update building setting and save - multiplier first!
            Multipliers.instance.UpdateMultiplier(currentBuilding, currentMult);
            PopData.instance.UpdateBuildingPack(currentBuilding, currentPopPack);
            FloorData.instance.UpdateBuildingPack(currentBuilding, currentFloorPack);

            // Update multiplier.
            if (multCheck.isChecked)
            {
                // If the multiplier override checkbox is selected, update the multiplier with the slider value.
                Multipliers.instance.UpdateMultiplier(currentBuilding, currentMult);
            }
            else
            {
                // Otherwise, delete any multiplier override.
                Multipliers.instance.DeleteMultiplier(currentBuilding.name);
            }

            // Make sure SchoolData is called AFTER student count is settled via Pop and Floor packs, so it can work from updated data.
            SchoolData.instance.UpdateBuildingPack(currentBuilding, currentSchoolPack);
            ConfigUtils.SaveSettings();

            // Refresh the selection list (to make sure settings checkboxes reflect new state).
            BuildingDetailsPanel.Panel.RefreshList();
        }


        /// <summary>
        /// Adds a column header label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="text">Label text</param>
        /// <param name="xPos">Label x-position</param>
        /// <param name="xPos">Label y-position</param>
        /// <returns>New column label</returns>
        private UILabel ColumnLabel(UIComponent parent, string text, float xPos, float yPos)
        {
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector3(xPos, yPos);
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
        /// <param name="parent">Parent component</param>
        /// <param name="xPos">Label x-position</param>
        /// <param name="xPos">Label y-position</param>
        /// <returns></returns>
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
        /// <param name="parent">Panel to add the control to</param>
        /// <param name="text">Descriptive label text</param>
        /// <param name="min">Slider minimum value</param>
        /// <param name="max">Slider maximum value</param>
        /// <param name="step">Slider minimum step</param>
        /// <param name="defaultValue">Slider initial value</param>
        /// <param name="eventCallback">Slider event handler</param>
        /// <param name="width">Slider width (excluding value label to right) (default 600)</param>
        /// <returns>New UI slider with attached labels</returns>
        private UISlider AddSliderWithMultipler(UIComponent parent, string text, float min, float max, float step, float defaultValue, OnValueChanged eventCallback, float width = 600f)
        {
            // Add slider component.
            UISlider newSlider = UIControls.AddSlider(parent, text, min, max, step, defaultValue, width);
            UIPanel sliderPanel = (UIPanel)newSlider.parent;

            // Value label.
            UILabel valueLabel = sliderPanel.AddUIComponent<UILabel>();
            valueLabel.name = "ValueLabel";
            valueLabel.relativePosition = UIControls.PositionUnder(newSlider, 2, 0f);
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
