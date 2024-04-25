// ApparelUtilityPatches.cs created by Iron Wolf for Pawnmorph on 09/16/2020 8:34 AM
// last updated 09/16/2020  8:34 AM

using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(ApparelUtility))]
	static class ApparelUtilityPatches
	{
		[HarmonyPriority(Priority.Last)]
		[HarmonyPatch(nameof(ApparelUtility.HasPartsToWear)), HarmonyPrefix]
		static bool FixHasPartsFor(Pawn p, ThingDef apparel, ref bool __result) //vanilla function erroniously assumes if the pawn is not missing any body parts 
		{
			IEnumerable<BodyPartRecord> notMissingParts = p.health.hediffSet.GetNotMissingParts();
			List<BodyPartGroupDef> groups = apparel.apparel.bodyPartGroups;
			int i;
			for (i = 0; i < groups.Count; i++)
			{
				foreach (BodyPartRecord notMissingPart in notMissingParts)
				{
					if (notMissingPart.IsInGroup(groups[i])) //this is actually another search through a list and could be cached if performance is bad 
					{
						__result = true;
						return false;
					}
				}
			}
			__result = false;

			return false;
		}
	}
}