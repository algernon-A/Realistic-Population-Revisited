// <copyright file="BuildingPreview.cs" company="algernon (K. Algernon A. Sheppard)">
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
    /// Building preview image.
    /// </summary>
    public class BuildingPreview : UIPanel
    {
        // Panel components.
        private UITextureSprite previewSprite;
        private UISprite noPreviewSprite;
        private BuildingPreviewRenderer previewRender;
        private UILabel buildingName;
        private UILabel buildingLevel;
        private UILabel buildingSize;

        // Currently selected building and floor calculation pack.
        private BuildingInfo _currentSelection;
        private FloorDataPack _floorPack;
        private FloorDataPack _overrideFloors;
        private bool _renderFloors;
        private bool _hideFloors;

        /// <summary>
        /// Sets the floor data pack for previewing.
        /// </summary>
        internal FloorDataPack FloorPack
        {
            set
            {
                _floorPack = value;
                RenderPreview();
            }
        }

        /// <summary>
        /// Sets a value indicating whether floor floor preview rendering should be suppressed regardless of user setting (e.g. when legacy calculations have been selected).
        /// </summary>
        internal bool HideFloors
        {
            set
            {
                _hideFloors = value;
                RenderPreview();
            }
        }

        /// <summary>
        /// Sets a manual floor override for previewing.
        /// </summary>
        internal FloorDataPack OverrideFloors
        {
            set
            {
                _overrideFloors = value;
                RenderPreview();
            }
        }

        /// <summary>
        /// Sets a value indicating whether floor previewing is on (true) or off (false).
        /// </summary>
        internal bool RenderFloors
        {
            set
            {
                _renderFloors = value;
                RenderPreview();
            }
        }

        /// <summary>
        /// Called by Unity when the object is created.
        /// Used to perform setup.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            // Set size.
            width = BuildingDetailsPanel.MiddleWidth;
            height = BuildingDetailsPanel.MiddlePanelHeight - 40f;

            // Set background and sprites.
            backgroundSprite = "GenericPanel";

            previewSprite = AddUIComponent<UITextureSprite>();
            previewSprite.size = size;
            previewSprite.relativePosition = Vector2.zero;

            noPreviewSprite = AddUIComponent<UISprite>();
            noPreviewSprite.size = size;
            noPreviewSprite.relativePosition = Vector2.zero;

            // Initialise renderer; use double size for anti-aliasing.
            previewRender = gameObject.AddComponent<BuildingPreviewRenderer>();
            previewRender.Size = previewSprite.size * 2;

            // Click-and-drag rotation.
            eventMouseDown += (component, mouseEvent) =>
            {
                eventMouseMove += RotateCamera;
            };

            eventMouseUp += (component, mouseEvent) =>
            {
                eventMouseMove -= RotateCamera;
            };

            // Zoom with mouse wheel.
            eventMouseWheel += (component, mouseEvent) =>
            {
                previewRender.Zoom -= Mathf.Sign(mouseEvent.wheelDelta) * 0.25f;

                // Render updated image.
                RenderPreview();
            };

            // Display building name.
            buildingName = AddUIComponent<UILabel>();
            buildingName.textScale = 0.9f;
            buildingName.useDropShadow = true;
            buildingName.dropShadowColor = new Color32(80, 80, 80, 255);
            buildingName.dropShadowOffset = new Vector2(2, -2);
            buildingName.text = "Name";
            buildingName.isVisible = false;
            buildingName.relativePosition = new Vector2(5, 10);

            // Display building level.
            buildingLevel = AddUIComponent<UILabel>();
            buildingLevel.textScale = 0.9f;
            buildingLevel.useDropShadow = true;
            buildingLevel.dropShadowColor = new Color32(80, 80, 80, 255);
            buildingLevel.dropShadowOffset = new Vector2(2, -2);
            buildingLevel.text = "Level";
            buildingLevel.isVisible = false;
            buildingLevel.relativePosition = new Vector2(5, height - 20);

            // Display building size.
            buildingSize = AddUIComponent<UILabel>();
            buildingSize.textScale = 0.9f;
            buildingSize.useDropShadow = true;
            buildingSize.dropShadowColor = new Color32(80, 80, 80, 255);
            buildingSize.dropShadowOffset = new Vector2(2, -2);
            buildingSize.text = "Size";
            buildingSize.isVisible = false;
            buildingSize.relativePosition = new Vector2(width - 50, height - 20);
        }

        /// <summary>
        /// Render and show a preview of a building.
        /// </summary>
        /// <param name="building">The building to render.</param>
        public void Show(BuildingInfo building)
        {
            // Update current selection to the new building.
            _currentSelection = building;

            // Generate render if there's a selection with a mesh.
            if (_currentSelection != null && _currentSelection.m_mesh != null)
            {
                // Set default values.
                previewRender.CameraRotation = 210f;
                previewRender.Zoom = 4f;

                // Set mesh and material for render.
                previewRender.SetTarget(_currentSelection);

                // Set background.
                previewSprite.texture = previewRender.Texture;
                noPreviewSprite.isVisible = false;

                // Render at next update.
                RenderPreview();
            }
            else
            {
                // No valid current selection with a mesh; reset background.
                previewSprite.texture = null;
                noPreviewSprite.isVisible = true;
            }

            // Hide any empty building names.
            if (building == null)
            {
                buildingName.isVisible = false;
                buildingLevel.isVisible = false;
                buildingSize.isVisible = false;
            }
            else
            {
                // Set and show building name.
                buildingName.isVisible = true;
                buildingName.text = BuildingDetailsPanel.GetDisplayName(_currentSelection.name);
                UILabels.TruncateLabel(buildingName, width - 45);
                buildingName.autoHeight = true;

                // Set and show building level.
                buildingLevel.isVisible = true;
                buildingLevel.text = Translations.Translate("RPR_OPT_LVL") + " " + Mathf.Min((int)_currentSelection.GetClassLevel() + 1, MaxLevelOf(_currentSelection.GetSubService()));
                UILabels.TruncateLabel(buildingLevel, width - 45);
                buildingLevel.autoHeight = true;

                // Set and show building size.
                buildingSize.isVisible = true;
                buildingSize.text = _currentSelection.GetWidth() + "x" + _currentSelection.GetLength();
                UILabels.TruncateLabel(buildingSize, width - 45);
                buildingSize.autoHeight = true;
            }
        }

        /// <summary>
        /// Rotates the preview camera (model rotation) in accordance with mouse movement.
        /// </summary>
        /// <param name="c">Calling component.</param>
        /// <param name="p">Mouse event parameter.</param>
        private void RotateCamera(UIComponent c, UIMouseEventParameter p)
        {
            // Change rotation.
            previewRender.CameraRotation -= p.moveDelta.x / previewSprite.width * 360f;

            // Render updated image.
            RenderPreview();
        }

        /// <summary>
        /// Returns the maximum level permitted for each subservice.
        /// </summary>
        /// <param name="subService">SubService to check.</param>
        /// <returns>Maximum permitted building level for the given SubService.</returns>
        private int MaxLevelOf(ItemClass.SubService subService)
        {
            switch (subService)
            {
                case ItemClass.SubService.ResidentialLow:
                case ItemClass.SubService.ResidentialHigh:
                case ItemClass.SubService.ResidentialLowEco:
                case ItemClass.SubService.ResidentialHighEco:
                    return 5;

                case ItemClass.SubService.CommercialLow:
                case ItemClass.SubService.CommercialHigh:
                case ItemClass.SubService.OfficeGeneric:
                case ItemClass.SubService.IndustrialGeneric:
                    return 3;

                default:
                    return 1;
            }
        }

        /// <summary>
        /// Render the preview image.
        /// </summary>
        private void RenderPreview()
        {
            // Don't do anything if there's no prefab to render.
            if (_currentSelection == null)
            {
                return;
            }

            // Select pack to render; override if there is one, otherwise the selected floor pack.
            FloorDataPack renderFloorPack = _overrideFloors ?? _floorPack;

            // Are we going to render floors?
            bool doFloors = _renderFloors && !_hideFloors;

            // If the selected building has colour variations, temporarily set the colour to the default for rendering.
            if (_currentSelection.m_useColorVariations && _currentSelection.m_material != null)
            {
                Color originalColor = _currentSelection.m_material.color;
                _currentSelection.m_material.color = _currentSelection.m_color0;
                previewRender.Render(doFloors, renderFloorPack);
                _currentSelection.m_material.color = originalColor;
            }
            else
            {
                // No temporary colour change needed.
                previewRender.Render(doFloors, renderFloorPack);
            }
        }
    }
}