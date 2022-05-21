using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Different mod calculations shown (in text labels) by this panel.
    /// </summary>
    public enum VanillaDetails
    {
        width,
        length,
        area,
        numDetails
    }


    /// <summary>
    /// Panel to display the mod's calculations for jobs/workplaces.
    /// </summary>
    public class UIVanillaCalcs : UIPanel
    {
        // Margin at left of standard selection
        private const float LeftPadding = 10;
        private const float LineHeight = 25f;

        // Panel components.
        private UILabel[] detailLabels;
        private UILabel messageLabel;

        // Special-purpose labels used to display either jobs or households as appropriate.
        private UILabel homesJobsCalcLabel, homesJobsCustomLabel, homesJobsActualLabel;
        private UILabel visitCountLabel, productionLabel;


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
            detailLabels = new UILabel[(int)VanillaDetails.numDetails];
            for (int i = 0; i < (int)VanillaDetails.numDetails; i++)
            {
                detailLabels[i] = this.AddUIComponent<UILabel>();
                detailLabels[i].relativePosition = new Vector2(LeftPadding, (i * LineHeight) + LineHeight);
                detailLabels[i].width = 270;
                detailLabels[i].textAlignment = UIHorizontalAlignment.Left;
            }

            // Homes/jobs labels.
            homesJobsCalcLabel = this.AddUIComponent<UILabel>();
            homesJobsCalcLabel.relativePosition = new Vector2(LeftPadding, ((int)VanillaDetails.numDetails + 1) * LineHeight);
            homesJobsCalcLabel.width = 270;
            homesJobsCalcLabel.textAlignment = UIHorizontalAlignment.Left;

            homesJobsCustomLabel = this.AddUIComponent<UILabel>();
            homesJobsCustomLabel.relativePosition = new Vector2(LeftPadding, ((int)VanillaDetails.numDetails + 2) * LineHeight);
            homesJobsCustomLabel.width = 270;
            homesJobsCustomLabel.textAlignment = UIHorizontalAlignment.Left;

            homesJobsActualLabel = this.AddUIComponent<UILabel>();
            homesJobsActualLabel.relativePosition = new Vector2(LeftPadding, ((int)VanillaDetails.numDetails + 4) * LineHeight);
            homesJobsActualLabel.width = 270;
            homesJobsActualLabel.textAlignment = UIHorizontalAlignment.Left;

            visitCountLabel = this.AddUIComponent<UILabel>();
            visitCountLabel.relativePosition = new Vector2(LeftPadding, ((int)VanillaDetails.numDetails + 5) * LineHeight);
            visitCountLabel.width = 270;
            visitCountLabel.textAlignment = UIHorizontalAlignment.Left;

            productionLabel = this.AddUIComponent<UILabel>();
            productionLabel.relativePosition = new Vector2(LeftPadding, ((int)VanillaDetails.numDetails + 5) * LineHeight);
            productionLabel.width = 270;
            productionLabel.textAlignment = UIHorizontalAlignment.Left;

            // Message label (initially hidden).
            messageLabel = this.AddUIComponent<UILabel>();
            messageLabel.relativePosition = new Vector2(LeftPadding, ((int)VanillaDetails.numDetails + 7) * LineHeight);
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
        public void SelectionChanged(BuildingInfo building)
        {
            // Make sure we have a valid selection before proceeding.
            if (building?.name == null)
            {
                return;
            }

            // Variables to compare actual counts vs. mod count, to see if there's another mod overriding counts.
            int appliedCount;

            // Customized home/jobcount.
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
                // Set calculated homes label.
                homesJobsCalcLabel.text = Translations.Translate("RPR_CAL_HOM_CALC");

                // Set customised homes label and get value (if any).
                homesJobsCustomLabel.text = Translations.Translate("RPR_CAL_HOM_CUST");
                customHomeJobs = OverrideUtils.GetResidential(building);

                // Applied homes is what's actually being returned by the CaclulateHomeCount call to this building AI.
                // It differs from calculated homes if there's an override value for that building with this mod, or if another mod is overriding.
                appliedCount = VanillaPopMethods.CalculateHomeCount(buildingAI, building.GetClassLevel(), new Randomizer(0), building.GetWidth(), building.GetLength());
                homesJobsActualLabel.text = Translations.Translate("RPR_CAL_HOM_APPL") + appliedCount;
            }
            else
            {
                // Workplace AI.
                VanillaPopMethods.WorkplaceCount(buildingAI, building.GetClassLevel(), building.GetWidth(), building.GetLength(), out int jobs0, out int jobs1, out int jobs2, out int jobs3);

                // Set calculated jobs label.
                homesJobsCalcLabel.text = Translations.Translate("RPR_CAL_JOB_CALC") + " " + (jobs0 + jobs1 + jobs2 + jobs3);

                // Set customised jobs label and get value (if any).
                homesJobsCustomLabel.text = Translations.Translate("RPR_CAL_JOB_CUST") + " ";
                customHomeJobs = OverrideUtils.GetWorker(building);

                // Applied jobs is what's actually being returned by the CalculateWorkplaceCount call to this building AI.
                // It differs from calculated jobs if there's an override value for that building with this mod, or if another mod is overriding.
                int[] jobs = new int[4];
                buildingAI.CalculateWorkplaceCount(building.GetClassLevel(), new Randomizer(0), building.GetWidth(), building.GetLength(), out jobs[0], out jobs[1], out jobs[2], out jobs[3]);
                appliedCount = jobs[0] + jobs[1] + jobs[2] + jobs[3];
                homesJobsActualLabel.text = Translations.Translate("RPR_CAL_JOB_APPL") + " " + appliedCount;

                // Show visitor count for commercial buildings.
                if (buildingAI is CommercialBuildingAI commercialAI)
                {
                    visitCountLabel.Show();
                    visitCountLabel.text = Translations.Translate("RPR_CAL_VOL_VIS") + " " + commercialAI.CalculateVisitplaceCount(building.GetClassLevel(), new Randomizer(), building.GetWidth(), building.GetLength());
                }
                else
                {
                    visitCountLabel.Hide();
                }

                // Display production count, or hide the label if not a production building.
                if (building.GetAI() is PrivateBuildingAI privateAI && (privateAI is OfficeBuildingAI || privateAI is IndustrialBuildingAI || privateAI is IndustrialExtractorAI))
                {
                    productionLabel.Show();
                    productionLabel.text = Translations.Translate("RPR_CAL_VOL_PRD") + " " + privateAI.CalculateProductionCapacity(building.GetClassLevel(), new Randomizer(), building.GetWidth(), building.GetLength()).ToString();
                }
                else
                {
                    productionLabel.Hide();
                }
            }

            // Reproduce CalcBase calculations to get building area.
            int calcWidth = building.GetWidth();
            int calcLength = building.GetLength();

            // Display calculated (and retrieved) details.
            detailLabels[(int)VanillaDetails.width].text = Translations.Translate("RPR_CAL_BLD_X") + " " + calcWidth;
            detailLabels[(int)VanillaDetails.length].text = Translations.Translate("RPR_CAL_BLD_Z") + " " + calcLength;

            // Set customised homes/jobs label (leave blank if no custom setting retrieved).
            if (customHomeJobs > 0)
            {
                homesJobsCustomLabel.text += customHomeJobs.ToString();
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
