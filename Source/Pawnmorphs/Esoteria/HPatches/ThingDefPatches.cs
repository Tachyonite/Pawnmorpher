using System.Collections.Generic;
using HarmonyLib;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(ThingDef), "SpecialDisplayStats")]
    static class ThingDefPatches
    {

        static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> stats, ThingDef __instance)
        {
            var plantInfo = __instance.GetModExtension<AdditionalPlantInfo>();

            // Just return the stats if no AdditionalPlantInfo is present
            if (plantInfo == null)
                foreach (var stat in stats)
                    yield return stat;

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

            // Replace the hard-coded growth stats with correct ones
            // Note that Same returns equal if the two stats have the same label
            foreach (var stat in stats)
                if (stat.Same(minGrowthTemp))
                    yield return minGrowthTemp;
                else if (stat.Same(maxGrowthTemp))
                    yield return maxGrowthTemp;
                else
                    yield return stat;
        }
    }
}