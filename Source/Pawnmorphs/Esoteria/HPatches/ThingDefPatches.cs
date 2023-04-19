using System.Collections.Generic;
using HarmonyLib;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(ThingDef))]
	static class ThingDefPatches
	{

		[HarmonyPatch("SpecialDisplayStats"), HarmonyPostfix]
		static IEnumerable<StatDrawEntry> FixPlantTempStats(IEnumerable<StatDrawEntry> stats, ThingDef __instance)
		{
			var plantInfo = __instance.GetModExtension<AdditionalPlantInfo>();

			// Just return the stats if no AdditionalPlantInfo is present
			if (plantInfo == null)
			{
				foreach (var stat in stats)
					yield return stat;
				yield break;
			}

			var minGrowthTemp = new StatDrawEntry(StatCategoryDefOf.Basics,
					"MinGrowthTemperature".Translate(),
					plantInfo.minGrowthTemperature.ToStringTemperature("F1"),
					"Stat_Thing_Plant_MinGrowthTemperature_Desc".Translate(),
					4152);
			var maxGrowthTemp = new StatDrawEntry(StatCategoryDefOf.Basics,
					"MaxGrowthTemperature".Translate(),
					plantInfo.maxGrowthTemperature.ToStringTemperature("F1"),
					"Stat_Thing_Plant_MaxGrowthTemperature_Desc".Translate(),
					4153);



			StatDrawEntry minOptimalGrowthTemp = null;
			StatDrawEntry maxOptimalGrowthTemp = null;

			if (plantInfo.minOptimalGrowthTemperature != 0 || plantInfo.maxOptimalGrowthTemperature != 0)
			{
				minOptimalGrowthTemp = new StatDrawEntry(StatCategoryDefOf.Basics,
					"MinOptimalGrowthTemperature".Translate(),
					plantInfo.minOptimalGrowthTemperature.ToStringTemperature("F1"),
					"MinOptimalGrowthTemperature_Desc".Translate(),
					4154);

				maxOptimalGrowthTemp = new StatDrawEntry(StatCategoryDefOf.Basics,
					"MaxOptimalGrowthTemperature".Translate(),
					plantInfo.maxOptimalGrowthTemperature.ToStringTemperature("F1"),
					"MaxOptimalGrowthTemperature_Desc".Translate(),
					4154);
			}



			// Replace the hard-coded growth stats with correct ones
			// Note that Same returns equal if the two stats have the same label
			foreach (var stat in stats)
			{
				if (stat.Same(minGrowthTemp))
					yield return minGrowthTemp;
				else if (stat.Same(maxGrowthTemp))
				{
					yield return maxGrowthTemp;

					// Insert optimal growth range right after normal growth range.
					if (minOptimalGrowthTemp != null)
						yield return minOptimalGrowthTemp;

					if (maxOptimalGrowthTemp != null)
						yield return maxOptimalGrowthTemp;
				}
				else
					yield return stat;
			}
		}
	}
}