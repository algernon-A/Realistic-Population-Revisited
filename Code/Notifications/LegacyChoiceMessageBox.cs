using ColossalFramework.UI;


namespace RealPop2.MessageBox
{
    /// <summary>
    /// Message box prompting the user to chose between legacy and new calculations for this save file.
    /// </summary>
    internal class LegacyChoiceMessageBox : ListMessageBox
    {
        // Button instances.
        UIButton vanillaButton, legacyButton, newButton;


        /// <summary>
        /// Constructor - sets text.
        /// </summary>
        public LegacyChoiceMessageBox()
        {
            AddParas(Translations.Translate("RPR_OLD_0"), Translations.Translate("RPR_OLD_1"), Translations.Translate("RPR_OLD_4"), Translations.Translate("RPR_OLD_5"), Translations.Translate("RPR_OLD_6"), Translations.Translate("RPR_OLD_3"));
        }


        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {
            // Add close button.
            newButton = AddButton(1, 3, ChooseNew);
            newButton.text = Translations.Translate("RPR_OLD_NEW");
            newButton.wordWrap = true;
            vanillaButton = AddButton(2, 3, ChooseVanilla);
            vanillaButton.text = Translations.Translate("RPR_OLD_VAN");
            vanillaButton.wordWrap = true;
            legacyButton = AddButton(3, 3, ChooseLegacy);
            legacyButton.text = Translations.Translate("RPR_OLD_LEG");
            legacyButton.wordWrap = true;
        }


        /// <summary>
        /// Button action for choosing new calculations for this save.
        /// </summary>
        private void ChooseNew()
        {
            ModSettings.ThisSaveDefaultRes = DefaultMode.New;
            ModSettings.ThisSaveDefaultCom = DefaultMode.New;
            ModSettings.ThisSaveDefaultInd = DefaultMode.New;
            ModSettings.ThisSaveDefaultOff = DefaultMode.New;
            RealisticVisitplaceCount.SetVisitModes = (int)RealisticVisitplaceCount.ComVisitModes.legacy;
            RealisticIndustrialProduction.SetProdModes = (int)RealisticIndustrialProduction.ProdModes.legacy;
            RealisticExtractorProduction.SetProdModes = (int)RealisticExtractorProduction.ProdModes.legacy;
            Close();
        }


        /// <summary>
        /// Button action for choosing legacy calculations for this save.
        /// </summary>
        private void ChooseVanilla()
        {
            ModSettings.ThisSaveDefaultRes = DefaultMode.Vanilla;
            ModSettings.ThisSaveDefaultCom = DefaultMode.Vanilla;
            ModSettings.ThisSaveDefaultInd = DefaultMode.Vanilla;
            ModSettings.ThisSaveDefaultOff = DefaultMode.Vanilla;
            ModSettings.EnableSchoolPop = false;
            ModSettings.enableSchoolProperties = false;
            RealisticVisitplaceCount.SetVisitModes = (int)RealisticVisitplaceCount.ComVisitModes.legacy;
            RealisticIndustrialProduction.SetProdModes = (int)RealisticIndustrialProduction.ProdModes.legacy;
            RealisticExtractorProduction.SetProdModes = (int)RealisticExtractorProduction.ProdModes.legacy;
            Close();
        }


        /// <summary>
        /// Button action for choosing legacy calculations for this save.
        /// </summary>
        private void ChooseLegacy()
        {
            ModSettings.ThisSaveDefaultRes = DefaultMode.Legacy;
            ModSettings.ThisSaveDefaultCom = DefaultMode.Legacy;
            ModSettings.ThisSaveDefaultInd = DefaultMode.Legacy;
            ModSettings.ThisSaveDefaultOff = DefaultMode.Legacy;
            ModSettings.EnableSchoolPop = false;
            ModSettings.enableSchoolProperties = false;
            RealisticVisitplaceCount.SetVisitModes = (int)RealisticVisitplaceCount.ComVisitModes.legacy;
            RealisticIndustrialProduction.SetProdModes = (int)RealisticIndustrialProduction.ProdModes.legacy;
            RealisticExtractorProduction.SetProdModes = (int)RealisticExtractorProduction.ProdModes.legacy;
            Close();
        }
    }
}