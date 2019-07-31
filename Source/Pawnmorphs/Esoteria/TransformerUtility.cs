using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Thoughts;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Pawnmorph
{
    public static class TransformerUtility
    // A class full of useful methods.
    {
        public static bool TryGivePostTransformationBondRelations(ref Pawn thrumbo, Pawn pawn, out Pawn otherPawn)
        {
            otherPawn = null;
            int minimumOpinion = 20;
            Func<Pawn, bool> predicate = (Pawn oP) => pawn.relations.OpinionOf(oP) >= minimumOpinion && oP.relations.OpinionOf(pawn) >= minimumOpinion;
            List<Pawn> list = pawn.Map.mapPawns.FreeColonists.Where(predicate).ToList();
            if (!GenList.NullOrEmpty<Pawn>((IList<Pawn>)list))
            {
                Dictionary<int, Pawn> dictionary = CandidateScorePairs(pawn, list);
                otherPawn = dictionary[dictionary.Keys.ToList().Max()];
                thrumbo.relations.AddDirectRelation(PawnRelationDefOf.Bond, otherPawn);
                for (int i = 0; i < otherPawn.relations.DirectRelations.Count; i++)
                {
                    DirectPawnRelation val = otherPawn.relations.DirectRelations[i];
                    if (val.otherPawn == pawn)
                    {
                        otherPawn.relations.RemoveDirectRelation(val);
                    }
                }
            }
            return otherPawn != null;
        }

        public static Dictionary<int, Pawn> CandidateScorePairs(Pawn pawn, List<Pawn> candidateList)
        {
            Dictionary<int, Pawn> dictionary = new Dictionary<int, Pawn>();
            for (int i = 0; i < candidateList.Count; i++)
            {
                Pawn val = candidateList[i];
                PawnRelationDef mostImportantRelation = PawnRelationUtility.GetMostImportantRelation(pawn, val);
                int num = pawn.relations.OpinionOf(val);
                int num2 = val.relations.OpinionOf(pawn);
                int key = Mathf.RoundToInt(mostImportantRelation.importance + (float)num + (float)num2);
                dictionary.Add(key, val);
            }
            return dictionary;
        }

        public static void AddHediffIfNotPermanentlyFeral(Pawn pawn, HediffDef hediff)
        {
            if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && !pawn.health.hediffSet.HasHediff(hediff))
            // If a pawn does not have the PermanentlyFeral hediff nor the provided hediff...
            {
                Hediff xhediff = HediffMaker.MakeHediff(hediff, pawn); // ...create an initialized version of the provided hediff...
                xhediff.Severity = Rand.Range(0.00f, 1.00f); // ...set it to a random severity...
                pawn.health.AddHediff(xhediff); // ...then apply it...
            }
        }

        public static void RemoveHediffIfPermanentlyFeral(Pawn pawn, HediffDef hediff)
        {
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && pawn.health.hediffSet.HasHediff(hediff))
            // If the pawn has become permanently feral but still has the provided hediff...
            {
                Hediff xhediff = pawn.health.hediffSet.hediffs.Find(x => x.def == hediff); // ...find a hediff on the pawn that matches the provided one...
                pawn.health.RemoveHediff(xhediff); // ...and remove it.
            }
        }




        public static void Transform(Pawn transformedPawn, Hediff cause, HediffDef hediffForAnimal, List<PawnKindDef> pawnkinds,
                                     TaleDef tale, TFGender forceGender = TFGender.Original, float forceGenderChance = 50f
                                     ) //might want to move the bulk of this somewhere else, in-case we want different tf behaviors? 
        {
            if (transformedPawn.RaceProps.intelligence == Intelligence.Humanlike)
            // If we haven't already checked for the pawn to be tf'd and it possesses humanlike intellegence, give it a chance to transform.
            {
                if (transformedPawn.Map == null) // If the pawn is not currently in the world (i.e. is on caravan)...
                {
                    transformedPawn.health.RemoveHediff(cause); // ...remove the hediff that would otherwise cause a transformation...
                    return; // ...and stop the transformation. (We do it this way because it's a little hard to check for and this keeps the hediff from erroring out.)
                }

                PawnKindDef pawnkind = pawnkinds.RandomElement();
                float animalAge = pawnkind.race.race.lifeExpectancy * transformedPawn.ageTracker.AgeBiologicalYears / transformedPawn.def.race.lifeExpectancy; // The animal is the same percent of the way through it's life as the source pawn is.

                Gender animalGender = transformedPawn.gender;
                if (forceGender != TFGender.Original && Rand.RangeInclusive(0, 100) <= forceGenderChance)
                // If forceGender was provided, give it a chance to be applied.
                {
                    if (forceGender == TFGender.Male)
                    {
                        animalGender = Gender.Male;
                    }
                    else if (forceGender == TFGender.Female)
                    {
                        animalGender = Gender.Female;
                    }
                    else if (forceGender == TFGender.Switch)
                    {
                        if (transformedPawn.gender == Gender.Male)
                        {
                            animalGender = Gender.Female;
                        }
                        else if (transformedPawn.gender == Gender.Female)
                        {
                            animalGender = Gender.Male;
                        }
                    }
                }

                Faction faction = null;
                if (transformedPawn.IsColonist)
                {
                    faction = transformedPawn.Faction;
                }

                Pawn animalToSpawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    pawnkind, faction, PawnGenerationContext.NonPlayer, -1, false, false,
                    false, false, true, false, 1f, false, true, true, false, false, false,
                    false, null, null, null, animalAge, transformedPawn.ageTracker.AgeChronologicalYearsFloat, animalGender));
                // Creates a new animal of pawnkind type, with some of its stats set as those calculated above.

                animalToSpawn.needs.food.CurLevel = transformedPawn.needs.food.CurLevel; // Copies the original pawn's food need to the animal's.
                animalToSpawn.needs.rest.CurLevel = transformedPawn.needs.rest.CurLevel; // Copies the original pawn's rest need to the animal's.
                animalToSpawn.Name = transformedPawn.Name; // Copies the original pawn's name to the animal's.

                if (animalToSpawn.Faction != null)
                {
                    animalToSpawn.training.SetWantedRecursive(TrainableDefOf.Obedience, true); // Sets obediance training to on for the animal.
                    animalToSpawn.training.Train(TrainableDefOf.Obedience, null, true); // Sets the animal's obedience to be fully trained.
                }

                Pawn spawnedAnimal = (Pawn)GenSpawn.Spawn(animalToSpawn, transformedPawn.PositionHeld, transformedPawn.MapHeld, 0); // Spawns the animal into the map.
                Hediff hediff = HediffMaker.MakeHediff(hediffForAnimal, spawnedAnimal, null); // Create a hediff from the one provided (i.e. TransformedHuman)...
                hediff.Severity = Rand.Range(0.00f, 1.00f); // ...give it a random severity...
                spawnedAnimal.health.AddHediff(hediff); // ...and apply it to the new animal.


                PawnMorphInstance pm = new PawnMorphInstance(transformedPawn, spawnedAnimal); // Put the original pawn somewhere safe and tie it to the animal.
                Find.World.GetComponent<PawnmorphGameComp>().addPawn(pm); // ...and put this data somewhere safe.

            

                for (int i = 0; i < 10; i++) // Create a cloud of magic.
                {
                    IntermittentMagicSprayer.ThrowMagicPuffDown(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                    IntermittentMagicSprayer.ThrowMagicPuffUp(spawnedAnimal.Position.ToVector3(), spawnedAnimal.MapHeld);
                }

                if (tale != null) // If a tale was provided, push it to the tale recorder.
                {
                    TaleRecorder.RecordTale(tale, new object[]
                    {
                            transformedPawn,
                            animalToSpawn
                    });
                }
                
                bool wasPrisoner = transformedPawn.IsPrisonerOfColony; 
                CleanUpHumanPawnPostTf(transformedPawn, cause);  //now clean up the original pawn (remove apparel, drop'em, ect) 

                Find.LetterStack.ReceiveLetter("LetterHediffFromTransformationLabel".Translate(transformedPawn.LabelShort, pawnkind.LabelCap).CapitalizeFirst(),
                    "LetterHediffFromTransformation".Translate(transformedPawn.LabelShort, pawnkind.LabelCap).CapitalizeFirst(),
                    LetterDefOf.NeutralEvent, spawnedAnimal, null, null); // Creates a letter saying "Oh no! Pawn X has transformed into a Y!"
                Find.TickManager.slower.SignalForceNormalSpeedShort(); // Slow down the speed of the game.

                transformedPawn.DeSpawn(); // Remove the original pawn from the current map.
                
                ReactionsHelper.OnPawnTransforms(transformedPawn, animalToSpawn, wasPrisoner);

            }
        }

        /// <summary>
        /// just cleans up all references to the original human pawn after creating the animal pawn
        ///  This does not call pawn.DeSpawn 
        /// </summary>
        ///     
        /// <param name="originalPawn"></param>
        static void CleanUpHumanPawnPostTf(Pawn originalPawn, Hediff cause)
        {
            originalPawn.apparel.DropAll(originalPawn.PositionHeld); // Makes the original pawn drop all apparel...
            originalPawn.equipment.DropAllEquipment(originalPawn.PositionHeld); // ... and equipment (i.e. guns).
            originalPawn
               .health.RemoveHediff(cause); // Remove the hediff that caused the transformation so they don't transform again if reverted.

            originalPawn.health.surgeryBills?.Clear(); //if this pawn has any additional surgery bills, get rid of them 

            if (originalPawn.ownership.OwnedBed != null) // If the original pawn owned a bed somewhere...
                originalPawn.ownership.UnclaimBed(); // ...unclaim it.

            if (originalPawn.CarriedBy != null) // If the original pawn was being carried when they transformed...
            {
                Pawn carryingPawn = originalPawn.CarriedBy;
                Thing outPawn;
                carryingPawn.carryTracker.TryDropCarriedThing(carryingPawn.Position, ThingPlaceMode.Direct,
                                                              out outPawn); // ...drop them so they can be removed.
            }

            if (originalPawn.IsPrisonerOfColony)
                originalPawn.guest.SetGuestStatus(null);
          


            //TODO notify faction that their pawn became an animal somehow (this should damage relations maybe?) 

            //originalPawn.GetCaravan()?.Notify_PawnRemoved(originalPawn); 

            originalPawn.GetLord()
                       ?.Notify_PawnLost(originalPawn,
                                         PawnLostCondition
                                            .IncappedOrKilled); //make sure any current lords know they can't use this pawn anymore 

            if (originalPawn.Faction != Faction.OfPlayer) return; //past here is only relevant for colonists 

            bool IsMasterOfOriginal(Pawn animalPawn) //function to find all animals our pawn is a master of 
            {
                if (animalPawn.playerSettings != null) return animalPawn.playerSettings.Master == originalPawn;

                return false;
            }

            foreach (Pawn animalPawn in PawnsFinder.AllMapsWorldAndTemporary_Alive.Where(IsMasterOfOriginal))
                animalPawn.playerSettings.Master = null; //set to null, these animals don't have a master anymore 
        }


        private const string ETHER_BOND_DEF_NAME = "EtherBond";
        private const string ETHER_BROKEN_DEF_NAME = "EtherBroken"; 

        /// <summary>
        /// get the "ether state" of the pawn (whether they have the ether broken or bonded hediff 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static EtherState GetEtherState([NotNull] this Pawn pawn)
        {
            HediffSet hediffs = pawn.health.hediffSet;
            if (hediffs.HasHediff(HediffDef.Named(ETHER_BOND_DEF_NAME)))
            {
                return EtherState.Bond; 
            }

            if (hediffs.HasHediff(HediffDef.Named(ETHER_BROKEN_DEF_NAME)))
            {
                return EtherState.Broken; 
            }

            return EtherState.None; 
        }

        /// <summary>
        /// try to give this pawn a new memory
        /// (this is the same as pawn.needs.mood.thoughts.memories.TryGainMemory just more convenient)
        /// </summary>
        /// if pawn does not have needs/mood/thoughts ect this call does nothing 
        /// 
        /// <param name="pawn"></param>
        /// <param name="thought"></param>
        /// <param name="otherPawn"></param>
        public static void TryGainMemory([NotNull] this Pawn pawn, Thought_Memory thought, Pawn otherPawn=null) //move extension methods elsewhere? 
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought, otherPawn);
            
        }

        /// <summary>
        /// try to give this pawn a new memory
        /// (this is the same as pawn.needs.mood.thoughts.memories.TryGainMemory just more convenient)
        /// </summary>
        /// if pawn does not have needs/mood/thoughts ect this call does nothing 
        /// <param name="pawn"></param>
        /// <param name="thoughtDef"></param>
        /// <param name="otherPawn"></param>
        public static void TryGainMemory([NotNull] this Pawn pawn, ThoughtDef thoughtDef, Pawn otherPawn = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtDef, otherPawn); 
        }

       
        
    }
}
