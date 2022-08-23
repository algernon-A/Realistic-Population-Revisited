// <copyright file="BuildingEditPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Panel for editing and creating building settings.
    /// </summary>
    public class BuildingEditPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float TitleY = 5f;
        private const float PopCheckY = TitleY + 30f;
        private const float HomeJobY = PopCheckY + 25f;
        private const float FloorCheckY = HomeJobY + 25f;
        private const float FirstFloorY = FloorCheckY + 25f;
        private const float FloorHeightY = FirstFloorY + 25f;
        private const float SaveY = FloorHeightY + 35f;
        private const float DeleteY = SaveY + 35f;
        private const float MessageY = DeleteY + 35f;
        private const float TextFieldWidth = 65f;
        private const float TextFieldX = BuildingDetailsPanel.MiddleWidth - TextFieldWidth - 20f - Margin;
        private const float ButtonWidth = BuildingDetailsPanel.MiddleWidth - (Margin * 2f);

        // Panel components
        private readonly UITextField _homeJobsCount;
        private readonly UITextField _firstFloorField;
        private readonly UITextField _floorHeightField;
        private readonly UICheckBox _popCheck;
        private readonly UICheckBox _floorCheck;
        private readonly UILabel _homeJobLabel;
        private readonly UIButton _saveButton;
        private readonly UIButton _deleteButton;
        private readonly UILabel _messageLabel;

        // Currently selected building.
        private BuildingInfo currentSelection;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingEditPanel"/> class.
        /// </summary>
        public BuildingEditPanel()
        {
            // Generic setup.
            width = BuildingDetailsPanel.MiddleWidth;
            height = BuildingDetailsPanel.MiddlePanelHeight;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoLayoutDirection = LayoutDirection.Vertical;
            autoLayoutPadding.top = 5;
            autoLayoutPadding.right = 5;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Panel title.
            UILabel titleLabel = this.AddUIComponent<UILabel>();
            titleLabel.textAlignment = UIHorizontalAlignment.Center;
            titleLabel.text = Translations.Translate("RPR_CUS_TITLE");
            titleLabel.textScale = 1.2f;
            titleLabel.autoSize = true;
            titleLabel.height = 30;

            // Autoscale tile label text, with minimum size 0.35.
            while (titleLabel.width > ButtonWidth && titleLabel.textScale > 0.35f)
            {
                titleLabel.textScale -= 0.05f;
            }

            // Center title label.
            titleLabel.relativePosition = new Vector2((this.width - titleLabel.width) / 2f, TitleY);

            // Checkboxes.
            _popCheck = UICheckBoxes.AddLabelledCheckBox(this, 20f, PopCheckY, Translations.Translate("RPR_EDT_POP"), textScale: 1.0f);
            _floorCheck = UICheckBoxes.AddLabelledCheckBox(this, 20f, FloorCheckY, Translations.Translate("RPR_EDT_FLR"), textScale: 1.0f);

            // Text fields.
            _homeJobsCount = UITextFields.AddSmallLabelledTextField(this, TextFieldX, HomeJobY, Translations.Translate("RPR_LBL_HOM"), TextFieldWidth);
            _homeJobLabel = _homeJobsCount.Find<UILabel>("label");
            _firstFloorField = UITextFields.AddSmallLabelledTextField(this, TextFieldX, FirstFloorY, Translations.Translate("RPR_LBL_OFF"), TextFieldWidth);
            UILabels.AddLabel(_firstFloorField, TextFieldWidth + Margin, 3f, Measures.LengthMeasure, textScale: 0.9f);
            _floorHeightField = UITextFields.AddSmallLabelledTextField(this, TextFieldX, FloorHeightY, Translations.Translate("RPR_LBL_OFH"), TextFieldWidth);
            UILabels.AddLabel(_floorHeightField, TextFieldWidth + Margin, 3f, Measures.LengthMeasure, textScale: 0.9f);

            // Save button.
            _saveButton = UIButtons.AddButton(this, Margin, SaveY, Translations.Translate("RPR_CUS_ADD"), ButtonWidth, scale: 0.8f);
            _saveButton.tooltip = Translations.Translate("RPR_CUS_ADD_TIP");
            _saveButton.Disable();

            // Delete button.
            _deleteButton = UIButtons.AddButton(this, Margin, DeleteY, Translations.Translate("RPR_CUS_DEL"), ButtonWidth, scale: 0.8f);
            _deleteButton.tooltip = Translations.Translate("RPR_CUS_DEL_TIP");
            _deleteButton.Disable();

            // Message label (initially hidden).
            _messageLabel = this.AddUIComponent<UILabel>();
            _messageLabel.relativePosition = new Vector2(Margin, MessageY);
            _messageLabel.textAlignment = UIHorizontalAlignment.Left;
            _messageLabel.autoSize = false;
            _messageLabel.autoHeight = true;
            _messageLabel.wordWrap = true;
            _messageLabel.width = BuildingDetailsPanel.MiddleWidth - (Margin * 2f);
            _messageLabel.isVisible = false;
            _messageLabel.text = "No message to display";

            // Checkbox event handlers.
            _popCheck.eventCheckChanged += (component, isChecked) =>
            {
                // If this is now selected and floorCheck is also selected, deselect floorCheck.
                if (isChecked && _floorCheck.isChecked)
                {
                    _floorCheck.isChecked = false;
                }
            };

            _floorCheck.eventCheckChanged += (component, isChecked) =>
            {
                // If this is now selected and popCheck is also selected, deselect popCheck.
                if (isChecked && _popCheck.isChecked)
                {
                    _popCheck.isChecked = false;
                }
            };

            // Save button event handler.
            _saveButton.eventClick += (component, clickEvent) => SaveAndApply();

            // Delete button event handler.
            _deleteButton.eventClick += (component, clickEvent) => DeleteOverride();
        }

        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">New building prefab selection.</param>
        public void SelectionChanged(BuildingInfo building)
        {
            // Hide message.
            _messageLabel.isVisible = false;

            // Set current selecion.
            currentSelection = building;

            // Blank all textfields and deselect checkboxes to start with.
            _homeJobsCount.text = string.Empty;
            UpdateFloorTextFields(0, 0);
            _popCheck.isChecked = false;
            _floorCheck.isChecked = false;

            // Disable buttons and exit if no valid building is selected.
            if (building?.name == null)
            {
                _saveButton.Disable();
                _deleteButton.Disable();
                return;
            }

            // Set label by building type.
            if (building.GetService() == ItemClass.Service.Residential)
            {
                // Residential building - homes.
                _homeJobLabel.text = Translations.Translate("RPR_LBL_HOM");
            }
            else if (building.GetService() == ItemClass.Service.Education)
            {
                // Schoool building - students.
                _homeJobLabel.text = Translations.Translate("RPR_LBL_STU");
            }
            else
            {
                // Workplace building - jobs.
                _homeJobLabel.text = Translations.Translate("RPR_LBL_JOB");
            }

            // Get any population override.
            int homesJobs = PopData.Instance.GetOverride(building.name);

            // If custom settings were found (return value was non-zero), then display the result, rename the save button, and enable the delete button.
            if (homesJobs != 0)
            {
                // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                _homeJobsCount.text = homesJobs.ToString();
                _saveButton.text = Translations.Translate("RPR_CUS_UPD");
                _deleteButton.Enable();

                // Select the 'has population override' check.
                _popCheck.isChecked = true;
            }
            else
            {
                // No population override - check for custom floor override.
                FloorDataPack overridePack = FloorData.Instance.HasOverride(building.name);
                if (overridePack != null)
                {
                    // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                    UpdateFloorTextFields(Measures.LengthFromMetric(overridePack.m_firstFloorMin), Measures.LengthFromMetric(overridePack.m_floorHeight));
                    _saveButton.text = Translations.Translate("RPR_CUS_UPD");
                    _deleteButton.Enable();

                    // Select the 'has floor override' check.
                    _floorCheck.isChecked = true;
                }
                else
                {
                    // No valid selection - rename the save button, and disable the delete button.
                    _saveButton.text = Translations.Translate("RPR_CUS_ADD");
                    _deleteButton.Disable();
                }

                // Communicate override to panel.
                BuildingDetailsPanelManager.Panel.OverrideFloors = overridePack;
            }

            // We've at least got a valid building, so enable the save button.
            _saveButton.Enable();
        }

        /// <summary>
        /// Clears the override checkbox (for when the user subsequently selects a floor pack override or legacy calcs).
        /// </summary>
        internal void ClearOverride() => _floorCheck.isChecked = false;

        /// <summary>
        /// Saves and applies settings - save button event handler.
        /// </summary>
        private void SaveAndApply()
        {
            // Hide message.
            _messageLabel.isVisible = false;

            // Don't do anything with invalid entries.
            if (currentSelection == null || currentSelection.name == null)
            {
                return;
            }

            // Are we doing population overrides?
            if (_popCheck.isChecked)
            {
                // Read total floor count textfield if possible; ignore zero values
                if (ushort.TryParse(_homeJobsCount.text, out ushort homesJobs) && homesJobs != 0)
                {
                    // Minimum value of 1.
                    if (homesJobs < 1)
                    {
                        // Print warning message in red.
                        _messageLabel.textColor = new Color32(255, 0, 0, 255);
                        _messageLabel.text = Translations.Translate("RPR_ERR_ZERO");
                        _messageLabel.isVisible = true;
                    }
                    else
                    {
                        // Set overide.
                        PopData.Instance.SetOverride(currentSelection, homesJobs);

                        // Update CitizenUnits for existing building instances.
                        CitizenUnitUtils.UpdateCitizenUnits(currentSelection.name, ItemClass.Service.None, currentSelection.GetSubService(), false);

                        // Repopulate field with parsed value.
                        _homeJobLabel.text = homesJobs.ToString();
                    }
                }
                else
                {
                    // TryParse couldn't parse any data; print warning message in red.
                    _messageLabel.textColor = new Color32(255, 0, 0, 255);
                    _messageLabel.text = Translations.Translate("RPR_ERR_INV");
                    _messageLabel.isVisible = true;
                }
            }
            else
            {
                // Population override checkbox wasn't checked; remove any custom settings.
                PopData.Instance.DeleteOverride(currentSelection);

                // Remove any legacy file settings to avoid conflicts.
                OverrideUtils.RemoveResidential(currentSelection);
                OverrideUtils.RemoveWorker(currentSelection);
            }

            // Are we doing floor overrides?
            if (_floorCheck.isChecked)
            {
                // Attempt to parse values into override floor pack.
                FloorDataPack overrideFloors = TryParseFloors();

                // Were we successful?.
                if (overrideFloors != null)
                {
                    // Successful parsing - add override.
                    FloorData.Instance.SetOverride(currentSelection, overrideFloors);

                    // Save configuration.
                    ConfigurationUtils.SaveSettings();

                    // Update panel override.
                    BuildingDetailsPanelManager.Panel.OverrideFloors = overrideFloors;

                    // Repopulate fields with parsed values.
                    UpdateFloorTextFields(overrideFloors.m_firstFloorMin, overrideFloors.m_floorHeight);
                }
                else
                {
                    // Couldn't parse values; print warning message in red.
                    _messageLabel.textColor = new Color32(255, 0, 0, 255);
                    _messageLabel.text = Translations.Translate("RPR_ERR_INV");
                    _messageLabel.isVisible = true;
                }
            }
            else
            {
                // Floor override checkbox wasn't checked; remove any floor override.
                FloorData.Instance.DeleteOverride(currentSelection);
            }

            // Refresh the display so that all panels reflect the updated settings.
            BuildingDetailsPanelManager.Panel.Refresh();
        }

        /// <summary>
        /// Removes and deletes a custom override.
        /// </summary>
        private void DeleteOverride()
        {
            // Hide message.
            _messageLabel.isVisible = false;

            // Don't do anything with invalid entries.
            if (currentSelection == null || currentSelection.name == null)
            {
                return;
            }

            Logging.Message("deleting custom override for ", currentSelection.name);

            // Remove any and all overrides.
            FloorData.Instance.DeleteOverride(currentSelection);
            PopData.Instance.DeleteOverride(currentSelection);

            // Update panel override.
            BuildingDetailsPanelManager.Panel.OverrideFloors = null;

            // Homes or jobs?
            if (currentSelection.GetService() == ItemClass.Service.Residential)
            {
                // Residential building - remove any legacy settings to avoid conflicts.
                OverrideUtils.RemoveResidential(currentSelection);

                // Update CitizenUnits for existing instances of this building.
                CitizenUnitUtils.UpdateCitizenUnits(currentSelection.name, currentSelection.GetService(), currentSelection.GetSubService(), false);
            }
            else
            {
                // Employment building - remove any legacy settings to avoid conflicts.
                OverrideUtils.RemoveWorker(currentSelection);
            }

            // Refresh the display so that all panels reflect the updated settings.
            BuildingDetailsPanelManager.Panel.Refresh();
            _homeJobsCount.text = string.Empty;
        }

        /// <summary>
        /// Attempts to parse floor data fields into a valid override floor pack.
        /// </summary>
        /// <returns>New floor data pack.</returns>
        private FloorDataPack TryParseFloors()
        {
            // Attempt to parse fields.
            if (!string.IsNullOrEmpty(_firstFloorField.text) && !string.IsNullOrEmpty(_floorHeightField.text) && float.TryParse(_firstFloorField.text, out float firstFloor) && float.TryParse(_floorHeightField.text, out float floorHeight))
            {
                // Success - create new override floor pack with parsed data.
                return new FloorDataPack(DataPack.DataVersion.OverrideOne)
                {
                    m_firstFloorMin = Measures.LengthToMetric(firstFloor),
                    m_floorHeight = Measures.LengthToMetric(floorHeight),
                };
            }

            // If we got here, we didn't get a valid parse; return null.
            return null;
        }

        /// <summary>
        /// Updates floor override textfield values without triggering event handler.
        /// </summary>
        /// <param name="firstFloor">First floor height field.</param>
        /// <param name="otherFloor">Other floor height field.</param>
        private void UpdateFloorTextFields(float firstFloor, float otherFloor)
        {
            // Populate fields.
            _firstFloorField.text = firstFloor == 0 ? string.Empty : firstFloor.ToString("N1");
            _floorHeightField.text = otherFloor == 0 ? string.Empty : otherFloor.ToString("N1");
        }
    }
}