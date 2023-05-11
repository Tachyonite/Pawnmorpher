// AnimalPenUtilityPatches.cs created by Iron Wolf for Pawnmorph on 07/06/2021 8:36 PM
// last updated 07/06/2021  8:36 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch]
	static class AnimalPenUtilityPatches
	{
		[HarmonyPatch(typeof(AnimalPenUtility), nameof(AnimalPenUtility.NeedsToBeManagedByRope)), HarmonyPostfix]
		static void NeedsToBeManagedByRopePatch(Pawn pawn, ref bool __result)
		{
			if (__result)
			{
				__result = !pawn.IsSapientFormerHuman();
			}
			else
			{
				__result = (pawn.IsWildMan());
			}
		}


		public static bool IsRopeManagedAnimalDefPatched(Thing thing)
		{
			bool result = AnimalPenUtility.IsRopeManagedAnimalDef(thing.def);
			if (thing is Pawn pawn)
				NeedsToBeManagedByRopePatch(pawn, ref result);

			return result;
		}

		[HarmonyPatch(typeof(AnimalPenUtility), nameof(AnimalPenUtility.GetHitchingPostAnimalShouldBeTakenTo))]
		static class IsRopeManagedAnimalDefPatch
		{
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				var codes = instructions.ToList(); //convert the code instructions to a list so we can do 2 at a time 

				FieldInfo defField = AccessTools.Field(typeof(Thing), nameof(Thing.def));
				MethodInfo targetMethod = AccessTools.Method(typeof(AnimalPenUtility), nameof(AnimalPenUtility.IsRopeManagedAnimalDef));
				MethodInfo subMethod = AccessTools.Method(typeof(AnimalPenUtilityPatches), nameof(IsRopeManagedAnimalDefPatched));

				const int len = 5;
				for (int i = 0; i < codes.Count - len; i++)
				{
					// If call is made to taget method
					if (codes[i].opcode == OpCodes.Call && codes[i].operand as MethodInfo == targetMethod)
					{

						CodeInstruction animalDefFieldRef = codes[i - 1];
						if (animalDefFieldRef.opcode != OpCodes.Ldfld || (animalDefFieldRef.operand as FieldInfo) != defField)
							continue; // Skip

						// Replace method call
						codes[i].operand = subMethod;

						// Remove .def
						animalDefFieldRef.opcode = OpCodes.Nop;
						animalDefFieldRef.operand = null;
					}
				}

				return codes;
			}
		}
	}
}