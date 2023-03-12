using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
	internal class CorpsePatch
	{
		[HarmonyPatch(typeof(Corpse), nameof(Corpse.IngestibleNow), MethodType.Getter)]
		static class IngestibleNowPatch
		{

			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				foreach (CodeInstruction code in instructions)
				{
					// If call is made to taget method
					if (code.opcode == OpCodes.Call && (code.operand as System.Reflection.MethodInfo).Name == "GetRotStage")
					{
						code.operand = typeof(IngestibleNowPatch).GetMethod(nameof(CanIngestRotten));
						break;
					}
				}

				return instructions;
			}

			public static int CanIngestRotten(Thing thing)
			{
				return (int)RimWorld.RottableUtility.GetRotStage(thing) > 2 ? 1 : 0;
			}
		}
	}
}
