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

            // Replace the vanilla bedroom thoughts with morph-specific thoughts
            memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
            memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);

            // More or less duplicating the vanilla logic here.  It sucks, but some of the the bedroom levels don't leave memories (and ascetics never have bedroom thoughts)
            // so we can't rely on there being an existing thought to tell us what to use.
            Building_Bed building_Bed = actor.CurrentBed();
            if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners)
            {

                RoomRoleDef roomRole = building_Bed.GetRoom(RegionType.Set_All).Role;

                //Ascetics have a different thought that doesn't take room quality into account
                if (actor.story.traits.HasTrait(TraitDefOf.Ascetic))
                {
                    if (roomRole == RoomRoleDefOf.Bedroom && group.asceticRoomThought != null)
                    {
                        memories.TryGainMemory(ThoughtMaker.MakeThought(group.asceticRoomThought, 0));
                    }
                    else if (roomRole == RoomRoleDefOf.Barracks && group.asceticRoomThought != null)
                    {
                        memories.TryGainMemory(ThoughtMaker.MakeThought(group.asceticRoomThought, 1));
                    }
                }
                else
                {
                    int scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));
                    if (roomRole == RoomRoleDefOf.Bedroom && group.bedroomThoughtReplacement != null)
                    {
                        memories.TryGainMemory(ThoughtMaker.MakeThought(group.bedroomThoughtReplacement, scoreStageIndex));
                    }
                    else if (roomRole == RoomRoleDefOf.Barracks && group.barrakThoughtReplacement != null)
                    {
                        memories.TryGainMemory(ThoughtMaker.MakeThought(group.barrakThoughtReplacement, scoreStageIndex));
                    }
                }

            }

        }
    }
}