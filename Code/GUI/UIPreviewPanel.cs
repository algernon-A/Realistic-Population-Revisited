// <copyright file="UIPreviewPanel.cs" company="algernon (K. Algernon A. Sheppard)">
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
    class UIPreviewPanel : UIPanel
    {
        // UI components.
        private UIPreview preview;
        private UICheckBox showFloorsCheck;
        private static bool lastFloorCheckState;

        /// <summary>
        /// Handles changes to selected floor data pack (for previewing).
        /// </summary>
        internal FloorDataPack FloorPack { set => preview.FloorPack = value; }

        /// <summary>
        /// Suppresses floor preview rendering (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors { set => preview.HideFloors = value; }

        /// <summary>
        /// Handles changes to selected floor data override pack (for previewing).
        /// </summary>
        internal FloorDataPack OverrideFloors { set => preview.OverrideFloors = value; }

        /// <summary>
        /// Render and show a preview of a building.
        /// </summary>
        /// <param name="building">The building to render</param>
        public void Show(BuildingInfo building)
        {
            preview.Show(building);
        }

        /// <summary>
        /// Performs initial setup for the panel.
        /// </summary>
        public void Setup()
        {
            // Basic setup.
            preview = AddUIComponent<UIPreview>();
            preview.width = width;
            preview.height = height - 40f;
            preview.relativePosition = Vector2.zero;
            preview.Setup();

            // 'Show floors' checkbox.
            showFloorsCheck = UICheckBoxes.AddLabelledCheckBox(this, 20f, height - 30f, Translations.Translate("RPR_PRV_SFL"));
            showFloorsCheck.eventCheckChanged += (control, isChecked) =>
            {
                preview.RenderFloors = isChecked;
                lastFloorCheckState = isChecked;
            };

            showFloorsCheck.isChecked = lastFloorCheckState;
        }
    }
}
