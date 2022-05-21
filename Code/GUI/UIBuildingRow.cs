using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// An individual row in the list of buildings.
    /// </summary>
    public class UIBuildingRow : UIPanel, IUIFastListRow
    {
        // Height of each row.
        private const float rowHeight = 30f;

        // Panel components.
        private UIPanel panelBackground;
        private UILabel buildingName;
        private BuildingInfo thisBuilding;
        private UISprite hasOverride, hasNonDefault;


        // Background for each list item.
        public UIPanel Background
        {
            get
            {
                if (panelBackground == null)
                {
                    panelBackground = AddUIComponent<UIPanel>();
                    panelBackground.width = width;
                    panelBackground.height = rowHeight;
                    panelBackground.relativePosition = Vector2.zero;

                    panelBackground.zOrder = 0;
                }

                return panelBackground;
            }
        }


        /// <summary>
        /// Called when dimensions are changed, including as part of initial setup (required to set correct relative position of label).
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (buildingName != null)
            {
                Background.width = width;
                buildingName.relativePosition = new Vector2(rowHeight / 2f, 5f);
            }
        }


        /// <summary>
        /// Mouse click event handler - updates the selected building to what was clicked.
        /// </summary>
        /// <param name="p">Mouse event parameter</param>
        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
            BuildingDetailsPanel.Panel.UpdateSelectedBuilding(thisBuilding);
        }


        /// <summary>
        /// Generates and displays a building row.
        /// </summary>
        /// <param name="data">Object to list</param>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Display(object data, bool isRowOdd)
        {
            // Perform initial setup for new rows.
            if (buildingName == null)
            {
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = parent.width;
                height = rowHeight;

                buildingName = AddUIComponent<UILabel>();
                buildingName.anchor = UIAnchorStyle.Left | UIAnchorStyle.CenterVertical;
                buildingName.relativePosition = new Vector2(rowHeight / 2f, 5f);
                buildingName.width = 200;

                // Checkboxes to indicate which items have custom settings.
                hasOverride = AddSettingsCheck(UIBuildingFilter.HasOverrideX, "RPR_FTR_OVR");
                hasNonDefault = AddSettingsCheck(UIBuildingFilter.HasNonDefaultX, "RPR_FTR_NDC");
            }

            // Set selected building.
            thisBuilding = data as BuildingInfo;
            string thisBuildingName = thisBuilding.name;
            buildingName.text = UIBuildingDetails.GetDisplayName(thisBuildingName);

            // Update 'has override' check to correct state.
            if (PopData.instance.GetOverride(thisBuildingName) != 0 || FloorData.instance.HasOverride(thisBuildingName) != null)
            {
                // Override found.
                hasOverride.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No override.
                hasOverride.spriteName = "AchievementCheckedFalse";
            }


            // Update 'has non-default calculation pack' check to correct state.
            if (PopData.instance.HasPackOverride(thisBuildingName) != null || (FloorData.instance.HasPackOverride(thisBuildingName) != null && FloorData.instance.HasOverride(thisBuildingName) == null) || SchoolData.instance.HasPackOverride(thisBuildingName) != null)// || Multipliers.instance.HasOverride(thisBuildingName))
            {
                // Non-default calculation found.
                hasNonDefault.spriteName = "AchievementCheckedTrue";
            }
            else
            {
                // No non-default packs.
                hasNonDefault.spriteName = "AchievementCheckedFalse";
            }

            // Set initial background as deselected state.
            Deselect(isRowOdd);
        }


        /// <summary>
        /// Highlights the selected row.
        /// </summary>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Select(bool isRowOdd)
        {
            Background.backgroundSprite = "ListItemHighlight";
            Background.color = new Color32(255, 255, 255, 255);
        }


        /// <summary>
        /// Unhighlights the (un)selected row.
        /// </summary>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Deselect(bool isRowOdd)
        {
            if (isRowOdd)
            {
                // Lighter background for odd rows.
                Background.backgroundSprite = "UnlockingItemBackground";
                Background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                // Darker background for even rows.
                Background.backgroundSprite = null;
            }
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