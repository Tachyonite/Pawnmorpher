// AutoSlaughterPatches.cs created by Iron Wolf for Pawnmorph on 08/26/2021 5:55 PM
// last updated 08/26/2021  5:55 PM

using HarmonyLib;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.HPatches
{
	static class AutoSlaughterPatches
	{
		[HarmonyPatch(typeof(AutoSlaughterManager), nameof(AutoSlaughterManager.CanAutoSlaughterNow))]
		static class CanAutoSlaughterPatch
		{
			static void Postfix([NotNull] Pawn animal, ref bool __result)
			{
				if (__result && animal.IsFormerHuman())
				{
					__result = animal.GetQuantizedSapienceLevel() == SapienceLevel.PermanentlyFeral;
				}
			}
		}
	}
}