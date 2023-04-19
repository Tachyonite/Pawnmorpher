using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.Utilities.PatchUtilities;

namespace Pawnmorph
{
	/// <summary> Draw an info icon for mutations with their descriptions in tooltip. </summary>
	[HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
	[StaticConstructorOnStartup]
	public static class PatchHealthCardUtilityDrawHediffRow
	{
		private static readonly Texture2D icon = ContentFinder<Texture2D>.Get("UI/Icons/Info", true);
		[HarmonyAfter("PeteTimesSix.CompactHediffs")]
		static void Prefix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
		{
			var dLst = diffs.MakeSafe().ToList();
			if (dLst.OfType<IDescriptiveHediff>().FirstOrDefault(x => x.Description != null) == null) return;

			float firstRowWidth = rect.width * 0.275f;
			Rect rectIcon = new Rect(firstRowWidth - icon.width - 4, curY + 1, icon.width, icon.height);
			var toolTipRect = rect;
			toolTipRect.x = rectIcon.x;
			toolTipRect.y = rectIcon.y;
			toolTipRect.height = rectIcon.height * dLst.Count;

			//GUI.DrawTexture(rectIcon, icon);
			TooltipHandler.TipRegion(toolTipRect, () => Tooltip(dLst), (int)curY + 117857);
		}

		static string Tooltip(IEnumerable<Hediff> diffs)
		{
			StringBuilder tooltip = new StringBuilder();
			foreach (var mutation in diffs.OfType<IDescriptiveHediff>())
			{
				var desc = mutation.Description;
				if (desc == null) continue;

				tooltip.AppendLine(desc);
			}

			return tooltip.ToString();

		}
	}


	[HarmonyPatch(typeof(RimWorld.HealthCardUtility), "GetTooltip")]
	static class GetTooltip
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

