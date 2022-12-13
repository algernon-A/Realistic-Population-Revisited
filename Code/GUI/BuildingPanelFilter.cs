// <copyright file="BuildingPanelFilter.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Panel containing filtering mechanisms (category buttons, name search) for the building list.
    /// </summary>
    internal class BuildingPanelFilter : UIPanel
    {
        /// <summary>
        /// Relative X-position for the 'has override' checkbox.
        /// </summary>
        internal const float HasOverrideX = AnyX + FilterSpacing;

        /// <summary>
        /// Relative X-position for the 'has non-default' checkbox.
        /// </summary>
        internal const float HasNonDefaultX = HasOverrideX + FilterSpacing;

        // Layout constants - private.
        private const float FilterSpacing = 25f;
        private const float AnyX = 335f;

        // ItemClass ServiceClass services for each toggle.
        private readonly ItemClass.Service[] _serviceMapping =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Office,
            ItemClass.Service.Office,
            ItemClass.Service.Office,
            ItemClass.Service.Office,
            ItemClass.Service.Industrial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Residential,
            ItemClass.Service.Education,
        };

        // ItemClass ServiceClass services for each toggle.
        private readonly ItemClass.SubService[] _subServiceMapping =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.ResidentialWallToWall,
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.CommercialWallToWall,
            ItemClass.SubService.OfficeGeneric,
            ItemClass.SubService.OfficeWallToWall,
            ItemClass.SubService.OfficeFinancial,
            ItemClass.SubService.OfficeHightech,
            ItemClass.SubService.None,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.None,
        };

        // Atlas that each icon sprite comes from.
        private readonly string[] _atlases =
        {
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Thumbnails",
            "Ingame",
        };

        // Icon sprite enabled names.
        private readonly string[] _spriteNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "DistrictSpecializationResidentialWallToWall",
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "DistrictSpecializationCommercialWallToWall",
            "ZoningOffice",
            "DistrictSpecializationOfficeWallToWall",
            "DistrictSpecializationOfficeFinancial",
            "DistrictSpecializationHightech",
            "ZoningIndustrial",
            "DistrictSpecializationTourist",
            "DistrictSpecializationLeisure",
            "DistrictSpecializationOrganic",
            "DistrictSpecializationSelfsufficient",
            "ToolbarIconEducation",
        };

        // Icon sprite tooltips.
        private readonly string[] _tooltips =
        {
            "RPR_CAT_RLO",
            "RPR_CAT_RHI",
            "RPR_CAT_RW2",
            "RPR_CAT_CLO",
            "RPR_CAT_CHI",
            "RPR_CAT_CW2",
            "RPR_CAT_OFF",
            "RPR_CAT_OW2",
            "RPR_CAT_FIN",
            "RPR_CAT_ITC",
            "RPR_CAT_IND",
            "RPR_CAT_TOU",
            "RPR_CAT_LEI",
            "RPR_CAT_ORG",
            "RPR_CAT_SSH",
            "RPR_CAT_SCH",
        };

        // Filter checkbox tooltips.
        private readonly string[] _filterTooltipKeys =
        {
            "RPR_FTR_ANY",
            "RPR_FTR_OVR",
            "RPR_FTR_NDC",
        };

        // Panel components.
        private readonly UICheckBox[] _categoryToggles;
        private readonly UICheckBox[] _settingsFilter;
        private readonly UIButton _allCategories;
        private readonly UITextField _nameFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingPanelFilter"/> class.
        /// </summary>
        internal BuildingPanelFilter()
        {
            m_AutoLayout = false;
            width = BuildingDetailsPanel.FilterWidth;
            height = BuildingDetailsPanel.FilterHeight;

            // Catgegory buttons.
            _categoryToggles = new UICheckBox[(int)BuildingCategories.NumCategories];

            for (int i = 0; i < (int)BuildingCategories.NumCategories; ++i)
            {
                // Basic setup.
                _categoryToggles[i] = UICheckBoxes.AddIconToggle(this, 40 * i, 0f, _atlases[i], _spriteNames[i], _spriteNames[i] + "Disabled", tooltip: Translations.Translate(_tooltips[i]));
                _categoryToggles[i].isChecked = true;
                _categoryToggles[i].readOnly = true;

                // Single click event handler - toggle state of this button.
                _categoryToggles[i].eventClick += (c, p) =>
                {
                    // If either shift or control is NOT held down, deselect all other toggles.
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    {
                        for (int j = 0; j < (int)BuildingCategories.NumCategories; j++)
                        {
                            _categoryToggles[j].isChecked = false;
                        }
                    }

                    // Select this toggle.
                    ((UICheckBox)c).isChecked = true;

                    // Trigger an update.
                    EventFilteringChanged(this, 0);
                };
            }

            // 'All categories' button.
            _allCategories = UIButtons.AddButton(this, (40 * (int)BuildingCategories.NumCategories) + 10f, 0f, Translations.Translate("RPR_CAT_ALL"), 200f);

            // All categories event handler.
            _allCategories.eventClick += (c, p) =>
            {
                // Select all category toggles.
                for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
                {
                    _categoryToggles[i].isChecked = true;
                }

                // Trigger an update.
                EventFilteringChanged(this, 0);
            };

            // Name filter.
            _nameFilter = UITextFields.AddBigLabelledTextField(this, width - 200f, 0, Translations.Translate("RPR_FIL_NAME"));

            // Name filter event handling - update on any change.
            _nameFilter.eventTextChanged += (c, text) => EventFilteringChanged(this, 5);
            _nameFilter.eventTextSubmitted += (c, text) => EventFilteringChanged(this, 5);

            // Settings filter label.
            UILabel filterLabel = SettingsFilterLabel(55f, Translations.Translate("RPR_FIL_SET"));

            // Settings filter checkboxes.
            _settingsFilter = new UICheckBox[(int)FilterCategories.NumCategories];
            for (int i = 0; i < (int)FilterCategories.NumCategories; ++i)
            {
                _settingsFilter[i] = this.AddUIComponent<UICheckBox>();
                _settingsFilter[i].width = 20f;
                _settingsFilter[i].height = 20f;
                _settingsFilter[i].clipChildren = true;
                _settingsFilter[i].relativePosition = new Vector2(AnyX + (FilterSpacing * i), 45f);

                // Checkbox sprites.
                UISprite sprite = _settingsFilter[i].AddUIComponent<UISprite>();
                sprite.spriteName = "ToggleBase";
                sprite.size = new Vector2(20f, 20f);
                sprite.relativePosition = Vector2.zero;

                _settingsFilter[i].checkedBoxObject = sprite.AddUIComponent<UISprite>();
                ((UISprite)_settingsFilter[i].checkedBoxObject).spriteName = "ToggleBaseFocused";
                _settingsFilter[i].checkedBoxObject.size = new Vector2(20f, 20f);
                _settingsFilter[i].checkedBoxObject.relativePosition = Vector2.zero;

                // Tooltip.
                _settingsFilter[i].tooltip = Translations.Translate(_filterTooltipKeys[i]);

                // Special event handling for 'any' checkbox.
                if (i == (int)FilterCategories.Any)
                {
                    _settingsFilter[i].eventCheckChanged += (c, isChecked) =>
                    {
                        if (isChecked)
                        {
                            // Unselect all other checkboxes if 'any' is checked.
                            _settingsFilter[(int)FilterCategories.HasOverride].isChecked = false;
                            _settingsFilter[(int)FilterCategories.HasNonDefault].isChecked = false;
                        }
                    };
                }
                else
                {
                    // Non-'any' checkboxes.
                    // Unselect 'any' checkbox if any other is checked.
                    _settingsFilter[i].eventCheckChanged += (c, isChecked) =>
                    {
                        if (isChecked)
                        {
                            _settingsFilter[0].isChecked = false;
                        }
                    };
                }

                // Trigger filtering changed event if any checkbox is changed.
                _settingsFilter[i].eventCheckChanged += (c, isChecked) => { EventFilteringChanged(this, 0); };
            }
        }

        /// <summary>
        /// Event generated when filter settings change.
        /// </summary>
        internal event PropertyChangedEventHandler<int> EventFilteringChanged;

        /// <summary>
        /// Index numbers for building category filter buttons.
        /// </summary>
        internal enum BuildingCategories
        {
            /// <summary>
            /// No category.
            /// </summary>
            None = -1,

            /// <summary>
            /// Low-density residential buildings.
            /// </summary>
            ResidentialLow,

            /// <summary>
            /// High-density residential buildings.
            /// </summary>
            ResidentialHigh,

            /// <summary>
            /// Wall-to-wall residential buildings.
            /// </summary>
            ResidentialWallToWall,

            /// <summary>
            /// Low-density commercial buildings.
            /// </summary>
            CommercialLow,

            /// <summary>
            /// High-density commercial buildings.
            /// </summary>
            CommercialHigh,

            /// <summary>
            /// Wall-to-wall commercial buildings.
            /// </summary>
            CommercialWallToWall,

            /// <summary>
            /// Generic office  buildings.
            /// </summary>
            OfficeGeneric,

            /// <summary>
            /// Wall-to-wall office buildings.
            /// </summary>
            OfficeWallToWall,

            /// <summary>
            /// Financial office buildings.
            /// </summary>
            OfficeFinancial,

            /// <summary>
            /// IT cluster (hightech office)  buildings.
            /// </summary>
            OfficeHighTech,

            /// <summary>
            /// Industrial buildings.
            /// </summary>
            Industrial,

            /// <summary>
            /// Tourism buildings.
            /// </summary>
            Tourism,

            /// <summary>
            /// Leisure buildings.
            /// </summary>
            Leisure,

            /// <summary>
            /// Eco commercial buildings (organic and local produce).
            /// </summary>
            Organic,

            /// <summary>
            /// Eco residential buildings (self-sufficient).
            /// </summary>
            Selfsufficient,

            /// <summary>
            /// Education buildings.
            /// </summary>
            Education,

            /// <summary>
            /// Number of building categories.
            /// </summary>
            NumCategories,
        }

        /// <summary>
        /// Index numbers for attribute filters.
        /// </summary>
        internal enum FilterCategories
        {
            /// <summary>
            /// No filter.
            /// </summary>
            Any = 0,

            /// <summary>
            /// Buildings with custom overrides.
            /// </summary>
            HasOverride,

            /// <summary>
            /// Buildings with non-default calculation packs.
            /// </summary>
            HasNonDefault,

            /// <summary>
            /// Number of filter categories.
            /// </summary>
            NumCategories,
        }

        /// <summary>
        /// Gets the filter by settings checkbox array.
        /// </summary>
        internal UICheckBox[] SettingsFilter => _settingsFilter;

        /// <summary>
        /// Gets the category toggles checkbox array.
        /// </summary>
        internal UICheckBox[] CategoryToggles => _categoryToggles;

        /// <summary>
        /// Gets or sets the name filter text.
        /// </summary>
        internal string NameFilterText { get => _nameFilter.text; set => _nameFilter.text = value; }

        /// <summary>
        /// Sets the category toggles so that the one that includes this building is on, and the rest are off.
        /// </summary>
        /// <param name="buildingClass">ItemClass of the building (to match toggle categories).</param>
        internal void SelectBuildingCategory(ItemClass buildingClass)
        {
            for (int i = 0; i < (int)BuildingCategories.NumCategories; ++i)
            {
                if (_subServiceMapping[i] == ItemClass.SubService.None && buildingClass.m_service == _serviceMapping[i])
                {
                    _categoryToggles[i].isChecked = true;
                }
                else if (buildingClass.m_subService == _subServiceMapping[i])
                {
                    _categoryToggles[i].isChecked = true;
                }
                else if (buildingClass.m_subService == ItemClass.SubService.ResidentialHighEco && _subServiceMapping[i] == ItemClass.SubService.ResidentialLowEco)
                {
                    _categoryToggles[i].isChecked = true;
                }
                else
                {
                    _categoryToggles[i].isChecked = false;
                }
            }
        }

        /// <summary>
        /// Returns the current filter state as a boolean array.
        /// </summary>
        /// <returns>Current filter state.</returns>
        internal bool[] GetFilter()
        {
            // Stores category toggle states and settings filter states, in that order.
            bool[] filterState = new bool[(int)BuildingCategories.NumCategories + (int)FilterCategories.NumCategories];

            // Iterate through all toggle states and add them to return array.
            for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
            {
                filterState[i] = _categoryToggles[i].isChecked;
            }

            // Iterate through all settings filter states and add them to return array, after the toggle states.
            for (int i = 0; i < (int)FilterCategories.NumCategories; i++)
            {
                filterState[i + (int)BuildingCategories.NumCategories] = _settingsFilter[i].isChecked;
            }

            return filterState;
        }

        /// <summary>
        /// Sets the current filter configuration from provided boolean array.
        /// </summary>
        /// <param name="filterState">Filter state to apply.</param>
        internal void SetFilter(bool[] filterState)
        {
            // Set toggle states from array.
            for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
            {
                _categoryToggles[i].isChecked = filterState[i];
            }

            // Set settings filter states from array (appended after category toggle states).
            for (int i = 0; i < (int)FilterCategories.NumCategories; i++)
            {
                _settingsFilter[i].isChecked = filterState[i + (int)BuildingCategories.NumCategories];
            }
        }

        /// <summary>
        /// Adds a filter label.
        /// </summary>
        /// <param name="yPos">Relative centre Y position of label.</param>
        /// <param name="text">Label text.</param>
        /// <returns>New label.</returns>
        private UILabel SettingsFilterLabel(float yPos, string text)
        {
            // Basic setup.
            UILabel newLabel = this.AddUIComponent<UILabel>();
            newLabel.textScale = 0.8f;
            newLabel.autoSize = true;
            newLabel.wordWrap = false;

            // Assign text.
            newLabel.text = text;

            // Set relative position.
            newLabel.relativePosition = new Vector2(10f, yPos - (newLabel.height / 2f));

            return newLabel;
        }
    }
}