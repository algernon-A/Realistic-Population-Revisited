// <copyright file="Measures.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using ColossalFramework.Globalization;

    /// <summary>
    /// Static utilities class to handle conversion of metric units to/from US customary units for display.
    /// </summary>
    public static class Measures
    {
        // Conversion constancts.
        private const float LengthFeet = 3.048f;
        private const float AreaFeet = 10.76391f;

        // UI options.
        private static bool s_metric = true;

        /// <summary>
        /// Gets or sets a value indicating whether we should display metric measurements (true) or US customary units (false).
        /// </summary>
        internal static bool UsingMetric
        {
            get => s_metric;

            set
            {
                s_metric = value;
            }
        }

        /// <summary>
        /// Gets the current length measure string (abbreviation).
        /// </summary>
        internal static string LengthMeasure => s_metric ? "m" : "ft";

        /// <summary>
        /// Gets the current area measure string (abbreviation).
        /// </summary>
        internal static string AreaMeasure => s_metric ? "m2" : "sq ft";

        /// <summary>
        /// Gets the current lenth measure multiplier (from metres).
        /// </summary>
        private static float LengthMult => s_metric ? 1f : LengthFeet;

        /// <summary>
        /// Gets the current area measure multiplier (from square metres).
        /// </summary>
        private static float AreaMult => s_metric ? 1f : AreaFeet;

        /// <summary>
        /// Converts the given linear measurement (in metres) to the current display format, appending the unit measure abbreviation.
        /// </summary>
        /// <param name="length">Length in metres.</param>
        /// <param name="format">String format.</param>
        /// <returns>String to display.</returns>
        internal static string LengthString(float length, string format) => LengthFromMetric(length).ToString(format, LocaleManager.cultureInfo) + " " + LengthMeasure;

        /// <summary>
        /// Converts the given area measurement (in square metres) to the current display format, appending the unit measure abbreviation.
        /// </summary>
        /// <param name="area">Area in square metres.</param>
        /// <param name="format">String format.</param>
        /// <returns>String to display.</returns>
        internal static string AreaString(float area, string format) => AreaFromMetric(area).ToString(format, LocaleManager.cultureInfo) + " " + AreaMeasure;

        /// <summary>
        /// Converts the given length measurement (in square metres) to current display units.
        /// </summary>
        /// <param name="length">Length in metres.</param>
        /// <returns>String to display.</returns>
        internal static float LengthFromMetric(float length) => length * LengthMult;

        /// <summary>
        /// Converts the given area measurement (in square metres) to current display units.
        /// </summary>
        /// <param name="area">Area in square metres.</param>
        /// <returns>String to display.</returns>
        internal static float AreaFromMetric(float area) => area * AreaMult;

        /// <summary>
        /// Converts the given length measurement(in current display units) to metres.
        /// </summary>
        /// <param name="length">Length in display units.</param>
        /// <returns>Length in metres.</returns>
        internal static float LengthToMetric(float length) => length / LengthMult;

        /// <summary>
        /// Converts the given area measurement (in current display units) to square metres.
        /// </summary>
        /// <param name="area">Area in display units.</param>
        /// <returns>Area in square metres.</returns>
        internal static float AreaToMetric(float area) => area / AreaMult;
    }
}