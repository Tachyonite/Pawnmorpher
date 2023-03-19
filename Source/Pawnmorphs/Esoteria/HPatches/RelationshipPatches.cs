// RelationshipPatches.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 8:22 PM
// last updated 12/22/2019  8:22 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class RelationshipPatches
	{
		[HarmonyPatch(typeof(ThoughtWorker_BondedAnimalMaster)), HarmonyPatch("CurrentStateInternal")]
		static class BondPatch
		{
			[HarmonyPrefix]
			static bool DisableForSapients([NotNull] Pawn p, ref ThoughtState __result)
			{
				if (p.IsFormerHuman() && (p.GetQuantizedSapienceLevel() ?? SapienceLevel.PermanentlyFeral) != SapienceLevel.PermanentlyFeral)
				{
					__result = false;
					return false;
				}

				return true;
			}
		}
	}
}