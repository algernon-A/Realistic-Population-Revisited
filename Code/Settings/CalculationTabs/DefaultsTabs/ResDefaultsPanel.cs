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
        // Layout constants - private.
        private const float MenuWidth = 300f;
        private const float RowAdditionX = LeftColumn + MenuWidth + (Margin * 2);

        // Service/subservice arrays.
        private readonly string[] _subServiceNames =
        {
            Translations.Translate("RPR_CAT_RLO"),
            Translations.Translate("RPR_CAT_RHI"),
            Translations.Translate("RPR_CAT_ERL"),
            Translations.Translate("RPR_CAT_ERH"),
        };

        private readonly ItemClass.Service[] _services =
        {
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
            ItemClass.Service.Residential,
        };

        private readonly ItemClass.SubService[] _subServices =
        {
            ItemClass.SubService.ResidentialLow,
            ItemClass.SubService.ResidentialHigh,
            ItemClass.SubService.ResidentialLowEco,
            ItemClass.SubService.ResidentialHighEco,
        };

        private readonly string[] _iconNames =
        {
            "ZoningResidentialLow",
            "ZoningResidentialHigh",
            "IconPolicySelfsufficient",
            "IconPolicySelfsufficient",
        };

        private readonly string[] _atlasNames =
        {
            "Thumbnails",
            "Thumbnails",
            "Ingame",
            "Ingame",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ResDefaultsPanel"/> class.
        /// </summary>
        /// <param name="tabStrip">Tab strip to add to.</param>
        /// <param name="tabIndex">Index number of tab.</param>
        internal ResDefaultsPanel(UITabstrip tabStrip, int tabIndex)
            : base(tabStrip, tabIndex)
        {
        }

        /// <summary>
        /// Gets the array of sub-service display names for this tab.
        /// </summary>
        protected override string[] SubServiceNames => _subServiceNames;

        /// <summary>
        /// Gets the array of relevant building services for this tab.
        /// </summary>
        protected override ItemClass.Service[] Services => _services;

        /// <summary>
        /// Gets the array of relevant building sub-services for this tab.
        /// </summary>
        protected override ItemClass.SubService[] SubServices => _subServices;

        /// <summary>
        /// Gets the array of building type icon sprite names for this tab.
        /// </summary>
        protected override string[] IconNames => _iconNames;

        /// <summary>
        /// Gets the array of building type icon atlas names for this tab.
        /// </summary>
        protected override string[] AtlasNames => _atlasNames;

        /// <summary>
        /// Gets the tab title translation key for this tab.
        /// </summary>
        protected override string TitleKey => "RPR_TIT_RDF";

        /// <summary>
        /// Gets or sets the default calculation mode for new saves for this tab.
        /// </summary>
        protected override DefaultMode NewDefaultMode { get => ModSettings.NewSaveDefaultRes; set => ModSettings.NewSaveDefaultRes = value; }

        /// <summary>
        /// Gets or sets the default calculation mode for this save for this tab.
        /// </summary>
        protected override DefaultMode ThisDefaultMode { get => ModSettings.ThisSaveDefaultRes; set => ModSettings.ThisSaveDefaultRes = value; }

        /// <summary>
        /// Gets the translation key for the legacy settings label for this tab.
        /// </summary>
        protected override string DefaultModeLabel => "RPR_DEF_DMR";

        /// <summary>
        /// Adds header controls to the panel.
        /// </summary>
        /// <param name="yPos">Relative Y position for buttons.</param>
        /// <returns>Relative Y coordinate below the finished setup.</returns>
        protected override float PanelHeader(float yPos)
        {
            float newYPos = base.PanelHeader(yPos);

            // Add 'save and apply to' label.
            UILabels.AddLabel(m_panel, RowAdditionX + Margin, newYPos + 10f, Translations.Translate("RPR_CAL_SAT"));
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
            UIButton applyNewButton = UIButtons.AddButton(m_panel, RowAdditionX, yPos - RowHeight, Translations.Translate("RPR_CAL_NBD"), 200f, ButtonHeight, 0.8f);
            applyNewButton.objectUserData = index;
            applyNewButton.eventClicked += ApplyToNew;

            // Add 'apply to existing buildings' button level with floor pack dropdown - only if in-game.
            if (LoadingManager.exists && ColossalFramework.Singleton<LoadingManager>.instance.m_loadingComplete == true)
            {
                UIButton applyExistButton = UIButtons.AddButton(m_panel, RowAdditionX, yPos, Translations.Translate("RPR_CAL_ABD"), 200f, ButtonHeight, 0.8f);
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
                ItemClass.Service service = _services[subServiceIndex];
                ItemClass.SubService subService = _subServices[subServiceIndex];

                // Get selected population pack.
                int popIndex = PopMenus[subServiceIndex].selectedIndex;
                PopDataPack selectedPopPack = AvailablePopPacks[subServiceIndex][popIndex];

                // Check to see if this is a change from the current default.
                if (!PopData.Instance.CurrentDefaultPack(service, subService).Name.Equals(selectedPopPack.Name))
                {
                    // A change has been confirmed - update default population dictionary for this subservice.
                    PopData.Instance.ChangeDefault(service, subService, selectedPopPack);

                    // Set status (we've changed the pack).
                    isDirty = true;
                }

                // Check floor pack if we're not using legacy or vanila calcs.
                if (selectedPopPack.Version != DataPack.DataVersion.Legacy && selectedPopPack.Version != DataPack.DataVersion.Vanilla && AvailableFloorPacks[FloorMenus[subServiceIndex].selectedIndex] is FloorDataPack selectedFloorPack)
                {
                    // Not legacy - check to see if this is a change from the current default.
                    if (!FloorData.Instance.CurrentDefaultPack(service, subService).Name.Equals(selectedFloorPack.Name))
                    {
                        // A change has been confirmed - update default population dictionary for this subservice.
                        FloorData.Instance.ChangeDefault(service, subService, selectedFloorPack);

                        // Set status (we've changed the pack).
                        isDirty = true;
                    }
                }

                // Did we make a change?
                if (isDirty)
                {
                    // Yes - clear population cache.
                    PopData.Instance.ClearHousholdCache();

                    // Save settings.
                    ConfigurationUtils.SaveSettings();
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
                ItemClass.SubService subService = _subServices[subServiceIndex];
                Logging.Message("new defaults applied; updating populations of all existing buildings with subservice ", subService);

                // Update CitizenUnits for existing building instances of this subservice.
                CitizenUnitUtils.UpdateCitizenUnits(null, ItemClass.Service.None, subService, false);
            }
        }
    }
}