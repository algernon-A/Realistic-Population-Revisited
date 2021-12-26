using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Panel for editing and creating building settings.
    /// </summary>
    public class UIEditPanel : UIPanel
    {
        // Layout constants.
        private const float MarginPadding = 10f;
        private const float LabelWidth = 150f;
        private const float TitleY = 5f;
        private const float PopCheckY = TitleY + 30f;
        private const float HomeJobY = PopCheckY + 25f;
        private const float FloorCheckY = HomeJobY + 25f;
        private const float FirstFloorY = FloorCheckY + 25f;
        private const float FloorHeightY = FirstFloorY + 25f;
        private const float SaveY = FloorHeightY + 35f;
        private const float DeleteY = SaveY + 35f;
        private const float MessageY = DeleteY + 35f;


        // Panel components
        private UILabelledTextfield homeJobsCount, firstFloorField, floorHeightField;
        private UICheckBox popCheck, floorCheck;
        private UILabel homeJobLabel;
        private UIButton saveButton;
        private UIButton deleteButton;
        private UILabel messageLabel;

        // Currently selected building.
        private BuildingInfo currentSelection;


        /// <summary>
        /// Create the panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Generic setup.
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
            while (titleLabel.width > (this.width - (MarginPadding * 2f)) && titleLabel.textScale > 0.35f)
            {
                titleLabel.textScale -= 0.05f;
            }

            // Center title label.
            titleLabel.relativePosition = new Vector2((this.width - titleLabel.width) /2f, TitleY);

            // Checkboxes.
            popCheck = UIControls.LabelledCheckBox(this, 20f, PopCheckY, Translations.Translate("RPR_EDT_POP"), textScale: 1.0f);
            floorCheck = UIControls.LabelledCheckBox(this, 20f, FloorCheckY, Translations.Translate("RPR_EDT_FLR"), textScale: 1.0f);

            // Text fields.
            homeJobsCount = AddLabelledTextfield(HomeJobY, "RPR_LBL_HOM");
            firstFloorField = AddLabelledTextfield(FirstFloorY,"RPR_LBL_OFF");
            floorHeightField = AddLabelledTextfield(FloorHeightY, "RPR_LBL_OFH");
            homeJobLabel = homeJobsCount.label;

            // Save button.
            saveButton = UIControls.AddButton(this, MarginPadding, SaveY, Translations.Translate("RPR_CUS_ADD"), this.width - (MarginPadding * 2), scale: 0.8f);
            saveButton.tooltip = Translations.Translate("RPR_CUS_ADD_TIP");
            saveButton.Disable();

            // Delete button.
            deleteButton = UIControls.AddButton(this, MarginPadding, DeleteY, Translations.Translate("RPR_CUS_DEL"), this.width - (MarginPadding * 2), scale: 0.8f);
            deleteButton.tooltip = Translations.Translate("RPR_CUS_DEL_TIP");
            deleteButton.Disable();

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector3(MarginPadding, MessageY);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.autoSize = false;
            messageLabel.autoHeight = true;
            messageLabel.wordWrap = true;
            messageLabel.width = this.width - (MarginPadding * 2);
            messageLabel.isVisible = false;
            messageLabel.text = "No message to display";

            // Checkbox event handlers.
            popCheck.eventCheckChanged += (component, isChecked) =>
            {
                // If this is now selected and floorCheck is also selected, deselect floorCheck.
                if (isChecked && floorCheck.isChecked)
                {
                    floorCheck.isChecked = false;
                }
            };
            floorCheck.eventCheckChanged += (component, isChecked) =>
            {
                // If this is now selected and popCheck is also selected, deselect popCheck.
                if (isChecked && popCheck.isChecked)
                {
                    popCheck.isChecked = false;
                }
            };

            // Save button event handler.
            saveButton.eventClick += (component, clickEvent) => SaveAndApply();

            // Delete button event handler.
            deleteButton.eventClick += (component, clickEvent) => DeleteOverride();
        }


        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building"></param>
        public void SelectionChanged(BuildingInfo building)
        {
            string buildingName = building.name;

            // Hide message.
            messageLabel.isVisible = false;

            // Set current selecion.
            currentSelection = building;

            // Blank all textfields and deselect checkboxes to start with.
            homeJobsCount.textField.text = string.Empty;
            UpdateFloorTextFields(string.Empty, string.Empty);
            popCheck.isChecked = false;
            floorCheck.isChecked = false;

            // Disable buttons and exit if no valid building is selected.
            if (building == null || building.name == null)
            {
                saveButton.Disable();
                deleteButton.Disable();
                return;
            }
            // Set label by building type.
            if (building.GetService() == ItemClass.Service.Residential)
            {
                // Residential building - homes.
                homeJobLabel.text = Translations.Translate("RPR_LBL_HOM");
            }
            else if (building.GetService() == ItemClass.Service.Education)
            {
                // Schoool building - students.
                homeJobLabel.text = Translations.Translate("RPR_LBL_STU");
            }
            else
            {
                // Workplace building - jobs.
                homeJobLabel.text = Translations.Translate("RPR_LBL_JOB");
            }

            // Get any population override.
            int homesJobs = PopData.instance.GetOverride(buildingName);

            // If custom settings were found (return value was non-zero), then display the result, rename the save button, and enable the delete button.
            if (homesJobs != 0)
            {
                // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                homeJobsCount.textField.text = homesJobs.ToString();
                saveButton.text = Translations.Translate("RPR_CUS_UPD");
                deleteButton.Enable();

                // Select the 'has population override' check.
                popCheck.isChecked = true;
            }
            else
            {
                // No population override - check for custom floor override.
                FloorDataPack overridePack = FloorData.instance.HasOverride(buildingName);
                if (overridePack != null)
                {
                    // Valid custom settings found; display the result, rename the save button, and enable the delete button.
                    UpdateFloorTextFields(overridePack.firstFloorMin.ToString(), overridePack.floorHeight.ToString());
                    saveButton.text = Translations.Translate("RPR_CUS_UPD");
                    deleteButton.Enable();

                    // Select the 'has floor override' check.
                    floorCheck.isChecked = true;
                }
                else
                {
                    //  No valid selection - rename the save button, and disable the delete button.
                    saveButton.text = Translations.Translate("RPR_CUS_ADD");
                    deleteButton.Disable();
                }

                // Communicate override to panel.
                BuildingDetailsPanel.Panel.OverrideFloors = overridePack;
            }

            // We've at least got a valid building, so enable the save button.
            saveButton.Enable();
        }


        /// <summary>
        /// Clears the override checkbox (for when the user subsequently selects a floor pack override or legacy calcs).
        /// </summary>
        internal void ClearOverride() => floorCheck.isChecked = false;


        /// <summary>
        /// Saves and applies settings - save button event handler.
        /// </summary>
        private void SaveAndApply()
        {
            // Hide message.
            messageLabel.isVisible = false;

            // Don't do anything with invalid entries.
            if (currentSelection == null || currentSelection.name == null)
            {
                return;
            }

            // Are we doing population overrides?
            if (popCheck.isChecked)
            {
                // Read total floor count textfield if possible; ignore zero values
                if (ushort.TryParse(homeJobsCount.textField.text, out ushort homesJobs) && homesJobs != 0)
                {
                    // Minimum value of 1.
                    if (homesJobs < 1)
                    {
                        // Print warning message in red.
                        messageLabel.textColor = new Color32(255, 0, 0, 255);
                        messageLabel.text = Translations.Translate("RPR_ERR_ZERO");
                        messageLabel.isVisible = true;
                    }
                    else
                    {
                        // Set overide.
                        PopData.instance.SetOverride(currentSelection, homesJobs);

                        // Update CitizenUnits for existing building instances.
                        CitizenUnitUtils.UpdateCitizenUnits(currentSelection.name, ItemClass.Service.None, currentSelection.GetSubService(), false);

                        // Repopulate field with parsed value.
                        homeJobLabel.text = homesJobs.ToString();
                    }
                }
                else
                {
                    // TryParse couldn't parse any data; print warning message in red.
                    messageLabel.textColor = new Color32(255, 0, 0, 255);
                    messageLabel.text = Translations.Translate("RPR_ERR_INV");
                    messageLabel.isVisible = true;
                }
            }
            else
            {
                // Population override checkbox wasn't checked; remove any custom settings.
                PopData.instance.DeleteOverride(currentSelection);

                // Remove any legacy file settings to avoid conflicts.
                OverrideUtils.RemoveResidential(currentSelection);
                OverrideUtils.RemoveWorker(currentSelection);
            }

            // Are we doing floor overrides?
            if (floorCheck.isChecked)
            {
                // Attempt to parse values into override floor pack.
                FloorDataPack overrideFloors = TryParseFloors();

                // Were we successful?.
                if (overrideFloors != null)
                {
                    // Successful parsing - add override.
                    FloorData.instance.SetOverride(currentSelection, overrideFloors);

                    // Save configuration.
                    ConfigUtils.SaveSettings();

                    // Update panel override.
                    BuildingDetailsPanel.Panel.OverrideFloors = overrideFloors;

                    // Repopulate fields with parsed values.
                    UpdateFloorTextFields(overrideFloors.firstFloorMin.ToString(), overrideFloors.floorHeight.ToString());
                }
                else
                {
                    // Couldn't parse values; print warning message in red.
                    messageLabel.textColor = new Color32(255, 0, 0, 255);
                    messageLabel.text = Translations.Translate("RPR_ERR_INV");
                    messageLabel.isVisible = true;
                }
            }
            else
            {
                // Floor override checkbox wasn't checked; remove any floor override.
                FloorData.instance.DeleteOverride(currentSelection);
            }

            // Refresh the display so that all panels reflect the updated settings.
            BuildingDetailsPanel.Panel.Refresh();
        }


        /// <summary>
        /// Removes and deletes a custom override.
        /// </summary>
        private void DeleteOverride()
        {
            // Hide message.
            messageLabel.isVisible = false;

            // Don't do anything with invalid entries.
            if (currentSelection == null || currentSelection.name == null)
            {
                return;
            }

            Logging.Message("deleting custom override for ", currentSelection.name);

            // Remove any and all overrides.
            FloorData.instance.DeleteOverride(currentSelection);
            PopData.instance.DeleteOverride(currentSelection);

            // Update panel override.
            BuildingDetailsPanel.Panel.OverrideFloors = null;

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
            BuildingDetailsPanel.Panel.Refresh();
            homeJobsCount.textField.text = string.Empty;
        }


        /// <summary>
        /// Attempts to parse floor data fields into a valid override floor pack.
        /// </summary>
        /// <returns></returns>
        private FloorDataPack TryParseFloors()
        {
            // Attempt to parse fields.
            if (!string.IsNullOrEmpty(firstFloorField.textField.text) && !string.IsNullOrEmpty(floorHeightField.textField.text) && float.TryParse(firstFloorField.textField.text, out float firstFloor) && float.TryParse(floorHeightField.textField.text, out float floorHeight))
            {
                // Success - create new override floor pack with parsed data.
                return new FloorDataPack
                {
                    version = DataVersion.overrideOne,
                    firstFloorMin = firstFloor,
                    floorHeight = floorHeight
                };
            }

            // If we got here, we didn't get a valid parse; return null.
            return null;
        }


        /// <summary>
        /// Adds a textfield with a label to the left.
        /// </summary>
        /// <param name="yPos">Relative y-position of textfield</param>
        /// <param name="key">Translation key for label</param>
        /// <returns></returns>
        private UILabelledTextfield AddLabelledTextfield(float yPos, string key)
        {
            // Create textfield.
            UILabelledTextfield newField = new UILabelledTextfield
            {
                textField = UIControls.AddTextField(this, MarginPadding + LabelWidth + MarginPadding, yPos, width: this.width - (MarginPadding * 3) - LabelWidth)
            };
            newField.textField.clipChildren = false;

            // Label.
            newField.label = newField.textField.AddUIComponent<UILabel>();
            newField.label.anchor = UIAnchorStyle.Right | UIAnchorStyle.CenterVertical;
            newField.label.relativePosition = new Vector2(-MarginPadding * 2f, newField.textField.height / 2);
            newField.label.textAlignment = UIHorizontalAlignment.Right;
            newField.label.textScale = 0.7f;
            newField.label.text = Translations.Translate(key);

            return newField;
        }


        /// <summary>
        /// Updates floor override textfield values without triggering event handler.
        /// </summary>
        /// <param name="firstFloorText">Text for first floor height field</param>
        /// <param name="floorText">Text for other floor height field</param>
        private void UpdateFloorTextFields(string firstFloorText, string floorText)
        {
            // Populate fields.
            firstFloorField.textField.text = firstFloorText;
            floorHeightField.textField.text = floorText;
        }
    }
}