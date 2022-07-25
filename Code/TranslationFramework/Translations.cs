namespace RealPop2
{
    /// <summary>
    /// Static class to provide translation interface.
    /// </summary>
    public static class Translations
    {
        // Instance reference.
        private static Translator _translator;


        /// <summary>
        /// The current language code.
        /// </summary>
        public static string CurrentLanguage
        {
            get
            {
                return Instance.CurrentLanguage;
            }
            set
            {
                Instance.SetLanguage(value);
            }
        }


        /// <summary>
        /// Static interface to instance's language list property.
        /// Returns an alphabetically-sorted (by unique name) string array of language display names, with an additional "system settings" item as the first item.
        /// Useful for automatically populating drop-down language selection menus; works in conjunction with Index.
        /// </summary>
        public static string[] LanguageList => Instance.LanguageList;


        /// <summary>
        /// The current language index number (equals the index number of the language names list provied by LanguageList).
        /// Useful for easy automatic drop-down language selection menus, working in conjunction with LanguageList:
        /// Set to set the language to the equivalent LanguageList index.
        /// Get to return the LanguageList index of the current languge.
        /// </summary>
        public static int Index
        {
            // Internal index is one less than here.
            // I.e. internal index is -1 for system and 0 for first language, here we want 0 for system and 1 for first language.
            // So we add one when getting and subtract one when setting.
            get
            {
                return Instance.Index + 1;
            }
            set
            {
                Instance.SetLanguage(value - 1);
            }
        }


        /// <summary>
        /// On-demand initialisation of translator.
        /// </summary>
        /// <returns>Translator instance</returns>
        private static Translator Instance
        {
            get
            {
                if (_translator == null)
                {
                    _translator = new Translator();
                }

                return _translator;
            }
        }


        /// <summary>
        /// Static interface to instance's translate method.
        /// </summary>
        /// <param name="text">Key to translate</param>
        /// <returns>Translation (or key if translation failed)</returns>
        public static string Translate(string key) => Instance.Translate(key);
    }
}