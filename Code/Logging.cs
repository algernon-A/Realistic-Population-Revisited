using System;
using System.Text;
using UnityEngine;


namespace RealPop2
{
    /// <summary>
    /// Logging utility class.
    /// </summary>
    internal static class Logging
    {
        // Logging detail flag.
        internal static bool detailLogging = false;

        // Stringbuilder for messaging.
        private static StringBuilder message = new StringBuilder(128);


        /// <summary>
        /// Prints a single-line debugging message to the Unity output log with an "ERROR: " prefix, regardless of the 'detailed logging' setting.
        /// </summary>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        internal static void Error(params object[] messages) => WriteMessage("ERROR: ", messages);


        /// <summary>
        /// Prints a single-line debugging message to the Unity output log, regardless of the 'detailed logging' setting.
        /// </summary>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        internal static void KeyMessage(params object[] messages) => WriteMessage(String.Empty, messages);


        /// <summary>
        /// Prints a single-line debugging message to the Unity output log if the 'detailed logging' option is set (otherwise does nothing).
        /// </summary>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        internal static void Message(params object[] messages)
        {
            if (detailLogging)
            {
                WriteMessage(String.Empty, messages);
            }
        }


        /// <summary>
        /// Prints an exception message to the Unity output log.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        internal static void LogException(Exception exception, params object[] messages)
        {
            // Use StringBuilder for efficiency since we're doing a lot of manipulation here.
            // Start with mod name (to easily identify relevant messages), followed by colon to indicate start of actual message.
            message.Length = 0;
            message.Append(Mod.ModName);
            message.Append(": ");

            // Add each message parameter.
            for (int i = 0; i < messages.Length; ++i)
            {
                message.Append(messages[i]);
            }

            // Finish with a new line and the exception information.
            message.AppendLine();
            message.AppendLine("Exception: ");
            message.AppendLine(exception.Message);
            message.AppendLine(exception.Source);
            message.AppendLine(exception.StackTrace);

            // Log inner exception as well, if there is one.
            if (exception.InnerException != null)
            {
                message.AppendLine("Inner exception:");
                message.AppendLine(exception.InnerException.Message);
                message.AppendLine(exception.InnerException.Source);
                message.AppendLine(exception.InnerException.StackTrace);
            }

            // Write to log.
            Debug.Log(message);
        }


        /// <summary>
        /// Prints a single-line debugging message to the Unity output log with a specified prefix.
        /// </summary>
        /// <param name="prefix">Prefix for message, if any</param>
        /// <param name="messages">Message to log (individual strings will be concatenated)</param>
        private static void WriteMessage(string prefix, params object[] messages)
        {
            // Use StringBuilder for efficiency since we're doing a lot of manipulation here.
            // Start with mod name (to easily identify relevant messages), followed by colon to indicate start of actual message.
            message.Length = 0;
            message.Append(Mod.ModName);
            message.Append(": ");

            // Append prefix.
            message.Append(prefix);

            // Add each message parameter.
            for (int i = 0; i < messages.Length; ++i)
            {
                message.Append(messages[i]);
            }

            // Terminating period to confirm end of messaage..
            message.Append(".");

            Debug.Log(message);
        }
    }
}
