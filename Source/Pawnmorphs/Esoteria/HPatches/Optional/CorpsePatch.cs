using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches.Optional
{
	[OptionalPatch("PMRottingCaption", "PMRottingDescription", nameof(_enabled), true)]
	[HarmonyPatch(typeof(Corpse), nameof(Corpse.IngestibleNow), MethodType.Getter)]
	static class IngestibleNowPatch
	{
		static bool _enabled = true;

		static bool Prepare(MethodBase original)
		{
			if (original == null && _enabled)
				Log.Message("[PM] Optional rotten patch enabled.");

			return _enabled;
		}

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
