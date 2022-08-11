// <copyright file="UIThreading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using AlgernonCommons;
    using AlgernonCommons.Keybinding;
    using AlgernonCommons.UI;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// Threading to capture hotkeys.
    /// </summary>
    public class UIThreading : ThreadingExtensionBase
    {
        // Instance reference.
        private static UIThreading s_instance;

        // Hotkey.
        private static Keybinding s_hotKey = new Keybinding(KeyCode.E, false, false, true);

        // Flags.
        private bool _operating = true;
        private bool _processed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIThreading"/> class.
        /// </summary>
        public UIThreading()
        {
            // Set instance reference.
            s_instance = this;
        }

        /// <summary>
        /// Sets a value indicating whether hotkey detection is active.
        /// </summary>
        internal static bool Operating
        {
            set
            {
                if (s_instance != null)
                {
                    s_instance._operating = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the hotkey.
        /// </summary>
        internal static Keybinding HotKey { get => s_hotKey; set => s_hotKey = value; }

        /// <summary>
        /// Look for keypress to open GUI.
        /// </summary>
        /// <param name="realTimeDelta">Real-time delta since last update.</param>
        /// <param name="simulationTimeDelta">Simulation time delta since last update.</param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Don't do anything if not active.
            if (_operating)
            {
                // Has hotkey been pressed?
                if (s_hotKey.IsPressed())
                {
                    // Cancel if key input is already queued for processing.
                    if (_processed) return;

                    _processed = true;

                    try
                    {
                        // Is options panel open?  If so, we ignore this and don't do anything.
                        if (!OptionsPanelManager<OptionsPanel>.IsOpen)
                        {
                            BuildingDetailsPanel.Open();
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e, "exception opening building details panel");
                    }
                }
                else
                {
                    // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                    _processed = false;
                }
            }
        }
    }
}