// DebugPatches.cs created by Iron Wolf for Pawnmorph on 02/16/2021 4:44 PM
// last updated 02/16/2021  4:44 PM

using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	internal static class DebugPatches
	{



		[Conditional("DEBUG")]
		static internal void DoDebugPatches([NotNull] Harmony har)
		{
			var jgLovin = typeof(JobGiver_DoLovin);
			var lovingMethod = jgLovin.GetMethod("TryGiveJob", BindingFlags.Instance | BindingFlags.NonPublic);

			var lovinPatch =
				typeof(DebugPatches).GetMethod(nameof(DoLovingDebugPrint), BindingFlags.NonPublic | BindingFlags.Static);


			har.Patch(lovingMethod, new HarmonyMethod(lovinPatch));


		}

		static void DoLovingDebugPrint(Pawn pawn)
		{
			if (pawn.jobs?.debugLog != true) return;
			if (!pawn.IsFormerHuman()) return;

			if (pawn.CurrentBed() == null || pawn.CurrentBed().Medical || !pawn.health.capacities.CanBeAwake)
			{
				Log.Message($"{pawn.Label} failed to love at current bed check");
				return;
			}
			Pawn partnerInMyBed = LovePartnerRelationUtility.GetPartnerInMyBed(pawn);
			if (partnerInMyBed == null || !partnerInMyBed.health.capacities.CanBeAwake || Find.TickManager.TicksGame < partnerInMyBed.mindState.canLovinTick)
			{
				Log.Message($"{pawn.Label} failed at partner check or canBeAwake check or ");
				return;
			}
			if (!pawn.CanReserve(partnerInMyBed) || !partnerInMyBed.CanReserve(pawn))
			{
				Log.Message($"{pawn.Label} failed at partner in bed check");

				return;
			}


		}

	}
}