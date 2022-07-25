using System.IO;
using ICities;
using RealPop2.MessageBox;


namespace RealPop2
{
    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public class Loading : LoadingExtensionBase
    {
        private static bool isModEnabled = false;
        private bool harmonyLoaded = false;

        // Used to flag if a conflicting mod is running.
        private static bool conflictingMod = false;


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
                isModEnabled = false;
                Logging.KeyMessage("not loading into game, skipping activation");

                // Set harmonyLoaded flag to suppress Harmony warning when e.g. loading into editor.
                harmonyLoaded = true;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.UnpatchAll();
                return;
            }

            // Ensure that Harmony patches have been applied.
            harmonyLoaded = Patcher.Patched;
            if (!harmonyLoaded)
            {
                isModEnabled = false;
                Logging.KeyMessage("Harmony patches not applied; aborting");
                return;
            }

            // Check for mod conflicts.
            if (AssemblyUtils.IsModConflict())
            {
                // Conflict detected.
                conflictingMod = true;
                isModEnabled = false;

                // Unload Harmony patches and exit before doing anything further.
                Patcher.UnpatchAll();
                return;
            }

            // Passed all checks - okay to load (if we haven't already fo some reason).
            if (!isModEnabled)
            {
                isModEnabled = true;
                Logging.KeyMessage("version v", Mod.Version, " loading");

                // Perform legacy datastore setup.
                XMLUtilsWG.Setup();

                // Check for Ploppable RICO Revisited.
                AssemblyUtils.RICOReflection();

                // Initialise volumetric datastores.
                EmploymentData.Setup();

                // Initialize data.
                DataUtils.Setup();

                // Apply any needed Advanced Building Level Control Harmony patches.
                Patcher.PatchABLC();
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
            if (!harmonyLoaded)
            {
                // Harmony 2 wasn't loaded; display warning notification and exit.
                ListMessageBox harmonyBox = MessageBoxBase.ShowModal<ListMessageBox>();

                // Key text items.
                harmonyBox.AddParas(Translations.Translate("ERR_HAR0"), Translations.Translate("RPR_ERR_HAR"), Translations.Translate("RPR_ERR_FAT"), Translations.Translate("ERR_HAR1"));

                // List of dot points.
                harmonyBox.AddList(Translations.Translate("ERR_HAR2"), Translations.Translate("ERR_HAR3"));

                // Closing para.
                harmonyBox.AddParas(Translations.Translate("MES_PAGE"));
            }

            // Check to see if a conflicting mod has been detected.
            if (conflictingMod)
            {
                // Mod conflict detected - display warning notification and exit.
                ListMessageBox modConflictBox = MessageBoxBase.ShowModal<ListMessageBox>();

                // Key text items.
                modConflictBox.AddParas(Translations.Translate("ERR_CON0"), Translations.Translate("RPR_ERR_CON0"), Translations.Translate("RPR_ERR_FAT"), Translations.Translate("ERR_CON1"));

                // Add conflicting mod name(s).
                modConflictBox.AddList(AssemblyUtils.conflictingModNames.ToArray());

                // Closing para.
                modConflictBox.AddParas(Translations.Translate("RPR_ERR_CON1"));
            }

            // Don't do anything further if mod hasn't activated for whatever reason (mod conflict, harmony error, something else).
            if (!isModEnabled)
            {
                // Disable keystrokes.
                UIThreading.operating = false;

                return;
            }

            // Show legacy choice message box if this save hasn't been flagged as being from Realistic Population 2.
            if (!ModSettings.isRealPop2Save)
            {
                MessageBoxBase.ShowModal<LegacyChoiceMessageBox>();
            }

            // Record initial (default) school settings and apply ours over the top.
            SchoolData.instance.OnLoad();

            // IF a legacy file exists, flag it for writing.
            if (File.Exists(DataStore.currentFileLocation))
            {
                Logging.KeyMessage("found legacy settings file");
                XMLUtilsWG.writeToLegacy = true;
            }

            // Add button to building info panels.
            BuildingDetailsPanel.AddInfoPanelButton();

            Logging.KeyMessage("loading complete");

            // Display update notification.
            WhatsNew.ShowWhatsNew();

            // Set up options panel event handler.
            OptionsPanel.OptionsEventHook();

            // Check and record CitizenUnits count.
            Logging.KeyMessage("citizen unit count is currently ", ColossalFramework.Singleton<CitizenManager>.instance.m_unitCount);
        }
    }
}
