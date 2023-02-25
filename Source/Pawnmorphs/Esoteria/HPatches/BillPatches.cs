// BillPatches.cs created by Iron Wolf for Pawnmorph on 03/07/2022 3:21 PM
// last updated 03/07/2022  3:21 PM

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Bill))]
	internal static class BillPatches
	{
		[HarmonyPatch(nameof(Bill.Notify_IterationCompleted))]
		[HarmonyPostfix]
		private static void Notify_IterationCompletedPatch(Pawn billDoer, List<Thing> ingredients, Bill __instance)
		{
			if (billDoer == null) return;
			ThingDef prod = __instance.recipe?.ProducedThingDef;
			if (prod?.IsMutagenicWeapon() == true)
			{
				var hEvent = new HistoryEvent(PMHistoryEventDefOf.CreateMutagenicWeapon,
											  billDoer.Named(HistoryEventArgsNames.Doer),
											  prod.Named(HistoryEventArgsNames.Subject));

				Find.HistoryEventsManager.RecordEvent(hEvent);
			}
		}


		[HarmonyPatch(nameof(Bill.PawnAllowedToStartAnew))]
		[HarmonyPostfix]
		private static void PawnAllowedToStartBillPatch(Pawn p, Bill __instance, ref bool __result)
		{
			if (!__result || p == null || __instance.recipe.ProducedThingDef?.IsMutagenicWeapon() != true) return;

			__result = PMHistoryEventDefOf.CreateMutagenicWeapon.DoerWillingToDo(p);
			if (!__result) JobFailReason.Is("IdeoligionForbids".Translate());
		}
	}

	[HarmonyPatch(typeof(Bill_Medical))]
	internal static class MedicalBillPatches
	{
		[HarmonyPatch(nameof(Bill.Notify_IterationCompleted))]
		[HarmonyPostfix]
		private static void Notify_IterationCompletedPatch(Pawn billDoer, List<Thing> ingredients, Bill_Medical __instance)
		{
			if (billDoer == null) return;
			if (__instance.recipe?.ingredients?[0]?.filter?.BestThingRequest.singleDef?.IsMutagenOrMutagenicDrug() == true
			 && __instance.recipe.Worker is Recipe_AdministerIngestible)
			{
				var hEv = new HistoryEvent(PMHistoryEventDefOf.ApplyMutagenicsOn, billDoer.Named(HistoryEventArgsNames.Doer),
										   __instance.GiverPawn.Named(HistoryEventArgsNames.Victim));
				Find.HistoryEventsManager.RecordEvent(hEv);
			}
		}

		[HarmonyPatch(nameof(Bill.PawnAllowedToStartAnew))]
		[HarmonyPostfix]
		private static void PawnAllowedToStartBillPatch(Pawn pawn, Bill_Medical __instance, ref bool __result)
		{
			if (!__result || pawn == null) return;

			if (!(__instance.recipe.Worker is Recipe_AdministerIngestible)) return;


			if (__instance.recipe?.ingredients?[0]?.filter?.BestThingRequest.singleDef?.IsMutagenOrMutagenicDrug() == true)
				__result = new HistoryEvent(PMHistoryEventDefOf.ApplyMutagenicsOn, pawn.Named(HistoryEventArgsNames.Doer), __instance.GiverPawn.Named(HistoryEventArgsNames.Victim))
				   .Notify_PawnAboutToDo();
		}
	}
}