// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.IO;
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ICities;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public class Loading : LoadingExtensionBase
    {
        // Status flags.
        private bool _conflictingMod = false;
        private bool _isModEnabled = false;
        private bool _harmonyLoaded = false;

        /// <summary>
        /// Called by the game when the mod is initialised at the start of the loading process.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            // Don't do anything if not in game (e.g. if we're going into an editor).
            if (loading.currentMode != AppMode.Game)
            {
                _isModEnabled = false;
                Logging.KeyMessage("not loading into game, skipping activation");

                // Set harmonyLoaded flag to suppress Harmony warning when e.g. loading into editor.
                _harmonyLoaded = true;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.Instance.UnpatchAll();
                return;
            }

            // Ensure that Harmony patches have been applied.
            _harmonyLoaded = Patcher.Instance.Patched;
            if (!_harmonyLoaded)
            {
                _isModEnabled = false;
                Logging.KeyMessage("Harmony patches not applied; aborting");
                return;
            }

            // Check for mod conflicts.
            if (ConflictDetection.IsModConflict())
            {
                // Conflict detected.
                _conflictingMod = true;
                _isModEnabled = false;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.Instance.UnpatchAll();
                return;
            }

            // Passed all checks - okay to load (if we haven't already fo some reason).
            if (!_isModEnabled)
            {
                _isModEnabled = true;
                Logging.KeyMessage("version v", AssemblyUtils.TrimmedCurrentVersion, " loading");

                // Perform legacy datastore setup.
                XMLUtilsWG.Setup();

                // Check for Ploppable RICO Revisited.
                ModUtils.RICOReflection();

                // Initialise volumetric datastores.
                EmploymentData.Setup();

                // Initialize data.
                CalcData.Setup();

                // Apply any needed Advanced Building Level Control Harmony patches.
                Patcher.Instance.PatchABLC();
            }
        }

        /// <summary>
        /// Called by the game when level loading is complete.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.)</param>
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            // Check to see that Harmony 2 was properly loaded.
            if (!_harmonyLoaded)
            {
                // Harmony 2 wasn't loaded; display warning notification and exit.
                ListNotification harmonyNotification = NotificationBase.ShowNotification<ListNotification>();

                // Key text items.
                harmonyNotification.AddParas(Translations.Translate("ERR_HAR0"), Translations.Translate("RPR_ERR_HAR"), Translations.Translate("RPR_ERR_FAT"), Translations.Translate("ERR_HAR1"));

                // List of dot points.
                harmonyNotification.AddList(Translations.Translate("ERR_HAR2"), Translations.Translate("ERR_HAR3"));

                // Closing para.
                harmonyNotification.AddParas(Translations.Translate("MES_PAGE"));
            }

            // Check to see if a conflicting mod has been detected.
            if (_conflictingMod)
            {
                // Mod conflict detected - display warning notification and exit.
                ListNotification modConflictNotification = NotificationBase.ShowNotification<ListNotification>();

                // Key text items.
                modConflictNotification.AddParas(Translations.Translate("ERR_CON0"), Translations.Translate("RPR_ERR_CON0"), Translations.Translate("RPR_ERR_FAT"), Translations.Translate("ERR_CON1"));

                // Add conflicting mod name(s).
                modConflictNotification.AddList(ConflictDetection.ConflictingModNames.ToArray());

                // Closing para.
                modConflictNotification.AddParas(Translations.Translate("RPR_ERR_CON1"));
            }

            // Don't do anything further if mod hasn't activated for whatever reason (mod conflict, harmony error, something else).
            if (!_isModEnabled)
            {
                // Disable keystrokes.
                HotkeyThreading.Operating = false;

                return;
            }

            // Show legacy choice message box if this save hasn't been flagged as being from Realistic Population 2.
            if (!ModSettings.IsRealPop2Save)
            {
                NotificationBase.ShowNotification<LegacyChoiceNotification>();
            }

            // Record initial (default) school settings and apply ours over the top.
            SchoolData.Instance.OnLoad();

            // IF a legacy file exists, flag it for writing.
            if (File.Exists(DataStore.currentFileLocation))
            {
                Logging.KeyMessage("found legacy settings file");
                XMLUtilsWG.WriteToLegacy = true;
            }

            // Add button to building info panels.
            BuildingDetailsPanelManager.AddInfoPanelButton();

            Logging.KeyMessage("loading complete");

            // Display update notification.
            WhatsNew.ShowWhatsNew();

            // Set up options panel event handler.
            OptionsPanelManager<OptionsPanel>.OptionsEventHook();

            // Enable hotkey.
            HotkeyThreading.Operating = true;

            // Check and record CitizenUnits count.
            Logging.KeyMessage("citizen unit count is currently ", ColossalFramework.Singleton<CitizenManager>.instance.m_unitCount);
        }
    }
}
