// <copyright file="BuildingPreviewPanel.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Panel showing the building preview.
    /// </summary>
    internal class BuildingPreviewPanel : UIPanel
    {
        // Previous state.
        private static bool s_lastFloorCheckState;

        // UI components.
        private readonly BuildingPreview _preview;
        private readonly UICheckBox _showFloorsCheck;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuildingPreviewPanel"/> class.
        /// </summary>
        internal BuildingPreviewPanel()
        {
            // Basic setup.
            width = BuildingDetailsPanel.MiddleWidth;
            height = BuildingDetailsPanel.MiddlePanelHeight;

            // Preview component.
            _preview = AddUIComponent<BuildingPreview>();
            _preview.width = BuildingDetailsPanel.MiddleWidth;
            _preview.height = BuildingDetailsPanel.MiddlePanelHeight - 40f;
            _preview.relativePosition = Vector2.zero;

            // 'Show floors' checkbox.
            _showFloorsCheck = UICheckBoxes.AddLabelledCheckBox(this, 20f, height - 30f, Translations.Translate("RPR_PRV_SFL"));
            _showFloorsCheck.eventCheckChanged += (c, isChecked) =>
            {
                _preview.RenderFloors = isChecked;
                s_lastFloorCheckState = isChecked;
            };

            _showFloorsCheck.isChecked = s_lastFloorCheckState;
        }

        /// <summary>
        /// Sets the floor data pack for previewing.
        /// </summary>
        internal FloorDataPack FloorPack { set => _preview.FloorPack = value; }

        /// <summary>
        /// Sets a value indicating whether floor floor preview rendering should be suppressed regardless of user setting (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors { set => _preview.HideFloors = value; }

        /// <summary>
        /// Sets a manual floor override for previewing.
        /// </summary>
        internal FloorDataPack OverrideFloors { set => _preview.OverrideFloors = value; }

        /// <summary>
        /// Render and show a preview of a building.
        /// </summary>
        /// <param name="building">The building to render.</param>
        internal void Show(BuildingInfo building)
        {
            _preview.Show(building);
        }
    }
}
