// BedPatches.cs created by Iron Wolf for Pawnmorph on 03/23/2020 4:43 PM
// last updated 03/23/2020  4:43 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(RestUtility))]
	internal static class BedPatches
	{
		[HarmonyPatch(nameof(RestUtility.CanUseBedEver))]
		[HarmonyPostfix]
		private static void CanUsebedEverPostfix(ref bool __result, [NotNull] Pawn p, ThingDef bedDef)
		{
			if (__result == false)
			{
				// If mechanoid, do nothing.
				if (p.RaceProps.IsMechanoid)
					return;

				// Humanlike trying to use animal bed
				// Animal trying to use humanoid bed

				BuildingProperties building = bedDef?.building;
				if (building != null)
				{
					// If bed is too small, do nothing.
					if (p.BodySize > building.bed_maxBodySize)
						return;

					if (p.GetIntelligence() == Intelligence.Humanlike)
					{
						// The pawn has humanlike intelligence and so can only use humanlike beds. (And not cribs!)
						__result = building.bed_humanlike;
					}
					else
					{
						// If humanoids with less than humanlike intelligence must use an animal bed that fits their body size.
						__result = building.bed_humanlike == false;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(Building_Bed))]
	static class BedBuildingPatches
	{
		[HarmonyPatch(nameof(Building_Bed.ForPrisoners), MethodType.Getter), HarmonyPostfix]
		static void FixForPrisoner(ref bool __result, Building_Bed __instance)
		{
			if (!__result)
			{
				if (__instance.Map == null) return;
				__result = __instance.def.building?.bed_humanlike == false && __instance.Position.IsInPrisonCell(__instance.Map);
			}
		}
	}
}