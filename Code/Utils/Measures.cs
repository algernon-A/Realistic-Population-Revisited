using ColossalFramework.Globalization;


namespace RealPop2
{
    /// <summary>
    /// Static utilities class to handle conversion of metric units to/from US customary units for display.
    /// </summary>
    public static class Measures
    {
        // Conversion constancts.
        private const float LengthFeet = 3.048f;
        private const float AreaFeet = 10.76391f;


        // UI options.
        private static bool metric = true;


        /// <summary>
        /// Use metric measurements (false means US customary)
        /// </summary>
        internal static bool UsingMetric
        {
            get => metric;

            set
            {
                metric = value;
            }
        }



        /// <summary>
        /// Converts the given linear measurement (in metres) to the current display format, appending the unit measure abbreviation.
        /// </summary>
        /// <param name="length">Length in metres</param>
        /// <param name="format">String format</param>
        /// <returns>String to display</returns>
        internal static string LengthString(float length, string format) => LengthFromMetric(length).ToString(format, LocaleManager.cultureInfo) + " " + LengthMeasure;


        /// <summary>
        /// Converts the given area measurement (in square metres) to the current display format, appending the unit measure abbreviation.
        /// </summary>
        /// <param name="area">Area in square metres</param>
        /// <param name="format">String format</param>
        /// <returns>String to display</returns>
        internal static string AreaString(float area, string format) => AreaFromMetric(area).ToString(format, LocaleManager.cultureInfo) + " " + AreaMeasure;


        /// <summary>
        /// Converts the given length measurement (in square metres) to current display units.
        /// </summary>
        /// <param name="length">Length in metres</param>
        /// <returns>String to display</returns>
        internal static float LengthFromMetric(float length) => length * LengthMult;


        /// <summary>
        /// Converts the given area measurement (in square metres) to current display units.
        /// </summary>
        /// <param name="area">Area in square metres</param>
        /// <returns>String to display</returns>
        internal static float AreaFromMetric(float area) => area * AreaMult;


        /// <summary>
        /// Converts the given length measurement(in current display units) to metres.
        /// </summary>
        /// <param name="length">Length in display units</param>
        /// <returns>Length in metres</returns>
        internal static float LengthToMetric(float length) => length / LengthMult;


        /// <summary>
        /// Converts the given area measurement (in current display units) to square metres.
        /// </summary>
        /// <param name="area">Area in display units</param>
        /// <returns>Area in square metres</returns>
        internal static float AreaToMetric(float area) => area / AreaMult;


        /// <summary>
        /// Current length measure string (abbreviation).
        /// </summary>
        internal static string LengthMeasure => metric ? "m" : "ft";


        /// <summary>
        /// Current area measure string (abbreviation).
        /// </summary>
        internal static string AreaMeasure => metric ? "m2" : "sq ft";


        /// <summary>
        /// Current lenth measure multiplier (from metres).
        /// </summary>
        private static float LengthMult => metric ? 1f : LengthFeet;


        /// <summary>
        /// Current area measure multiplier (from square metres).
        /// </summary>
        private static float AreaMult => metric ? 1f : AreaFeet;
    }
}