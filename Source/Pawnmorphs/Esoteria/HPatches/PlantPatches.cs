// PlantPatches.cs modified by Iron Wolf for Pawnmorph on 12/31/2019 3:25 PM
// last updated 12/31/2019  3:25 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using HugsLib.Utils;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Plants;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
    static class PlantPatches
    {

        [HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch(nameof(WorkGiver_GrowerSow.JobOnCell))]
        static class SowerJobOnCellPatch
        {
            [HarmonyPrefix]
            static bool RespectRestrictionsPatch([NotNull] Pawn pawn, IntVec3 c, bool forced, ref Job __result)
            {
                if (pawn.Map == null) return true; 
                
                var plant = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
                if (plant != null)
                {
                    if (!plant.IsValidFor(pawn))
                    {
                        __result = null;
                        return false; 
                    }
                }

                return true; 
            }
        }

        [HarmonyPatch(typeof(WorkGiver_GrowerSow)), HarmonyPatch("ExtraRequirements")]
        static class ExtraRequirementsPatch
        {
            [HarmonyPostfix]
            static void RespectRequirementsPatch([NotNull] IPlantToGrowSettable settable, [NotNull] Pawn pawn, ref bool __result)
            {
                if (!__result) return;
                IntVec3 c;
                if (settable is Zone_Growing growingZone)
                {
                    c = growingZone.Cells[0]; 
                }
                else
                {
                    c = ((Thing) settable).Position; 
                }

                var plant = WorkGiver_Grower.CalculateWantedPlantDef(c, pawn.Map);
                if (plant == null) return;
                __result = plant.IsValidFor(pawn);
            }
        }

        public static class PlantHarvestTPatch
        {
            public static MethodInfo match = typeof(Plant).GetMethod("YieldNow");
            public static MethodInfo replaceWith = typeof(PlantHarvestTPatch).GetMethod("YieldNowPatch");
            public static MethodInfo TargetMethod()
            {
                Type mainType = typeof(JobDriver_PlantWork);
                //Log.Message("TargetMethod: Main Type Found");


                BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
                try
                {
                    Type iteratorType = mainType.GetNestedTypes(bindingFlags).First(t => t.FullName.Contains("c__DisplayClass"));
                    
                    
                    //Log.Message("TargetMethod: Iterator Type Resolved");
                    //Type anonStoreyType = iteratorType.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance).First(t => t.FullName.Contains("b__1"));
                    //Log.Message("TargetMethod: AnonStorey Type Resolved");
                    return iteratorType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).First(m => m.ReturnType == typeof(void));
                }
                catch (InvalidOperationException iO)
                {
                    var names = mainType.GetNestedTypes(bindingFlags).Select(t => t.FullName).Join(","); 

                    throw new InvalidOperationException($"unable to find type with \"c__DisplayClass\" among \"{names}\"", iO); 
                }

            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
            {
                foreach (CodeInstruction i in instr)
                {
                    if (i.operand == match)
                    {
                        //Log.Message("Instruction insertion complete!");
                        yield return new CodeInstruction(OpCodes.Ldloc_0); //TODO transpiler no longer works, need to fix 
                        yield return new CodeInstruction(OpCodes.Call, replaceWith);
                    }
                    else
                    {
                        yield return i;
                    }
                }
            }
            public static int YieldNowPatch(Plant p, Pawn actor)
            {
                if (p is SpecialHarvestFailPlant sPlant)
                {
                    return sPlant.GetYieldNow(actor); 
                }

                return p.YieldNow(); // Whatever you want to do here
            }
        }
    }
}