// GiddyUpPatch.cs created by Iron Wolf for Pawnmorph on 01/02/2021 10:19 AM
// last updated 01/02/2021  10:19 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(RimWorld.Selector))]
	internal static class SelectorPatch
	{

		private static Predicate<Thing> _adjustedPredicate = (Thing t) => t is Pawn pawn && FormerHumanUtilities.IsHumanlike(pawn) && pawn.Faction == Faction.OfPlayer;

		[HarmonyPatch("SelectInsideDragBox")]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> instructionList = instructions.ToList();

			CodeInstruction predicateField = instructionList.FirstOrDefault(x => x.opcode == OpCodes.Ldsfld && (x.operand as FieldInfo)?.FieldType == typeof(Predicate<Thing>));
			if (predicateField == null)
				return instructions;

			int instructionIndex = instructionList.IndexOf(predicateField);

			// Insurance checks
			if (instructionList[instructionIndex + 1].opcode != OpCodes.Dup)
				return instructions;

			// Replace field reference with adjusted predicate.
			instructionList[instructionIndex].operand = AccessTools.Field(typeof(SelectorPatch), nameof(_adjustedPredicate));

			return instructionList;
		}
	}
}