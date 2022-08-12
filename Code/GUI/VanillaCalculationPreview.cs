// <copyright file="VanillaCalculationPreview.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.Math;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Panel to display the mod's calculations for jobs/workplaces.
    /// </summary>
    internal class VanillaCalculationPreview : UIPanel
    {
        // Layout constants.
        private const float LeftPadding = 10;
        private const float LineHeight = 25f;
        private const float Column1X = 280f;
        private const float ColumnWidth = 70f;

        // Number of columns.
        private const int NumColumns = 4;

        // Panel components.
        private UILabel messageLabel;

        // Labels.
        private UILabel[] _titleLabels = new UILabel[(int)LabelIndex.NumIndexes];
        private UILabel[][] _figLabels = new UILabel[NumColumns][];

        // Label indexes.
        private enum LabelIndex : int
        {
            Width = 0,
            Length,
            PopCalc,
            PopCustom,
            AppliedPop,
            Visit,
            Production,
            NumIndexes,
        }

        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        internal void Setup()
        {
            // Generic setup.
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            backgroundSprite = "UnlockingPanel";
            autoLayout = false;
            autoLayoutPadding.top = 5;
            autoLayoutPadding.right = 5;
            builtinKeyNavigation = true;
            clipChildren = true;

            // Add title labels.
            _titleLabels[(int)LabelIndex.Width] = UILabels.AddLabel(this, LeftPadding, LineHeight * 1f, Translations.Translate("RPR_CAL_LOT_X"));
            _titleLabels[(int)LabelIndex.Length] = UILabels.AddLabel(this, LeftPadding, LineHeight * 2f, Translations.Translate("RPR_CAL_LOT_Z"));
            _titleLabels[(int)LabelIndex.PopCalc] = UILabels.AddLabel(this, LeftPadding, LineHeight * 4f, Translations.Translate("RPR_CAL_JOB_CALC"));
            _titleLabels[(int)LabelIndex.PopCustom] = UILabels.AddLabel(this, LeftPadding, LineHeight * 5f, Translations.Translate("RPR_CAL_JOB_CUST"));
            _titleLabels[(int)LabelIndex.AppliedPop] = UILabels.AddLabel(this, LeftPadding, LineHeight * 6f, Translations.Translate("RPR_CAL_JOB_APPL"));
            _titleLabels[(int)LabelIndex.Visit] = UILabels.AddLabel(this, LeftPadding, LineHeight * 8f, Translations.Translate("RPR_CAL_VOL_VIS"));
            _titleLabels[(int)LabelIndex.Production] = UILabels.AddLabel(this, LeftPadding, LineHeight * 8f, Translations.Translate("RPR_CAL_VOL_PRD"));

            // Set up figure columns.
            for (int i = 0; i < NumColumns; ++i)
            {
                // Initialize array.
                _figLabels[i] = new UILabel[(int)LabelIndex.NumIndexes];

                // Relative x-position for this column.
                float xPos = Column1X + (i * ColumnWidth);

                // Set up figures in column
                for (int j = 0; j < (int)LabelIndex.NumIndexes; ++j)
                {
                    _figLabels[i][j] = UILabels.AddLabel(_titleLabels[j], xPos, 0f, string.Empty);
                }
            }

            // Hide production label to start.
            _titleLabels[(int)LabelIndex.Production].Hide();

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector2(LeftPadding, LineHeight * 10f);
            messageLabel.textAlignment = UIHorizontalAlignment.Left;
            messageLabel.autoSize = false;
            messageLabel.autoHeight = true;
            messageLabel.wordWrap = true;
            messageLabel.width = this.width - (LeftPadding * 2);
            messageLabel.text = "No message to display";
            messageLabel.isVisible = false;
        }

        /// <summary>
        /// Called whenever the currently selected building is changed to update the panel display.
        /// </summary>
        /// <param name="building">Newly selected building.</param>
        internal void SelectionChanged(BuildingInfo building)
        {
            // Make sure we have a valid selection before proceeding.
            if (building?.name == null)
            {
                return;
            }

            // Check for valid building AI.
            if (!(building.m_buildingAI is PrivateBuildingAI buildingAI))
            {
                Logging.Error("invalid building AI type in building details for building ", building.name);
                return;
            }

            // Set title labels according to AI type.
            if (buildingAI is ResidentialBuildingAI)
            {
                // Residential AI.
                _titleLabels[(int)LabelIndex.PopCalc].text = Translations.Translate("RPR_CAL_HOM_CALC");
                _titleLabels[(int)LabelIndex.PopCustom].text = Translations.Translate("RPR_CAL_HOM_CUST");
                _titleLabels[(int)LabelIndex.AppliedPop].text = Translations.Translate("RPR_CAL_HOM_APPL");

                // Hide redundant labels.
                _titleLabels[(int)LabelIndex.Visit].Hide();
                _titleLabels[(int)LabelIndex.Production].Hide();
            }
            else
            {
                // Workplace AI.
                _titleLabels[(int)LabelIndex.PopCalc].text = Translations.Translate("RPR_CAL_JOB_CALC");
                _titleLabels[(int)LabelIndex.PopCustom].text = Translations.Translate("RPR_CAL_JOB_CUST");
                _titleLabels[(int)LabelIndex.AppliedPop].text = Translations.Translate("RPR_CAL_JOB_APPL");

                // Set vistor/production label visibility.
                bool showProduction = buildingAI is OfficeBuildingAI || buildingAI is IndustrialBuildingAI || buildingAI is IndustrialExtractorAI;
                _titleLabels[(int)LabelIndex.Production].isVisible = showProduction;
                _titleLabels[(int)LabelIndex.Visit].isVisible = !showProduction;
            }

            // Iterate through each column to set figures.
            int width = building.GetWidth();
            int length = building.GetLength();
            for (int i = 0; i < NumColumns; ++i)
            {
                // Set figures for each building length increment.
                if (length <= NumColumns)
                {
                    // Valid length increment; set figures and increment length counter by one square.
                    SetFigures(_figLabels[i], building, buildingAI, width, length++);
                }
                else
                {
                    // Invalid length increment (greater than 4); clear figures for that column.
                    for (int j = 0; j < (int)LabelIndex.NumIndexes; ++j)
                    {
                        _figLabels[i][j].text = string.Empty;
                    }
                }
            }

            // Check to see if Ploppable RICO Revisited is controlling this building's population.
            if (ModUtils.CheckRICOPopControl(building))
            {
                messageLabel.text = Translations.Translate("RPR_CAL_RICO");
                messageLabel.Show();
            }
            else
            {
                // Hide message text by default.
                messageLabel.Hide();
            }
        }

        /// <summary>
        /// Sets the figures for a given column.
        /// </summary>
        /// <param name="labels">Column label array.</param>
        /// <param name="building">Selected BuildingInfo.</param>
        /// <param name="privateAI">Building AI as PrivateBuildingAI.</param>
        /// <param name="width">Building lot width.</param>
        /// <param name="length">Building lot length.</param>
        private void SetFigures(UILabel[] labels, BuildingInfo building, PrivateBuildingAI privateAI, int width, int length)
        {
            // Calculated totals.
            int calculatedCount, appliedCount;

            // Set dimension labels.
            labels[(int)LabelIndex.Width].text = width.ToString();
            labels[(int)LabelIndex.Length].text = length.ToString();

            // Randomizer for calculations.
            Randomizer randomizer = new Randomizer(building.m_prefabDataIndex);

            // Residential vs. workplace AI for calculating applied count.
            if (privateAI is ResidentialBuildingAI residentialAI)
            {
                calculatedCount = VanillaPopMethods.CalculateHomeCount(residentialAI, building.GetClassLevel(), randomizer, width, length);
                appliedCount = residentialAI.CalculateHomeCount(building.GetClassLevel(), randomizer, width, length);
            }
            else
            {
                // Workplace AI - get jobs count.
                int jobs0 = 0, jobs1 = 0, jobs2 = 0, jobs3 = 0;
                if (privateAI is CommercialBuildingAI)
                {
                    VanillaPopMethods.CommercialWorkplaceCount(privateAI, building.GetClassLevel(), randomizer, width, length, out jobs0, out jobs1, out jobs2, out jobs3);
                }
                else if (privateAI is OfficeBuildingAI)
                {
                    VanillaPopMethods.OfficeWorkplaceCount(privateAI, building.GetClassLevel(), randomizer, width, length, out jobs0, out jobs1, out jobs2, out jobs3);
                }
                else if (privateAI is IndustrialBuildingAI)
                {
                    VanillaPopMethods.IndustrialWorkplaceCount(privateAI, building.GetClassLevel(), randomizer, width, length, out jobs0, out jobs1, out jobs2, out jobs3);
                }
                else if (privateAI is IndustrialExtractorAI)
                {
                    VanillaPopMethods.ExtractorWorkplaceCount(privateAI, building.GetClassLevel(), randomizer, width, length, out jobs0, out jobs1, out jobs2, out jobs3);
                }
                calculatedCount = jobs0 + jobs1 + jobs2 + jobs3;

                // Get actual applied jobs.
                privateAI.CalculateWorkplaceCount(building.GetClassLevel(), randomizer, width, length, out jobs0, out jobs1, out jobs2, out jobs3);
                appliedCount = jobs0 + jobs1 + jobs2 + jobs3;
            }

            // Set population labels.
            labels[(int)LabelIndex.PopCalc].text = calculatedCount.ToString();
            labels[(int)LabelIndex.AppliedPop].text = appliedCount.ToString();

            // Set customised homes/jobs (leave blank if no custom setting retrieved).
            int customHomeJobs = PopData.Instance.GetOverride(building.name);
            labels[(int)LabelIndex.PopCustom].text = (customHomeJobs > 0) ? customHomeJobs.ToString() : string.Empty;

            // Visitor and production values.
            labels[(int)LabelIndex.Visit].text = privateAI.CalculateVisitplaceCount(building.GetClassLevel(), randomizer, width, length).ToString();
            labels[(int)LabelIndex.Production].text = privateAI.CalculateProductionCapacity(building.GetClassLevel(), randomizer, width, length).ToString();
        }
    }
}
