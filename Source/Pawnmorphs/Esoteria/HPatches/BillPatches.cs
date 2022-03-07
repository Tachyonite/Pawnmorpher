// BillPatches.cs created by Iron Wolf for Pawnmorph on 03/07/2022 3:21 PM
// last updated 03/07/2022  3:21 PM

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(Bill))]
    internal static class BillPatches
    {

        [HarmonyPatch(nameof(Bill.PawnAllowedToStartAnew))]
        [HarmonyPostfix]
        static void PawnAllowedToStartBillPatch(Pawn p, Bill __instance, ref bool __result)
        {
            if (!__result || p == null || __instance.recipe.ProducedThingDef?.IsMutagenicWeapon() != true) return;

            __result = PMHistoryEventDefOf.CreateMutagenicWeapon.DoerWillingToDo(p); 

        }

        [HarmonyPatch(nameof(Bill.Notify_IterationCompleted))]
        [HarmonyPostfix]
        static void Notify_IterationCompletedPatch(Pawn billDoer, List<Thing> ingredients,  Bill __instance)
        {

            if (billDoer == null) return; 
            var prod = __instance.recipe?.ProducedThingDef;
            if (prod?.IsMutagenicWeapon() == true)
            {
                var hEvent = new HistoryEvent(PMHistoryEventDefOf.CreateMutagenicWeapon,
                                              billDoer.Named(HistoryEventArgsNames.Doer),
                                              prod.Named(HistoryEventArgsNames.Subject));
            }

        }
    }
}