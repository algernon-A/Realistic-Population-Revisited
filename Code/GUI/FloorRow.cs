// <copyright file="FloorRow.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using AlgernonCommons.UI;
    using ColossalFramework.UI;

    /// <summary>
    /// UIListRow for building calculated floor display.
    /// </summary>
    public class FloorRow : UIListRow
    {
        // Panel components.
        private UILabel _floorName;

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
                _floorName = AddLabel(Margin, 200f);
            }

            if (data is string text)
            {
                _floorName.text = text;
            }

            // Set initial background as deselected state.
            Deselect(rowIndex);
        }
    }
}