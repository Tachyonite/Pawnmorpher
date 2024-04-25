using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.HPatches
{
	[UsedImplicitly]
	[HarmonyPatch(typeof(Pawn_HealthTracker))]
	internal static class PawnHealthTrackerPatches
	{
		[HarmonyPatch(nameof(Pawn_HealthTracker.LethalDamageThreshold), MethodType.Getter), HarmonyPostfix]
		static float LethalDamageThresholdPostfix(float __result, [NotNull] Pawn ___pawn)
		{
			if (___pawn.RaceProps?.Humanlike == true)
			{
				return __result * ___pawn.BodySize;
			}

			return __result;
		}
	}
}
