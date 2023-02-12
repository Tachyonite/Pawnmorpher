// HuntingPatches.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 7:40 AM
// last updated 12/15/2019  7:40 AM

using System;
using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	static class HuntingPatches
	{
		[HarmonyPatch(typeof(JobDriver_PredatorHunt)), HarmonyPatch("MakeNewToils")]
		static class JobDriver_PredatorHuntPatch
		{
			static IEnumerable<Toil> Postfix(IEnumerable<Toil> values, [NotNull] JobDriver_PredatorHunt __instance)
			{

				Toil lastToil = null;

				foreach (Toil toil in values)
				{
					lastToil = toil;

					yield return toil;
				}



				if (!__instance.pawn.IsFormerHuman() || __instance.pawn?.needs?.mood == null || lastToil == null) yield break;


				bool passed = false;

				lastToil.finishActions = lastToil.finishActions ?? new List<Action>();

				lastToil.finishActions.Add(() =>
				{
					if (passed) return;
					passed = true;
					var ideo = __instance.pawn.Ideo;
					if (!ModsConfig.IdeologyActive || ideo?.HasPositionOn(PMIssueDefOf.PM_FormerHuman_Nudity) != true) //if no ideo or the ideo doesn't care use the defaults 
					{
						FormerHumanUtilities.GiveSapientAnimalHuntingThought(__instance.pawn,
																			 __instance.Prey);
					}

					PMHistoryEventDefOf.FormerHumanHunted.SendEvent(__instance.pawn.Named(HistoryEventArgsNames.Doer),
																	__instance.Prey.Named(HistoryEventArgsNames.Victim));


				});


			}


		}

		[HarmonyPatch(typeof(Designator_Hunt), nameof(Designator_Hunt.CanDesignateThing))]
		static class HuntingDesignatorPatch
		{
			static void Postfix(ref AcceptanceReport __result, [NotNull] Thing t)
			{
				if (!__result.Accepted && Find.CurrentMap?.designationManager.DesignationOn(t, DesignationDefOf.Hunt) == null)
				{
					var p = t as Pawn;
					if (p == null) return;
					if (p.IsFormerHuman() && p.Faction == null) __result = true;
				}
			}
		}
	}
}