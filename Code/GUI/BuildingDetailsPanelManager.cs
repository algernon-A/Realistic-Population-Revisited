// <copyright file="BuildingDetailsPanelManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
    using System;
    using AlgernonCommons;
    using AlgernonCommons.Translation;
    using AlgernonCommons.UI;
    using ColossalFramework;
    using ColossalFramework.UI;
    using UnityEngine;

    /// <summary>
    /// Buidling details panel manager static class.
    /// </summary>
    public static class BuildingDetailsPanelManager
    {
        // Instance references.
        private static GameObject s_uiGameObject;
        private static BuildingDetailsPanel s_panel;

        // Previous selection.
        private static BuildingInfo s_lastSelection;
        private static bool[] s_lastFilter;
        private static int s_lastPostion = 0;
        private static int s_lastIndex = -1;

        // Info panel buttons.
        private static UIButton s_zonedButton;
        private static UIButton s_serviceButton;

        /// <summary>
        /// Gets the current panel instance.
        /// </summary>
        public static BuildingDetailsPanel Panel => s_panel;

        /// <summary>
        /// Creates the panel object in-game and displays it.
        /// </summary>
        /// <param name="selected">Selected building prefab to display (null if none).</param>
        internal static void Open(BuildingInfo selected = null)
        {
            try
            {
                // If no instance already set, create one.
                if (s_uiGameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    s_uiGameObject = new GameObject("RealPopBuildingDetails");
                    s_uiGameObject.transform.parent = UIView.GetAView().transform;

                    s_panel = s_uiGameObject.AddComponent<BuildingDetailsPanel>();

                    // Create panel close event handler.
                    s_panel.EventClose += DestroyPanel;
                }

                // Select appropriate building if there's a preselection.
                if (selected != null)
                {
                    Panel.SelectBuilding(selected);
                }
                else if (s_lastSelection != null)
                {
                    // Restore previous filter state.
                    if (s_lastFilter != null)
                    {
                        Panel.SetFilter(s_lastFilter);
                    }

                    // Restore previous building selection list postion and selected item (specifically in that order to ensure correct item is selected).
                    s_panel.SetListPosition(s_lastIndex, s_lastPostion);
                    s_panel.SelectBuilding(s_lastSelection);
                }

                Panel.Show();
            }
            catch (Exception e)
            {
                Logging.LogException(e, "exception opening building panel");
                return;
            }
        }

        /// <summary>
        /// Closes the panel by destroying the object (removing any ongoing UI overhead).
        /// </summary>
        internal static void DestroyPanel()
        {
            // Save current selection for next time.
            s_lastSelection = s_panel?.CurrentSelection;
            s_lastFilter = s_panel?.GetFilter();
            Panel?.GetListPosition(out s_lastIndex, out s_lastPostion);

            // Destroy objects and nullify for GC.
            GameObject.Destroy(s_panel);
            GameObject.Destroy(s_uiGameObject);
            s_panel = null;
            s_uiGameObject = null;
        }

        /// <summary>
        /// Adds button to access building details from building info panels.
        /// </summary>
        internal static void AddInfoPanelButton()
        {
            // Zoned building panel - get parent panel and add button.
            ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);
            if (infoPanel == null)
            {
                Logging.Error("unable to find zoned building info panel");
                return;
            }

            s_zonedButton = UIButtons.AddButton(infoPanel.component, infoPanel.component.width - 133f - 10, 120, Translations.Translate("RPR_REALPOP"), 133f, 19.5f, 0.65f);
            s_zonedButton.textPadding = new RectOffset(2, 2, 4, 0);

            // Just in case other mods are interfering.
            s_zonedButton.Enable();

            // Event handler.
            s_zonedButton.eventClick += (c, p) =>
            {
                // Select current building in the building details panel and show.
                Open(InstanceManager.GetPrefabInfo(WorldInfoPanel.GetCurrentInstanceID()) as BuildingInfo);
            };

            // Service building panel - get parent panel and add button.
            CityServiceWorldInfoPanel servicePanel = UIView.library.Get<CityServiceWorldInfoPanel>(typeof(CityServiceWorldInfoPanel).Name);
            if (servicePanel == null)
            {
                Logging.Error("unable to find service building info panel");
                return;
            }

            s_serviceButton = UIButtons.AddButton(servicePanel.component, servicePanel.component.width - 133f - 10, 120, Translations.Translate("RPR_REALPOP"), 133f, 19.5f, 0.65f);

            s_serviceButton.textPadding = new RectOffset(2, 2, 4, 0);

            // Event handler.
            s_serviceButton.eventClick += (c, p) =>
            {
                // Select current building in the building details panel and show.
                Open(InstanceManager.GetPrefabInfo(WorldInfoPanel.GetCurrentInstanceID()) as BuildingInfo);
            };
        }

        /// <summary>
        /// Updates the state of the service panel Realistic Population button - should only be visible and enabled when looking at elementary and high schools.
        /// </summary>
        internal static void UpdateServicePanelButton()
        {
            // Get current building instance.
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Ensure valid building before proceeding.
            if (building > 0)
            {
                // Check for eduction service and level 1 or 2.
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info;
                if (info.GetService() == ItemClass.Service.Education && info.GetClassLevel() < ItemClass.Level.Level3)
                {
                    // It's a school!  Enable and show the button, and return.
                    s_serviceButton.Enable();
                    s_serviceButton.Show();
                    return;
                }
            }

            // If we got here, it's not a valid school building; disable and hide the button.
            s_serviceButton.Disable();
            s_serviceButton.Hide();
        }
    }
}