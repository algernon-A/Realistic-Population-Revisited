// <copyright file="HandleCrimeTranspiler.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache license. See LICENSE.txt file in the project root for full license information.
// </copyright>
    
namespace RealPop2
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;

    /// <summary>
    /// Transpiler to patch CommonBuildingAI.HandleCrime.
    /// This implements the mods custom crime settings.
    /// </summary>
    [HarmonyPatch(typeof(CommonBuildingAI), "HandleCrime")]
    public static class HandleCrimeTranspiler
    {
        // Inserting a call to our custom replacement method after these instructions (normally at the start of the method):
        /* // if (crimeAccumulation != 0)
         * IL_0000: ldarg.3
         * IL_0001: brfalse IL_00a3
         */

        // The following code is inserted for the call "crimeAccumulation = RealisticCrime();" (crimeAccumulation is argument 3 of HandleCrime)
        /* 
         * ldarg.3
         * call HandleCrimeTranspiler.RealisticCrime
         * starg.s 3
        */

        /// <summary>
        /// StartConnectionTransferImpl transpiler.
        /// </summary>
        /// <param name="original">Original method to patch.</param>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="generator">ILCode generator.</param>
        /// <returns>Replacement ILCode.</returns>
        public static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Transpiler meta.
            IEnumerator<CodeInstruction> instructionsEnumerator = instructions.GetEnumerator();
            bool inserted = false;


            Logging.Message("transpiling CommonBuildingAI.HandleCrime");

            // Iterate through ILCode instructions.
            while (instructionsEnumerator.MoveNext())
            {
                // Get next instruction and add it to output.
                CodeInstruction instruction = instructionsEnumerator.Current;
                yield return instruction;

                // If we haven't inserted our code yet, we need to check to see if we're at the right spot to do so.
                if (!inserted)
                {
                    // Haven't yet patched.  Check to see if we have a pattern match - looking for ldarg.3 followed by brfalse.
                    if (instruction.opcode == OpCodes.Ldarg_3)
                    {
                        // Got an  ldarg.3 - add it to output and then check for a following brfalse.
                        instructionsEnumerator.MoveNext();
                        instruction = instructionsEnumerator.Current;
                        yield return instruction;
                        
                        if (instruction.opcode == OpCodes.Brfalse)
                        {
                            // Found our spot! Insert our custom instructions in the output.
                            Logging.Message("inserting custom method call after ldarg.3, brfalse");

                            // Push crimeAccumulation onto stack as argument for patch method.
                            yield return new CodeInstruction(OpCodes.Ldarg_3);

                            // Push building data reference onto stack as argument for patch method.
                            yield return new CodeInstruction(OpCodes.Ldarg_2);

                            // Add call to patch method.
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HandleCrimeTranspiler), nameof(HandleCrimeTranspiler.RealisticCrime)));

                            // Store patch method return value as crimeAccumulation (argument 3).
                            yield return new CodeInstruction(OpCodes.Starg_S, 3);

                            // Set flag to indicate that insertion is complete.
                            inserted = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adjusts crime accumulation for mod calculations.
        /// </summary>
        /// <param name="crimeAccumulation">Raw crime accumulation.</param>
        /// <param name="data">Building data.</param>
        /// <returns>Modified crime accumulation.</returns>
        public static int RealisticCrime(int crimeAccumulation, ref Building data)
        {
            int pereentageFactor = 10;
            ItemClass itemClass = data.Info.m_class;

            // Higher levels of building have lower crime rates.
            int levelBasedReduction = (int)((itemClass.m_level >= 0) ? itemClass.m_level : 0);

            // Reduce crime rates for high density buildings and offices, due to the larger number of workers/households with this mod's settings.
            if (itemClass.m_service == ItemClass.Service.Residential)
            {
                // Increase base percentage factor to account for five levels, not three.
                pereentageFactor = 16;

                if (itemClass.m_subService == ItemClass.SubService.ResidentialHigh)
                {
                    crimeAccumulation /= 2;
                }
            }
            else if (itemClass.m_service == ItemClass.Service.Office)
            {
                crimeAccumulation /= 5;
            }
            else if (itemClass.m_subService == ItemClass.SubService.CommercialHigh)
            {
                crimeAccumulation /= 3;
            }

            // Percentage reduction.
            double reductionPercentage = (pereentageFactor - levelBasedReduction) / pereentageFactor;
            crimeAccumulation = (int)(crimeAccumulation * reductionPercentage);

            // Crime multiplier application.
            return (int)((crimeAccumulation * ModSettings.crimeMultiplier) / 100f);
        }
    }
}