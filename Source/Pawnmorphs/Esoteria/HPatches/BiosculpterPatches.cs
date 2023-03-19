// BiosculptingPatches.cs created by Iron Wolf for Pawnmorph on 02/01/2022 7:31 AM
// last updated 02/01/2022  7:31 AM

using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	/// <summary>
	/// patches to the CompBiosculpter and related classes 
	/// </summary>
	public static class BiosculpterPatches
	{
		[HarmonyPatch(typeof(CompBiosculpterPod), nameof(CompBiosculpterPod.CannotUseNowPawnReason))]
		static class DisallowFHSculpting
		{
			static void Postfix(Pawn p, ref string __result)
			{
				if (__result == null && p?.IsFormerHuman(false) == true)
				{
					__result = "PMNoFormerHumanSculpting".Translate();
				}
			}
		}

		[HarmonyPatch(typeof(CompBiosculpterPod_HealingCycle), "WillHeal")]
		static class DontHealMutations
		{
			static void Postfix(Pawn pawn, Hediff hediff, ref bool __result)
			{
				// Always prevent mutations from being healed by the biosculptor
				if (hediff is Hediff_AddedMutation) __result = false;
			}
		}
	}
}