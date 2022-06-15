using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Panel to display the mod's calculations for jobs/workplaces.
    /// </summary>
    internal class UIVanillaCalcs : UIPanel
    {
        private enum LabelIndex : int
        {
            Width = 0,
            Length,
            PopCalc,
            PopCustom,
            AppliedPop,
            Visit,
            Production,
            NumIndexes
        }

        // Layout constants.
        private const float LeftPadding = 10;
        private const float LineHeight = 25f;
        private const float Column1X = 280f;
        private const float ColumnWidth = 75f;
        private const float Column2X = Column1X + ColumnWidth;
        private const float Column3X = Column2X + ColumnWidth;

        // Panel components.
        private UILabel messageLabel;

        // Labels.
        UILabel[] titleLabel = new UILabel[(int)LabelIndex.NumIndexes];
        UILabel[] fig1Labels = new UILabel[(int)LabelIndex.NumIndexes];
        UILabel[] fig2Labels = new UILabel[(int)LabelIndex.NumIndexes];
        UILabel[] fig3Labels = new UILabel[(int)LabelIndex.NumIndexes];


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
            titleLabel[(int)LabelIndex.Width] = UIControls.AddLabel(this, LeftPadding, LineHeight * 1f, Translations.Translate("RPR_CAL_LOT_X"));
            titleLabel[(int)LabelIndex.Length] = UIControls.AddLabel(this, LeftPadding, LineHeight * 2f, Translations.Translate("RPR_CAL_LOT_Z"));
            titleLabel[(int)LabelIndex.PopCalc] = UIControls.AddLabel(this, LeftPadding, LineHeight * 4f, Translations.Translate("RPR_CAL_JOB_CALC"));
            titleLabel[(int)LabelIndex.PopCustom] = UIControls.AddLabel(this, LeftPadding, LineHeight * 5f, Translations.Translate("RPR_CAL_JOB_CUST"));
            titleLabel[(int)LabelIndex.AppliedPop] = UIControls.AddLabel(this, LeftPadding, LineHeight * 6f, Translations.Translate("RPR_CAL_JOB_APPL"));
            titleLabel[(int)LabelIndex.Visit] = UIControls.AddLabel(this, LeftPadding, LineHeight * 8f, Translations.Translate("RPR_CAL_VOL_VIS"));
            titleLabel[(int)LabelIndex.Production] = UIControls.AddLabel(this, LeftPadding, LineHeight * 8f, Translations.Translate("RPR_CAL_VOL_PRD"));

            // Set up first column.
            fig1Labels[(int)LabelIndex.Width] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Width], Column1X, 0f, string.Empty);
            fig1Labels[(int)LabelIndex.Length] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Length], Column1X, 0f, string.Empty);
            fig1Labels[(int)LabelIndex.PopCalc] = UIControls.AddLabel(titleLabel[(int)LabelIndex.PopCalc], Column1X, 0f, string.Empty);
            fig1Labels[(int)LabelIndex.PopCustom] = UIControls.AddLabel(titleLabel[(int)LabelIndex.PopCustom], Column1X, 0f, string.Empty);
            fig1Labels[(int)LabelIndex.AppliedPop] = UIControls.AddLabel(titleLabel[(int)LabelIndex.AppliedPop], Column1X, 0f, string.Empty);
            fig1Labels[(int)LabelIndex.Visit] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Visit], Column1X, 0f, string.Empty);
            fig1Labels[(int)LabelIndex.Production] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Production], Column1X, 0f, string.Empty);

            // Set up second column.
            fig2Labels[(int)LabelIndex.Width] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Width], Column2X, 0f, string.Empty);
            fig2Labels[(int)LabelIndex.Length] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Length], Column2X, 0f, string.Empty);
            fig2Labels[(int)LabelIndex.PopCalc] = UIControls.AddLabel(titleLabel[(int)LabelIndex.PopCalc], Column2X, 0f, string.Empty);
            fig2Labels[(int)LabelIndex.PopCustom] = UIControls.AddLabel(titleLabel[(int)LabelIndex.PopCustom], Column2X, 0f, string.Empty);
            fig2Labels[(int)LabelIndex.AppliedPop] = UIControls.AddLabel(titleLabel[(int)LabelIndex.AppliedPop], Column2X, 0f, string.Empty);
            fig2Labels[(int)LabelIndex.Visit] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Visit], Column2X, 0f, string.Empty);
            fig2Labels[(int)LabelIndex.Production] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Production], Column2X, 0f, string.Empty);

            // Set up third column.
            fig3Labels[(int)LabelIndex.Width] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Width], Column3X, 0f, string.Empty);
            fig3Labels[(int)LabelIndex.Length] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Length], Column3X, 0f, string.Empty);
            fig3Labels[(int)LabelIndex.PopCalc] = UIControls.AddLabel(titleLabel[(int)LabelIndex.PopCalc], Column3X, 0f, string.Empty);
            fig3Labels[(int)LabelIndex.PopCustom] = UIControls.AddLabel(titleLabel[(int)LabelIndex.PopCustom], Column3X, 0f, string.Empty);
            fig3Labels[(int)LabelIndex.AppliedPop] = UIControls.AddLabel(titleLabel[(int)LabelIndex.AppliedPop], Column3X, 0f, string.Empty);
            fig3Labels[(int)LabelIndex.Visit] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Visit], Column3X, 0f, string.Empty);
            fig3Labels[(int)LabelIndex.Production] = UIControls.AddLabel(titleLabel[(int)LabelIndex.Production], Column3X, 0f, string.Empty);

            // Hide production label to start.
            titleLabel[(int)LabelIndex.Production].Hide();

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
        /// <param name="building">Newly selected building</param>
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
                titleLabel[(int)LabelIndex.PopCalc].text = Translations.Translate("RPR_CAL_HOM_CALC");
                titleLabel[(int)LabelIndex.PopCustom].text = Translations.Translate("RPR_CAL_HOM_CUST");
                titleLabel[(int)LabelIndex.AppliedPop].text = Translations.Translate("RPR_CAL_HOM_APPL");

                // Hide redundant labels.
                titleLabel[(int)LabelIndex.Visit].Hide();
                titleLabel[(int)LabelIndex.Production].Hide();
            }
            else
            {
                // Workplace AI.
                titleLabel[(int)LabelIndex.PopCalc].text = Translations.Translate("RPR_CAL_JOB_CALC");
                titleLabel[(int)LabelIndex.PopCustom].text = Translations.Translate("RPR_CAL_JOB_CUST");
                titleLabel[(int)LabelIndex.AppliedPop].text = Translations.Translate("RPR_CAL_JOB_APPL");

                // Set vistor/production label visibility.
                bool showProduction = buildingAI is OfficeBuildingAI || buildingAI is IndustrialBuildingAI || buildingAI is IndustrialExtractorAI;
                titleLabel[(int)LabelIndex.Production].isVisible = showProduction;
                titleLabel[(int)LabelIndex.Visit].isVisible = !showProduction;
            }


            // Set figures.
            int width = building.GetWidth();
            int length = building.GetLength();
            SetFigures(fig1Labels, building, buildingAI, width, length);

            // Column 2 figures and visibility.
            bool hideCol2 = true;
            if (length < 4)
            {
                SetFigures(fig2Labels, building, buildingAI, width, ++length);
                hideCol2 = false;
            }

            // Column 3 figures and visibility.
            bool hideCol3 = true;
            if (length < 4)
            {
                SetFigures(fig3Labels, building, buildingAI, width, ++length);
                hideCol3 = false;
            }

            // Apply visibility.
            for (int i = 0; i < fig2Labels.Length; ++i)
            {
                fig2Labels[i].isVisible = !hideCol2;
                fig3Labels[i].isVisible = !hideCol3;
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
            int customHomeJobs = PopData.instance.GetOverride(building.name);
            labels[(int)LabelIndex.PopCustom].text = (customHomeJobs > 0) ? customHomeJobs.ToString() : string.Empty;

            // Visitor and production values.
            labels[(int)LabelIndex.Visit].text = privateAI.CalculateVisitplaceCount(building.GetClassLevel(), randomizer, width, length).ToString();
            labels[(int)LabelIndex.Production].text = privateAI.CalculateProductionCapacity(building.GetClassLevel(), randomizer, width, length).ToString();
        }
    }
}
