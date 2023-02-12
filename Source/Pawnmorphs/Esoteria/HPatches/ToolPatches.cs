// ToolPatches.cs created by Iron Wolf for Pawnmorph on 08/25/2021 4:35 PM
// last updated 08/25/2021  4:35 PM

using HarmonyLib;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class ToolPatches
	{
		[HarmonyPatch(typeof(Tool), nameof(Tool.AdjustedBaseMeleeDamageAmount), typeof(Thing),
			typeof(DamageDef))] //this is only for melee weapons currently, may want to patch VerbProperties functions instead 
		private static class AddEffectivenessMeleeStats
		{
			private static void Postfix(Thing ownerEquipment, DamageDef damageDef, ref float __result, Tool __instance)
			{
				if (__instance?.IsNaturalWeapon() != true) return;
				if (!(ownerEquipment?.ParentHolder is Pawn pawn)) return;
				__result *= pawn.GetStatValue(PMStatDefOf.PM_NaturalMeleeEffectiveness);
			}
		}

		[HarmonyPatch(typeof(Tool), nameof(Tool.AdjustedCooldown), typeof(Thing))]
		private static class AddSpeedMeleeStat
		{
			private static void Postfix(Thing ownerEquipment, ref float __result, ref Tool __instance)
			{
				if (__instance?.IsNaturalWeapon() != true) return;
				if (!(ownerEquipment?.ParentHolder is Pawn pawn)) return;
				__result /= pawn.GetStatValue(PMStatDefOf.PM_NaturalMeleeSpeed);
			}
		}
	}
}