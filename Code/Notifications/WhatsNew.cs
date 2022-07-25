using System;
using System.Reflection;
using RealPop2.MessageBox;


namespace RealPop2
{
    /// <summary>
    /// "What's new" message box.  Based on macsergey's code in Intersection Marking Tool (Node Markup) mod.
    /// </summary>
    internal static class WhatsNew
    {
        // List of versions and associated update message lines (as translation keys).
        private readonly static WhatsNewMessage[] WhatsNewMessages = new WhatsNewMessage[]
        {
            new WhatsNewMessage
            {
                version = new Version("2.0.1.0"),
                versionHeader = "",
                messageKeys = true,
                messages = new string[]
                {
                    "RPR_201_0"
                }
            },
            new WhatsNewMessage
            {
                version = new Version("2.0.0.0"),
                versionHeader = "",
                messageKeys = true,
                messages = new string[]
                {
                    "RPR_200_0",
                    "RPR_200_1",
                    "RPR_200_2",
                    "RPR_200_3",
                    "RPR_200_4",
                    "RPR_200_5",
                    "RPR_200_6",
                    "RPR_200_7",
                    "RPR_200_8",
                    "RPR_200_9"
                }
            }
        };


        /// <summary>
        /// Close button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal static bool Confirm() => true;

        /// <summary>
        /// 'Don't show again' button action.
        /// </summary>
        /// <returns>True (always)</returns>
        internal static bool DontShowAgain()
        {
            // Save current version to settings file.
            ModSettings.whatsNewVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            XMLSettingsFile.Save();

            return true;
        }


        /// <summary>
        /// Check if there's been an update since the last notification, and if so, show the update.
        /// </summary>
        internal static void ShowWhatsNew()
        {
            // Get last notified version and current mod version.
            Version whatsNewVersion = new Version(ModSettings.whatsNewVersion);
            WhatsNewMessage latestMessage = WhatsNewMessages[0];

            // Don't show notification if we're already up to (or ahead of) the first what's new message (including Beta updates).
            if (whatsNewVersion < latestMessage.version)
            {
                // Show messagebox.
                WhatsNewMessageBox messageBox = MessageBoxBase.ShowModal<WhatsNewMessageBox>();
                messageBox.Title = Mod.ModName + " " + Mod.Version;
                messageBox.DSAButton.eventClicked += (component, clickEvent) => DontShowAgain();
                messageBox.SetMessages(whatsNewVersion, WhatsNewMessages);
            }
        }
    }


    /// <summary>
    /// Version message struct.
    /// </summary>
    public struct WhatsNewMessage
    {
        public Version version;
        public string versionHeader;
        public bool messageKeys;
        public string[] messages;
    }
}