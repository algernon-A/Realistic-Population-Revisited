using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Globalization;


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
        /// Static interface to instance's translate method.
        /// </summary>
        /// <param name="text">Key to translate</param>
        /// <returns>Translation (or key if translation failed)</returns>
        public static string Translate(string key) => Instance.Translate(key);

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
    }


    /// <summary>
    /// Handles translations.  Framework by algernon, based off BloodyPenguin's framework.
    /// </summary>
    public class Translator
    {
        private Language systemLanguage = null;
        private readonly SortedList<string, Language> languages;
        private readonly string defaultLanguage = "en";
        private int currentIndex = -1;


        /// <summary>
        /// Returns the current zero-based index number of the current language setting.
        /// Less than zero is 'use system setting'.
        /// </summary>
        public int Index => currentIndex;


        /// <summary>
        /// Returns the current language code if one has specifically been set; otherwise, return "default".
        /// </summary>
        public string CurrentLanguage => currentIndex < 0 ? "default" : languages.Values[currentIndex].uniqueName;


        /// <summary>
        /// Actions to update the UI on a language change go here.
        /// </summary>
        public void UpdateUILanguage()
        {
            Logging.Message("setting language to ", currentIndex < 0 ? "system" : languages.Values[currentIndex].uniqueName);

            // UI update code goes here.

            // TOOO:  Add dynamic UI update.
        }


        /// <summary>
        /// Returns an alphabetically-sorted (by code) array of language display names, with an additional "system settings" item as the first item.
        /// </summary>
        /// <returns>Readable language names in alphabetical order by unique name (language code) as string array</returns>
        public string[] LanguageList
        {
            get
            {
                // Get list of readable language names.
                List<string> readableNames = languages.Values.Select((language) => language.readableName).ToList();

                // Insert system settings item at the start.
                readableNames.Insert(0, Translate("TRN_SYS"));

                // Return out list as a string array.
                return readableNames.ToArray();
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Translator()
        {
            // Initialise languages list.
            languages = new SortedList<string, Language>();

            // Load translation files.
            LoadLanguages();

            // Event handler to update the current language when system locale changes.
            LocaleManager.eventLocaleChanged += SetSystemLanguage;
        }


        /// <summary>
        /// Returns the translation for the given key in the current language.
        /// </summary>
        /// <param name="key">Translation key to transate</param>
        /// <returns>Translation </returns>
        public string Translate(string key)
        {
            Language currentLanguage;


            // Check to see if we're using system settings.
            if (currentIndex < 0)
            {
                // Using system settings - initialise system language if we haven't already.
                if (systemLanguage == null)
                {
                    SetSystemLanguage();
                }

                currentLanguage = systemLanguage;
            }
            else
            {
                currentLanguage = languages.Values[currentIndex];
            }

            // Check that a valid current language is set.
            if (currentLanguage != null)
            {
                // Check that the current key is included in the translation.
                if (currentLanguage.translationDictionary.ContainsKey(key))
                {
                    // All good!  Return translation.
                    return currentLanguage.translationDictionary[key];
                }
                else
                {
                    Logging.Message("no translation for language ", currentLanguage.uniqueName, " found for key " + key);

                    // Attempt fallack translation.
                    return FallbackTranslation(currentLanguage.uniqueName, key);
                }
            }
            else
            {
                Logging.Error("no current language when translating key ", key);
            }

            // If we've made it this far, something went wrong; just return the key.
            return key;
        }


        /// <summary>
        /// Sets the current system language; sets to null if none.
        /// </summary>
        public void SetSystemLanguage()
        {
            // Make sure Locale Manager is ready before calling it.
            if (LocaleManager.exists)
            {
                // Try to set our system language from system settings.
                try
                {
                    // Get new locale id.
                    string newLanguageCode = LocaleManager.instance.language;

                    // Check to see if we have a translation for this language code; if not, we revert to default.
                    if (!languages.ContainsKey(newLanguageCode))
                    {
                        newLanguageCode = defaultLanguage;
                    }

                    // If we've already been set to this locale, do nothing.
                    if (systemLanguage != null && systemLanguage.uniqueName == newLanguageCode)
                    {
                        return;
                    }

                    // Set the new system language,
                    systemLanguage = languages[newLanguageCode];

                    // If we're using system language, update the UI.
                    if (currentIndex < 0)
                    {
                        UpdateUILanguage();
                    }

                    // All done.
                    return;
                }
                catch (Exception e)
                {
                    // Don't really care.
                    Logging.LogException(e, "exception setting system language");
                }
            }

            // If we made it here, there's no valid system language.
            systemLanguage = null;
        }


        /// <summary>
        /// Sets the current language to the provided language code.
        /// If the key isn't in the list of loaded translations, then the system default is assigned instead(IndexOfKey returns -1 if key not found).
        /// </summary>
        /// <param name="uniqueName">Language unique name (code)</param>
        public void SetLanguage(string uniqueName) => SetLanguage(languages.IndexOfKey(uniqueName));


        /// <summary>
        /// Sets the current language to the supplied index number.
        /// If index number is invalid (out-of-bounds) then current language is set to -1 (system language setting).
        /// </summary>
        /// <param name="index">1-based language index number (negative values will use system language settings instead)</param>
        public void SetLanguage(int index)
        {
            // Don't do anything if no languages have been loaded.
            if (languages != null && languages.Count > 0)
            {
                // Bounds check; if out of bounds, use -1 (system language) instead.
                int newIndex = index >= languages.Count ? -1 : index;

                // Change the language if what we've done is new.
                if (newIndex != currentIndex)
                {
                    currentIndex = newIndex;

                    // Trigger UI update.
                    UpdateUILanguage();
                }
            }
        }


        /// <summary>
        /// Attempts to find a fallback language translation in case the primary one fails (for whatever reason).
        /// First tries a shortened version of the current reference (e.g. zh-tw -> zh), then system language, then default language.
        /// If all that fails, it just returns the raw key.
        /// </summary>
        /// <param name="attemptedLanguage">Language code that was previously attempted</param>
        /// <returns>Fallback translation if successful, or raw key if failed</returns>
        private string FallbackTranslation(string attemptedLanguage, string key)
        {
            // First check to see if there is a shortened version of this language id (e.g. zh-tw -> zh).
            if (attemptedLanguage.Length > 2)
            {
                string newName = attemptedLanguage.Substring(0, 2);

                if (languages.ContainsKey(newName))
                {
                    Language fallbackLanguage = languages[newName];
                    if (fallbackLanguage.translationDictionary.ContainsKey(key))
                    {
                        // All good!  Return translation.
                        return fallbackLanguage.translationDictionary[key];
                    }
                }
            }

            // Secondly, try to use system language if we're not already doing so.
            if (currentIndex > 0 && systemLanguage != null && attemptedLanguage != systemLanguage.uniqueName)
            {
                if (systemLanguage.translationDictionary.ContainsKey(key))
                {
                    // All good!  Return translation.
                    return systemLanguage.translationDictionary[key];
                }
            }

            // Final attempt - try default language.
            try
            {
                Language fallbackLanguage = languages[defaultLanguage];
                return fallbackLanguage.translationDictionary[key];
            }
            catch (Exception e)
            {
                // Don't care.  Just log the exception, as we really should have a default language.
                Logging.LogException(e, "exception attempting fallback translation");
            }

            // At this point we've failed; just return the key.
            return key;
        }


        /// <summary>
        /// Loads languages from CSV files.
        /// </summary>
        private void LoadLanguages()
        {
            // Clear existing dictionary.
            languages.Clear();

            // Get the current assembly path and append our locale directory name.
            string assemblyPath = ModUtils.GetAssemblyPath();
            if (!assemblyPath.IsNullOrWhiteSpace())
            {
                string localePath = Path.Combine(assemblyPath, "Translations");
                Logging.Message("loading translations from ", localePath);

                // Ensure that the directory exists before proceeding.
                if (Directory.Exists(localePath))
                {
                    // Load each file in directory and attempt to deserialise as a translation file.
                    string[] translationFiles = Directory.GetFiles(localePath);
                    foreach (string translationFile in translationFiles)
                    {
                        // Skip anything that's not marked as a .csv file.
                        if (!translationFile.EndsWith(".csv"))
                        {
                            continue;
                        }

                        Logging.Message("reading translation file ", translationFile);

                        // Read file.
                        FileStream fileStream = new FileStream(translationFile, FileMode.Open, FileAccess.Read);
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            // Create new language instance for this file.
                            Language thisLanguage = new Language();
                            string key = null;
                            bool quoting = false;

                            // Iterate through each line of file.
                            string line = reader.ReadLine();
                            while (line != null)
                            {
                                // Are we parsing quoted lines?
                                if (quoting)
                                {
                                    // Parsing a quoted line - make sure we have a valid current key.
                                    if (!key.IsNullOrWhiteSpace())
                                    {
                                        // Yes - if the line ends with a quote, trim the quote and add to existing dictionary entry and stop quoting.
                                        if (line.EndsWith("\""))
                                        {
                                            quoting = false;
                                            thisLanguage.translationDictionary[key] += line.Substring(0, line.Length - 1);
                                        }
                                        else
                                        {
                                            // Line doesn't end with a quote - add line to existing dictionary entry and keep going.
                                            thisLanguage.translationDictionary[key] += line + Environment.NewLine;
                                        }
                                    }
                                }
                                else
                                {
                                    // Not parsing quoted line - look for comma separator on this line.
                                    int commaPos = line.IndexOf(",");
                                    if (commaPos > 0)
                                    {
                                        // Comma found - split line into key and value, delimited by first comma.
                                        key = line.Substring(0, commaPos);
                                        string value = line.Substring(commaPos + 1);

                                        // Don't do anything if either key or value is invalid.
                                        if (!key.IsNullOrWhiteSpace() && !value.IsNullOrWhiteSpace())
                                        {
                                            // Trim quotes off keys.
                                            if (key.StartsWith("\""))
                                            {
                                                // Starts with quotation mark - if it also ends in a quotation mark, strip both quotation marks.
                                                if (key.EndsWith("\""))
                                                {
                                                    key = key.Substring(1, key.Length - 2);
                                                }
                                                else
                                                {
                                                    // Doesn't end in a quotation mark, so just strip leading quotation mark.
                                                    key = key.Substring(1);
                                                }
                                            }

                                            // Does this value start with a quotation mark?
                                            if (value.StartsWith("\""))
                                            {
                                                // Starts with quotation mark - if it also ends in a quotation mark, strip both quotation marks.
                                                if (value.EndsWith("\""))
                                                {
                                                    value = value.Substring(1, value.Length - 2);
                                                }
                                                else
                                                {
                                                    // Doesn't end in a quotation mark, so we've (presumably) got a multi-line quoted entry
                                                    // Flag quoting mode and set initial value to start of quoted string (less leading quotation mark), plus trailing newline.
                                                    quoting = true;
                                                    value = value.Substring(1) + Environment.NewLine;
                                                }
                                            }

                                            Logging.Message("key: ", key, " value: ", value);

                                            // Check for reserved keywords.
                                            if (key.Equals(Language.CodeKey))
                                            {
                                                // Language code.
                                                thisLanguage.uniqueName = value;
                                            }
                                            else if (key.Equals(Language.NameKey))
                                            {
                                                // Language readable name.
                                                thisLanguage.readableName = value;
                                            }
                                            else
                                            {
                                                // Try to add key/value pair to translation dictionary.
                                                if (!thisLanguage.translationDictionary.ContainsKey(key))
                                                {
                                                    thisLanguage.translationDictionary.Add(key, value);
                                                }
                                                else
                                                {
                                                    Logging.Error("duplicate translation key ", key, " in file ", translationFile);
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // No comma delimiter found - append to previous line (if last-used key is valid).
                                        if (!key.IsNullOrWhiteSpace())
                                        {
                                            thisLanguage.translationDictionary[key] += line;
                                        }
                                    }
                                }

                                // Read next line.
                                line = reader.ReadLine();
                            }

                            // Did we get a valid dictionary from this?
                            if (thisLanguage.uniqueName != null && thisLanguage.readableName != null && thisLanguage.translationDictionary.Count > 0)
                            {
                                // Yes - add to languages dictionary.
                                if (!languages.ContainsKey(thisLanguage.uniqueName))
                                {
                                    languages.Add(thisLanguage.uniqueName, thisLanguage);
                                }
                                else
                                {
                                    Logging.Error("duplicate translation file for language ", thisLanguage.uniqueName);
                                }
                            }
                            else
                            {
                                Logging.Error("file ", translationFile, " did not produce a valid translation dictionary");
                            }
                        }
                    }
                }
                else
                {
                    Logging.Error("translations directory not found");
                }
            }
            else
            {
                Logging.Error("assembly path was empty");
            }
        }
    }
}