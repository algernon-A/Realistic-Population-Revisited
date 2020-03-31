﻿using System;
using System.IO;
using ColossalFramework.Plugins;
using System.Text;
using System.Collections.Generic;

namespace RealisticPopulationRevisited
{
    class Debugging
    {
        private static StringBuilder sb = new StringBuilder();
        private static Dictionary<String, int> messagesToSuppress = new Dictionary<string, int>();


        // Buffer warning
        public static void bufferWarning(string text)
        {
            sb.AppendLine("Realistic Population Revisited: " + text);
        }

        // Output buffer
        public static void releaseBuffer()
        {
            if (sb.Length > 0)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Warning, sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }
    }
}