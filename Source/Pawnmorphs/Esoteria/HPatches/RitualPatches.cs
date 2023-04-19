// RitualPatches.cs created by Iron Wolf for Pawnmorph on 09/09/2021 6:50 AM
// last updated 09/09/2021  6:50 AM

using HarmonyLib;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class RitualPatches
	{
		[HarmonyPatch(typeof(RitualObligationTrigger_MemberCorpseDestroyed), nameof(RitualObligationTrigger_MemberCorpseDestroyed.Notify_MemberCorpseDestroyed))]
		static class RTriggerObligation_MemberCorpseDestroyedPatch
		{

			static bool Prefix(Pawn p)
			{
				if (p.RaceProps.Animal) return true;

				var pmWorldComp =
					Find.World.GetComponent<PawnmorphGameComp>(); //just check if the pawn being notified about is actually the original form of an alive pawn  
				(TransformedPawn pawn, TransformedStatus status)? tup = pmWorldComp.GetTransformedPawnContaining(p); //something is causing extra funerals for transformed pawns and I can't find it 
				if (tup != null) //so just doing a dumb brute force fix. this isn't particularly efficient. Too bad! 
				{
					(TransformedPawn _, TransformedStatus status) = tup.Value;
					if (status == TransformedStatus.Original)
						return false;
				}
				return true;
			}
		}
	}
}