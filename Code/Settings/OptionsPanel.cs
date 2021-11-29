using System;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;


namespace RealPop2
{
    /// <summary>
    /// Class to handle the mod settings options panel.
    /// </summary>
    internal static class OptionsPanel
    {
        // Parent UI panel reference.
        internal static UIScrollablePanel optionsPanel;
        private static UIPanel gameOptionsPanel;

        // Instance reference.
        private static GameObject optionsGameObject;
        internal static bool IsOpen => optionsGameObject != null;


        /// <summary>
        /// Attaches an event hook to options panel visibility, to create/destroy our options panel as appropriate.
        /// Destroying when not visible saves UI overhead and performance impacts, especially with so many UITextFields.
        /// </summary>
        public static void OptionsEventHook()
        {
            // Get options panel instance.
            gameOptionsPanel = UIView.library.Get<UIPanel>("OptionsPanel");

            if (gameOptionsPanel == null)
            {
                Logging.Error("couldn't find OptionsPanel");
            }
            else
            {
                // Simple event hook to create/destroy GameObject based on appropriate visibility.
                gameOptionsPanel.eventVisibilityChanged += (control, isVisible) =>
                {
                    // Create/destroy based on whether or not we're now visible.
                    if (isVisible)
                    {
                        Create();
                    }
                    else
                    {
                        Close();
                    }
                };

                // Recreate panel on system locale change.
                LocaleManager.eventLocaleChanged += LocaleChanged;
            }
        }


        /// <summary>
        /// Refreshes the options panel (destroys and rebuilds) on a locale change when the options panel is open.
        /// </summary>
        public static void LocaleChanged()
        {
            if (gameOptionsPanel != null && gameOptionsPanel.isVisible)
            {
                Close();
                Create();
            }
        }


        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        private static void Create()
        {
            try
            {
                // We're now visible - create our gameobject, and give it a unique name for easy finding with ModTools.
                optionsGameObject = new GameObject("RealPopOptionsPanel");

                // Attach to game options panel.
                optionsGameObject.transform.parent = optionsPanel.transform;

                // Create a base panel attached to our game object, perfectly overlaying the game options panel.
                UIPanel basePanel = optionsGameObject.AddComponent<UIPanel>();
                basePanel.absolutePosition = optionsPanel.absolutePosition;
                basePanel.width = optionsPanel.width;
                basePanel.height = 735f;
                basePanel.clipChildren = false;

                // Add tabstrip.
                UITabstrip tabStrip = basePanel.AddUIComponent<UITabstrip>();
                tabStrip.relativePosition = new Vector3(0, 0);
                tabStrip.width = basePanel.width;
                tabStrip.height = basePanel.height;
                tabStrip.clipChildren = false;

                // Tab container (the panels underneath each tab).
                UITabContainer tabContainer = basePanel.AddUIComponent<UITabContainer>();
                tabContainer.relativePosition = new Vector3(0, 30f);
                tabContainer.width = tabStrip.width;
                tabContainer.height = tabStrip.height;
                tabContainer.clipChildren = false;
                tabStrip.tabPages = tabContainer;

                // Initialize data.
                DataUtils.Setup();

                // Add tabs and panels.
                new ModOptionsPanel(tabStrip, 0);
                new CalculationsPanel(tabStrip, 1);
                new EducationPanel(tabStrip, 2);
                new CrimePanel(tabStrip, 3);

                /*// Change tab size and text scale (to fit them all in...).
                foreach (UIButton button in tabStrip.components)
                {
                    button.textScale = 0.8f;
                    button.width = 100f;
                }*/

                // Event handler for tab index change; setup the selected tab.
                tabStrip.eventSelectedIndexChanged += (control, index) =>
                {
                    if (tabStrip.tabs[index].objectUserData is OptionsPanelTab tab)
                    {
                        tab.Setup();
                    }
                };

                // Ensure initial selected tab (doing a 'quickstep' via the second tab to ensure proper events are triggered).
                tabStrip.selectedIndex = 1;
                tabStrip.selectedIndex = 0;
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception creating options panel");
            }
        }


        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        private static void Close()
        {
            // Save settings first.
            XMLSettingsFile.Save();

            // We're no longer visible - destroy our game object.
            if (optionsGameObject != null)
            {
                GameObject.Destroy(optionsGameObject);
                optionsGameObject = null;
            }
        }
    }
}