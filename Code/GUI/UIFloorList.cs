using UnityEngine;
using ColossalFramework.UI;


namespace RealPop2
{
    /// <summary>
    /// An individual row in the list of buildings.
    /// </summary>
    public class UIFloorRow : UIPanel, IUIFastListRow
    {
        // Height of each row.
        private const int rowHeight = 20;

        // Panel components.
        private UIPanel panelBackground;
        private UILabel floorName;
        //private BuildingInfo thisBuilding;
        private string floorText;


        // Background for each list item.
        public UIPanel Background
        {
            get
            {
                if (panelBackground == null)
                {
                    panelBackground = AddUIComponent<UIPanel>();
                    panelBackground.width = width;
                    panelBackground.height = rowHeight;
                    panelBackground.relativePosition = Vector2.zero;

                    panelBackground.zOrder = 0;
                }

                return panelBackground;
            }
        }


        /// <summary>
        /// Called when dimensions are changed, including as part of initial setup (required to set correct relative position of label).
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (floorName != null)
            {
                Background.width = width;
                floorName.relativePosition = new Vector2(10f, 5f);
            }
        }


        /// <summary>
        /// Mouse click event handler - updates the selected building to what was clicked.
        /// </summary>
        /// <param name="p">Mouse event parameter</param>
        protected override void OnClick(UIMouseEventParameter p)
        {
            base.OnClick(p);
        }


        /// <summary>
        /// Generates and displays a building row.
        /// </summary>
        /// <param name="data">Object to list</param>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Display(object data, bool isRowOdd)
        {
            // Perform initial setup for new rows.
            if (floorName == null)
            {
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = parent.width;
                height = rowHeight;

                floorName = AddUIComponent<UILabel>();
                floorName.relativePosition = new Vector2(10f, 5f);
                floorName.width = 200;
                floorName.textScale = 0.9f;
            }

            // Set selected building.
            floorText = data as string;
            floorName.text = floorText;


            // Set initial background as deselected state.
            Deselect(isRowOdd);
        }


        /// <summary>
        /// Highlights the selected row.
        /// </summary>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Select(bool isRowOdd)
        {
            Background.backgroundSprite = "ListItemHighlight";
            Background.color = new Color32(255, 255, 255, 255);
        }


        /// <summary>
        /// Unhighlights the (un)selected row.
        /// </summary>
        /// <param name="isRowOdd">If the row is an odd-numbered row (for background banding)</param>
        public void Deselect(bool isRowOdd)
        {
            if (isRowOdd)
            {
                // Lighter background for odd rows.
                Background.backgroundSprite = "UnlockingItemBackground";
                Background.color = new Color32(0, 0, 0, 128);
            }
            else
            {
                // Darker background for even rows.
                Background.backgroundSprite = null;
            }
        }
    }
}