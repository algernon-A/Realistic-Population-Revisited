using System.Collections.Generic;


namespace RealPop2
{
    /// <summary>
    /// Translation language class.
    /// </summary>
    public class Language
    {
        // Translation file keywords - language code and readable name.
        public static readonly string CodeKey = "CODE";
        public static readonly string NameKey = "NAME";


        // Dictionary of translations for this language.
        public Dictionary<string, string> translationDictionary = new Dictionary<string, string>();

        // Language unique name.
        public string uniqueName = null;

        // Language human-readable name.
        public string readableName = null;
    }
}