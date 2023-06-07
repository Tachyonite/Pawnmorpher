// ProductionCompPatches.cs modified by Iron Wolf for Pawnmorph on 12/26/2019 7:34 AM
// last updated 12/26/2019  7:34 AM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	static class ProductionCompPatches
	{
		[HarmonyPatch(typeof(CompEggLayer)), HarmonyPatch(nameof(CompEggLayer.ProduceEgg))]
		static class EggLayerCompPatch
		{
			[HarmonyPostfix]
			static void ProduceEggPatch([NotNull] CompEggLayer __instance, ref Thing __result)
			{
				if (__result == null || __result.def != PMThingDefOf.EggChickenUnfertilized) return;
				var isInfused = (__instance.parent as Pawn)?.GetAspectTracker()?.Contains(AspectDefOf.MutagenInfused, 0) == true;

				if (isInfused)
				{
					__result.def = PMThingDefOf.TFEgg;
				}

			}
		}

	}
}