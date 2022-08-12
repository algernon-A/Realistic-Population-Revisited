// <copyright file="LegacyChoiceNotification.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using ColossalFramework.UI;

    /// <summary>
    /// Message box prompting the user to chose between legacy and new calculations for this save file.
    /// </summary>
    internal class LegacyChoiceNotification : ListNotification
    {
        // Button instances.
        private UIButton _vanillaButton;
        private UIButton _legacyButton;
        private UIButton _newButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyChoiceNotification"/> class.
        /// </summary>
        public LegacyChoiceNotification()
        {
            AddParas(Translations.Translate("RPR_OLD_0"), Translations.Translate("RPR_OLD_1"), Translations.Translate("RPR_OLD_4"), Translations.Translate("RPR_OLD_5"), Translations.Translate("RPR_OLD_6"), Translations.Translate("RPR_OLD_3"));
        }

        /// <summary>
        /// Adds buttons to the message box.
        /// </summary>
        public override void AddButtons()
        {
            // Add close button.
            _newButton = AddButton(1, 3, Translations.Translate("RPR_OLD_NEW"), ChooseNew);
            _newButton.wordWrap = true;
            _vanillaButton = AddButton(2, 3, Translations.Translate("RPR_OLD_VAN"), ChooseVanilla);
            _vanillaButton.wordWrap = true;
            _legacyButton = AddButton(3, 3, Translations.Translate("RPR_OLD_LEG"), ChooseLegacy);
            _legacyButton.wordWrap = true;
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
            Visitors.SetVisitModes = (int)Visitors.ComVisitModes.PopCalcs;
            IndustrialProduction.SetProdModes = (int)IndustrialProduction.ProdModes.PopCalcs;
            ExtractorProduction.SetProdModes = (int)ExtractorProduction.ProdModes.PopCalcs;

            // Update exiting buildings.
            UpdateBuildings();

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
            Visitors.SetVisitModes = (int)Visitors.ComVisitModes.Legacy;
            IndustrialProduction.SetProdModes = (int)IndustrialProduction.ProdModes.Legacy;
            ExtractorProduction.SetProdModes = (int)ExtractorProduction.ProdModes.Legacy;

            // Update exiting buildings.
            UpdateBuildings();

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
            Visitors.SetVisitModes = (int)Visitors.ComVisitModes.Legacy;
            IndustrialProduction.SetProdModes = (int)IndustrialProduction.ProdModes.Legacy;
            ExtractorProduction.SetProdModes = (int)ExtractorProduction.ProdModes.Legacy;

            // Update exiting buildings.
            UpdateBuildings();

            Close();
        }

        /// <summary>
        /// Updates all buildings to ensure that they match the selected option, without any force-eviction.
        /// </summary>
        private void UpdateBuildings()
        {
            CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.Residential, ItemClass.SubService.None, true);
            CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.Commercial, ItemClass.SubService.None, true);
            CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.Industrial, ItemClass.SubService.None, true);
            CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.Office, ItemClass.SubService.None, true);
        }
    }
}