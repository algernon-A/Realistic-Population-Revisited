// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System.Collections.Generic;
    using System.IO;
    using AlgernonCommons;
    using AlgernonCommons.Notifications;
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using ICities;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, Patcher>
    {
        /// <summary>
        /// Gets any text for a trailing confict notification paragraph (e.g. "These mods must be removed before this mod can operate").
        /// </summary>
        protected override string ConflictRemovedText => Translations.Translate("RPR_ERR_CON1");

        /// <summary>
        /// Checks for any mod conflicts.
        /// Called as part of checking prior to executing any OnCreated actions.
        /// </summary>
        /// <returns>A list of conflicting mod names (null or empty if none).</returns>
        protected override List<string> CheckModConflicts() => ConflictDetection.CheckConflictingMods();

        /// <summary>
        /// Performs any actions upon successful creation of the mod.
        /// E.g. Can be used to patch any other mods.
        /// </summary>
        /// <param name="loading">Loading mode (e.g. game or editor).</param>
        protected override void CreatedActions(ILoading loading)
        {
            // Perform legacy datastore setup.
            XMLUtilsWG.Setup();

            // Check for Ploppable RICO Revisited.
            ModUtils.RICOReflection();

            // Initialise volumetric datastores.
            EmploymentData.Setup();

            // Initialize data.
            CalcData.Setup();

            // Apply any needed Advanced Building Level Control Harmony patches.
            PatcherManager<Patcher>.Instance.PatchABLC();
        }

        /// <summary>
        /// Performs any actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
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

            // Enable hotkey.
            HotkeyThreading.Operating = true;

            // Check and record CitizenUnits count.
            Logging.KeyMessage("citizen unit count is currently ", ColossalFramework.Singleton<CitizenManager>.instance.m_unitCount);
        }
    }
}
