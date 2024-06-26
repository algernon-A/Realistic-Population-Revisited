﻿// <copyright file="LegacyCalculationPreview.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using ColossalFramework.Math;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Panel to display the mod's calculations for jobs/workplaces.
    /// </summary>
    public class LegacyCalculationPreview : UIPanel
    {
        // Margin at left of standard selection
        private const float LeftPadding = 10;
        private const float LineHeight = 25f;

        // Panel components.
        private UILabel[] detailLabels;
        private UILabel messageLabel;

        // Special-purpose labels used to display either jobs or households as appropriate.
        private UILabel _homesJobsCalcLabel;
        private UILabel _homesJobsCustomLabel;
        private UILabel _homesJobsActualLabel;
        private UILabel _visitCountLabel;
        private UILabel _productionLabel;

        /// <summary>
        /// Different mod calculations shown (in text labels) by this panel.
        /// </summary>
        private enum LegacyDetails : int
        {
            Width,
            Length,
            Area,
            PersonArea,
            Height,
            FloorHeight,
            Floors,
            ExtraFloors,
            NumDetails,
        }

        /// <summary>
        /// Create the mod calcs panel; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
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

            // Set up detail fields.
            detailLabels = new UILabel[(int)LegacyDetails.NumDetails];
            for (int i = 0; i < (int)LegacyDetails.NumDetails; i++)
            {
                detailLabels[i] = this.AddUIComponent<UILabel>();
                detailLabels[i].relativePosition = new Vector2(LeftPadding, (i * LineHeight) + LineHeight);
                detailLabels[i].width = 270;
                detailLabels[i].textAlignment = UIHorizontalAlignment.Left;
            }

            // Homes/jobs labels.
            _homesJobsCalcLabel = this.AddUIComponent<UILabel>();
            _homesJobsCalcLabel.relativePosition = new Vector2(LeftPadding, ((int)LegacyDetails.NumDetails + 1) * LineHeight);
            _homesJobsCalcLabel.width = 270;
            _homesJobsCalcLabel.textAlignment = UIHorizontalAlignment.Left;

            _homesJobsCustomLabel = this.AddUIComponent<UILabel>();
            _homesJobsCustomLabel.relativePosition = new Vector2(LeftPadding, ((int)LegacyDetails.NumDetails + 2) * LineHeight);
            _homesJobsCustomLabel.width = 270;
            _homesJobsCustomLabel.textAlignment = UIHorizontalAlignment.Left;

            _homesJobsActualLabel = this.AddUIComponent<UILabel>();
            _homesJobsActualLabel.relativePosition = new Vector2(LeftPadding, ((int)LegacyDetails.NumDetails + 4) * LineHeight);
            _homesJobsActualLabel.width = 270;
            _homesJobsActualLabel.textAlignment = UIHorizontalAlignment.Left;

            _visitCountLabel = this.AddUIComponent<UILabel>();
            _visitCountLabel.relativePosition = new Vector2(LeftPadding, ((int)LegacyDetails.NumDetails + 5) * LineHeight);
            _visitCountLabel.width = 270;
            _visitCountLabel.textAlignment = UIHorizontalAlignment.Left;

            _productionLabel = this.AddUIComponent<UILabel>();
            _productionLabel.relativePosition = new Vector2(LeftPadding, ((int)LegacyDetails.NumDetails + 5) * LineHeight);
            _productionLabel.width = 270;
            _productionLabel.textAlignment = UIHorizontalAlignment.Left;

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector2(LeftPadding, ((int)LegacyDetails.NumDetails + 7) * LineHeight);
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
        public void SelectionChanged(BuildingInfo building)
        {
            // Make sure we have a valid selection before proceeding.
            if (building?.name == null)
            {
                return;
            }

            // Variables to compare actual counts vs. mod count, to see if there's another mod overriding counts.
            int appliedCount;
            int modCount;

            // Building model size, not plot size.
            Vector3 buildingSize = building.m_size;
            int floorCount;

            // Array used for calculations depending on building service/subservice (via DataStore).
            int[] array;

            // Default minimum number of homes or jobs is one; different service types will override this.
            int minHomesJobs = 1;
            int customHomeJobs;

            // Check for valid building AI.
            if (!(building.GetAI() is PrivateBuildingAI buildingAI))
            {
                Logging.Error("invalid building AI type in building details for building ", building.name);
                return;
            }

            // Residential vs. workplace AI.
            if (buildingAI is ResidentialBuildingAI)
            {
                // Get appropriate calculation array.
                array = LegacyAIUtils.GetResidentialArray(building, (int)building.GetClassLevel());

                // Set calculated homes label.
                _homesJobsCalcLabel.text = Translations.Translate("RPR_CAL_HOM_CALC");

                // Set customised homes label and get value (if any).
                _homesJobsCustomLabel.text = Translations.Translate("RPR_CAL_HOM_CUST");
                customHomeJobs = OverrideUtils.GetResidential(building);

                // Applied homes is what's actually being returned by the CaclulateHomeCount call to this building AI.
                // It differs from calculated homes if there's an override value for that building with this mod, or if another mod is overriding.
                appliedCount = buildingAI.CalculateHomeCount(building.GetClassLevel(), new Randomizer(0), building.GetWidth(), building.GetLength());
                _homesJobsActualLabel.text = Translations.Translate("RPR_CAL_HOM_APPL") + appliedCount;
            }
            else
            {
                // Workplace AI.
                // Default minimum number of jobs is 4.
                minHomesJobs = 4;

                // Find the correct array for the relevant building AI.
                switch (building.GetService())
                {
                    case ItemClass.Service.Commercial:
                        array = LegacyAIUtils.GetCommercialArray(building, (int)building.GetClassLevel());
                        break;

                    case ItemClass.Service.Office:
                        array = LegacyAIUtils.GetOfficeArray(building, (int)building.GetClassLevel());
                        break;

                    case ItemClass.Service.Industrial:
                        if (buildingAI is IndustrialExtractorAI)
                        {
                            array = LegacyAIUtils.GetExtractorArray(building);
                        }
                        else
                        {
                            array = LegacyAIUtils.GetIndustryArray(building, (int)building.GetClassLevel());
                        }

                        break;

                    default:
                        Logging.Error("invalid building service in building details for building ", building.name);
                        return;
                }

                // Set calculated jobs label.
                _homesJobsCalcLabel.text = Translations.Translate("RPR_CAL_JOB_CALC") + " ";

                // Set customised jobs label and get value (if any).
                _homesJobsCustomLabel.text = Translations.Translate("RPR_CAL_JOB_CUST") + " ";
                customHomeJobs = OverrideUtils.GetWorker(building);

                // Applied jobs is what's actually being returned by the CalculateWorkplaceCount call to this building AI.
                // It differs from calculated jobs if there's an override value for that building with this mod, or if another mod is overriding.
                int[] jobs = new int[4];
                buildingAI.CalculateWorkplaceCount(building.GetClassLevel(), new Randomizer(0), building.GetWidth(), building.GetLength(), out jobs[0], out jobs[1], out jobs[2], out jobs[3]);
                appliedCount = jobs[0] + jobs[1] + jobs[2] + jobs[3];
                _homesJobsActualLabel.text = Translations.Translate("RPR_CAL_JOB_APPL") + " " + appliedCount;

                // Show visitor count for commercial buildings.
                if (buildingAI is CommercialBuildingAI commercialAI)
                {
                    _visitCountLabel.Show();
                    _visitCountLabel.text = Translations.Translate("RPR_CAL_VOL_VIS") + " " + commercialAI.CalculateVisitplaceCount(building.GetClassLevel(), default(Randomizer), building.GetWidth(), building.GetLength());
                }
                else
                {
                    _visitCountLabel.Hide();
                }

                // Display production count, or hide the label if not a production building.
                if (building.GetAI() is PrivateBuildingAI privateAI && (privateAI is OfficeBuildingAI || privateAI is IndustrialBuildingAI || privateAI is IndustrialExtractorAI))
                {
                    _productionLabel.Show();
                    _productionLabel.text = Translations.Translate("RPR_CAL_VOL_PRD") + " " + privateAI.CalculateProductionCapacity(building.GetClassLevel(), default(Randomizer), building.GetWidth(), building.GetLength()).ToString();
                }
                else
                {
                    _productionLabel.Hide();
                }
            }

            // Reproduce CalcBase calculations to get building area.
            int calcWidth = building.GetWidth();
            int calcLength = building.GetLength();
            floorCount = Mathf.Max(1, Mathf.FloorToInt(buildingSize.y / array[DataStore.LEVEL_HEIGHT]));

            // If CALC_METHOD is zero, then calculations are based on building model size, not plot size.
            if (array[DataStore.CALC_METHOD] == 0)
            {
                // If asset has small x dimension, then use plot width in squares x 6m (75% of standard width) instead.
                if (buildingSize.x <= 1)
                {
                    calcWidth *= 6;
                }
                else
                {
                    calcWidth = (int)buildingSize.x;
                }

                // If asset has small z dimension, then use plot length in squares x 6m (75% of standard length) instead.
                if (buildingSize.z <= 1)
                {
                    calcLength *= 6;
                }
                else
                {
                    calcLength = (int)buildingSize.z;
                }
            }
            else
            {
                // If CALC_METHOD is nonzero, then caluclations are based on plot size, not building size.
                // Plot size is 8 metres per square.
                calcWidth *= 8;
                calcLength *= 8;
            }

            // Display calculated (and retrieved) details.
            detailLabels[(int)LegacyDetails.Width].text = Translations.Translate("RPR_CAL_BLD_X") + " " + calcWidth;
            detailLabels[(int)LegacyDetails.Length].text = Translations.Translate("RPR_CAL_BLD_Z") + " " + calcLength;
            detailLabels[(int)LegacyDetails.Height].text = Translations.Translate("RPR_CAL_BLD_Y") + " " + (int)buildingSize.y;
            detailLabels[(int)LegacyDetails.PersonArea].text = Translations.Translate("RPR_CAL_BLD_M2") + " " + array[DataStore.PEOPLE];
            detailLabels[(int)LegacyDetails.FloorHeight].text = Translations.Translate("RPR_CAL_FLR_Y") + " " + array[DataStore.LEVEL_HEIGHT];
            detailLabels[(int)LegacyDetails.Floors].text = Translations.Translate("RPR_CAL_FLR") + " " + floorCount;

            // Area calculation - will need this later.
            int calculatedArea = calcWidth * calcLength;
            detailLabels[(int)LegacyDetails.Area].text = Translations.Translate("RPR_CAL_M2") + " " + calculatedArea;

            // Show or hide extra floor modifier as appropriate (hide for zero or less, otherwise show).
            if (array[DataStore.DENSIFICATION] > 0)
            {
                detailLabels[(int)LegacyDetails.ExtraFloors].text = Translations.Translate("RPR_CAL_FLR_M") + " " + array[DataStore.DENSIFICATION];
                detailLabels[(int)LegacyDetails.ExtraFloors].isVisible = true;
            }
            else
            {
                detailLabels[(int)LegacyDetails.ExtraFloors].isVisible = false;
            }

            // Set minimum residences for high density.
            if ((building.GetSubService() == ItemClass.SubService.ResidentialHigh) || (building.GetSubService() == ItemClass.SubService.ResidentialHighEco))
            {
                // Minimum of 2, or 90% number of floors, whichever is greater. This helps the 1x1 high density.
                minHomesJobs = Mathf.Max(2, Mathf.CeilToInt(0.9f * floorCount));
            }

            // Perform actual household or workplace calculation.
            modCount = Mathf.Max(minHomesJobs, (calculatedArea * (floorCount + Mathf.Max(0, array[DataStore.DENSIFICATION]))) / array[DataStore.PEOPLE]);
            _homesJobsCalcLabel.text += modCount;

            // Set customised homes/jobs label (leave blank if no custom setting retrieved).
            if (customHomeJobs > 0)
            {
                _homesJobsCustomLabel.text += customHomeJobs.ToString();
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
    }
}
