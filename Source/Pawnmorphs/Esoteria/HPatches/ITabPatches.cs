// ITabPatches.cs created by Iron Wolf for Pawnmorph on 10/24/2020 9:11 AM
// last updated 10/24/2020  9:11 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches
{
    [StaticConstructorOnStartup]
    static class ITabPatches
    {
        internal static void DoPrisonerPatch(Harmony harInstance)
        {
            //if prison labor is loaded don't patch the visitor tab, they already handle it
            if (LoadedModManager.RunningMods.Any(m => m.PackageId == "vius.prisonlabor")) return;

            var flg = BindingFlags.NonPublic | BindingFlags.Static;
            var fillMethod = typeof(ITab_Pawn_Visitor).GetMethod("FillTab", BindingFlags.NonPublic | BindingFlags.Instance);
            var sizeTMethod = typeof(ITabPatches).GetMethod(nameof(SizeTranspiler), flg);
            var scrollTMethod = typeof(ITabPatches).GetMethod(nameof(ScrollTranspiler), flg);
            harInstance.Patch(fillMethod, transpiler: new HarmonyMethod(sizeTMethod));
            harInstance.Patch(fillMethod, transpiler: new HarmonyMethod(scrollTMethod)); 

        }


        //Code taken from PrisonLabor 
        private static IEnumerable<CodeInstruction> SizeTranspiler(IEnumerable<CodeInstruction> instructions)
        {

            foreach (var ci in instructions)
            {
                if (ci.operand is float && (float)ci.operand == 200f)
                    ci.operand = 30f * DefDatabase<PrisonerInteractionModeDef>.DefCount + 10;
                yield return ci;
            }
        }

        //Code taken from PrisonLabor 
        private static IEnumerable<CodeInstruction> ScrollTranspiler(ILGenerator gen, IEnumerable<CodeInstruction> instr)
        {
            #region fragment>>GUI.BeginGroup(position);
            OpCode[] opCodes1 =
            {
                OpCodes.Call,
                OpCodes.Stloc_S,
                OpCodes.Ldloc_S,
                OpCodes.Call,
            };
            string[] operands1 =
            {
                "UnityEngine.Rect ContractedBy(UnityEngine.Rect, Single)",
                "UnityEngine.Rect (7)",
                "UnityEngine.Rect (7)",
                "Void BeginGroup(UnityEngine.Rect)",
            };
            int step1 = 0;
            #endregion

            #region fragment>>GUI.EndGroup();
            OpCode[] opCodes2 =
            {
                OpCodes.Ldloc_S,
                OpCodes.Callvirt,
                OpCodes.Endfinally,
                OpCodes.Call,
            };
            string[] operands2 =
            {
                "System.Collections.Generic.IEnumerator`1[RimWorld.PrisonerInteractionModeDef] (22)",
                "Void Dispose()",
                "",
                "Void EndGroup()",
            };
            int step2 = 0;
            #endregion

            #region fragment>>Rect position = rect6.ContractedBy(10f);
            OpCode[] opCodes3 =
            {
                OpCodes.Ldc_R4,
                OpCodes.Call,
                OpCodes.Stloc_S,
                OpCodes.Ldloc_S,
            };
            String[] operands3 =
            {
                "10",
                "UnityEngine.Rect ContractedBy(UnityEngine.Rect, Single)",
                "UnityEngine.Rect (7)",
                "UnityEngine.Rect (7)",
            };
            int step3 = 0;
            var rect = PatchUtilities.FindOperandAfter(opCodes3, operands3, instr);
            #endregion

            foreach (var ci in instr)
            {
                // end scroll
                if (PatchUtilities.IsFragment(opCodes2, operands2, ci, ref step2, "AddScrollToPrisonerTab2"))
                {
                    var instruction = new CodeInstruction(OpCodes.Call, typeof(ITabPatches).GetMethod(nameof(StopScrolling)));
                    instruction.labels.AddRange(ci.labels);
                    ci.labels.Clear();
                    yield return instruction;
                }

                /*                // resize
                                if (HPatcher.IsFragment(opCodes3, operands3, ci, ref step3, "AddScrollToPrisonerTab3"))
                                {
                                }*/

                yield return ci;

                // begin scroll
                if (PatchUtilities.IsFragment(opCodes1, operands1, ci, ref step1, "AddScrollToPrisonerTab1"))
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_S, rect);
                    yield return new CodeInstruction(OpCodes.Call, typeof(ITabPatches).GetMethod(nameof(StartScrolling)));
                    yield return new CodeInstruction(OpCodes.Stloc_S, rect);
                }
            }
        }

        public static Vector2 position;
        public static Rect StartScrolling(Rect rect)
        {
            Rect viewRect = new Rect(0, 0, rect.width - 16, rect.height + 56);
            Rect outRect = new Rect(0, 0, rect.width, rect.height);
            Widgets.BeginScrollView(outRect, ref position, viewRect, true);
            return viewRect;
        }

        public static void StopScrolling()
        {
            Widgets.EndScrollView();
        }
    }
}