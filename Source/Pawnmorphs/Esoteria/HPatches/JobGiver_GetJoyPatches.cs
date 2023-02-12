// JobGiver_GetJoyPatches.cs created by Iron Wolf for Pawnmorph on 04/25/2020 4:53 PM
// last updated 04/25/2020  4:53 PM

using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(JobGiver_GetJoy))]
	static class JobGiver_GetJoyPatches
	{
		[HarmonyPatch("TryGiveJob"), HarmonyPrefix]
		static bool FixTryGiveJob(ref Job __result, Pawn pawn)
		{
			if (pawn?.needs?.joy == null)
			{
				__result = null;
				return false;
			}

			return true;
		}
	}
}