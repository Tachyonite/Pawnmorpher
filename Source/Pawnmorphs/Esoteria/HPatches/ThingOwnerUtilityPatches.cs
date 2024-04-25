// ThingOwnerUtilityPatches.cs created by Iron Wolf for Pawnmorph on 11/18/2020 12:46 PM
// last updated 11/18/2020  12:46 PM

using HarmonyLib;
using Pawnmorph.Chambers;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(ThingOwnerUtility))]
	static class ThingOwnerUtilityPatches
	{
		[HarmonyPatch(nameof(ThingOwnerUtility.ContentsSuspended)), HarmonyPostfix]
		static void SuspendPawnsFix(IThingHolder holder, ref bool __result)
		{
			if (__result) return;

			while (holder != null)
			{
				if (holder is MutaChamber)
				{
					__result = true;
					return;
				}

				holder = holder.ParentHolder;
			}
		}
	}
}