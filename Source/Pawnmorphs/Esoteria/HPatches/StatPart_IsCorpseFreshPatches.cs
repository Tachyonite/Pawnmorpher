using HarmonyLib;
using RimWorld;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(StatPart_IsCorpseFresh))]
	static class StatPart_IsCorpseFreshPatches
	{
		[HarmonyPatch("TryGetIsFreshFactor"), HarmonyPostfix]
		static bool TryGetIsFreshFactor(bool result, ref float factor)
		{
			if (result)
			{
				// Change nutritional factor for rotten corpses from a factor of 0 to 0.5.
				// Only pawns with iron stomach even get the option to eat rotten corpses, and then a massive mood debuff for doing so.
				factor = 0.5f;
			}

			return result;
		}
	}
}
