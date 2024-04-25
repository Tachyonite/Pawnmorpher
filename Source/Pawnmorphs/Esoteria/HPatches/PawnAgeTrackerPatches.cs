using HarmonyLib;
using Pawnmorph.Utilities;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static Verse.Pawn_AgeTracker;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Pawn_AgeTracker))]
	[HotSwappable]
	static class PawnAgeTrackerPatches
	{
		[HarmonyPatch(nameof(Pawn_AgeTracker.ResetAgeReversalDemand)), HarmonyTranspiler]
		static IEnumerable<CodeInstruction> ResetAgeReversalDemand_Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.LoadsConstant(90000000L))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return CodeInstruction.LoadField(typeof(Pawn_AgeTracker), "pawn");
					yield return CodeInstruction.Call<Pawn, long>(x => GetMinimumReversalDemandAge(x));
				}
				else
					yield return instruction;
			}
		}

		static long GetMinimumReversalDemandAge(Pawn pawn)
		{
			float minAgeRatio = ThingDefOf.Human.race.lifeExpectancy / 25;
			return (long)(pawn.RaceProps.lifeExpectancy / minAgeRatio * TimeMetrics.TICKS_PER_YEAR);
		}
	}
}
