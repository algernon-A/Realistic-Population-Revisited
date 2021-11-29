using ICities;
using ColossalFramework.UI;
using CitiesHarmony.API;


namespace RealPop2
{
    public class RealPopMod : IUserMod
    {
        // Public mod name and description.
        public string Name => ModName + " " + Version;
        public string Description => Translations.Translate("RPR_DESC");

        // Internal and private name and version components.
        internal static string ModName => "Realistic Population 2";
        internal static string Version => BaseVersion + " " + Beta;
        internal static string Beta => "rc6";
        private static string BaseVersion => "2.0";


        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
        public void OnSettingsUI(UIHelperBase helper)
        {
            // Setup options panel reference.
            OptionsPanel.optionsPanel = ((UIHelper)helper).self as UIScrollablePanel;
            OptionsPanel.optionsPanel.autoLayout = false;
        }


        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }


        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());

            // Load settings file.
            XMLSettingsFile.Load();

            // Populate (legacy) Datastore from configuration file.
            // Make sure this happens before loading the new configuration file, which will overwrite any settings here.
            // This establishes the correct priority (new over legacy).
            XMLUtilsWG.ReadFromXML();

            // Attaching options panel event hook - check to see if UIView is ready.
            if (UIView.GetAView() != null)
            {
                // It's ready - attach the hook now.
                OptionsPanel.OptionsEventHook();
            }
            else
            {
                // Otherwise, queue the hook for when the intro's finished loading.
                LoadingManager.instance.m_introLoaded += OptionsPanel.OptionsEventHook;
            }
        }
    }
}
