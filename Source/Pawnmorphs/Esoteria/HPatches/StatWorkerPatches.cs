// StatWorkerPatches.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 8:45 AM
// last updated 09/29/2019  8:45 AM

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
#pragma warning disable 1591
namespace Pawnmorph.HPatches
{
    public static class StatWorkerPatches
    {
        [HarmonyPatch(typeof(StatWorker))]
        [HarmonyPatch(nameof(StatWorker.GetValueUnfinalized))]
        [HarmonyPatch(new Type[]
        {
            typeof(StatRequest), typeof(bool)
        })]
        static class GetValueUnfinalizedPatch
        {
            static void Postfix(ref float __result, StatRequest req, bool applyPostProcess,  StatDef ___stat)
            {
                if (req.Thing is Pawn pawn)
                {
                    var allAspects = pawn.GetAspectTracker()?.Aspects;
                    if (allAspects == null) return; 
                    foreach (var aspect in allAspects)
                    {
                        foreach (StatModifier statOffset in aspect.StatOffsets)
                        {
                            if(statOffset.stat != ___stat) continue;
                            __result += statOffset.value; 
                        }
                    }



                }


            }
        }
    }
}