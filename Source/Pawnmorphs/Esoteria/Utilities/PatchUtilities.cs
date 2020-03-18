// PatchUtilities.cs created by Iron Wolf for Pawnmorph on 03/18/2020 4:51 PM
// last updated 03/18/2020  4:51 PM

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
    /// <summary>
    /// static class containing various utilities for patching functions 
    /// </summary>
    public static class PatchUtilities
    {

        static PatchUtilities()
        { 
            System.Type fhUtilType = typeof(FormerHumanUtilities);
            IsAnimalMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsAnimal));
          
            IsHumanoidMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsHumanlike));
            IsToolUserMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsToolUser));
            _getRacePropsMethod = typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod();
            _getAnimalMethod = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Animal)).GetGetMethod();
            _toolUserMethod = typeof(RaceProperties).GetProperty(nameof(RaceProperties.ToolUser)).GetGetMethod();
            _getHumanlikeMethod = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Humanlike)).GetGetMethod(); 

        }


        [NotNull] private static readonly MethodInfo _getRacePropsMethod;
        [NotNull] private static readonly MethodInfo _getAnimalMethod;
        [NotNull] private static readonly MethodInfo _toolUserMethod;
        [NotNull] private static readonly MethodInfo _getHumanlikeMethod; 
        /// <summary>
        /// Gets method info for <see cref="FormerHumanUtilities.IsAnimal"/>
        /// </summary>
        /// <value>
        /// The is animal method.
        /// </value>
        [NotNull]
        public static MethodInfo IsAnimalMethod { get; }


        /// <summary>
        /// Gets the method info for <see cref="FormerHumanUtilities.IsHumanlike"/>
        /// </summary>
        /// <value>
        /// The is humanoid method.
        /// </value>
        [NotNull]
        public static MethodInfo IsHumanoidMethod { get; }
        /// <summary>
        /// Gets the method info for <see cref="FormerHumanUtilities.IsToolUser"/>
        /// </summary>
        /// <value>
        /// The is tool user method.
        /// </value>
        [NotNull]
        public static MethodInfo IsToolUserMethod { get; }

        /// <summary>
        /// substitutes all instances of RaceProps Humanlike, Animal, and Tooluser with their equivalent in FormerHumanUtilities
        /// </summary>
        /// <param name="codeInstructions">The code instructions.</param>
        /// <exception cref="System.ArgumentNullException">codeInstructions</exception>
        public static void SubstituteFormerHumanMethods([NotNull] IList<CodeInstruction> codeInstructions)
        {
            if (codeInstructions == null) throw new ArgumentNullException(nameof(codeInstructions));

            for (int i = 0; i < codeInstructions.Count - 1; i++)
            {
                int j = i + 1;
                var opI = codeInstructions[i];
                var opJ = codeInstructions[j];
                if(opI ==null || opJ == null) continue;
                //the segment we're interested in always start with pawn.get_RaceProps() (ie pawn.RaceProps) 
                if (opI.opcode == OpCodes.Callvirt && ((MethodInfo) opI.operand )== _getRacePropsMethod) 
                {
                    //signatures we care about always have a second callVirt 
                    if(opJ.opcode != OpCodes.Callvirt) continue;

                    var jMethod = opJ.operand as MethodInfo;
                    bool patched; 
                    //figure out which method, if any, we're going to be replacing 
                    if (jMethod == _getHumanlikeMethod)
                    {
                        patched = true;
                        opI.operand = IsHumanoidMethod; 
                    }else if (jMethod == _getAnimalMethod)
                    {
                        patched = true;
                        opI.operand = IsAnimalMethod; 
                    }else if (jMethod == _toolUserMethod)
                    {
                        patched = true;
                        opI.operand = IsToolUserMethod;
                    }
                    else
                        patched = false;

                    if (patched)
                    {
                        //now clean up if we did any patching 
                        
                        opI.opcode = OpCodes.Call;//always uses call 
                        
                        //replace opJ with nop (no operation) so we don't fuck up the stack 
                        opJ.opcode = OpCodes.Nop;
                        opJ.operand = null; 
                    }
                }

            }

        }

    }
}