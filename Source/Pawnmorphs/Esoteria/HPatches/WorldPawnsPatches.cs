using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(RimWorld.Planet.WorldPawns))]
	internal static class WorldPawnsPatches
	{
		[HarmonyPatch("ShouldMothball"), HarmonyPostfix]
		private static bool ShouldMothballPostfix(bool __result, Pawn p)
		{
			if (__result)
			{
				MutationTracker tracker = p.GetMutationTracker();
				if (tracker != null)
				{
					tracker.ClearMutationLog();
				}
			}
			return __result;
		}
	}
}
