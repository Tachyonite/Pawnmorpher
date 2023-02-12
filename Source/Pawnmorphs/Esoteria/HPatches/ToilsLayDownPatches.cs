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
		[HarmonyPatch("ApplyBedThoughts"), HarmonyPostfix]
		static void PostfixApplyBedThoughts(Pawn actor)
		{
			var memories = actor.needs?.mood?.thoughts?.memories;
			if (memories == null) return;


			//remove any morph sleeping thoughts 
			foreach (ThoughtDef sleepingThought in MorphUtilities.AllMorphSleepingThoughts)
			{
				memories.RemoveMemoriesOfDef(sleepingThought);
			}

			// Make sure we get the morphdef regardless of whether they're anthro or feral
			var morph = actor.def.GetMorphOfRace() ?? actor.def.TryGetBestMorphOfAnimal();
			var group = morph?.@group;
			if (group == null) return;

			// More or less duplicating the vanilla logic here.  It sucks, but some of the the bedroom levels don't leave memories (and ascetics never have bedroom thoughts)
			// so we can't rely on there being an existing thought to tell us what to use.
			Building_Bed building_Bed = actor.CurrentBed();
			if (building_Bed != null && building_Bed == actor.ownership.OwnedBed && !building_Bed.ForPrisoners)
			{
				RoomRoleDef roomRole = building_Bed.GetRoom(RegionType.Set_All).Role;

				ThoughtDef thought = null;
				int scoreStageIndex = 0;

				//Ascetics have a different thought that doesn't take room quality into account
				if (actor.story.traits.HasTrait(TraitDefOf.Ascetic))
				{
					if (roomRole == RoomRoleDefOf.Bedroom)
					{
						thought = group.asceticRoomThought;
						scoreStageIndex = 0;

						memories.TryGainMemory(ThoughtMaker.MakeThought(group.asceticRoomThought, 0));
					}
					else if (roomRole == RoomRoleDefOf.Barracks)
					{
						thought = group.asceticRoomThought;
						scoreStageIndex = 1;
					}
				}
				else
				{
					scoreStageIndex = RoomStatDefOf.Impressiveness.GetScoreStageIndex(building_Bed.GetRoom().GetStat(RoomStatDefOf.Impressiveness));

					if (roomRole == RoomRoleDefOf.Bedroom)
					{
						thought = group.bedroomThoughtReplacement;
					}
					else if (roomRole == RoomRoleDefOf.Barracks)
					{
						thought = group.barrakThoughtReplacement;
					}
				}

				// Replace the vanilla thoughts with the modified one, if it exists
				if (thought != null)
				{
					memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
					memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
					memories.TryGainMemory(ThoughtMaker.MakeThought(thought, scoreStageIndex));
				}

			}

		}
	}
}