// HediffSetPatches.cs created by Iron Wolf for Pawnmorph on 08/25/2021 6:13 PM
// last updated 08/25/2021  6:14 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
	[StaticConstructorOnStartup]
	static class HediffSetPatches
	{

		[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.GetHungerRateFactor)), HarmonyPostfix]
		static void GetHungerRateFactorPatch(ref float __result, HediffDef ignore, List<Hediff> ___hediffs)
		{
			for (int i = 0; i < ___hediffs.Count; i++)
			{
				if (___hediffs[i].def == ignore)
					continue;

				HediffComp_Production comp = ___hediffs[i].TryGetComp<HediffComp_Production>();
				if (comp != null)
				{
					__result *= comp.CurStage.hungerRateFactor;
				}
			}
		}



		[HarmonyPatch(typeof(HediffSet), nameof(HediffSet.GetPartHealth))]
		static class GetPartHealthTranspiler
		{
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> insts)
			{
				var list = insts.ToList();
				FieldInfo pawnField =
					typeof(HediffSet).GetField(nameof(HediffSet.pawn), BindingFlags.Instance | BindingFlags.Public);
				FieldInfo defField =
					typeof(BodyPartRecord).GetField(nameof(BodyPartRecord.def), BindingFlags.Instance | BindingFlags.Public);
				MethodInfo getMaxHealthTarget = typeof(BodyPartDef).GetMethod(nameof(BodyPartDef.GetMaxHealth), BindingFlags.Instance | BindingFlags.Public);
				MethodInfo subMethod =
					typeof(BodyUtilities).GetMethod(nameof(BodyUtilities.GetPartMaxHealth),
													BindingFlags.Static | BindingFlags.Public);

				const int len = 5;
				for (int i = 0; i < list.Count - len; i++)
				{
					CodeInstruction i0 = list[i];
					CodeInstruction i1 = list[i + 1];
					CodeInstruction i2 = list[i + 2];
					CodeInstruction i3 = list[i + 3];
					CodeInstruction i4 = list[i + 4];

					if (i0.opcode != OpCodes.Ldarg_1) continue;
					if (i1.opcode != OpCodes.Ldfld || (FieldInfo)i1.operand != defField) continue;
					if (i2.opcode != OpCodes.Ldarg_0) continue;
					if (i3.opcode != OpCodes.Ldfld || (FieldInfo)i3.operand != pawnField) continue;
					if (i4.opcode != OpCodes.Callvirt || (MethodInfo)i4.operand != getMaxHealthTarget) continue;
					i1.opcode = OpCodes.Nop;
					i1.operand = null;
					i4.opcode = OpCodes.Call;
					i4.operand = subMethod;

					//BodyUtilities.GetPartMaxHeath arguments must be in order (BodyPartRecord, Pawn) 
					//first push part the pawn onto the stack 
				}

				return list;

			}
		}
	}
}