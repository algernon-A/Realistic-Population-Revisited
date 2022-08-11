// <copyright file="UIBuildingRow.cs" company="algernon (K. Algernon A. Sheppard)">
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
    /// UIListRow for building prefab selection.
    /// </summary>
    public class UIBuildingRow : UIListRow
    {
        // Panel components.
        private UILabel _buildingName;
        private BuildingInfo _thisBuilding;
        private UISprite _hasOverride;
        private UISprite _hasNonDefault;

        /// <summary>
        /// Generates and displays a list row.
        /// </summary>
        /// <param name="data">Object data to display.</param>
        /// <param name="rowIndex">Row index number (for background banding).</param>
        public override void Display(object data, int rowIndex)
        {
            // Perform initial setup for new rows.
            if (_buildingName == null)
            {
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = parent.width;
                height = RowHeight;

                _buildingName = AddUIComponent<UILabel>();
                _buildingName.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
                _buildingName.relativePosition = new Vector2(RowHeight / 2f, 5f);
                _buildingName.width = 200;

                // Checkboxes to indicate which items have custom settings.
                _hasOverride = AddSettingsCheck(UIBuildingFilter.HasOverrideX, "RPR_FTR_OVR");
                _hasNonDefault = AddSettingsCheck(UIBuildingFilter.HasNonDefaultX, "RPR_FTR_NDC");
            }

            // Set selected building.
            _thisBuilding = data as BuildingInfo;
            string thisBuildingName = _thisBuilding.name;
            _buildingName.text = UIBuildingDetails.GetDisplayName(thisBuildingName);

            // Update 'has override' check to correct state.
            if (PopData.instance.GetOverride(thisBuildingName) != 0 || FloorData.instance.HasOverride(thisBuildingName) != null)
            {
                // Override found.
                _hasOverride.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No override.
                _hasOverride.spriteName = "AchievementCheckedFalse";
            }

            // Update 'has non-default calculation pack' check to correct state.
            if (PopData.instance.HasPackOverride(thisBuildingName) != null
                || (FloorData.instance.HasPackOverride(thisBuildingName) != null && FloorData.instance.HasOverride(thisBuildingName) == null)
                || SchoolData.instance.HasPackOverride(thisBuildingName) != null)
            {
                // Non-default calculation found.
                _hasNonDefault.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No non-default packs.
                _hasNonDefault.spriteName = "AchievementCheckedFalse";
            }

            // Set initial background as deselected state.
            Deselect(rowIndex);
        }

        /// <summary>
        /// Adds a settings check to the current row.
        /// </summary>
        /// <param name="xPos">Check relative x-position</param>
        /// <param name="translationKey">Tooltip translation key</param>
        /// <returns></returns>
        private UISprite AddSettingsCheck(float xPos, string translationKey)
        {
            UISprite newSprite = AddUIComponent<UISprite>();
            newSprite.size = new Vector2(20, 20);
            newSprite.relativePosition = new Vector2(xPos, 5f);
            newSprite.tooltip = Translations.Translate(translationKey);

            return newSprite;
        }
    }
}