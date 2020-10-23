// ToilsLayDownPatches.cs created by Iron Wolf for Pawnmorph on 09/05/2020 4:04 PM
// last updated 09/05/2020  4:04 PM

using HarmonyLib;
using Pawnmorph.Hybrids;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch(typeof(Toils_LayDown))]
    static class ToilsLayDownPatches
    {
        [HarmonyPatch("ApplyBedThoughts"),HarmonyPostfix]
        static void PostfixApplyBedThoughts(Pawn actor)
        {
            var memories = actor.needs?.mood?.thoughts?.memories;
            if (memories == null) return;


            //remove any morph sleeping thoughts 
            foreach (ThoughtDef sleepingThought in MorphUtilities.AllMorphSleepingThoughts)
            {
                memories.RemoveMemoriesOfDef(sleepingThought); 
            }


            var group = actor.def.GetMorphOfRace()?.@group;
            if (group == null) return;
            Building_Bed building_Bed = null; 
            int? scoreStageIndex = null;


            if (group.bedroomThoughtReplacement != null && memories.GetFirstMemoryOfDef(ThoughtDefOf.SleptInBedroom) != null)
            {
                building_Bed = actor.CurrentBed();
                scoreStageIndex =
                    RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness)); 
                memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
                var mem = ThoughtMaker.MakeThought(group.bedroomThoughtReplacement, scoreStageIndex.Value); 
                memories.TryGainMemory(mem);
            }

            if (group.barrakThoughtReplacement != null && memories.GetFirstMemoryOfDef(ThoughtDefOf.SleptInBarracks) != null)
            {
                building_Bed = building_Bed ?? actor.CurrentBed(); //only do this if it wasn't done previously 
                scoreStageIndex = scoreStageIndex ??
                    RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
                var mem = ThoughtMaker.MakeThought(group.barrakThoughtReplacement, scoreStageIndex.Value);
                memories.TryGainMemory(mem); 
            }

        }
    }
}