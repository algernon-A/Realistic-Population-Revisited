using System.Text;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;
using System.Collections.Generic;


namespace RealPop2
{
    /// <summary>
    /// Panel to display volumetric calculations.
    /// </summary>
    public class UIVolumetricPanel : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float ColumnWidth = UIBuildingDetails.RightWidth / 2f;
        private const float LabelOffset = 240f;
        private const float LeftColumn = LabelOffset;
        private const float RightColumn = ColumnWidth + LabelOffset;
        private const float RowTopMargin = Margin;
        private const float RowHeight = 20f;
        private const float Row1 = RowTopMargin;
        private const float Row2 = Row1 + RowHeight;
        private const float Row3 = Row2 + RowHeight;
        private const float Row4 = Row3 + RowHeight;
        private const float Row5 = Row4 + RowHeight;
        private const float Row6 = Row5 + RowHeight;
        private const float Row7 = Row6 + RowHeight;
        private const float Row8 = Row7 + RowHeight;
        private const float MessageY = Row8 + RowHeight + Margin;
        private const float FloorListY = MessageY + RowHeight;


        // Panel components.
        private UIFastList floorsList;
        private UILabel numFloorsLabel, floorAreaLabel, visitCountLabel, productionLabel, firstMinLabel, firstExtraLabel, floorHeightLabel;
        private UILabel emptyAreaLabel, emptyPercentLabel, perLabel, unitsLabel;
        private UILabel totalHomesLabel, totalJobsLabel, totalStudentsLabel;
        private UILabel schoolWorkerLabel, costLabel;
        private UILabel overrideFloorsLabel, overridePopLabel;
        private UILabel messageLabel;
        private UICheckBox fixedPopCheckBox, multiFloorCheckBox, ignoreFirstCheckBox;


        private UILabel OverrideLabel(UIComponent parentLabel)
        {
            UILabel thisLabel = UIControls.AddLabel(this, 0f - Margin, 0f, Translations.Translate("RPR_CAL_OVR"), textScale: 0.6f);
            
            thisLabel.relativePosition = new Vector2(parentLabel.relativePosition.x - thisLabel.width - Margin, parentLabel.relativePosition.y + ((parentLabel.height - thisLabel.height) / 2f));

            thisLabel.Hide();

            return thisLabel;
        }

        /// <summary>
        /// Create the panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        internal void Setup()
        {
            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoSize = false;
            width = parent.width;
            height = 395f;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Labels.
            floorHeightLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FLH", RightColumn, Row1, "RPR_CAL_VOL_FLH_TIP");
            firstMinLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FMN", RightColumn, Row2, "RPR_CAL_VOL_FMN_TIP");
            firstExtraLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FMX", RightColumn, Row3, "RPR_CAL_VOL_FMX_TIP");
            numFloorsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FLR", RightColumn, Row5, "RPR_CAL_VOL_FLR_TIP");
            floorAreaLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_TFA", RightColumn, Row6, "RPR_CAL_VOL_TFA_TIP");
            totalHomesLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_HOU", RightColumn, Row7, "RPR_CAL_VOL_UTS_TIP");
            totalJobsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_WOR", RightColumn, Row7, "RPR_CAL_VOL_UTS_TIP");
            totalStudentsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_STU", RightColumn, Row7, "RPR_CAL_VOL_UTS_TIP");
            visitCountLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_VIS", RightColumn, Row8, "RPR_CAL_VOL_VIS_TIP");
            productionLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_PRD", RightColumn, Row8, "RPR_CAL_VOL_PRD_TIP");
            unitsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_UNI", LeftColumn, Row2, "RPR_CAL_VOL_UNI_TIP");
            emptyPercentLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_EPC", LeftColumn, Row2, "RPR_CAL_VOL_EPC_TIP");
            emptyAreaLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_EMP", LeftColumn, Row3, "RPR_CAL_VOL_EMP_TIP");
            perLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_APU", LeftColumn, Row4, "RPR_CAL_VOL_APU_TIP");
            schoolWorkerLabel = AddVolumetricLabel(this, "RPR_CAL_SCH_WKR", LeftColumn, Row7, "RPR_CAL_SCH_WKR_TIP", 20f);
            costLabel = AddVolumetricLabel(this, "RPR_CAL_SCH_CST", LeftColumn, Row8, "RPR_CAL_SCH_CST_TIP", 20f);

            overridePopLabel = OverrideLabel(totalHomesLabel);
            overrideFloorsLabel = OverrideLabel(numFloorsLabel);

            // Intially hidden (just to avoid ugliness if no building is selected).
            totalHomesLabel.Hide();
            totalStudentsLabel.Hide();

            // Fixed population checkbox.
            fixedPopCheckBox = CalcCheckBox(this, "RPR_CAL_VOL_FXP", LeftColumn, Row1, "RPR_CAL_VOL_FXP_TIP");
            fixedPopCheckBox.isInteractive = false;
            fixedPopCheckBox.Disable();

            // Multi-floor units checkbox.
            multiFloorCheckBox = CalcCheckBox(this, "RPR_CAL_VOL_MFU", LeftColumn, Row5, "RPR_CAL_VOL_MFU_TIP");
            multiFloorCheckBox.isInteractive = false;
            multiFloorCheckBox.Disable();

            // Ignore first floor checkbox.
            ignoreFirstCheckBox = CalcCheckBox(this, "RPR_CAL_VOL_IGF", RightColumn, Row4, "RPR_CAL_VOL_IGF_TIP");
            ignoreFirstCheckBox.isInteractive = false;
            ignoreFirstCheckBox.Disable();

            // Message label.
            messageLabel = UIControls.AddLabel(this, Margin, MessageY, string.Empty);

            // Floor list - attached to root panel as scrolling and interactivity can be unreliable otherwise.
            floorsList = UIFastList.Create<UIFloorRow>(BuildingDetailsPanel.Panel);

            // Size, appearance and behaviour.
            floorsList.backgroundSprite = "UnlockingPanel";
            floorsList.width = this.width;
            floorsList.isInteractive = true;
            floorsList.canSelect = false;
            floorsList.rowHeight = 20;
            floorsList.autoHideScrollbar = true; ;
            ResetFloorListPosition();

            // Data.
            floorsList.rowsData = new FastList<object>();
            floorsList.selectedIndex = -1;

            // Toggle floorsList visibility on this panel's visibility change (because floorsList is attached to root panel).
            this.eventVisibilityChanged += (control, isVisible) => floorsList.isVisible = isVisible;
        }


        /// <summary>
        /// Updates the population summary text labels with data from the current level.
        /// </summary>
        /// <param name="levelData">LevelData record to summarise</param>
        internal void UpdatePopText(LevelData levelData)
        {
            // Update and display text labels depending on 'use fixed pop' setting.
            bool fixedPop = levelData.areaPer < 0;
            fixedPopCheckBox.isChecked = fixedPop;
            emptyAreaLabel.isVisible = !fixedPop;
            emptyPercentLabel.isVisible = !fixedPop;
            perLabel.isVisible = !fixedPop;
            multiFloorCheckBox.isVisible = !fixedPop;
            unitsLabel.isVisible = fixedPop;

            // Set values.
            emptyAreaLabel.text = Measures.AreaString(levelData.emptyArea, "N0");
            emptyPercentLabel.text = levelData.emptyPercent.ToString();
            perLabel.text = Measures.AreaString(levelData.areaPer, "N0");
            unitsLabel.text = Measures.AreaString(levelData.areaPer * -1, "N0");
            multiFloorCheckBox.isChecked = levelData.multiFloorUnits;
        }


        /// <summary>
        /// Updates the floor summary text labels with data from the current floor.
        /// </summary>
        /// <param name="floorData">FloorData record to summarise</param>
        internal void UpdateFloorText(FloorDataPack floorData)
        {
            // Set textfield values.
            firstMinLabel.text = Measures.LengthString(floorData.firstFloorMin, "N1");
            firstExtraLabel.text = Measures.LengthString(floorData.firstFloorExtra, "N1");
            floorHeightLabel.text = Measures.LengthString(floorData.floorHeight, "N1");

            // Set checkbox.
            ignoreFirstCheckBox.isChecked = floorData.firstFloorEmpty;
        }


        /// <summary>
        /// Perform and display volumetric calculations for the currently selected building.
        /// </summary>
        /// <param name="building">Selected building prefab</param>
        /// <param name="levelData">Population (level) calculation data to apply to calculations</param>
        /// <param name="floorData">Floor calculation data to apply to calculations</param>
        /// <param name="schoolData">School calculation data to apply to calculations</param>
        /// <param name="schoolData">Multiplier to apply to calculations</param>
        internal void CalculateVolumetric(BuildingInfo building, LevelData levelData, FloorDataPack floorData, SchoolDataPack schoolData, float multiplier)
        {
            // Safety first!
            if (building == null)
            {
                return;
            }

            // Reset message label.
            messageLabel.text = string.Empty;

            // Perform calculations.
            // Get floors and allocate area an number of floor labels.
            SortedList<int, float> floors = PopData.instance.VolumetricFloors(building.m_generatedInfo, floorData, out float totalArea);
            floorAreaLabel.text = Measures.AreaString(totalArea, "N0");
            numFloorsLabel.text = floors.Count.ToString();

            // Get total units.
            int totalUnits = PopData.instance.VolumetricPopulation(building.m_generatedInfo, levelData, floorData, multiplier, floors, totalArea);

            // Floor labels list.
            List<string> floorLabels = new List<string>();

            // What we call our units for this building.
            string unitName;
            switch (building.GetService())
            {
                case ItemClass.Service.Residential:
                    // Residential - households.
                    unitName = Translations.Translate("RPR_CAL_UNI_HOU");
                    break;
                case ItemClass.Service.Education:
                    // Education - students.
                    unitName = Translations.Translate("RPR_CAL_UNI_STU");
                    break;
                default:
                    // Default - workplaces.
                    unitName = Translations.Translate("RPR_CAL_UNI_WOR");
                    break;
            }

            // See if we're using area calculations for numbers of units, i.e. areaPer is at least one.
            if (levelData.areaPer > 0)
            {
                // Determine area percentage to use for calculations (inverse of empty area percentage).
                float areaPercent = 1 - (levelData.emptyPercent / 100f);

                // Create new floor area labels by iterating through each floor.
                for (int i = 0; i < floors.Count; ++i)
                {
                    // StringBuilder, because we're doing a fair bit of manipulation here.
                    StringBuilder floorString = new StringBuilder("Floor ");

                    // Floor number.
                    floorString.Append(i + 1);
                    floorString.Append(" " + Translations.Translate("RPR_CAL_VOL_ARA") + " ");
                    floorString.Append(Measures.AreaString(floors[i], "N0"));

                    // See if we're calculating units per individual floor.
                    if (!levelData.multiFloorUnits)
                    {
                        // Number of units on this floor - always rounded down.
                        int floorUnits = (int)((floors[i] * areaPercent) / levelData.areaPer);
                        // Adjust by multiplier (after rounded calculation above).
                        floorUnits = (int)(floorUnits * multiplier);

                        // Add extra info to label.
                        floorString.Append(" (");
                        floorString.Append(floorUnits.ToString("N0"));
                        floorString.Append(" ");
                        floorString.Append(unitName);
                        floorString.Append(")");
                    }

                    // Add new floor label item with results for this calculation.
                    floorLabels.Add(floorString.ToString());
                }
            }

            // Do we have a current school selection, and are we using school property overrides?
            if (schoolData != null && ModSettings.enableSchoolProperties)
            {
                // Yes - calculate and display school worker breakdown.
                int[] workers = SchoolData.instance.CalcWorkers(schoolData, totalUnits);
                schoolWorkerLabel.Show();
                schoolWorkerLabel.text = workers[0] + " / " + workers[1] + " / " + workers[2] + " / " + workers[3];

                // Calculate construction cost to display.
                int cost = SchoolData.instance.CalcCost(schoolData, totalUnits);
                ColossalFramework.Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref cost, building.m_class.m_service, building.m_class.m_subService, building.m_class.m_level);

                // Calculate maintenance cost to display.
                int maintenance = SchoolData.instance.CalcMaint(schoolData, totalUnits) * 100;
                ColossalFramework.Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetMaintenanceCost(ref maintenance, building.m_class.m_service, building.m_class.m_subService, building.m_class.m_level);
                float displayMaint = Mathf.Abs(maintenance * 0.0016f);

                // And display school cost breakdown.
                costLabel.Show();
                costLabel.text = cost.ToString((!(displayMaint >= 10f)) ? Settings.moneyFormat : Settings.moneyFormatNoCents, LocaleManager.cultureInfo) + " / " + displayMaint.ToString((!(displayMaint >= 10f)) ? Settings.moneyFormat : Settings.moneyFormatNoCents, LocaleManager.cultureInfo);

                // Enforce school floors list position.
                ResetFloorListPosition();
            }
            else
            {
                // No - hide school worker breakdown and cost labels.
                schoolWorkerLabel.Hide();
                costLabel.Hide();

                // Enforce default floors list position.
                ResetFloorListPosition();
            }

            // Allocate our new list of labels to the floors list (via an interim fastlist to avoid race conditions if we 'build' manually directly into floorsList).
            FastList<object> fastList = new FastList<object>()
            {
                m_buffer = floorLabels.ToArray(),
                m_size = floorLabels.Count
            };
            floorsList.rowsData = fastList;

            // Display total unit calculation result.
            switch (building.GetService())
            {
                case ItemClass.Service.Residential:
                    // Residential building.
                    totalJobsLabel.Hide();
                    totalStudentsLabel.Hide();
                    totalHomesLabel.Show();
                    totalHomesLabel.text = totalUnits.ToString("N0", LocaleManager.cultureInfo);
                    break;

                case ItemClass.Service.Education:
                    // School building.
                    totalHomesLabel.Hide();
                    totalJobsLabel.Hide();
                    totalStudentsLabel.Show();
                    totalStudentsLabel.text = totalUnits.ToString("N0", LocaleManager.cultureInfo);
                    break;

                default:
                    // Workplace building.
                    totalHomesLabel.Hide();
                    totalStudentsLabel.Hide();
                    totalJobsLabel.Show();
                    totalJobsLabel.text = totalUnits.ToString("N0", LocaleManager.cultureInfo);
                    break;
            }

            // Display commercial visit count, or hide the label if not commercial.
            if (building.GetAI() is CommercialBuildingAI)
            {
                visitCountLabel.Show();
                visitCountLabel.text = RealisticVisitplaceCount.CalculateVisitCount(building, totalUnits).ToString();
            }
            else
            {
                visitCountLabel.Hide();
            }

            // Display production count, or hide the label if not a production building.
            if (building.GetAI() is PrivateBuildingAI privateAI && (privateAI is OfficeBuildingAI || privateAI is IndustrialBuildingAI || privateAI is IndustrialExtractorAI))
            {
                productionLabel.Show();
                productionLabel.text = privateAI.CalculateProductionCapacity(building.GetClassLevel(), new ColossalFramework.Math.Randomizer(), building.GetWidth(), building.GetLength()).ToString();
            }
            else
            {
                productionLabel.Hide();
            }

            // Show override lavels if floors are being overridden.
            if (FloorData.instance.HasOverride(building.name) != null)
            {
                overrideFloorsLabel.Show();
                messageLabel.text = Translations.Translate("RPR_CAL_OVM");
            }
            else
            {
                overrideFloorsLabel.Hide();
            }

            // Show override labels if population is being overriden (population message text will clobber any previous floor override message, which is by design).
            if (ModUtils.CheckRICOPopControl(building))
            {
                // Overridden by Ploppable RICO Revisited.
                overridePopLabel.Show();
                messageLabel.text = Translations.Translate("RPR_CAL_RICO");
            }
            else if (PopData.instance.GetOverride(building.name) > 0)
            {
                // Overriden by manual population override.
                overridePopLabel.Show();
                messageLabel.text = Translations.Translate("RPR_CAL_OVM");
            }
            else
            {
                // No pop override - hide 'override' label.
                overridePopLabel.Hide();
            }
        }


        /// <summary>
        /// Resets the floor list position relative to the volumetic calculations panel.
        /// Needed when the volumetric calculations panel shifts its relative position (e.g. with schoolds), as the floor list is attached directly to the parent panel.
        /// </summary>
        private void ResetFloorListPosition()
        {
            // Enforce floors list position.
            float xPos = this.relativePosition.x + parent.relativePosition.x;
            float yPos = this.relativePosition.y + parent.relativePosition.y + FloorListY;
            floorsList.relativePosition = new Vector2(xPos, yPos);

            // Enforce height to match position within panel.
            floorsList.height = floorsList.parent.height - yPos - UIBuildingDetails.BottomMargin;
        }


        /// <summary>
        /// Adds a volumetric calculation text label.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="textKey">Label text translation key</param
        /// <param name="yPos">Relative X position</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="toolKey">Tooltip translation key</param>
        /// <param name="inset">X inset (default 0)</param>
        /// <returns>New UILabel</returns>
        private UILabel AddVolumetricLabel(UIComponent parent, string textKey, float xPos, float yPos, string toolKey, float inset = 0f)
        {
            // Create new label.
            UILabel newLabel = parent.AddUIComponent<UILabel>();
            newLabel.relativePosition = new Vector2(xPos - inset, yPos);
            newLabel.textAlignment = UIHorizontalAlignment.Left;
            newLabel.textScale = 0.8f;
            newLabel.text = string.Empty;

            // Add label title to the left.
            AddLabelToComponent(newLabel, Translations.Translate(textKey), inset);

            // Add tooltip.
            newLabel.tooltip = Translations.Translate(toolKey);
            newLabel.tooltipBox = TooltipUtils.TooltipBox;

            return newLabel;
        }


        /// <summary>
        /// Adds a checkbox with a text label to the left.
        /// </summary>
        /// <param name="parent">Parent component</param>
        /// <param name="textKey">Label text translation key</param
        /// <param name="yPos">Relative X position</param>
        /// <param name="yPos">Relative Y position</param>
        /// <param name="toolKey">Tooltip translation key</param>
        /// <returns>New checkbox with attached label to left</returns>
        private UICheckBox CalcCheckBox(UIComponent parent, string textKey, float xPos, float yPos, string toolKey)
        {
            // Create checkbox.
            UICheckBox checkBox = parent.AddUIComponent<UICheckBox>();

            // Size and position.
            checkBox.width = parent.width - xPos;
            checkBox.height = 20f;
            checkBox.relativePosition = new Vector2(xPos, yPos);

            // Unselected sprite.
            UISprite sprite = checkBox.AddUIComponent<UISprite>();
            //sprite.spriteName = "AchievementCheckedFalse";
            sprite.size = new Vector2(16f, 16f);
            sprite.relativePosition = Vector2.zero;

            // Selected sprite.
            checkBox.checkedBoxObject = sprite.AddUIComponent<UISprite>();
            ((UISprite)checkBox.checkedBoxObject).spriteName = "CheckDLCOwned";
            checkBox.checkedBoxObject.size = new Vector2(16f, 16f);
            checkBox.checkedBoxObject.relativePosition = Vector2.zero;

            // Text label.
            AddLabelToComponent(checkBox, Translations.Translate(textKey));

            // Add tooltip.
            checkBox.tooltip = Translations.Translate(toolKey);
            checkBox.tooltipBox = TooltipUtils.TooltipBox;

            return checkBox;
        }


        /// <summary>
        /// Adds a text label to the left of the specified component.
        /// </summary>
        /// <param name="parent">Component to add label to</param>
        /// <param name="text">Label text</param>
        /// <param name="inset">Parent x-inset (default 0)</param>
        private void AddLabelToComponent(UIComponent parent, string text, float inset = 0f)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.relativePosition = new Vector2(-(LabelOffset - Margin - inset), 0);
            label.autoSize = false;
            label.width = LabelOffset - (Margin * 2);
            label.textScale = 0.8f;
            label.autoHeight = true;
            label.wordWrap = true;
            label.text = text;
        }
    }
}