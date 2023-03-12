// PawnCapacityUtilsPatch.cs modified by Iron Wolf for Pawnmorph on 09/26/2019 6:10 PM
// last updated 09/26/2019  6:10 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using static Pawnmorph.Utilities.PatchUtilities;

#pragma warning disable 1591
namespace Pawnmorph.HPatches
{

	public static class PawnCapacityUtilsPatch
	{
		[HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculateCapacityLevel), new Type[]
		{
			typeof(HediffSet), typeof(PawnCapacityDef), typeof(List<PawnCapacityUtility.CapacityImpactor>), typeof(bool)
		})]
		public static class GetCapacityLvPatch
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
			{
				LinkedList<CodeInstruction> instructions = new LinkedList<CodeInstruction>(insts);

				MethodInfo insertOperandLocation = AccessTools.PropertyGetter(typeof(ModsConfig), nameof(ModsConfig.BiotechActive));
				MethodInfo hookMethodInfo = AccessTools.Method(typeof(PawnCapacityUtilsPatch), nameof(CalculateCapacityLevelHook));


				CodeInstruction instruction = null;
				for (LinkedListNode<CodeInstruction> node = instructions.First; node != null; node = node.Next)
				{
					instruction = node.Value;

					if (instruction.Calls(insertOperandLocation))
					{
						// Load "num" offset value
						instructions.AddBefore(node, new CodeInstruction(OpCodes.Ldloca_S, (byte)0));

						// Load "num3" postFactor value
						instructions.AddBefore(node, new CodeInstruction(OpCodes.Ldloca_S, (byte)2));

						// Load diffset argument
						instructions.AddBefore(node, new CodeInstruction(OpCodes.Ldarg_0));

						// Load capacity argument
						instructions.AddBefore(node, new CodeInstruction(OpCodes.Ldarg_1));

						// Load capacity impactors argument
						instructions.AddBefore(node, new CodeInstruction(OpCodes.Ldarg_2));

						// Call hook method
						instructions.AddBefore(node, new CodeInstruction(OpCodes.Call, hookMethodInfo));

						break;
					}
				}

				return instructions.ToArray();
			}
		}

		static void CalculateCapacityLevelHook(ref float offset, ref float postFactor, HediffSet diffSet, PawnCapacityDef capacity, List<PawnCapacityUtility.CapacityImpactor> impactors)
		{
			var pawn = diffSet.pawn;
			var aspectTracker = pawn.GetAspectTracker();
			if (aspectTracker != null)
			{
				float setMax = float.PositiveInfinity;
				foreach (Aspect aspect in aspectTracker.Aspects)
				{
					if (!aspect.HasCapMods) continue;
					foreach (PawnCapacityModifier capMod in aspect.CapMods)
					{
						if (capMod.capacity != capacity) continue;

						offset += capMod.offset;
						postFactor *= capMod.postFactor;
						if (capMod.SetMaxDefined && (capMod.setMax < setMax))
						{
							setMax = capMod.setMax;
						}
					}

					impactors?.Add(new AspectCapacityImpactor(aspect));
				}
			}
		}



		[HarmonyPatch(typeof(PawnCapacityUtility), nameof(PawnCapacityUtility.CalculatePartEfficiency))]
		static class GetPartEfficiencyFix
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
			{
				var pattern = new ValueTuple<OpCode, OpCodeOperand?>[]
				{
					(OpCodes.Ldarg_1, null), //part 
                    (OpCodes.Ldfld,
					 new OpCodeOperand(typeof(BodyPartRecord).GetField(nameof(BodyPartRecord.def),
																	   BindingFlags.Public | BindingFlags.Instance))),
					(OpCodes.Ldarg_0, null), //hediff set 
                    (OpCodes.Ldfld,
					 new OpCodeOperand(typeof(HediffSet).GetField(nameof(HediffSet.pawn),
																  BindingFlags.Public | BindingFlags.Instance))),
					(OpCodes.Callvirt,
					 new OpCodeOperand(typeof(BodyPartDef).GetMethod(nameof(BodyPartDef.GetMaxHealth),
																	 BindingFlags.Public | BindingFlags.Instance)))
				};

				return PatchMaxHealthReference(insts, pattern, 1);
			}
		}

		[HarmonyPatch(typeof(PawnCapacityUtility.CapacityImpactorBodyPartHealth), nameof(PawnCapacityUtility.CapacityImpactorBodyPartHealth.Readable))]
		static class CapacityImpactorBodyPartHealth
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
			{
				var pattern = new ValueTuple<OpCode, OpCodeOperand?>[]
				{
					(OpCodes.Ldarg_0, null), //part 
                    (OpCodes.Ldfld, new OpCodeOperand(typeof(PawnCapacityUtility.CapacityImpactorBodyPartHealth).GetField(nameof(PawnCapacityUtility.CapacityImpactorBodyPartHealth.bodyPart),
																		BindingFlags.Public | BindingFlags.Instance))),
					(OpCodes.Ldfld,
					 new OpCodeOperand(typeof(BodyPartRecord).GetField(nameof(BodyPartRecord.def),
																	   BindingFlags.Public | BindingFlags.Instance))),
					(OpCodes.Ldarg_1, null), //pawn 
                    (OpCodes.Callvirt,
					 new OpCodeOperand(typeof(BodyPartDef).GetMethod(nameof(BodyPartDef.GetMaxHealth),
																	 BindingFlags.Public | BindingFlags.Instance)))
				};

				return PatchMaxHealthReference(insts, pattern, 2);
			}
		}

		private static IEnumerable<CodeInstruction> PatchMaxHealthReference(IEnumerable<CodeInstruction> insts, ValueTuple<OpCode, OpCodeOperand?>[] pattern, int nullifyPatternIndex)
		{

			List<CodeInstruction> lst = insts.ToList();

			int len = pattern.Length;
			var subArr = new CodeInstruction[len];

			MethodInfo subMethod =
				typeof(BodyUtilities).GetMethod(nameof(BodyUtilities.GetPartMaxHealth),
												BindingFlags.Public | BindingFlags.Static);

			for (var i = 0; i < lst.Count - len; i++)
			{
				for (var j = 0; j < len; j++) subArr[j] = lst[i + j];

				if (!subArr.MatchesPattern(pattern)) continue;

				lst[i + nullifyPatternIndex].opcode = OpCodes.Nop;
				lst[i + nullifyPatternIndex].operand = null;
				lst[i + pattern.Length - 1].opcode = OpCodes.Call;
				lst[i + pattern.Length - 1].operand = subMethod;

				break;
			}


			return lst;
		}
	}
}