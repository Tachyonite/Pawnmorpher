// MentalBreakPatches.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 2:40 PM
// last updated 12/07/2019  2:40 PM

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	internal static class MentalBreakPatches
	{
		[NotNull] private static readonly List<Pawn> _scratchListPawn = new List<Pawn>();

		[HarmonyPatch(typeof(MentalBreakWorker), nameof(MentalBreakWorker.BreakCanOccur))]
		private static class BreakCanOccurPatch
		{
			[HarmonyPostfix]
			private static void CheckDefRestrictions([NotNull] Pawn pawn, [NotNull] MentalBreakWorker __instance,
													 ref bool __result)
			{
				if (__result) __result = __instance.def.IsValidFor(pawn);
			}
		}

		[HarmonyPatch(typeof(MentalState), nameof(MentalState.RecoverFromState))]
		private static class RecoverFromStatePatch
		{
			[HarmonyPostfix]
			private static void SendNotificationToComp([NotNull] MentalState __instance)
			{
				var receivers = __instance.pawn?.AllComps?.OfType<IMentalStateRecoveryReceiver>();
				foreach (IMentalStateRecoveryReceiver receiver in receivers.MakeSafe())
				{
					receiver.OnRecoveredFromMentalState(__instance);
				}
			}
		}

	}
}