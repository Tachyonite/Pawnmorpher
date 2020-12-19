// WorkGiverPatches.cs created by Iron Wolf for Pawnmorph on 05/10/2020 7:48 AM
// last updated 05/10/2020  7:49 AM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    static class WorkGiverPatches
    {
        private static bool CanInteract(Pawn p)
        {
            return p.RaceProps.Animal || p.GetIntelligence() == Intelligence.Animal;
        }

        [HarmonyPatch(typeof(WorkGiver_InteractAnimal))]
        static class InteractionPatches
        {
            [HarmonyPatch("CanInteractWithAnimal"), HarmonyPrefix]
            static bool DontInteractSelfFix(ref bool __result, Pawn pawn, Pawn animal, bool forced)
            {
                if (pawn == animal)
                {
                    __result = false;
                    return false; 
                }

                return true; 
            }
        }


        [HarmonyPatch(typeof(WorkGiver_GatherAnimalBodyResources))]
        static class GatherAnimalBodyResourcesPatches
        {
            [HarmonyPatch(nameof(WorkGiver_GatherAnimalBodyResources.HasJobOnThing))]
            [HarmonyPostfix]
            private static void DontInteractSelfFix(ref bool __result, Pawn pawn, Thing t, bool forced)
            {
                if (__result) __result = pawn != t;
            }

        }

        [HarmonyPatch(typeof(WorkGiver_Slaughter))]
        private static class SlaughterPatches
        {
            [HarmonyPatch("HasJobOnThing")]
            [HarmonyPostfix]
            private static void DontInteractSelfFix(ref bool __result, Pawn pawn, Thing t, bool forced)
            {
                if (__result) __result = pawn != t;
            }
        }



        [HarmonyPatch(typeof(WorkGiver_Train))]
        private static class TrainPatches
        {
            
            [HarmonyPatch("JobOnThing")]
            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> instructionList = instructions.ToList();
                Log.Message("Patching WorkGiver Tamed");
                for (var i = 0; i < instructionList.Count - 1; i++)
                {
                    CodeInstruction jInst = instructionList[i + 1];
                    CodeInstruction iInst = instructionList[i];
                    if (iInst.opcode == OpCodes.Callvirt && (MethodInfo) iInst.operand == PatchUtilities.GetRacePropsMethod)
                        if (jInst.opcode == OpCodes.Callvirt
                         && (MethodInfo) jInst.operand == PatchUtilities.RimworldIsAnimalMethod)
                        {
                            iInst.opcode = OpCodes.Call;
                            iInst.operand =
                                typeof(WorkGiverPatches).GetMethod(nameof(CanInteract),
                                                                   BindingFlags.NonPublic | BindingFlags.Static);
                            jInst.opcode = OpCodes.Nop;
                            jInst.operand = null;
                            break;
                        }
                }

                return instructionList;
            }
        }
    }
}