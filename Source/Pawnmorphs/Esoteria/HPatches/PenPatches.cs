// PenPatches.cs created by Iron Wolf for Pawnmorph on 07/16/2021 5:24 PM
// last updated 07/16/2021  5:24 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	internal static class PenPatches
	{
		private static bool ShouldIncludeIntoPen(Thing thing)
		{
			var p = thing as Pawn;
			if (p == null) return false;
			if (p.IsAnimal()) return true;


			if (p.IsPrisonerOfColony) return true;

			return false;
		}

		[HarmonyPatch(typeof(PenFoodCalculator), "ProcessCell")]
		private static class PenGuiPatch
		{
			private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				MethodInfo callMethod =
					typeof(PenPatches).GetMethod("ShouldIncludeIntoPen", BindingFlags.Static | BindingFlags.NonPublic);
				MethodInfo isAnimalGetter = typeof(RaceProperties)
										   .GetProperty(nameof(RaceProperties.Animal),
														BindingFlags.Public | BindingFlags.Instance)
										   .GetMethod;
				FieldInfo defField = typeof(Thing).GetField(nameof(Thing.def), BindingFlags.Public | BindingFlags.Instance);
				FieldInfo racePropField =
					typeof(ThingDef).GetField(nameof(ThingDef.race), BindingFlags.Public | BindingFlags.Instance);
				CodeInstruction[] instArr = instructions.ToArray();

				if (defField == null) Log.Error("unable to find def field");
				if (racePropField == null) Log.Error("unable to find race propField");
				if (isAnimalGetter == null) Log.Error("unable to find is animal getter");
				for (var i = 0; i < instArr.Length - 3; i++)
				{
					CodeInstruction inst0 = instArr[i];
					CodeInstruction inst1 = instArr[i + 1];
					CodeInstruction inst2 = instArr[i + 2];

					if (inst0.opcode != OpCodes.Ldfld || (FieldInfo)inst0.operand != defField) continue;
					if (inst1.opcode != OpCodes.Ldfld || (FieldInfo)inst1.operand != racePropField) continue;
					if (inst2.opcode != OpCodes.Callvirt || (MethodInfo)inst2.operand != isAnimalGetter) continue;
					inst0.opcode = OpCodes.Call;
					inst0.operand = callMethod;
					inst1.opcode = OpCodes.Nop;
					inst2.opcode = OpCodes.Nop;
					break;
				}

				return instArr;
			}
		}


		[HarmonyPatch(typeof(MapPlantGrowthRateCalculator), nameof(MapPlantGrowthRateCalculator.IsPastureAnimal))]
		private static class IsPastureAnimalFixCl
		{
			private static void Postfix(ThingDef td, ref bool __result)
			{
				if (!__result)
				{
					FoodTypeFlags foodFlags = td.race?.foodType ?? (FoodTypeFlags)0;
					__result = (foodFlags & (FoodTypeFlags.Plant | FoodTypeFlags.Tree | FoodTypeFlags.Seed)) != 0;

				}
			}
		}
	}
}