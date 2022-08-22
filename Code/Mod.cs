// <copyright file="Mod.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Patching;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using ICities;

    /// <summary>
    /// The base mod class for instantiation by the game.
    /// </summary>
    public sealed class Mod : PatcherMod<OptionsPanel, Patcher>, IUserMod
    {
        /// <summary>
        /// Gets the mod's base display name (name only).
        /// </summary>
        public override string BaseName => "Realistic Population 2";

        /// <summary>
        /// Gets the mod's unique Harmony identfier.
        /// </summary>
        public override string HarmonyID => "algernon-A.csl.realpop2";

        /// <summary>
        /// Gets the mod's description for display in the content manager.
        /// </summary>
        public string Description => Translations.Translate("RPR_DESC");

        /// <summary>
        /// Called by the game when the mod is enabled.
        /// </summary>
        public override void OnEnabled()
        {
            // Populate (legacy) Datastore from configuration file.
            // Make sure this happens before loading the new configuration file, which will overwrite any settings here.
            // This establishes the correct priority (new over legacy).
            XMLUtilsWG.ReadFromXML();

            base.OnEnabled();
        }

        /// <summary>
        /// Saves settings file.
        /// </summary>
        public override void SaveSettings() => XMLSettingsFile.Save();

        /// <summary>
        /// Loads settings file.
        /// </summary>
        public override void LoadSettings() => XMLSettingsFile.Load();
    }
}
