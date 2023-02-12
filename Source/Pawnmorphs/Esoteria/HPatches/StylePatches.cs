// StylePatches.cs created by Iron Wolf for Pawnmorph on 07/28/2021 5:31 PM
// last updated 07/28/2021  5:31 PM

using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class StylePatches
	{
		[HarmonyPatch(typeof(Pawn_StyleTracker), nameof(Pawn_StyleTracker.CanDesireLookChange), MethodType.Getter)]
		static class DisableStyleTrackerForFH
		{
			static void Postfix(Pawn ___pawn, ref bool __result)
			{
				if (__result) __result = !(___pawn?.IsFormerHuman() ?? true);
			}
		}
	}
}