using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// Titlebar for the building details screen.
    /// </summary>
    public class UITitleBar : UIPanel
    {
        // Titlebar components.
        private UILabel titleLabel;
        private UIDragHandle dragHandle;
        private UISprite iconSprite;
        private UIButton closeButton;


        /// <summary>
        /// Create the titlebar; we no longer use Start() as that's not sufficiently reliable (race conditions), and is no longer needed, with the new create/destroy process.
        /// </summary>
        public void Setup()
        {
            // Basic setup.
            width = parent.width;
            height = UIBuildingDetails.TitleHeight;
            isVisible = true;
            canFocus = true;
            isInteractive = true;
            relativePosition = Vector2.zero;

            // Make it draggable.
            dragHandle = AddUIComponent<UIDragHandle>();
            dragHandle.width = width - 50;
            dragHandle.height = height;
            dragHandle.relativePosition = Vector2.zero;
            dragHandle.target = parent;

            // Decorative icon (top-left).
            iconSprite = AddUIComponent<UISprite>();
            iconSprite.relativePosition = new Vector2(10, 5);
            iconSprite.spriteName = "ToolbarIconZoomOutCity";
            UIUtils.ResizeIcon(iconSprite, new Vector2(30, 30));
            iconSprite.relativePosition = new Vector2(10, 5);

            // Titlebar label.
            titleLabel = AddUIComponent<UILabel>();
            titleLabel.relativePosition = new Vector2(50, 13);
            titleLabel.text = RealPopMod.ModName;

            // Close button.
            closeButton = AddUIComponent<UIButton>();
            closeButton.relativePosition = new Vector2(width - 35, 2);
            closeButton.normalBgSprite = "buttonclose";
            closeButton.hoveredBgSprite = "buttonclosehover";
            closeButton.pressedBgSprite = "buttonclosepressed";
            closeButton.eventClick += (component, param) =>
            {
                BuildingDetailsPanel.Close();
            };
        }
    }
}