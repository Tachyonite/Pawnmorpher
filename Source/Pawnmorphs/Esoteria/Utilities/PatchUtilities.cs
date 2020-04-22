// PatchUtilities.cs created by Iron Wolf for Pawnmorph on 03/18/2020 4:51 PM
// last updated 03/18/2020  4:51 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
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
            IsAnimalMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsAnimal), new[] {typeof(Pawn)});
          
            IsHumanoidMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsHumanlike), new [] {typeof(Pawn)});
            IsToolUserMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsToolUser), new []{typeof(Pawn)});
            _getRacePropsMethod = typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod();
            _getAnimalMethod = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Animal)).GetGetMethod();
            _toolUserMethod = typeof(RaceProperties).GetProperty(nameof(RaceProperties.ToolUser)).GetGetMethod();
            _getHumanlikeMethod = typeof(RaceProperties).GetProperty(nameof(RaceProperties.Humanlike)).GetGetMethod();
            AllFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            CommonTranspiler = typeof(PatchUtilities).GetMethod(nameof(SubstituteFormerHumanMethodsPatch)); 
        }

        /// <summary>
        /// MethodInfo for a common transpiler method that replaces all instances of RaceProps.Animal/Tooluser/Humanlike
        /// with the FormerHumanUtilities equivalents 
        /// </summary>
        [NotNull]
        public static MethodInfo CommonTranspiler { get; }

        /// <summary>
        /// Determines whether this type is compiler generated.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if this type is compiler generated; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public static bool IsCompilerGenerated([NotNull] this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.HasAttribute<CompilerGeneratedAttribute>()) return true;
            if (type.Name.Contains("<") || type.Name.Contains(">")) return true;
            return false; 
        }

        private static BindingFlags AllFlags { get; }

        /// <summary>
        /// patches every method in the given type, including sub types and delegates, with the given transpiler 
        /// </summary>
        /// <param name="harmony">The harmony.</param>
        /// <param name="type">The type.</param>
        /// <param name="transpiler">The transpiler.</param>
        /// <param name="methodInfoPredicate">The method information predicate.</param>
        /// <exception cref="ArgumentNullException">
        /// harmony
        /// or
        /// type
        /// or
        /// transpiler
        /// </exception>
        public static void MassIlPatchType([NotNull] this Harmony harmony, [NotNull] Type type, [NotNull] MethodInfo transpiler, Predicate<MethodInfo> methodInfoPredicate = null)
        {
            if (harmony == null) throw new ArgumentNullException(nameof(harmony));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (transpiler == null) throw new ArgumentNullException(nameof(transpiler));
            
            List<MethodInfo> methodInfoList = new List<MethodInfo>();
            methodInfoList.AddRange(type.GetMethods(AllFlags).Where(m => methodInfoPredicate?.Invoke(m) != false));

            List<Type> internalTypes = new List<Type>();
            GetInternalTypes(type, internalTypes);
            methodInfoList.AddRange(internalTypes.SelectMany(t => t.GetMethods(AllFlags)
                                                                   .Where(m => methodInfoPredicate?.Invoke(m) != false)));


            foreach (MethodInfo methodInfo in methodInfoList)
            {

                try
                {

                    var hMethod = new HarmonyMethod(transpiler);
                    harmony.Patch(methodInfo, transpiler: hMethod);

                }
                catch (Exception e)
                {
                    Log.Error($"encountered {e.GetType().Name} while trying to patch \"{methodInfo.Name}\" with transpiler \"{transpiler.Name}\"\n{e}");
                }

            }

        }

        /// <summary>
        /// patch the given method,  replacing all instances of RaceProps.Animal/Tooluser/Humanlike
        /// with the FormerHumanUtilities equivalents 
        /// </summary>
        /// <param name="harmony"></param>
        /// <param name="targetMethod"></param>
        public static void ILPatchCommonMethods([NotNull] this Harmony harmony, [NotNull] MethodInfo targetMethod)
        {
            if (harmony == null) throw new ArgumentNullException(nameof(harmony));
            if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

            var hTs = new HarmonyMethod(CommonTranspiler);
            try
            {
                harmony.Patch(targetMethod, transpiler: hTs);
            }
            catch (Exception e)
            {
                Log.Error($"encountered {e.GetType().Name} while patching {targetMethod.Name}\n{e}");
            }
        }

        static void GetInternalTypes([NotNull] Type type, [NotNull] List<Type> outList)
        {
            var internalTypes = type.GetNestedTypes(AllFlags);

            foreach (Type internalType in internalTypes)
            {
                GetInternalTypes(internalType, outList); 
            }

            outList.AddRange(internalTypes); 
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


        /// <summary>
        /// substitutes all instances of RaceProps Humanlike, Animal, and Tooluser with their equivalent in FormerHumanUtilities
        /// </summary>
        /// <param name="instructions">The code instructions.</param>
        /// <exception cref="System.ArgumentNullException">codeInstructions</exception>
        public static IEnumerable<CodeInstruction> SubstituteFormerHumanMethodsPatch([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            if (instructions == null) throw new ArgumentNullException(nameof(instructions));
            List<CodeInstruction> codeInstructions = instructions.ToList();
            for (var i = 0; i < codeInstructions.Count - 1; i++)
            {
                int j = i + 1;
                CodeInstruction opI = codeInstructions[i];
                CodeInstruction opJ = codeInstructions[j];
                if (opI == null || opJ == null) continue;
                //the segment we're interested in always start with pawn.get_RaceProps() (ie pawn.RaceProps) 
                if (opI.opcode == OpCodes.Callvirt && (MethodInfo) opI.operand == _getRacePropsMethod)
                {
                    //signatures we care about always have a second callVirt 
                    if (opJ.opcode != OpCodes.Callvirt) continue;

                    var jMethod = opJ.operand as MethodInfo;
                    bool patched;
                    //figure out which method, if any, we're going to be replacing 
                    if (jMethod == _getHumanlikeMethod)
                    {
                        patched = true;
                        opI.operand = IsHumanoidMethod;
                    }
                    else if (jMethod == _getAnimalMethod)
                    {
                        patched = true;
                        opI.operand = IsAnimalMethod;
                    }
                    else if (jMethod == _toolUserMethod)
                    {
                        patched = true;
                        opI.operand = IsToolUserMethod;
                    }
                    else
                    {
                        patched = false;
                    }

                    if (patched)
                    {
                        //now clean up if we did any patching 

                        opI.opcode = OpCodes.Call; //always uses call 

                        //replace opJ with nop (no operation) so we don't fuck up the stack 
                        opJ.opcode = OpCodes.Nop;
                        opJ.operand = null;
                    }
                }
            }

            return codeInstructions; 
        }
    }
}