// VerbTrackerPatches.cs created by Iron Wolf for Pawnmorph on 08/13/2020 5:12 PM
// last updated 08/13/2020  5:12 PM

using HarmonyLib;
using UnityEngine;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(VerbTracker))]
	internal static class VerbTrackerPatches
	{
		[HarmonyPatch("CreateVerbTargetCommand")]
		[HarmonyPostfix]
		private static void CreateVerbTargetCommandPostfix(Thing ownerThing, Verb verb, Command_VerbTarget __result)
		{
			if (__result == null || ownerThing == null || verb == null) return;

			if (verb is ICustomVerb cVerb)
			{
				string desc = cVerb.GetDescription(ownerThing);
				Texture2D uiIcon = cVerb.GetUIIcon(ownerThing);
				string label = cVerb.GetLabel(ownerThing);
				__result.defaultDesc = string.IsNullOrEmpty(desc) ? __result.defaultDesc : desc;
				__result.icon = uiIcon ? uiIcon : __result.icon;
				__result.defaultLabel = string.IsNullOrEmpty(label) ? __result.defaultLabel : label;
			}
		}
	}
}