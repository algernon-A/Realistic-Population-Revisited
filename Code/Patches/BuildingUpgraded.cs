﻿using System.Reflection;
using ColossalFramework.Math;
using HarmonyLib;


namespace RealPop2
{
	/// <summary>
	/// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades, applied to base game PrivateBuildingAI.BuildingUpgraded.
	/// </summary>
	[HarmonyPatch(typeof(PrivateBuildingAI), nameof(PrivateBuildingAI.BuildingUpgraded))]
	[HarmonyBefore("com.github.algernon-A.csl.ablc")]
	public static class BuildingUpgradedPatch
	{
		/// <summary>
		/// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades.
		/// </summary>
		/// <param name="__instance">Instance reference</param>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		public static void Postfix(PrivateBuildingAI __instance, ushort buildingID, ref Building data)
		{
			// Only interested in residential builidngs.
			if (__instance is ResidentialBuildingAI)
			{
				// Recalculate homecount.
				int homeCount = (__instance.CalculateHomeCount((ItemClass.Level)data.m_level, new Randomizer(buildingID), data.Width, data.Length));

				Logging.Message("residential building ", buildingID.ToString(), " (", data.Info.name, ") upgraded to level ", (data.m_level + 1).ToString(), "; calculated homecount is ", homeCount.ToString());

				// Remove any extra households.
				CitizenUnitUtils.RemoveCitizenUnits(ref data, homeCount, 0, 0);
			}
		}
	}


	/// <summary>
	/// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades, applied to Advanced Building Level Control's custom BuildingUpgraded method.
	/// Needs to be manually applied after all mods are instantiated (otherwise there's a race condition whereby if this mod is instantiated first it won't see ABLC).
	/// So, best to call from OnCreated instead of OnEnabled.
	/// </summary>
	public static class ABLCBuildingUpgradedPatch
    {
		/// <summary>
		/// Harmony Postfix patch to handle *reductions* in homecounts when a building upgrades.
		/// </summary>
		/// <param name="buildingAI">Building AI instance</param>
		/// <param name="buildingID">Building instance ID</param>
		/// <param name="data">Building data</param>
		public static void Postfix(PrivateBuildingAI buildingAI, ushort buildingID, ref Building data) => BuildingUpgradedPatch.Postfix(buildingAI, buildingID, ref data);
	}
}
