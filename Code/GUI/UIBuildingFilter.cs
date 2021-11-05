using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Index numbers for building category filter buttons.
    /// </summary>
    public enum BuildingCategories
    {
        None = -1,
        ResidentialLow,
        ResidentialHigh,
        CommercialLow,
        CommercialHigh,
        Office,
        Industrial,
        Tourism,
        Leisure,
        Organic,
        Selfsufficient,
        Education,
        NumCategories
    }

    /// <summary>
    /// Index numbers for attribute filters.
    /// </summary>
    public enum FilterCategories
    {
        Any = 0,
        HasOverride,
        HasNonDefault,
        NumCategories
    }


    /// <summary>
    /// Building filter category buttons.
    /// </summary>
    public class CategoryIcons
    {
        // ItemClass ServiceClass services for each toggle.
        public static readonly ItemClass.Service[] serviceMapping =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Office,
            ItemClass.Service.Industrial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Commercial,
            ItemClass.Service.Residential,
            ItemClass.Service.Education
        };

        // ItemClass ServiceClass services for each toggle.
        public static readonly ItemClass.SubService[] subServiceMapping =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.CommercialLow,
            ItemClass.SubService.CommercialHigh,
            ItemClass.SubService.None,
            ItemClass.SubService.None,
            ItemClass.SubService.CommercialTourist,
            ItemClass.SubService.CommercialLeisure,
            ItemClass.SubService.CommercialEco,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.None
        };


        // Atlas that each icon sprite comes from.
        public static readonly string[] atlases = { "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Thumbnails", "Ingame" };

        // Icon sprite enabled names.
        public static readonly string[] spriteNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "ZoningCommercialLow",
            "ZoningCommercialHigh",
            "ZoningOffice",
            "ZoningIndustrial",
            "DistrictSpecializationTourist",
            "DistrictSpecializationLeisure",
            "DistrictSpecializationOrganic",
            "DistrictSpecializationSelfsufficient",
            "ToolbarIconEducation"
        };

        // Icon sprite disnabled names.
        public static readonly string[] disabledSpriteNames =
        {
            "ZoningResidentialLowDisabled",
            "ZoningResidentialHighDisabled",
            "ZoningCommercialLowDisabled",
            "ZoningCommercialHighDisabled",
            "ZoningOfficeDisabled",
            "ZoningIndustrialDisabled",
            "IconPolicyTourist",
            "IconPolicyLeisure",
            "IconPolicyOrganic",
            "IconPolicySelfsufficient",
            "ToolbarIconEducationDisabled"
        };

        // Icon sprite tooltips.
        public static readonly string[] tooltips =
        {
            "RPR_CAT_RLO",
            "RPR_CAT_RHI",
            "RPR_CAT_CLO",
            "RPR_CAT_CHI",
            "RPR_CAT_OFF",
            "RPR_CAT_IND",
            "RPR_CAT_TOU",
            "RPR_CAT_LEI",
            "RPR_CAT_ORG",
            "RPR_CAT_SSH",
            "RPR_CAT_SCH"
        };
    }


    /// <summary>
    /// Panel containing filtering mechanisms (category buttons, name search) for the building list.
    /// </summary>
    public class UIBuildingFilter : UIPanel
    {
        // Layout constants.
        internal const float FilterSpacing = 25f;
        internal const float AnyX = 335f;
        internal const float HasOverrideX = AnyX + FilterSpacing;
        internal const float HasNonDefaultX = HasOverrideX + FilterSpacing;

        // Panel components.
        internal UICheckBox[] categoryToggles, settingsFilter;
        internal UIButton allCategories;
        internal UITextField nameFilter;


        // FIlter by settings checkboxes.
        internal UICheckBox[] SettingsFilter => settingsFilter;


        // Basic event handler for filtering changes.
        public event PropertyChangedEventHandler<int> EventFilteringChanged;

        // Filter checkbox tooltips.
        private readonly string[] FilterTooltipKeys = { "RPR_FTR_ANY", "RPR_FTR_OVR", "RPR_FTR_NDC" };


        /// <summary>
        /// Set up filter bar.
        /// We don't use Start() here as we need to access the category toggle states to set up the initial filtering list before Start() is called by UnityEngine.
        /// </summary>
        public void Setup()
        {
            // Catgegory buttons.
            categoryToggles = new UICheckBox[(int)BuildingCategories.NumCategories];

            for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
            {
                // Basic setup.
                categoryToggles[i] = UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i] + "Disabled");
                categoryToggles[i].tooltip = Translations.Translate(CategoryIcons.tooltips[i]);
                categoryToggles[i].relativePosition = new Vector3(40 * i, 0);
                categoryToggles[i].isChecked = true;
                categoryToggles[i].readOnly = true;

                // Single click event handler - toggle state of this button.
                categoryToggles[i].eventClick += (c, p) =>
                {
                    // If either shift or control is NOT held down, deselect all other toggles.
                    if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
                    {
                        for (int j = 0; j < (int)BuildingCategories.NumCategories; j++)
                        {
                            categoryToggles[j].isChecked = false;
                        }
                    }

                    // Select this toggle.
                    ((UICheckBox)c).isChecked = true;

                    // Trigger an update.
                    EventFilteringChanged(this, 0);
                };
            }

            // 'All categories' button.
            allCategories = UIControls.AddButton(this, 445f, 0f, Translations.Translate("RPR_CAT_ALL"), 200f);

            // All categories event handler.
            allCategories.eventClick += (c, p) =>
            {
                // Select all category toggles.
                for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
                {
                    categoryToggles[i].isChecked = true;
                }

                // Trigger an update.
                EventFilteringChanged(this, 0);
            };

            // Name filter.
            nameFilter = UIControls.BigLabelledTextField(this, width - 200f, 0, Translations.Translate("RPR_FIL_NAME"));

            // Name filter event handling - update on any change.
            nameFilter.eventTextChanged += (control, text) => EventFilteringChanged(this, 5);
            nameFilter.eventTextSubmitted += (control, text) => EventFilteringChanged(this, 5);

            // Settings filter label.
            UILabel filterLabel = SettingsFilterLabel(55f, Translations.Translate("RPR_FIL_SET"));

            // Settings filter checkboxes.
            settingsFilter = new UICheckBox[(int)FilterCategories.NumCategories];
            for (int i = 0; i < (int)FilterCategories.NumCategories; ++i)
            {
                settingsFilter[i] = this.AddUIComponent<UICheckBox>();
                settingsFilter[i].width = 20f;
                settingsFilter[i].height = 20f;
                settingsFilter[i].clipChildren = true;
                settingsFilter[i].relativePosition = new Vector3(AnyX + (FilterSpacing * i), 45f);

                // Checkbox sprites.
                UISprite sprite = settingsFilter[i].AddUIComponent<UISprite>();
                sprite.spriteName = "ToggleBase";
                sprite.size = new Vector2(20f, 20f);
                sprite.relativePosition = Vector3.zero;

                settingsFilter[i].checkedBoxObject = sprite.AddUIComponent<UISprite>();
                ((UISprite)settingsFilter[i].checkedBoxObject).spriteName = "ToggleBaseFocused";
                settingsFilter[i].checkedBoxObject.size = new Vector2(20f, 20f);
                settingsFilter[i].checkedBoxObject.relativePosition = Vector3.zero;

                // Tooltip.
                settingsFilter[i].tooltip = Translations.Translate(FilterTooltipKeys[i]);

                // Special event handling for 'any' checkbox.
                if (i == (int)FilterCategories.Any)
                {
                    settingsFilter[i].eventCheckChanged += (control, isChecked) =>
                    {
                        if (isChecked)
                        {
                            // Unselect all other checkboxes if 'any' is checked.
                            settingsFilter[(int)FilterCategories.HasOverride].isChecked = false;
                            settingsFilter[(int)FilterCategories.HasNonDefault].isChecked = false;
                        }
                    };
                }
                else
                {
                    // Non-'any' checkboxes.
                    // Unselect 'any' checkbox if any other is checked.
                    settingsFilter[i].eventCheckChanged += (control, isChecked) =>
                    {
                        if (isChecked)
                        {
                            settingsFilter[0].isChecked = false;
                        }
                    };
                }

                // Trigger filtering changed event if any checkbox is changed.
                settingsFilter[i].eventCheckChanged += (control, isChecked) => { EventFilteringChanged(this, 0); };
            }
        }


        /// <summary>
        /// Sets the category toggles so that the one that includes this building is on, and the rest are off
        /// </summary>
        /// <param name="buildingClass">ItemClass of the building (to match toggle categories)</param>
        public void SelectBuildingCategory(ItemClass buildingClass)
        {
            for (int i = 0; i < (int)BuildingCategories.NumCategories; i ++)
            {
                if (CategoryIcons.subServiceMapping[i] == ItemClass.SubService.None && buildingClass.m_service == CategoryIcons.serviceMapping[i])
                {
                    categoryToggles[i].isChecked = true;
                }
                else if (buildingClass.m_subService == CategoryIcons.subServiceMapping[i])
                {
                    categoryToggles[i].isChecked = true;
                }
                else if (buildingClass.m_subService == ItemClass.SubService.ResidentialHighEco && CategoryIcons.subServiceMapping[i] == ItemClass.SubService.ResidentialLowEco)
                {
                    categoryToggles[i].isChecked = true;
                }
                else
                {
                    categoryToggles[i].isChecked = false;
                }
            }
        }


        /// <summary>
        /// Returns the current filter state as a boolean array.
        /// </summary>
        /// <returns>Current filter state</returns>
        internal bool[] GetFilter()
        {
            // Stores category toggle states and settings filter states, in that order.
            bool[] filterState = new bool[(int)BuildingCategories.NumCategories + (int)FilterCategories.NumCategories];

            // Iterate through all toggle states and add them to return array.
            for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
            {
                filterState[i] = categoryToggles[i].isChecked;
            }

            // Iterate through all settings filter states and add them to return array, after the toggle states.
            for (int i = 0; i < (int)FilterCategories.NumCategories; i++)
            {
                filterState[i + (int)BuildingCategories.NumCategories] = settingsFilter[i].isChecked;
            }

            return filterState;
        }


        /// <summary>
        /// Sets the current filter configuration from provided boolean array.
        /// </summary>
        /// <param name="filterState">Filter state to apply</param>
        internal void SetFilter(bool[] filterState)
        {
            // Set toggle states from array.
            for (int i = 0; i < (int)BuildingCategories.NumCategories; i++)
            {
                categoryToggles[i].isChecked = filterState[i];
            }

            // Set settings filter states from array (appended after category toggle states).
            for (int i = 0; i < (int)FilterCategories.NumCategories; i++)
            {
                settingsFilter[i].isChecked = filterState[i + (int)BuildingCategories.NumCategories];
            }
        }


        /// <summary>
        /// Adds a filter label.
        /// </summary>
        /// <param name="yPos">Relative centre Y position of label</param>
        /// <param name="text">Label text</param>
        /// <returns>New label</returns>
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
            newLabel.relativePosition = new Vector3(10f, yPos - (newLabel.height / 2f), 0);

            return newLabel;
        }
    }
}