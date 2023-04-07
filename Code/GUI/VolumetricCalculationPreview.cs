// <copyright file="VolumetricCalculationPreview.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using System.Text;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.Globalization;
    using ColossalFramework.Math;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Panel to display volumetric calculations.
    /// </summary>
    public class VolumetricCalculationPreview : UIPanel
    {
        // Layout constants.
        private const float Margin = 5f;
        private const float PanelWidth = BuildingDetailsPanel.RightWidth;
        private const float PanelHeight = 395f;
        private const float ColumnWidth = BuildingDetailsPanel.RightWidth / 2f;
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
        private UIList _floorsList;
        private UILabel _numFloorsLabel;
        private UILabel _floorAreaLabel;
        private UILabel _visitCountLabel;
        private UILabel _productionLabel;
        private UILabel _firstMinLabel;
        private UILabel _firstExtraLabel;
        private UILabel _floorHeightLabel;
        private UILabel _emptyAreaLabel;
        private UILabel _emptyPercentLabel;
        private UILabel _perLabel;
        private UILabel _unitsLabel;
        private UILabel _totalHomesLabel;
        private UILabel _totalJobsLabel;
        private UILabel _totalStudentsLabel;
        private UILabel _schoolWorkerLabel;
        private UILabel _costLabel;
        private UILabel _overrideFloorsLabel;
        private UILabel _overridePopLabel;
        private UILabel _messageLabel;
        private UICheckBox fixedPopCheckBox;
        private UICheckBox _multiFloorCheckBox;
        private UICheckBox _ignoreFirstCheckBox;

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoSize = false;
            width = PanelWidth;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Labels.
            _floorHeightLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FLH", RightColumn, Row1, "RPR_CAL_VOL_FLH_TIP");
            _firstMinLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FMN", RightColumn, Row2, "RPR_CAL_VOL_FMN_TIP");
            _firstExtraLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FMX", RightColumn, Row3, "RPR_CAL_VOL_FMX_TIP");
            _numFloorsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_FLR", RightColumn, Row5, "RPR_CAL_VOL_FLR_TIP");
            _floorAreaLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_TFA", RightColumn, Row6, "RPR_CAL_VOL_TFA_TIP");
            _totalHomesLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_HOU", RightColumn, Row7, "RPR_CAL_VOL_UTS_TIP");
            _totalJobsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_WOR", RightColumn, Row7, "RPR_CAL_VOL_UTS_TIP");
            _totalStudentsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_STU", RightColumn, Row7, "RPR_CAL_VOL_UTS_TIP");
            _visitCountLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_VIS", RightColumn, Row8, "RPR_CAL_VOL_VIS_TIP");
            _productionLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_PRD", RightColumn, Row8, "RPR_CAL_VOL_PRD_TIP");
            _unitsLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_UNI", LeftColumn, Row2, "RPR_CAL_VOL_UNI_TIP");
            _emptyPercentLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_EPC", LeftColumn, Row2, "RPR_CAL_VOL_EPC_TIP");
            _emptyAreaLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_EMP", LeftColumn, Row3, "RPR_CAL_VOL_EMP_TIP");
            _perLabel = AddVolumetricLabel(this, "RPR_CAL_VOL_APU", LeftColumn, Row4, "RPR_CAL_VOL_APU_TIP");
            _schoolWorkerLabel = AddVolumetricLabel(this, "RPR_CAL_SCH_WKR", LeftColumn, Row7, "RPR_CAL_SCH_WKR_TIP", 20f);
            _costLabel = AddVolumetricLabel(this, "RPR_CAL_SCH_CST", LeftColumn, Row8, "RPR_CAL_SCH_CST_TIP", 20f);

            _overridePopLabel = OverrideLabel(_totalHomesLabel);
            _overrideFloorsLabel = OverrideLabel(_numFloorsLabel);

            // Intially hidden (just to avoid ugliness if no building is selected).
            _totalHomesLabel.Hide();
            _totalStudentsLabel.Hide();

            // Fixed population checkbox.
            fixedPopCheckBox = CalcCheckBox(this, "RPR_CAL_VOL_FXP", LeftColumn, Row1, "RPR_CAL_VOL_FXP_TIP");
            fixedPopCheckBox.isInteractive = false;
            fixedPopCheckBox.Disable();

            // Multi-floor units checkbox.
            _multiFloorCheckBox = CalcCheckBox(this, "RPR_CAL_VOL_MFU", LeftColumn, Row5, "RPR_CAL_VOL_MFU_TIP");
            _multiFloorCheckBox.isInteractive = false;
            _multiFloorCheckBox.Disable();

            // Ignore first floor checkbox.
            _ignoreFirstCheckBox = CalcCheckBox(this, "RPR_CAL_VOL_IGF", RightColumn, Row4, "RPR_CAL_VOL_IGF_TIP");
            _ignoreFirstCheckBox.isInteractive = false;
            _ignoreFirstCheckBox.Disable();

            // Message label.
            _messageLabel = UILabels.AddLabel(this, Margin, MessageY, string.Empty);

            // Floor list.
            _floorsList = UIList.AddUIList<FloorRow>(this, 0f, FloorListY, PanelWidth, PanelHeight - FloorListY);
        }

        /// <summary>
        /// Updates the population summary text labels with data from the current level.
        /// </summary>
        /// <param name="levelData">LevelData record to summarise.</param>
        internal void UpdatePopText(VolumetricPopPack.LevelData levelData)
        {
            // Update and display text labels depending on 'use fixed pop' setting.
            bool fixedPop = levelData.AreaPer < 0;
            fixedPopCheckBox.isChecked = fixedPop;
            _emptyAreaLabel.isVisible = !fixedPop;
            _emptyPercentLabel.isVisible = !fixedPop;
            _perLabel.isVisible = !fixedPop;
            _multiFloorCheckBox.isVisible = !fixedPop;
            _unitsLabel.isVisible = fixedPop;

            // Set values.
            _emptyAreaLabel.text = Measures.AreaString(levelData.EmptyArea, "N0");
            _emptyPercentLabel.text = levelData.EmptyPercent.ToString();
            _perLabel.text = Measures.AreaString(levelData.AreaPer, "N0");
            _unitsLabel.text = Measures.AreaString(levelData.AreaPer * -1, "N0");
            _multiFloorCheckBox.isChecked = levelData.MultiFloorUnits;
        }

        /// <summary>
        /// Updates the floor summary text labels with data from the current floor.
        /// </summary>
        /// <param name="floorData">FloorData record to summarise.</param>
        internal void UpdateFloorText(FloorDataPack floorData)
        {
            // Set textfield values.
            _firstMinLabel.text = Measures.LengthString(floorData.m_firstFloorMin, "N1");
            _firstExtraLabel.text = Measures.LengthString(floorData.m_firstFloorExtra, "N1");
            _floorHeightLabel.text = Measures.LengthString(floorData.m_floorHeight, "N1");

            // Set checkbox.
            _ignoreFirstCheckBox.isChecked = floorData.m_firstFloorEmpty;
        }

        /// <summary>
        /// Perform and display volumetric calculations for the currently selected building.
        /// </summary>
        /// <param name="building">Selected building prefab.</param>
        /// <param name="levelData">Population (level) calculation data to apply to calculations.</param>
        /// <param name="floorData">Floor calculation data to apply to calculations.</param>
        /// <param name="schoolData">School calculation data to apply to calculations.</param>
        /// <param name="multiplier">Multiplier to apply to calculations.</param>
        internal void CalculateVolumetric(BuildingInfo building, VolumetricPopPack.LevelData levelData, FloorDataPack floorData, SchoolDataPack schoolData, float multiplier)
        {
            // Safety first!
            if (building == null || floorData == null)
            {
                return;
            }

            // Reset message label.
            _messageLabel.text = string.Empty;

            // Perform calculations.
            // Get floors and allocate area an number of floor labels.
            SortedList<int, float> floors = PopData.Instance.VolumetricFloors(building.m_generatedInfo, floorData, out float totalArea);
            _floorAreaLabel.text = Measures.AreaString(totalArea, "N0");
            _numFloorsLabel.text = floors.Count.ToString();

            // Get total units.
            List<KeyValuePair<ushort, ushort>> perFloor = new List<KeyValuePair<ushort, ushort>>();
            int totalUnits = PopData.Instance.VolumetricPopulation(building.m_generatedInfo, levelData, floorData, multiplier, floors, totalArea, perFloor);

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
            if (levelData.AreaPer > 0)
            {
                // Create new floor area labels by iterating through each floor.
                foreach (KeyValuePair<ushort, ushort> floor in perFloor)
                {
                    // StringBuilder, because we're doing a fair bit of manipulation here.
                    StringBuilder floorString = new StringBuilder("Floor ");

                    // Floor number.
                    floorString.Append(floor.Key + 1);

                    // See if we're calculating units per individual floor.
                    if (!levelData.MultiFloorUnits)
                    {
                        // Add unit count to label.
                        floorString.Append(" (");
                        floorString.Append(floor.Value.ToString("N0"));
                        floorString.Append(" ");
                        floorString.Append(unitName);
                        floorString.Append(")");
                    }

                    // Add new floor label item with results for this calculation.
                    floorLabels.Add(floorString.ToString());
                }
            }

            // Do we have a current school selection, and are we using school property overrides?
            if (schoolData != null && ModSettings.EnableSchoolProperties)
            {
                // Yes - calculate and display school worker breakdown.
                int[] workers = SchoolData.Instance.CalcWorkers(schoolData, totalUnits);
                _schoolWorkerLabel.Show();
                _schoolWorkerLabel.text = workers[0] + " / " + workers[1] + " / " + workers[2] + " / " + workers[3];

                // Calculate construction cost to display.
                int cost = SchoolData.Instance.CalcCost(schoolData, totalUnits);
                ColossalFramework.Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetConstructionCost(ref cost, building.m_class.m_service, building.m_class.m_subService, building.m_class.m_level);

                // Calculate maintenance cost to display.
                int maintenance = SchoolData.Instance.CalcMaint(schoolData, totalUnits) * 100;
                ColossalFramework.Singleton<EconomyManager>.instance.m_EconomyWrapper.OnGetMaintenanceCost(ref maintenance, building.m_class.m_service, building.m_class.m_subService, building.m_class.m_level);
                float displayMaint = Mathf.Abs(maintenance * 0.0016f);

                // And display school cost breakdown.
                _costLabel.Show();
                _costLabel.text = cost.ToString((!(displayMaint >= 10f)) ? Settings.moneyFormat : Settings.moneyFormatNoCents, LocaleManager.cultureInfo) + " / " + displayMaint.ToString((!(displayMaint >= 10f)) ? Settings.moneyFormat : Settings.moneyFormatNoCents, LocaleManager.cultureInfo);
            }
            else
            {
                // No - hide school worker breakdown and cost labels.
                _schoolWorkerLabel.Hide();
                _costLabel.Hide();
            }

            // Allocate our new list of labels to the floors list (via an interim fastlist to avoid race conditions if we 'build' manually directly into floorsList).
            FastList<object> fastList = new FastList<object>()
            {
                m_buffer = floorLabels.ToArray(),
                m_size = floorLabels.Count,
            };
            _floorsList.Data = fastList;

            // Display total unit calculation result.
            switch (building.GetService())
            {
                case ItemClass.Service.Residential:
                    // Residential building.
                    _totalJobsLabel.Hide();
                    _totalStudentsLabel.Hide();
                    _totalHomesLabel.Show();
                    _totalHomesLabel.text = totalUnits.ToString("N0", LocaleManager.cultureInfo);
                    break;

                case ItemClass.Service.Education:
                    // School building.
                    _totalHomesLabel.Hide();
                    _totalJobsLabel.Hide();
                    _totalStudentsLabel.Show();
                    _totalStudentsLabel.text = totalUnits.ToString("N0", LocaleManager.cultureInfo);
                    break;

                default:
                    // Workplace building.
                    _totalHomesLabel.Hide();
                    _totalStudentsLabel.Hide();
                    _totalJobsLabel.Show();
                    _totalJobsLabel.text = totalUnits.ToString("N0", LocaleManager.cultureInfo);
                    break;
            }

            // Display commercial visit count, or hide the label if not commercial.
            if (building.GetAI() is CommercialBuildingAI)
            {
                _visitCountLabel.Show();
                _visitCountLabel.text = Visitors.CalculateVisitCount(building, totalUnits).ToString();
            }
            else
            {
                _visitCountLabel.Hide();
            }

            // Display production count, or hide the label if not a production building.
            if (building.GetAI() is PrivateBuildingAI privateAI && (privateAI is OfficeBuildingAI || privateAI is IndustrialBuildingAI || privateAI is IndustrialExtractorAI))
            {
                _productionLabel.Show();
                _productionLabel.text = privateAI.CalculateProductionCapacity(building.GetClassLevel(), default, building.GetWidth(), building.GetLength()).ToString();
            }
            else
            {
                _productionLabel.Hide();
            }

            // Show override lavels if floors are being overridden.
            if (FloorData.Instance.HasOverride(building.name) != null)
            {
                _overrideFloorsLabel.Show();
                _messageLabel.text = Translations.Translate("RPR_CAL_OVM");
            }
            else
            {
                _overrideFloorsLabel.Hide();
            }

            // Show override labels if population is being overriden (population message text will clobber any previous floor override message, which is by design).
            if (ModUtils.CheckRICOPopControl(building))
            {
                // Overridden by Ploppable RICO Revisited.
                _overridePopLabel.Show();
                _messageLabel.text = Translations.Translate("RPR_CAL_RICO");
            }
            else if (PopData.Instance.GetOverride(building.name) > 0)
            {
                // Overriden by manual population override.
                _overridePopLabel.Show();
                _messageLabel.text = Translations.Translate("RPR_CAL_OVM");
            }
            else
            {
                // No pop override - hide 'override' label.
                _overridePopLabel.Hide();
            }
        }

        /// <summary>
        /// Called by game when panel size changes.
        /// </summary>
        protected override void OnSizeChanged()
        {
            if (_floorsList != null)
            {
                _floorsList.height = height - FloorListY;
            }
        }

        /// <summary>
        /// Adds a volumetric calculation text label.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="textKey">Label text translation key.</param>
        /// <param name="xPos">Relative X position.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <param name="toolKey">Tooltip translation key.</param>
        /// <param name="inset">X inset (default 0).</param>
        /// <returns>New UILabel.</returns>
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
            newLabel.tooltipBox = UIToolTips.WordWrapToolTip;

            return newLabel;
        }

        /// <summary>
        /// Adds a checkbox with a text label to the left.
        /// </summary>
        /// <param name="parent">Parent component.</param>
        /// <param name="textKey">Label text translation key.</param>
        /// <param name="xPos">Relative X position.</param>
        /// <param name="yPos">Relative Y position.</param>
        /// <param name="toolKey">Tooltip translation key.</param>
        /// <returns>New checkbox with attached label to left.</returns>
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

            // sprite.spriteName = "AchievementCheckedFalse";
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
            checkBox.tooltipBox = UIToolTips.WordWrapToolTip;

            return checkBox;
        }

        /// <summary>
        /// Adds a text label to the left of the specified component.
        /// </summary>
        /// <param name="parent">Component to add label to.</param>
        /// <param name="text">Label text.</param>
        /// <param name="inset">Parent x-inset (default 0).</param>
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

        /// <summary>
        /// Adds an "overriden" notification label.
        /// </summary>
        /// <param name="parentLabel">Parent label to add to.</param>
        /// <returns>New 'overridden' UILabel..</returns>
        private UILabel OverrideLabel(UIComponent parentLabel)
        {
            UILabel thisLabel = UILabels.AddLabel(parentLabel, 0f, 0f, Translations.Translate("RPR_CAL_OVR"), textScale: 0.6f);
            thisLabel.relativePosition = new Vector2(-thisLabel.width - Margin, (parentLabel.height - thisLabel.height) / 2f);
            thisLabel.Hide();
            return thisLabel;
        }
    }
}