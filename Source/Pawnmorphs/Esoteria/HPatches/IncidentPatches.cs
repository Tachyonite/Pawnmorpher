// IncidentPatches.cs created by Iron Wolf for Pawnmorph on 07/31/2021 4:17 PM
// last updated 07/31/2021  4:17 PM

using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class IncidentPatches
	{
		[HarmonyPatch(typeof(IncidentWorker_AnimalInsanityMass), "AnimalUsable")]
		static class StopMadSapientAnimalPatch
		{
			static bool Prefix(Pawn p, ref bool __result)
			{
				if (p.IsSapientFormerHuman())
				{
					__result = false;
					return false;
				}

				return true;
			}
		}
	}
}