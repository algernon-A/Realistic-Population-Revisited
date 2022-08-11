// <copyright file="CommercialSimStepPatch.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace RealPop2
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Reflection.Emit;
	using AlgernonCommons;
	using HarmonyLib;

	/// <summary>
	/// Hamony patch to cap commercial building incoming goods demand.
	/// </summary>
	[HarmonyPatch]
	[HarmonyPatch(typeof(CommercialBuildingAI), "SimulationStepActive")]
	public static class CommercialSimStepPatch
	{
		/// <summary>
		/// Harmony transpiler for CommercialBuildingAI.SimulationStep, to insert upper limit to perceived goods requirement.
		/// </summary>
		/// <param name="original">Original method.</param>
		/// <param name="instructions">Original ILCode.</param>
		/// <param name="generator">IL generator.</param>
		/// <returns>Patched ILCode.</returns>
		public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			/* Changing:
			 * int num6 = Mathf.Max(num5, num2 * 4);
			 * To:
			 * int num6 = Mathf.Min(Mathf.Max(num5, num2 * 4), MaxGoodsDemand;
			 * 
			 * This ensures that outstanding goods demand is capped at MaxGoodsDemand, so the building won't order more goods beyond that point.
			 * 
			 * Finding this is easy, as it's the only call in this method (of any kind) immediately after a mul.
			 */

			// Status flag.
			bool isPatched = false;


			// Instruction parsing.
			IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
			CodeInstruction instruction;

			// Iterate through all instructions in original method.
			while (instructionsEnumerator.MoveNext())
			{
				// Get next instruction and add it to output.
				instruction = instructionsEnumerator.Current;
				yield return instruction;

				// Looking for possible precursor calls to "call Math.Max".
				if (!isPatched && instruction.opcode == OpCodes.Mul)
				{
					// Found mul - are there following instructions?
					if (instructionsEnumerator.MoveNext())
					{
						// Yes - get the next instruction.
						instruction = instructionsEnumerator.Current;
						yield return instruction;

						// Is this new instruction a call to int32 Math.Max?

						if (instruction.opcode == OpCodes.Call && instruction.operand.ToString().Equals("Int32 Max(Int32, Int32)"))
						{
							// Yes - insert call to new Math.Min(x, MaxGoodsDemand) after original call.
							Logging.KeyMessage("transpiler adding MaxGoodsDemand check after Int32 Max(Int32, Int32)");
							yield return new CodeInstruction(OpCodes.Ldarg_0);
							yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GoodsUtils), nameof(GoodsUtils.GetInventoryCap), new Type[] { typeof(CommercialBuildingAI) }));
							yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Min), new Type[] { typeof(int), typeof(int) }));

							// Set flag.
							isPatched = true;
						}
					}
				}
			}
		}
	}
}
