using ColossalFramework;
using ColossalFramework.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace RealPop2
{
    /// <summary>
    /// Handles translations.  Framework by algernon, based off BloodyPenguin's framework.
    /// </summary>
    public class Translator
    {
        private Language systemLanguage = null;
        private readonly SortedList<string, Language> languages;
        private readonly string defaultLanguage = "en-EN";
        private int currentIndex = -1;

        // Last recorded system language.
        private string systemLangaugeCode;


        /// <summary>
        /// Returns the current zero-based index number of the current language setting.
        /// Less than zero is 'use system setting'.
        /// </summary>
        public int Index => currentIndex;


        /// <summary>
        /// Returns the current language code if one has specifically been set; otherwise, return "default".
        /// </summary>
        public string CurrentLanguage => currentIndex < 0 ? "default" : languages.Values[currentIndex].code;


        /// <summary>
        /// Actions to update the UI on a language change go here.
        /// </summary>
        public void UpdateUILanguage()
        {
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

            // Set initial system language reference.
            systemLangaugeCode = string.Empty;
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
                // Using system settings - initialise system language if we haven't already, or if the system language has changed since last time.
                if (LocaleManager.exists & (LocaleManager.instance.language != systemLangaugeCode | systemLanguage == null))
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
                    // Lookup failed - fallack translation.
                    Logging.Message("no translation for language ", currentLanguage.code, " found for key " + key);
                    return FallbackTranslation(currentLanguage.code, key);
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

                    // If we've already been set to this locale, do nothing.
                    if (systemLanguage != null & systemLangaugeCode == newLanguageCode)
                    {
                        return;
                    }

                    // Set the new system language,
                    Logging.Message("game language is ", newLanguageCode);
                    systemLangaugeCode = newLanguageCode;
                    systemLanguage = FindLanguage(newLanguageCode);

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
        /// If the key isn't in the list of loaded translations, then the system default is assigned instead.
        /// </summary>
        /// <param name="languageCode">Language code</param>
        public void SetLanguage(string languageCode)
        {
            Logging.Message("setting language to ", languageCode);

            // Default (game) language.
            if (languageCode == "default")
            {
                SetLanguage(-1);
                return;
            }

            // Try for direct match.
            if (languages.ContainsKey(languageCode))
            {
                SetLanguage(languages.IndexOfKey(languageCode));
                return;
            }

            // No direct match found - attempt to find any other suitable translation file (code matches first two letters).
            string shortCode = languageCode.Substring(0, 2);
            foreach (KeyValuePair<string, Language> entry in languages)
            {
                if (entry.Key.StartsWith(shortCode))
                {
                    // Found an alternative.
                    Logging.Message("using language ", entry.Key, " as replacement for unknown language code ", languageCode);
                    SetLanguage(languages.IndexOfKey(entry.Key));
                    return;
                }
            }

            // If we got here, no match was found; revert to system language.
            Logging.Message("no suitable translation file for language ", languageCode, " was found; reverting to game default");
            SetLanguage(-1);
        }


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
        /// Attempts to find the most appropriate translation file for the specified language code.
        /// An exact match is attempted first; then a match with the first available language with the same two intial characters.
        /// e.g. 'zh' will match to 'zh', 'zh-CN' or 'zh-TW' (in that order), or 'zh-CN' will match to 'zh-CN', 'zh' or 'zh-TW' (in that order).
        /// If no match is made,the default language will be returned.
        /// </summary>
        /// <param name="languageCode">Language code to match</param>
        /// <returns>Matched language code correspondign to a loaded translation file</returns>
        private Language FindLanguage(string languageCode)
        {
            // First attempt to find the language code as-is.
            if (languages.TryGetValue(languageCode, out Language language))
            {
                return language;
            }

            // If that fails, take the first two characters of the provided code and match with the first language code we have starting with those two letters.
            // This will automatically prioritise any translations with only two letters (e.g. 'en' takes priority over 'en-US'),
            KeyValuePair<string, Language> firstMatch = languages.FirstOrDefault(x => x.Key.StartsWith(languageCode.Substring(0, 2)));
            if (!string.IsNullOrEmpty(firstMatch.Key))
            {
                // Found one - return translation.
                Logging.KeyMessage("using translation file ", firstMatch.Key, " for language ", languageCode);
                return firstMatch.Value;
            }

            // Fall back to default language.
            Logging.Error("no translation file found for language ", languageCode, "; reverting to ", defaultLanguage);
            return languages[defaultLanguage];
        }


        /// <summary>
        /// Attempts to find a fallback language translation in case the primary one fails (for whatever reason).
        /// </summary>
        /// <param name="attemptedLanguage">Language code that was previously attempted</param>
        /// <returns>Fallback translation if successful, or raw key if failed</returns>
        private string FallbackTranslation(string attemptedLanguage, string key)
        {
            try
            {
                // Attempt to find any other suitable translation file (code matches first two letters).
                string shortCode = attemptedLanguage.Substring(0, 2);
                foreach (KeyValuePair<string, Language> entry in languages)
                {
                    if (entry.Key.StartsWith(shortCode) && entry.Value.translationDictionary.TryGetValue(key, out string result))
                    {
                        // Found an alternative.
                        return result;
                    }
                }

                // No alternative was found - return default language.
                return languages[defaultLanguage].translationDictionary[key];

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
            string assemblyPath = AssemblyUtils.AssemblyPath;
            if (assemblyPath.IsNullOrWhiteSpace())
            {
                Logging.Error("assembly path was empty");
                return;
            }

            string localePath = Path.Combine(assemblyPath, "Translations");

            // Ensure that the directory exists before proceeding.
            if (!Directory.Exists(localePath))
            {
                Logging.Error("translations directory not found");
                return;
            }

            // Load each file in directory and attempt to deserialise as a translation file.
            string[] translationFiles = Directory.GetFiles(localePath);
            foreach (string translationFile in translationFiles)
            {
                // Skip anything that's not marked as a .csv file.
                if (!translationFile.EndsWith(".csv"))
                {
                    continue;
                }

                // Read file.
                try
                {
                    FileStream fileStream = new FileStream(translationFile, FileMode.Open, FileAccess.Read);
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        // Create new language instance for this file.
                        Language thisLanguage = new Language
                        {
                            // Language code is filename.
                            code = Path.GetFileNameWithoutExtension(translationFile),
                        };

                        // Parsing fields.
                        StringBuilder builder = new StringBuilder();
                        string key = null;
                        bool quoting = false, parseKey = true;

                        // Iterate through each line of file.
                        string line = reader.ReadLine();
                        while (line != null)
                        {
                            // Iterate through each character in line.
                            for (int i = 0; i < line.Length; ++i)
                            {
                                // Local reference.
                                char thisChar = line[i];

                                // Are we parsing quoted text?
                                if (quoting)
                                {
                                    // Is this character a quote?
                                    if (thisChar == '"')
                                    {
                                        // Is this a double quote?
                                        int j = i + 1;
                                        if (j < line.Length && line[j] == '"')
                                        {
                                            // Yes - append single quote to output and continue.
                                            i = j;
                                            builder.Append('"');
                                            continue;
                                        }

                                        // It's a single quote - stop quoting here.
                                        quoting = false;

                                        // If we're parsing a value, this is also the end of parsing this line (discard everything else).
                                        if (!parseKey)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // Not a closing quote - just append character to our parsed value.
                                        builder.Append(thisChar);
                                    }
                                }
                                else
                                {
                                    // Not parsing quoted text - is this a comma?
                                    if (thisChar == ',')
                                    {
                                        // Comma - if we're parsing a value, this is also the end of parsing this line (discard everything else).
                                        if (!parseKey)
                                        {
                                            break;
                                        }

                                        // Otherwise, what we've parsed is the key - store value and reset the builder.
                                        parseKey = false;
                                        key = builder.ToString();
                                        builder.Length = 0;
                                    }
                                    else if (thisChar == '"' & builder.Length == 0)
                                    {
                                        // If this is a quotation mark at the start of a field (immediately after comma), then we start parsing this as quoted text.
                                        quoting = true;
                                    }
                                    else
                                    {
                                        // Otherwise, just append character to our parsed string.
                                        builder.Append(thisChar);
                                    }
                                }
                            }

                            // Finished looping through chars - are we still parsing quoted text?
                            if (quoting)
                            {
                                // Yes; continue, after adding a newline.
                                builder.AppendLine();
                                goto NextLine;
                            }

                            // Was key empty?
                            if (key.IsNullOrWhiteSpace())
                            {
                                Logging.Error("invalid key in line ", line);
                                goto Reset;
                            }

                            // Did we get two delimited fields (key and value?)
                            if (parseKey | builder.Length == 0)
                            {
                                Logging.Error("no value field found in line ", line);
                                goto Reset;
                            }

                            // Convert value to string and reset builder.
                            string value = builder.ToString();
                            builder.Length = 0;

                            // Check if this entry is the language entry.
                            if (key.Equals(Language.NameKey))
                            {
                                // Language readable name.
                                thisLanguage.readableName = value;
                            }
                            else
                            {
                                // Normal entry - check for duplicates.
                                if (!thisLanguage.translationDictionary.ContainsKey(key))
                                {
                                    thisLanguage.translationDictionary.Add(key, value);
                                }
                                else
                                {
                                    Logging.Error("duplicate translation key ", key, " in file ", translationFile);
                                }
                            }

                        Reset:
                            // Reset for next line.
                            parseKey = true;

                        NextLine:
                            // Read next line.
                            line = reader.ReadLine();
                        }

                        // Did we get a valid dictionary from this?
                        if (thisLanguage.code != null && thisLanguage.translationDictionary.Count > 0)
                        {
                            // Yes - add to languages dictionary.

                            // If we didn't get a readable name, use the key instead.
                            if (thisLanguage.readableName.IsNullOrWhiteSpace())
                            {
                                thisLanguage.readableName = thisLanguage.code;
                            }

                            // Check for duplicates.
                            if (!languages.ContainsKey(thisLanguage.code))
                            {
                                Logging.Message("read translation file ", translationFile, " with language ", thisLanguage.code, " (", thisLanguage.readableName, ") with ", thisLanguage.translationDictionary.Count, " entries");
                                languages.Add(thisLanguage.code, thisLanguage);
                            }
                            else
                            {
                                Logging.Error("duplicate translation file for language ", thisLanguage.code);
                            }
                        }
                        else
                        {
                            Logging.Error("file ", translationFile, " did not produce a valid translation dictionary");
                        }
                    }
                }
                catch (Exception e)
                {
                    // Don't let a single exception stop us; keep going through remaining files.
                    Logging.LogException(e, "exception reading translation file ", translationFile);
                }
            }
        }
    }
}