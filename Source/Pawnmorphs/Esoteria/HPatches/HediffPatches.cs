// HediffPatches.cs created by Iron Wolf for Pawnmorph on 08/25/2021 7:13 PM
// last updated 08/25/2021  7:13 PM

using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
	internal static class HediffPatches
	{
		[HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.BleedRate), MethodType.Getter)]
		private static class BleedRatePatch
		{
			private static void Postfix(Hediff_Injury __instance, ref float __result)
			{
				if (__result <= 0 || __instance.Part == null) return;
				float mul = BodyUtilities.GetPartHealthMultiplier(__instance.pawn, __instance.Part);

				__result = mul * __result;

			}
		}

		[HarmonyPatch(typeof(Hediff_Injury), nameof(Hediff_Injury.PainOffset), MethodType.Getter)]
		static class PainOffsetPatch
		{
			private static void Postfix(Hediff_Injury __instance, ref float __result)
			{
				if (__result <= 0 || __instance.Part == null) return;
				float mul = BodyUtilities.GetPartHealthMultiplier(__instance.pawn, __instance.Part);

				__result = mul * __result;

			}
		}
	}
}