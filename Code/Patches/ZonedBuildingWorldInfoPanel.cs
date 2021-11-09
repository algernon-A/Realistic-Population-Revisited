using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;


namespace RealPop2
{
    /// <summary>
    /// Harmony Postfix patch to add visitor count display to commercial info panels.
    /// </summary>
    [HarmonyPatch(typeof(ZonedBuildingWorldInfoPanel), "UpdateBindings")]
    public static class InfoPanelPatch
    {
        // Visitor label reference.
        public static UILabel visitLabel;


        /// <summary>
        /// Harmony Postfix patch to ZonedBuildingWorldInfoPanel.UpdateBindings to display visitor counts for commercial buildings.
        /// </summary>
        public static void Postfix()
        {
            // Currently selected building.
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;

            // Create visit label if it isn't already set up.
            if (visitLabel == null)
            {
                // Get info panel.
                ZonedBuildingWorldInfoPanel infoPanel = UIView.library.Get<ZonedBuildingWorldInfoPanel>(typeof(ZonedBuildingWorldInfoPanel).Name);

                // Add current visitor count label.
                visitLabel = UIControls.AddLabel(infoPanel.component, 65f, 280f, Translations.Translate("RPR_INF_VIS"), textScale: 0.75f);
                visitLabel.textColor = new Color32(185, 221, 254, 255);
                visitLabel.font = Resources.FindObjectsOfTypeAll<UIFont>().FirstOrDefault((UIFont f) => f.name == "OpenSans-Regular");

                // Position under existing Highly Educated workers count row in line with total workplace count label.
                UIComponent situationLabel = infoPanel.Find("WorkSituation");
                UIComponent workerLabel = infoPanel.Find("HighlyEducatedWorkers");
                if (situationLabel != null && workerLabel != null)
                {
                    visitLabel.absolutePosition = new Vector2(situationLabel.absolutePosition.x, workerLabel.absolutePosition.y + 25f);
                }
                else
                {
                    Logging.Error("couldn't find ZonedBuildingWorldInfoPanel components");
                }
            }

            // Local references.
            Building[] buildingBuffer = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            BuildingInfo buildingInfo = buildingBuffer[building].Info;

            // Is this a commercial building?
            CommercialBuildingAI commercialAI = buildingInfo.GetAI() as CommercialBuildingAI;
            if (commercialAI == null)
            {
                // Not a commercial building - hide the label.
                visitLabel.Hide();
            }
            else
            {
                // Commercial building - show the label.
                visitLabel.Show();

                // Get current visitor count.
                int aliveCount = 0, totalCount = 0;
                Citizen.BehaviourData behaviour = new Citizen.BehaviourData();
                GetVisitBehaviour(commercialAI, building, ref buildingBuffer[building], ref behaviour, ref aliveCount, ref totalCount);

                // Display visitor count.
                visitLabel.text = totalCount.ToString() + " / " + commercialAI.CalculateVisitplaceCount((ItemClass.Level)buildingBuffer[building].m_level, new ColossalFramework.Math.Randomizer(building), buildingBuffer[building].Width, buildingBuffer[building].Length).ToString() +  " " + Translations.Translate("RPR_INF_VIS");
            }
        }


        /// <summary>
        /// Reverse patch for CommonBuildingAI.GetVisitBehaviour to access private method of original instance.
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="citizenID">ID of this building (for game method)</param>
        /// <param name="buildingData">Building data reference (for game method)</param>
        /// <param name="behaviour">Citizen behaviour reference (for game method)</param>
        /// <param name="aliveCount">Alive citizen count</param>
        /// <param name="totalCount">Total citizen count</param>
        [HarmonyReversePatch]
        [HarmonyPatch((typeof(CommonBuildingAI)), "GetVisitBehaviour")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetVisitBehaviour(object instance, ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveCount, ref int totalCount)
        {
            string message = "GetVisitBehaviour reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, buildingID, buildingData, behaviour, aliveCount, totalCount);
            throw new NotImplementedException(message);
        }
    }
}