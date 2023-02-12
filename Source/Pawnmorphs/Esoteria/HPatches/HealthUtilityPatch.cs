using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using static Pawnmorph.Utilities.PatchUtilities;

namespace Pawnmorph.HPatches.HealthUtilityPatchs
{
	[HarmonyPatch(typeof(HealthUtility), nameof(HealthUtility.GetPartConditionLabel))]
	static class GetPartConditionLabel
	{
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
		{
			List<CodeInstruction> lst = insts.ToList();

			var pattern = new ValueTuple<OpCode, OpCodeOperand?>[]
			{
				(OpCodes.Ldarg_1, null), //part 
                (OpCodes.Ldfld,
					new OpCodeOperand(typeof(BodyPartRecord).GetField(nameof(BodyPartRecord.def),
																	BindingFlags.Public | BindingFlags.Instance))),
				(OpCodes.Ldarg_0, null), //pawn 
                (OpCodes.Callvirt,
					new OpCodeOperand(typeof(BodyPartDef).GetMethod(nameof(BodyPartDef.GetMaxHealth),
																	BindingFlags.Public | BindingFlags.Instance)))
			};
			int len = pattern.Length;
			var subArr = new CodeInstruction[len];


			MethodInfo subMethod =
				typeof(BodyUtilities).GetMethod(nameof(BodyUtilities.GetPartMaxHealth),
												BindingFlags.Public | BindingFlags.Static);

			for (var i = 0; i < lst.Count - len; i++)
			{
				for (var j = 0; j < len; j++) subArr[j] = lst[i + j];

				if (!subArr.MatchesPattern(pattern)) continue;

				lst[i + 1].opcode = OpCodes.Nop;
				lst[i + 1].operand = null;
				lst[i + pattern.Length - 1].opcode = OpCodes.Call;
				lst[i + pattern.Length - 1].operand = subMethod;

				break;
			}


			return lst;
		}
	}
}
