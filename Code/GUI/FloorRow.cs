// <copyright file="FloorRow.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// UIListRow for building calculated floor display.
    /// </summary>
    public class FloorRow : UIListRow
    {
        // Panel components.
        private UILabel _floorName;
        private string _floorText;

        /// <summary>
        /// Generates and displays a list row.
        /// </summary>
        /// <param name="data">Object data to display.</param>
        /// <param name="rowIndex">Row index number (for background banding).</param>
        public override void Display(object data, int rowIndex)
        {
            // Perform initial setup for new rows.
            if (_floorName == null)
            {
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                width = parent.width;
                height = RowHeight;

                _floorName = AddUIComponent<UILabel>();
                _floorName.relativePosition = new Vector2(10f, 5f);
                _floorName.width = 200;
                _floorName.textScale = 0.9f;
            }

            // Set selected building.
            _floorText = data as string;
            _floorName.text = _floorText;

            // Set initial background as deselected state.
            Deselect(rowIndex);
        }
    }
}