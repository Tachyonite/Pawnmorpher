// GatherableBodyResourcePatch.cs modified by Iron Wolf for Pawnmorph on 12/02/2019 4:26 PM
// last updated 12/02/2019  4:26 PM

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches
{

	[HarmonyPatch(typeof(CompHasGatherableBodyResource))]
	[HarmonyPatch(nameof(CompHasGatherableBodyResource.Gathered))]
	internal static class GatherableBodyResourcePatch
	{
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> ColoriseTextileProductPatch(IEnumerable<CodeInstruction> instructions)
		{
			var found = false;
			foreach (var instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Call && instruction.Calls(typeof(ThingMaker).GetMethod(nameof(ThingMaker.MakeThing))))
				{
					found = true;
					yield return instruction;

					MethodInfo _coloriseTextileProduct = typeof(GatherableBodyResourcePatch).GetMethod(nameof(GatherableBodyResourcePatch.ColoriseTextileAnimalProduct), new[] { typeof(Thing), typeof(ThingWithComps) });

					yield return new CodeInstruction(OpCodes.Dup);
					// load parent to stack
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldfld, typeof(ThingComp).GetField(nameof(ThingComp.parent)));
					// call ColoriseTextileProduct
					yield return new CodeInstruction(OpCodes.Call, _coloriseTextileProduct);
				}
				else
				{
					yield return instruction;
				}
			}
			if (!found)
			{
				Log.Warning("PM: Failed to transpile CompHasGatherableBodyResource.Gathered.");
			}
		}

		public static void ColoriseTextileAnimalProduct(Thing thing, ThingWithComps parent)
		{
			if (!(parent is Pawn))
				return;
			Pawn pawn = parent as Pawn;
			ThingDef resource = thing.def;
			Color? skinColor = pawn.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>()?.ColorSet?.skinColor;

			if (skinColor.HasValue && resource.thingCategories.Contains(PMThingCategoryDefOf.Textiles) && resource.CompDefFor<CompColorable>() != null)
				thing.SetColor(skinColor.Value);
		}

		[HarmonyPostfix]
		internal static void GenerateThoughtsAbout([NotNull] Pawn doer, [NotNull] CompHasGatherableBodyResource __instance)
		{
			var selfPawn = __instance.parent as Pawn;
			if (!selfPawn.IsFormerHuman() || selfPawn.needs?.mood == null) return;

			//TODO put this in a def extension or something 
			if (__instance is CompMilkable)
			{
				if (ThoughtMaker.MakeThought(PMThoughtDefOf.SapientAnimalMilked) is Thought_Memory memory)
				{
					selfPawn.TryGainMemory(memory);
				}

			}
		}
	}
}