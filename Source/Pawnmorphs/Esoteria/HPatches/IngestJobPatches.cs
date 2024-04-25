// InjectJobPatches.cs created by Iron Wolf for Pawnmorph on 03/01/2020 4:08 PM
// last updated 03/01/2020  4:08 PM

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse.AI;

#pragma warning disable 1591
namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(JobDriver_Ingest), "PrepareToIngestToils")]
	public class IngestJobPatches
	{
		[NotNull]
		private static readonly MethodInfo _isToolUser = typeof(FormerHumanUtilities).GetMethod(nameof(FormerHumanUtilities.IsToolUser));

		[HarmonyTranspiler]
		static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions) //evil byte code level hacking 
		{
			var codes = instructions.ToList(); //convert the code instructions to a list so we can do 2 at a time 

			for (var i = 0; i < codes.Count - 1; i++)
			{
				int j = i + 1;
				CodeInstruction instI = codes[i];
				if (instI.opcode == OpCodes.Callvirt && codes[j].opcode == OpCodes.Callvirt)
				{
					instI.opcode =
						OpCodes.Call; //replace the callVirt to get_RaceProps with call to FormerHumanUtilities.IsToolUser 
					instI.operand = _isToolUser; //set the method that the call op is going to call 
					codes[j].opcode = OpCodes.Nop; //replace the second  callVirt to a No op so we don't fuck up the stack 
				}
			}

			return codes;
		}
	}

	static class InjectJobDriverPatches
	{
		[HarmonyPatch(typeof(JobDriver_Ingest), "MakeNewToils")]
		static class AddCleanupToilPatch
		{
			static IEnumerable<Toil> Postfix(IEnumerable<Toil> __result, JobDriver_Ingest __instance)
			{
				foreach (Toil toil in __result)
				{
					yield return toil;
				}

				var eatenThing = __instance?.job?.GetTarget(TargetIndex.A).Thing;
				if (__instance?.pawn?.IsAnimal() != false) yield break;

				if (eatenThing == null || eatenThing.Destroyed) yield break;
				yield return new Toil()
				{
					initAction = Cleanup
				};
				void Cleanup()
				{
					if (eatenThing == null || eatenThing.Destroyed) return;
					if (!(eatenThing is Plant)) return;
					eatenThing.Destroy(); //if a former human carries off a plant make sure it is destroyed when they finish eating it 
				}
			}
		}
	}
}