﻿// PatchUtilities.cs created by Iron Wolf for Pawnmorph on 03/18/2020 4:51 PM
// last updated 03/18/2020  4:51 PM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Utilities
{
    /// <summary>
    ///     static class containing various utilities for patching functions
    /// </summary>
    public static class PatchUtilities
    {
        // For logging purposes, it stores whenever each fragment was completed
        private static readonly Dictionary<string, bool> fragments;


        [NotNull] private static readonly MethodInfo _getRacePropsMethod;
        [NotNull] private static readonly MethodInfo _getAnimalMethod;
        [NotNull] private static readonly MethodInfo _toolUserMethod;
        [NotNull] private static readonly MethodInfo _getHumanlikeMethod;
        [NotNull] private static readonly MethodInfo _fenceBlockedTargetMethod;
        [NotNull] private static readonly MethodInfo _canPassFenceTargetMethod;
        [NotNull] private static readonly MethodInfo _roamerTargetMethod;

        static PatchUtilities()
        {
            fragments = new Dictionary<string, bool>();
            Type fhUtilType = typeof(FormerHumanUtilities);
            Type racePropType = typeof(RaceProperties);
            const BindingFlags publicInstance = BindingFlags.Public | BindingFlags.Instance;
            const BindingFlags allFlags = publicInstance | BindingFlags.NonPublic | BindingFlags.Static;

            IsAnimalMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsAnimal), new[] {typeof(Pawn)});

            IsHumanoidMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsHumanlike), new[] {typeof(Pawn)});
            IsToolUserMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsToolUser), new[] {typeof(Pawn)});
            _getRacePropsMethod = typeof(Pawn).GetProperty(nameof(Pawn.RaceProps)).GetGetMethod();
            GetRacePropsMethod = _getRacePropsMethod;
            _getAnimalMethod = racePropType.GetProperty(nameof(RaceProperties.Animal)).GetGetMethod();
            RimworldIsAnimalMethod = _getAnimalMethod;
            _toolUserMethod = racePropType.GetProperty(nameof(RaceProperties.ToolUser)).GetGetMethod();
            _getHumanlikeMethod = racePropType.GetProperty(nameof(RaceProperties.Humanlike)).GetGetMethod();
            AllFlags = BindingFlags.NonPublic | publicInstance | BindingFlags.Static;
            CommonTranspiler = typeof(PatchUtilities).GetMethod(nameof(SubstituteFormerHumanMethodsPatch));
            FenceBlockMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsFenceBlocked), AllFlags);
            RoamerMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.IsRoamer), BindingFlags.Static | BindingFlags.Public);
            CanPassFencesMethod = fhUtilType.GetMethod(nameof(FormerHumanUtilities.CanPassFences), AllFlags);
            _roamerTargetMethod = racePropType.GetProperty(nameof(RaceProperties.Roamer), publicInstance).GetMethod;
            _fenceBlockedTargetMethod = racePropType
                                       .GetProperty(nameof(RaceProperties.FenceBlocked),
                                                    publicInstance)
                                       .GetMethod;
            _canPassFenceTargetMethod = racePropType.GetProperty(nameof(RaceProperties.CanPassFences), publicInstance).GetMethod;

            if (_fenceBlockedTargetMethod == null) Log.Error($"unable to find {nameof(RaceProperties.FenceBlocked)}");

            if (_canPassFenceTargetMethod == null) Log.Error($"unable to find {nameof(RaceProperties.CanPassFences)}");
        }

        /// <summary>
        ///     MethodInfo for a common transpiler method that replaces all instances of RaceProps.Animal/Tooluser/Humanlike
        ///     with the FormerHumanUtilities equivalents
        /// </summary>
        [NotNull]
        public static MethodInfo CommonTranspiler { get; }

        /// <summary>
        ///     gets <see cref="RaceProperties.Animal" /> getter method
        /// </summary>
        [NotNull]
        public static MethodInfo RimworldIsAnimalMethod { get; }


        /// <summary>
        ///     Gets Rimworld's Animal getter method.
        /// </summary>
        /// <value>
        ///     The rimworld get animal method.
        /// </value>
        public static MethodInfo RimworldGetAnimalMethod => _getAnimalMethod;

        /// <summary>
        ///     Gets the rimworld get race method.
        /// </summary>
        /// <value>
        ///     The rimworld get race method.
        /// </value>
        public static MethodInfo RimworldGetRaceMethod => _getRacePropsMethod;


        /// <summary>
        ///     Gets the roamer method.
        /// </summary>
        /// <value>
        ///     The roamer method.
        /// </value>
        public static MethodInfo RoamerMethod { get; }

        /// <summary>
        ///     Gets the fence block method.
        /// </summary>
        /// <value>
        ///     The fence block method.
        /// </value>
        [NotNull]
        public static MethodInfo FenceBlockMethod { get; }

        /// <summary>
        ///     Gets the can pass fences method.
        /// </summary>
        /// <value>
        ///     The can pass fences method.
        /// </value>
        [NotNull]
        public static MethodInfo CanPassFencesMethod { get; }


        /// <summary>
        ///     Gets method info for <see cref="FormerHumanUtilities.IsAnimal" />
        /// </summary>
        /// <value>
        ///     The is animal method.
        /// </value>
        [NotNull]
        public static MethodInfo IsAnimalMethod { get; }


        /// <summary>
        ///     Gets the method info for <see cref="FormerHumanUtilities.IsHumanlike" />
        /// </summary>
        /// <value>
        ///     The is humanoid method.
        /// </value>
        [NotNull]
        public static MethodInfo IsHumanoidMethod { get; }

        /// <summary>
        ///     Gets the method info for <see cref="FormerHumanUtilities.IsToolUser" />
        /// </summary>
        /// <value>
        ///     The is tool user method.
        /// </value>
        [NotNull]
        public static MethodInfo IsToolUserMethod { get; }

        /// <summary>
        ///     Gets the get race props method.
        /// </summary>
        /// <value>
        ///     The get race props method.
        /// </value>
        [NotNull]
        public static MethodInfo GetRacePropsMethod { get; }

        private static BindingFlags AllFlags { get; }

        //taken from PrisonLabor
        /// <summary>
        ///     This method is used to find particular label that is assigned to last instruction's operand
        /// </summary>
        /// <param name="opCodes">The op codes.</param>
        /// <param name="operands">The operands.</param>
        /// <param name="instr">The instr.</param>
        /// <param name="perfectMatch">if set to <c>true</c> [perfect match].</param>
        /// <returns></returns>
        public static object FindOperandAfter(OpCode[] opCodes, string[] operands, IEnumerable<CodeInstruction> instr,
                                              bool perfectMatch = true)
        {
            if (opCodes.Length != operands.Length)
            {
                Log.Error("Pawnmorpher: FindOperandAfter() arguments does not match requirments. Trace:" + new StackTrace());
                return null;
            }

            int finalStep = opCodes.Length;

            var step = 0;
            foreach (CodeInstruction ci in instr)
            {
                if (InstructionMatching(ci, opCodes[step], operands[step], perfectMatch))
                    step++;
                else
                    step = 0;

                if (step == finalStep)
                    return ci.operand;
            }

            Log.Error("Pawnmorpher: FindOperandAfter() didn't find any lines. Trace:" + new StackTrace());
            return null;
        }

        /// <summary>
        ///     Determines whether this method has the given type signature
        /// </summary>
        /// Determines whether this method has the given type signature
        /// Note: this only checks the parameters not the return type, that must be checked separately
        /// <param name="methodInfo">The method information.</param>
        /// <param name="types">The types.</param>
        /// <returns>
        ///     <c>true</c> if the specified method information has signature; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     methodInfo
        ///     or
        ///     types
        /// </exception>
        public static bool HasSignature([NotNull] this MethodInfo methodInfo, [NotNull] params Type[] types)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            if (types == null) throw new ArgumentNullException(nameof(types));
            ParameterInfo[] parameters = methodInfo.GetParameters();
            if (parameters.Length != types.Length) return false;

            for (var i = 0; i < parameters.Length; i++)
                if (parameters[i].ParameterType != types[i])
                    return false;

            return true;
        }

        /// <summary>
        /// struct representing an operand to an opcode. needed to differentiate a null parameter to no parameter 
        /// </summary>
        public struct OpCodeOperand
        {
            
            /// <summary>
            /// Initializes a new instance of the <see cref="OpCodeOperand"/> struct.
            /// </summary>
            /// <param name="obj">The object.</param>
            public OpCodeOperand(object obj)
            {
                arg = obj; 
            }

            
            /// <summary>
            /// The argument
            /// </summary>
            public object arg; 
        }

        /// <summary>
        /// determines if the given instruction matches the given patern 
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        public static bool MatchesPattern([NotNull] this IReadOnlyList<CodeInstruction> instructions,
                                          params ValueTuple<OpCode, OpCodeOperand?>[] pattern)
        {
            if (pattern.Length != instructions.Count) return false;

            for (var i = 0; i < instructions.Count; i++)
            {
                CodeInstruction inst = instructions[i];
                (OpCode opC, OpCodeOperand? p) = pattern[i];


                if (inst.opcode != opC) return false;


                if (p != null)
                {
                    if (p.Value.arg is FieldInfo fInfo)
                    {
                        if (fInfo != (FieldInfo) inst.operand)
                        {
                            return false;
                        }
                    }
                    else if (p.Value.arg is MethodInfo mInfo)
                    {
                        if (mInfo != (MethodInfo) inst.operand) return false;
                    }
                    else if (p.Value.arg != inst.operand) return false; 
                }
            }

            return true; 

        }

        /// <summary>
        /// patch the given method,  replacing all instances of RaceProps.Animal/Tooluser/Humanlike
        /// with the FormerHumanUtilities equivalents
        /// </summary>
        /// <param name="harmony">The harmony.</param>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="debug">if set to <c>true</c> [debug].</param>
        /// <exception cref="ArgumentNullException">
        /// harmony
        /// or
        /// targetMethod
        /// </exception>
        public static void ILPatchCommonMethods([NotNull] this Harmony harmony, [NotNull] MethodInfo targetMethod,
                                                bool debug = false)
        {
            if (harmony == null) throw new ArgumentNullException(nameof(harmony));
            if (targetMethod == null) throw new ArgumentNullException(nameof(targetMethod));

            var hTs = new HarmonyMethod(CommonTranspiler);
            bool initDebug = Harmony.DEBUG;
            try
            {
                if (debug) Harmony.DEBUG = true;

                harmony.Patch(targetMethod, transpiler: hTs);
            }
            catch (Exception e)
            {
                Log.Error($"encountered {e.GetType().Name} while patching {targetMethod.Name}\n{e}");
            }
            finally
            {
                if (debug) Harmony.DEBUG = initDebug;
            }
        }

        /// <summary>
        ///     Determines whether this type is compiler generated.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///     <c>true</c> if this type is compiler generated; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        public static bool IsCompilerGenerated([NotNull] this Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.HasAttribute<CompilerGeneratedAttribute>()) return true;
            if (type.Name.Contains("<") || type.Name.Contains(">")) return true;
            return false;
        }


        //taken from PrisonLabor
        /// <summary>
        ///     This method is used to add some CIL instructions after certain fragment in original code.
        ///     It should be used inside foreach loop, and return true if particular iteration is the desired one.
        /// </summary>
        /// <param name="opCodes">The op codes.</param>
        /// <param name="operands">The operands.</param>
        /// <param name="instr">The instr.</param>
        /// <param name="step">The step.</param>
        /// <param name="fragmentName">Name of the fragment.</param>
        /// <param name="perfectMatch">if set to <c>true</c> [perfect match].</param>
        /// <returns>
        ///     <c>true</c> if the specified op codes is fragment; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFragment(OpCode[] opCodes, string[] operands, CodeInstruction instr, ref int step,
                                      string fragmentName, bool perfectMatch = true)
        {
            if (opCodes.Length != operands.Length)
            {
                Log.Error("PrisonLaborException: IsFragment() arguments does not match requirments. Trace:" + new StackTrace());
                return false;
            }

            if (!fragments.ContainsKey(fragmentName))
                fragments.Add(fragmentName, false);
            if (step < 0 || step >= opCodes.Length) return false;

            int finalStep = opCodes.Length;


            if (InstructionMatching(instr, opCodes[step], operands[step], perfectMatch))
                step++;
            else
                step = 0;

            if (step == finalStep)
            {
                step++;
                fragments[fragmentName] = true;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     patches every method in the given type, including sub types and delegates, with the given transpiler
        /// </summary>
        /// <param name="harmony">The harmony.</param>
        /// <param name="type">The type.</param>
        /// <param name="transpiler">The transpiler.</param>
        /// <param name="methodInfoPredicate">The method information predicate.</param>
        /// <exception cref="ArgumentNullException">
        ///     harmony
        ///     or
        ///     type
        ///     or
        ///     transpiler
        /// </exception>
        public static void MassIlPatchType([NotNull] this Harmony harmony, [NotNull] Type type, [NotNull] MethodInfo transpiler,
                                           Predicate<MethodInfo> methodInfoPredicate = null)
        {
            if (harmony == null) throw new ArgumentNullException(nameof(harmony));
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (transpiler == null) throw new ArgumentNullException(nameof(transpiler));

            var methodInfoList = new List<MethodInfo>();
            methodInfoList.AddRange(type.GetMethods(AllFlags).Where(m => methodInfoPredicate?.Invoke(m) != false));

            var internalTypes = new List<Type>();
            GetInternalTypes(type, internalTypes);
            methodInfoList.AddRange(internalTypes.SelectMany(t => t.GetMethods(AllFlags)
                                                                   .Where(m => methodInfoPredicate?.Invoke(m) != false)));


            foreach (MethodInfo methodInfo in methodInfoList)
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

        /// <summary>
        ///     substitutes all instances of RaceProps Humanlike, Animal, and Tooluser with their equivalent in
        ///     FormerHumanUtilities
        /// </summary>
        /// <param name="codeInstructions">The code instructions.</param>
        /// <exception cref="System.ArgumentNullException">codeInstructions</exception>
        public static void SubstituteFormerHumanMethods([NotNull] IList<CodeInstruction> codeInstructions)
        {
            if (codeInstructions == null) throw new ArgumentNullException(nameof(codeInstructions));

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
                    if (jMethod == _getHumanlikeMethod) //TODO refactor this to be more general 
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
                    else if (jMethod == _fenceBlockedTargetMethod)
                    {
                        patched = true;
                        opI.operand = FenceBlockMethod;
                    }
                    else if (jMethod == _canPassFenceTargetMethod)
                    {
                        patched = true;
                        opI.operand = CanPassFencesMethod;
                    }
                    else if (jMethod == _roamerTargetMethod)
                    {
                        patched = true;
                        opI.operand = RoamerMethod;
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
        }


        /// <summary>
        ///     substitutes all instances of RaceProps Humanlike, Animal, and Tooluser with their equivalent in
        ///     FormerHumanUtilities
        /// </summary>
        /// <param name="instructions">The code instructions.</param>
        /// <exception cref="System.ArgumentNullException">codeInstructions</exception>
        public static IEnumerable<CodeInstruction> SubstituteFormerHumanMethodsPatch(
            [NotNull] IEnumerable<CodeInstruction> instructions)
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
                    else if (jMethod == _fenceBlockedTargetMethod)
                    {
                        patched = true;
                        opI.operand = FenceBlockMethod;
                    }
                    else if (jMethod == _canPassFenceTargetMethod)
                    {
                        patched = true;
                        opI.operand = CanPassFencesMethod;
                    }
                    else if (jMethod == _roamerTargetMethod)
                    {
                        patched = true;
                        opI.operand = RoamerMethod;
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

        private static void GetInternalTypes([NotNull] Type type, [NotNull] List<Type> outList)
        {
            Type[] internalTypes = type.GetNestedTypes(AllFlags);

            foreach (Type internalType in internalTypes) GetInternalTypes(internalType, outList);

            outList.AddRange(internalTypes);
        }

        private static bool InstructionMatching(CodeInstruction instr, OpCode opCode, string operand, bool perfectMatch)
        {
            bool matchingOpCodes = instr.opcode == opCode;
            bool noOperands = instr.operand == null || string.IsNullOrEmpty(operand);
            bool matchingOperands;
            if (perfectMatch) matchingOperands = instr.operand != null && instr.operand.ToString() == operand;
            else matchingOperands = instr.operand != null && instr.operand.ToString().Contains(operand);

            return matchingOpCodes && (noOperands || matchingOperands);
        }
    }
}