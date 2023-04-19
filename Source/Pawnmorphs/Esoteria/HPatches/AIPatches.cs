// AIPatches.cs created by Iron Wolf for Pawnmorph on 03/09/2020 6:08 PM
// last updated 03/09/2020  6:08 PM

using System;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	internal static class AIPatches
	{
		[HarmonyPatch(typeof(JobGiver_EatRandom), "TryGiveJob")]
		private static class EatRandomPatch
		{
			[HarmonyPrefix]
			private static bool Prefix([NotNull] Pawn pawn, ref Job __result)
			{
				SapienceLevel? qSapienceLevel = pawn.GetQuantizedSapienceLevel();
				if (qSapienceLevel != null)
				{
					__result = null;
					switch (qSapienceLevel.Value)
					{
						case SapienceLevel.Sapient:
						case SapienceLevel.MostlySapient:
							return false; //sapient and mostly sapient always respect their restrictions 
						case SapienceLevel.Conflicted:
							return Rand.Value < 0.5f; //conflicted pawns mostly respect the food restrictions 
						case SapienceLevel.MostlyFeral:
						case SapienceLevel.Feral:
						case SapienceLevel.PermanentlyFeral:
							return true; //these never do 
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				return true;
			}
		}
	}

	[HarmonyPatch(typeof(GenAI))]
	internal static class GenAIPatches
	{
		[HarmonyPostfix]
		[HarmonyPatch(nameof(GenAI.MachinesLike))]
		private static void FixMachinesLike(ref bool __result, Faction machineFaction, Pawn p)
		{
			if (__result)
				if (p?.Faction == null && p.IsFormerHuman() && (p.HostFaction != machineFaction || p.IsPrisoner))
					__result = false;
		}
	}
}