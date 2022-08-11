// <copyright file="ResDefaultsPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// Options panel for setting default calculation packs.
    /// </summary>
    internal class ResDefaultsPanel : RICODefaultsPanel
    {
        // Service/subservice arrays.
        private readonly string[] subServiceNames =
        {
            Translations.Translate("RPR_CAT_RLO"),
            Translations.Translate("RPR_CAT_RHI"),
            Translations.Translate("RPR_CAT_ERL"),
            Translations.Translate("RPR_CAT_ERH")
        };

        private readonly ItemClass.Service[] services =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential
        };

        private readonly ItemClass.SubService[] subServices =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.ResidentialHighEco
        };

        private readonly string[] iconNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "IconPolicySelfsufficient",
            "IconPolicySelfsufficient"
        };

        private readonly string[] atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame"
        };

        protected override string[] SubServiceNames => subServiceNames;
        protected override ItemClass.Service[] Services => services;
        protected override ItemClass.SubService[] SubServices => subServices;
        protected override string[] IconNames => iconNames;
        protected override string[] AtlasNames => atlasNames;

        // Title key.
        protected override string TitleKey => "RPR_TIT_RDF";

        // Default mode references.
        protected override DefaultMode ThisDefaultMode { get => ModSettings.ThisSaveDefaultRes; set => ModSettings.ThisSaveDefaultRes = value; }
        protected override DefaultMode NewDefaultMode { get => ModSettings.newSaveDefaultRes; set => ModSettings.newSaveDefaultRes = value; }
        protected override string DefaultModeLabel => "RPR_DEF_DMR";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ResDefaultsPanel(UITabstrip tabStrip, int tabIndex) : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected override float PanelHeader(float yPos)
        {
            float newYPos = base.PanelHeader(yPos);

            // Add 'save and apply to' label.
            UILabels.AddLabel(panel, RowAdditionX + Margin, newYPos + 10f, Translations.Translate("RPR_CAL_SAT"));
            return newYPos;
        }

        /// <summary>
        /// Adds any additional controls to each row.
        /// </summary>
        /// <param name="yPos">Relative Y position at top of row items.</param>
        /// <param name="index">Index number of this row.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected override float RowAdditions(float yPos, int index)
        {
            const float ButtonHeight = 20f;

            // Add 'apply to new buildings' button level with population pack dropdown.
            UIButton applyNewButton = UIButtons.AddButton(panel, RowAdditionX, yPos - RowHeight, Translations.Translate("RPR_CAL_NBD"), 200f, ButtonHeight, 0.8f);
            applyNewButton.objectUserData = index;
            applyNewButton.eventClicked += ApplyToNew;

            // Add 'apply to existing buildings' button level with floor pack dropdown - only if in-game.
            if (LoadingManager.exists && ColossalFramework.Singleton<LoadingManager>.instance.m_loadingComplete == true)
            {
                UIButton applyExistButton = UIButtons.AddButton(panel, RowAdditionX, yPos, Translations.Translate("RPR_CAL_ABD"), 200f, ButtonHeight, 0.8f);
                applyExistButton.objectUserData = index;
                applyExistButton.eventClicked += ApplyToAll;
            }

            return yPos;
        }

        /// <summary>
        /// 'Apply to new buildings only' button event handler.
        /// </summary>
        /// <param name="c">Calling component (unused).</param>
        /// <param name="p">Mouse event (unused).</param>
        private void ApplyToNew(UIComponent c, UIMouseEventParameter p)
        {
            // Extract subservice index from this control's object user data.
            if (c.objectUserData is int subServiceIndex)
            {
                // Status flag.
                bool isDirty = false;

                // Local references.
                ItemClass.Service service = services[subServiceIndex];
                ItemClass.SubService subService = subServices[subServiceIndex];

                // Get selected population pack.
                int popIndex = PopMenus[subServiceIndex].selectedIndex;
                PopDataPack selectedPopPack = AvailablePopPacks[subServiceIndex][popIndex];

                // Check to see if this is a change from the current default.
                if (!PopData.instance.CurrentDefaultPack(service, subService).name.Equals(selectedPopPack.name))
                {
                    // A change has been confirmed - update default population dictionary for this subservice.
                    PopData.instance.ChangeDefault(service, subService, selectedPopPack);

                    // Set status (we've changed the pack).
                    isDirty = true;
                }

                // Check floor pack if we're not using legacy or vanila calcs.
                if (selectedPopPack.version != DataVersion.legacy && selectedPopPack.version != DataVersion.vanilla && AvailableFloorPacks[FloorMenus[subServiceIndex].selectedIndex] is FloorDataPack selectedFloorPack)
                {
                    // Not legacy - check to see if this is a change from the current default.
                    if (!FloorData.instance.CurrentDefaultPack(service, subService).name.Equals(selectedFloorPack.name))
                    {
                        // A change has been confirmed - update default population dictionary for this subservice.
                        FloorData.instance.ChangeDefault(service, subService, selectedFloorPack);

                        // Set status (we've changed the pack).
                        isDirty = true;
                    }
                }

                // Did we make a change?
                if (isDirty)
                {
                    // Yes - clear population cache.
                    PopData.instance.householdCache.Clear();

                    // Save settings.
                    ConfigUtils.SaveSettings();
                }
            }
            else
            {
                Logging.Error("ApplyToNew invalid objectUserData from control ", c.name);
            }
        }

        /// <summary>
        /// 'Apply to all buildings' button event handler.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event.</param>
        private void ApplyToAll(UIComponent c, UIMouseEventParameter p)
        {
            // Extract subservice index from this control's object user data.
            if (c.objectUserData is int subServiceIndex)
            {
                // Apply any changed settings.
                ApplyToNew(c, p);

                // Update existing CitizenUnits.
                ItemClass.SubService subService = subServices[subServiceIndex];
                Logging.Message("new defaults applied; updating populations of all existing buildings with subservice ", subService);

                // Update CitizenUnits for existing building instances of this subservice.
                CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.None, subService, false);
            }
        }
    }
}