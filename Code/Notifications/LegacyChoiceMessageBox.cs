using ColossalFramework.UI;


namespace RealPop2.MessageBox
{
    /// <summary>
    /// Message box prompting the user to chose between legacy and new calculations for this save file.
    /// </summary>
    internal class LegacyChoiceMessageBox : ListMessageBox
    {
        // Button instances.
        UIButton legacyButton, newButton;


        /// <summary>
        /// Constructor - sets text.
        /// </summary>
        public LegacyChoiceMessageBox()
        {
            AddParas(Translations.Translate("RPR_OLD_0"), Translations.Translate("RPR_OLD_1"), Translations.Translate("RPR_OLD_2"), Translations.Translate("RPR_OLD_3"));
        }


        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {
            // Add close button.
            legacyButton = AddButton(1, 2, ChoseLegacy);
            legacyButton.text = Translations.Translate("RPR_OLD_LEG");
            newButton = AddButton(2, 2, ChoseNew);
            newButton.text = Translations.Translate("RPR_OLD_NEW");
        }

        /// <summary>
        /// Button action for choosing legacy calculations for this save.
        /// </summary>
        private void ChoseLegacy()
        {
            ModSettings.ThisSaveLegacyRes = true;
            ModSettings.ThisSaveLegacyCom = true;
            ModSettings.ThisSaveLegacyInd = true;
            ModSettings.ThisSaveLegacyOff = true;
            ModSettings.EnableSchoolPop = false;
            ModSettings.enableSchoolProperties = false;
            RealisticVisitplaceCount.SetVisitModes = (int)RealisticVisitplaceCount.ComVisitModes.legacy;
            RealisticIndustrialProduction.SetProdModes = (int)RealisticIndustrialProduction.ProdModes.legacy;
            RealisticExtractorProduction.SetProdModes = (int)RealisticExtractorProduction.ProdModes.legacy;
            Close();
        }


        /// <summary>
        /// Button action for choosing new calculations for this save.
        /// </summary>
        private void ChoseNew()
        {
            ModSettings.ThisSaveLegacyRes = false;
            ModSettings.ThisSaveLegacyCom = false;
            ModSettings.ThisSaveLegacyInd = false;
            ModSettings.ThisSaveLegacyOff = false;
            RealisticVisitplaceCount.SetVisitModes = (int)RealisticVisitplaceCount.ComVisitModes.popCalcs;
            RealisticIndustrialProduction.SetProdModes = (int)RealisticIndustrialProduction.ProdModes.popCalcs;
            RealisticExtractorProduction.SetProdModes = (int)RealisticExtractorProduction.ProdModes.popCalcs;
            Close();
        }
    }
}