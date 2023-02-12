// OptimizeApparalPatch.cs created by Iron Wolf for Pawnmorph on 04/26/2020 5:26 PM
// last updated 04/26/2020  5:26 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(JobGiver_OptimizeApparel))]
	static class OptimizeApparelPatch
	{

		[HarmonyPatch("TryGiveJob"), HarmonyPrefix]
		static bool FixTryGiveJob([NotNull] Pawn pawn, ref Job __result)
		{
			if (pawn.IsFormerHuman())
			{
				__result = null;
				return false;
			}

			return true;
		}
	}
}