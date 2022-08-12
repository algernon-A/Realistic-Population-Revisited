// <copyright file="ModifyMaterialBufferFix.cs" company="algernon (K. Algernon A. Sheppard)">
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
    /// Harmony patch to fix a game bug whereby incoming goods amounts (uint16) can overflow and wrap-around.
    /// </summary>
    [HarmonyPatch(typeof(CommercialBuildingAI), nameof(CommercialBuildingAI.ModifyMaterialBuffer))]
    public static class ModifyMaterialBufferFix
    {
        /// <summary>
        /// Harmony transpiler for CommercialBuildingAI.ModifyMaterialBuffer, to insert a goods consumed multiplier and a custom call to fix a game bug (no bounds check on uint16).
        /// </summary>
        /// <param name="original">Original method.</param>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="generator">IL generator.</param>
        /// <returns>Patched ILCode.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Two insertions to make here.

            /*
             * Inserting:
             * amountDelta = (amountDelta * GoodsUtils.GetComMult(ref building)) / 100
             *
             * Just after:
             * int customBuffer = data.m_customBuffer2;
             *
             * To implement custom consumer consumption multiplier.
             */

            /* Inserting a call to our custom buffer overflow check method, so:
             * amountDelta = Mathf.Clamp(amountDelta, 0, num3 - customBuffer2)
             * Becomes:
             * amountDelta = BufferOverflowCheck(Mathf.Clamp(amountDelta, 0, num3 - customBuffer2), customBuffer2);
             *
             * This will be inserted at:
             * sub
             * call int32 [UnityEngine]UnityEngine.Mathf::Clamp(int32, int32, int32)
             *
             * [insert here]
             *
             * stind.i4
             *
             * To fix game bug where uint16 overflows can occur here.
             */

            // ILCode local variable indexes.
            const int CustomBuffer2VarIndex = 7;

            // Status flag.
            bool isFirstPatched = false, isSecondPatched = false;

            // Instruction parsing.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            CodeInstruction instruction;

            // Iterate through all instructions in original method.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                instruction = instructionsEnumerator.Current;
                yield return instruction;

                // Fist patch - looking for first field call to get m_customBuffer2.
                if (!isFirstPatched && instruction.opcode == OpCodes.Ldfld && instruction.operand.ToString().Equals("System.UInt16 m_customBuffer2"))
                {
                    // Found it - are there following instructions?
                    if (instructionsEnumerator.MoveNext())
                    {
                        // Yes - get the next instruction.
                        instruction = instructionsEnumerator.Current;
                        yield return instruction;

                        // Check if this one is storing the result in local variable 0 (customBuffer)
                        if (instruction.opcode == OpCodes.Stloc_0)
                        {
                            // Yes - insert multiplier code here.
                            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
                            yield return new CodeInstruction(OpCodes.Ldind_I4);
                            yield return new CodeInstruction(OpCodes.Ldarg_2);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GoodsUtils), nameof(GoodsUtils.GetComMult), new Type[] { typeof(Building).MakeByRefType() }));
                            yield return new CodeInstruction(OpCodes.Mul);
                            yield return new CodeInstruction(OpCodes.Ldc_I4, 100);
                            yield return new CodeInstruction(OpCodes.Div);
                            yield return new CodeInstruction(OpCodes.Stind_I4);
                            isFirstPatched = true;
                        }
                    }
                }

                // Looking for possible precursor calls to "call Math.Max".
                if (!isSecondPatched && instruction.opcode == OpCodes.Sub)
                {
                    // Found sub - are there following instructions?
                    if (instructionsEnumerator.MoveNext())
                    {
                        // Yes - get the next instruction.
                        instruction = instructionsEnumerator.Current;
                        yield return instruction;

                        // Is this new instruction a call to Math.Clamp?
                        if (instruction.opcode == OpCodes.Call && instruction.operand.ToString().Equals("Int32 Clamp(Int32, Int32, Int32)"))
                        {
                            // Yes - insert call to BufferOverflowCheck.
                            Logging.KeyMessage("transpiler adding call to BufferOverflowCheck in amountDelta = Mathf.Clamp(amountDelta, 0, num3 - customBuffer2)");
                            yield return new CodeInstruction(OpCodes.Ldloc_S, CustomBuffer2VarIndex);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModifyMaterialBufferFix), nameof(ModifyMaterialBufferFix.BufferOverflowCheck)));

                            // Set flag.
                            isSecondPatched = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Perfoms a uint16 buffer overflow check to prevent incoming goods overflows.
        /// </summary>
        /// <param name="amountDelta">Incoming goods load amount.</param>
        /// <param name="customBuffer">Current storage buffer amount.</param>
        /// <returns>Incoming goods load amount reduced to prevent any overflows.</returns>
        public static int BufferOverflowCheck(int amountDelta, int customBuffer)
        {
            if (customBuffer + amountDelta > 65500)
            {
                Logging.Message("caught incoming commercial goods overflow");
                amountDelta = 65500 - customBuffer;
            }

            return amountDelta;
        }
    }
}