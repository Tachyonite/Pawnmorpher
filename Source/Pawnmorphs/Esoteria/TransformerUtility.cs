using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hybrids;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace Pawnmorph
{
    /// A class full of useful methods.
    public static class TransformerUtility
    {
        private const string ETHER_BOND_DEF_NAME = "EtherBond";
        private const string ETHER_BROKEN_DEF_NAME = "EtherBroken";
        private static readonly PawnKindDef[] PossiblePawnKinds;


        /// <summary>
        /// Gets the net mutagenic buildup multiplier for this pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        public static float GetMutagenicBuildupMultiplier([NotNull] this Pawn pawn)
        {
            return (pawn.GetStatValue(StatDefOf.ToxicSensitivity) + pawn.GetStatValue(PMStatDefOf.MutagenSensitivity))/2; 
        }

        /// <summary>
        /// Gets the sapient animal comp.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        [CanBeNull]
        public static Comp_SapientAnimal GetSapientAnimalComp([NotNull] this Pawn pawn)
        {
            return pawn.TryGetComp<Comp_SapientAnimal>();
        }

        [NotNull]
        private static MutagenDamageProperties DefaultDamageValues { get; }

        private const float EPSILON = 0.001f;
        /// <summary>
        /// The maximum apparel percent difference. percent differences between old and new size will cause apparel to be completely destroyed 
        /// </summary>
        private const float MAX_APPAREL_PDIFF = 0.35f;

        private const float APPAREL_PDIFF_OFFSET = 0.15f;

        /// <summary>
        /// Transfers all transferable aspects from the original pawn to animal they turned into.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="animal">The animal.</param>
        public static void TransferAspectsToAnimal([NotNull] Pawn original, [NotNull] Pawn animal)
        {
            var oTracker = original.GetAspectTracker();
            var animalTracker = animal.GetAspectTracker();
            if (oTracker == null) return;
            if (animalTracker == null)
            {
                Log.Warning($"animal {animal.Name},{animal.def.defName} does not have an aspect tracker");
                return; 
            }


            foreach (Aspect aspect in oTracker)
            {
                if (aspect.def.transferToAnimal)
                {
                    var stageIndex = aspect.StageIndex;
                    animalTracker.Add(aspect.def, stageIndex); 
                }
            }
        }
        
        /// <summary>
        /// applies damage to all apparel the pawn is wearing based on
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="newRace">The new race.</param>
        /// <param name="mutagen">the mutagen that caused the transformation, if null uses default values for <see cref="MutagenDamageProperties"/></param>
        /// <exception cref="System.ArgumentNullException">
        /// pawn
        /// or
        /// newRace
        /// </exception>
        public static void ApplyTfDamageToApparel([NotNull] Pawn pawn, [NotNull] ThingDef newRace, [CanBeNull] MutagenDef mutagen)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (newRace == null) throw new ArgumentNullException(nameof(newRace));
            Pawn_ApparelTracker apparelTracker = pawn.apparel;
            List<Apparel> cachedApparel = apparelTracker?.WornApparel?.ToList(); //make a copy of all worn apparel
            if (cachedApparel == null || cachedApparel.Count == 0) return;

            MutagenDamageProperties damageProps = mutagen?.damageProperties ?? DefaultDamageValues;

            float oldSize = pawn.RaceProps.baseBodySize;
            float newSize = newRace.race.baseBodySize;
            float percentDiff;
            if (oldSize < EPSILON && newSize < EPSILON) //prevent division by zero 
                percentDiff = 0; //if they're both really small say no change
            else if (oldSize < EPSILON)
                percentDiff = MAX_APPAREL_PDIFF; //if just old size is small then completely destroy the apparel 
            else
            {
                percentDiff = (newSize - oldSize) / oldSize; //signed percent difference between 
                percentDiff += APPAREL_PDIFF_OFFSET; //add a little offset so if the body size is the same or slightly smaller we still apply some damage 
                                    //trying to account for differences in 'body shape' 
            }


            float percentDamage =
                Mathf.Clamp(percentDiff, 0,
                            MAX_APPAREL_PDIFF); //clamp pDiff between [0, Max], if they shrink don't damage apparel  
            percentDamage /= MAX_APPAREL_PDIFF; //normalize the percentDifference to get a percentage to damage apparel by  
            var totalDamage = 0;

            foreach (Apparel apparel in cachedApparel)
            {
                int damage = Mathf.CeilToInt(apparel.MaxHitPoints * percentDamage * damageProps.apparelDamageMultiplier)
                           + damageProps.apparelDamageOffset;
                int newHitPoints = Mathf.Max(apparel.HitPoints - damage, 0);
                totalDamage += apparel.HitPoints - newHitPoints; //save the actual damage done 
                apparel.HitPoints = newHitPoints;
                if (apparel.HitPoints == 0)
                {
                    apparelTracker.Remove(apparel);
                    apparelTracker.Notify_ApparelRemoved(apparel); 
                    apparel.Destroy();
                }
            }

            if (damageProps.biproduct != null && damageProps.spawnedBiproductMult > EPSILON)
            {
                int amountToSpawn = Mathf.RoundToInt(totalDamage * damageProps.spawnedBiproductMult);
                Thing thing = ThingMaker.MakeThing(damageProps.biproduct);
                thing.stackCount = amountToSpawn;

                if (pawn.Spawned)
                {
                    GenPlace.TryPlaceThing(thing, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near);
                }
                else
                {
                    Caravan caravan = pawn.GetCaravan();
                    caravan?.AddPawnOrItem(thing, false);
                }
            }
        }


        /// <summary>
        /// Tries the assign the correct backstory to transformed pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="originalPawn">The original pawn.</param>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static void TryAssignBackstoryToTransformedPawn([NotNull] Pawn pawn, [CanBeNull] Pawn originalPawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (pawn.GetFormerHumanStatus() != FormerHumanStatus.Sapient) return;
            if (pawn.story == null) return;

            if (originalPawn != null)
            {
                pawn.Name = originalPawn.Name; 
            }else if (pawn.Name == null)
            {
                pawn.Name = new NameSingle(pawn.LabelShort);
            }


            BackstoryDef backstoryDef;
            if (pawn.def.defName.ToLower().StartsWith("chao")) //TODO mod extension or something to add specific backgrounds for different animals 
            {
                backstoryDef = BackstoryDefOf.FormerHumanChaomorph;
            }
            else
            {
                backstoryDef = BackstoryDefOf.FormerHumanNormal; 
            }

            Log.Message($"adding {backstoryDef.defName} to {pawn.Name}");

            pawn.story.adulthood = backstoryDef.backstory; 

        }

        /// <summary>
        /// move all mutation related traits from the original pawn to the transformed pawn if they are sapient 
        /// </summary>
        /// <param name="transformedPawn"></param>
        /// <param name="originalPawn"></param>
        public static void MoveMutationTraitsToTransformedPawn([NotNull] Pawn transformedPawn, [NotNull] Pawn originalPawn)
        {
            if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
            if (originalPawn == null) throw new ArgumentNullException(nameof(originalPawn));

            if (transformedPawn.story?.traits == null) return;

            foreach (TraitDef mutationTrait in MutationTraits)
            {
                var trait = originalPawn.story?.traits?.GetTrait(mutationTrait);
                if (trait == null) continue;
                var newTrait = new Trait(mutationTrait, trait.Degree, true);
                transformedPawn.story.traits.GainTrait(newTrait); 
            }


        }
        [NotNull]
        private static IEnumerable<TraitDef> MutationTraits { get; }


        /// <summary>
        /// get the former human status of the given pawn 
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns>the former human status, null if the given pawn is not a former human </returns>
        public static FormerHumanStatus? GetFormerHumanStatus([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            var formerHumanHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (formerHumanHediff == null)
            {
                var hasPFeralHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.PermanentlyFeral) != null;

                if (hasPFeralHediff) return FormerHumanStatus.PermanentlyFeral;
                else return null; 
            }

            if (formerHumanHediff.CurStageIndex >= 2) return FormerHumanStatus.Sapient;
            return FormerHumanStatus.Feral; 
            
        }

        static TransformerUtility()
        {
            PossiblePawnKinds = new[]
            {
                PawnKindDefOf.Slave,
                PawnKindDefOf.Colonist,
                PawnKindDefOf.SpaceRefugee,
                PawnKindDefOf.Villager, 
                PawnKindDefOf.Drifter,
                PawnKindDefOf.AncientSoldier
            };

            MutationTraits = new[] //TODO mod extension on traits to specify which ones can carry over? 
            {
                TraitDefOf.BodyPurist,
                PMTraitDefOf.MutationAffinity
            };
            DefaultDamageValues= new MutagenDamageProperties();
            
        }

        /// <summary> Removes all mutations from a pawn (used post reversion). </summary>
        /// <param name="pawn">The pawn.</param>
        public static void RemoveAllMutations(Pawn pawn)
        {
            List<Hediff> hS2 = new List<Hediff>(pawn.health.hediffSet.hediffs);
            foreach (Hediff hediff in hS2)
            {
                Type hediffClass = hediff.def.hediffClass;
                if (hediffClass == typeof(Hediff_AddedMutation) || hediffClass == typeof(HediffGiver_TF))
                {
                    pawn.health.RemoveHediff(hediff);
                }
            }

            foreach (Hediff_Morph hediffMorph in hS2.OfType<Hediff_Morph>()) //do this second so the morph hediff can cleanup properly 
            {
                pawn.health.RemoveHediff(hediffMorph); //remove ongoing morph hediffs 
            }

            if (pawn.IsHybridRace())
            {
                RaceShiftUtilities.RevertPawnToHuman(pawn); 
            }
        }

        /// <summary>Tries the give post transformation bond relations.</summary> ???
        /// <param name="thrumbo">The thrumbo.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="otherPawn">The other pawn.</param>
        /// <returns></returns>
        public static bool TryGivePostTransformationBondRelations(ref Pawn thrumbo, Pawn pawn, out Pawn otherPawn)
        {
            otherPawn = null;
            var minimumOpinion = 20;
            Func<Pawn, bool> predicate = oP =>
                pawn.relations.OpinionOf(oP) >= minimumOpinion && oP.relations.OpinionOf(pawn) >= minimumOpinion;
            List<Pawn> list = pawn.Map.mapPawns.FreeColonists.Where(predicate).ToList();
            if (!list.NullOrEmpty())
            {
                Dictionary<int, Pawn> dictionary = CandidateScorePairs(pawn, list);
                otherPawn = dictionary[dictionary.Keys.ToList().Max()];
                thrumbo.relations.AddDirectRelation(PawnRelationDefOf.Bond, otherPawn);
                for (var i = 0; i < otherPawn.relations.DirectRelations.Count; i++)
                {
                    DirectPawnRelation val = otherPawn.relations.DirectRelations[i];
                    if (val.otherPawn == pawn) otherPawn.relations.RemoveDirectRelation(val);
                }
            }

            return otherPawn != null;
        }

        /// <summary>Candidates the score pairs.</summary> ???
        /// <param name="pawn">The pawn.</param>
        /// <param name="candidateList">The candidate list.</param>
        /// <returns></returns>
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

        /// <summary>Adds the hediff if not permanently feral.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="hediff">The hediff.</param>
        public static void AddHediffIfNotPermanentlyFeral(Pawn pawn, HediffDef hediff)
        {
            if (!pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && !pawn.health.hediffSet.HasHediff(hediff))
            // If a pawn does not have the PermanentlyFeral hediff nor the provided hediff...
            {
                Hediff xhediff = HediffMaker.MakeHediff(hediff, pawn); // ...create an initialized version of the provided hediff...
                xhediff.Severity = Rand.Range(0.00f, 1.00f); // ...set it to a random severity...
                pawn.health.AddHediff(xhediff); // ...then apply it..
                PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn); //make sure they have all the dynamic comps they need 
                
                //make sure it has a name 
                if(pawn.Name == null)
                {
                    pawn.Name = new NameSingle(pawn.LabelShort); 
                }

                TryAssignBackstoryToTransformedPawn(pawn, null); //try to assign them a background if they're sapient 

            }
        }

        /// <summary>Removes the hediff if permanently feral.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="hediff">The hediff.</param>
        public static void RemoveHediffIfPermanentlyFeral(Pawn pawn, HediffDef hediff)
        {
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("PermanentlyFeral")) && pawn.health.hediffSet.HasHediff(hediff))
            // If the pawn has become permanently feral but still has the provided hediff...
            {
                Hediff xhediff = pawn.health.hediffSet.hediffs.Find(x => x.def == hediff); // ...find a hediff on the pawn that matches the provided one...
                pawn.health.RemoveHediff(xhediff); // ...and remove it.
            }
        }

        /// <summary> Returns true if this pawn is currently an animal or merged morph. </summary>
        public static bool IsAnimalOrMerged([NotNull] this Pawn pawn)
        {
            var comp = Find.World.GetComponent<PawnmorphGameComp>();
            var status = comp.GetPawnStatus(pawn);
            return status == TransformedStatus.Transformed; 
        }


        /// <summary> Converts the age of the given pawn into an equivalent age of the given race. </summary>
        /// <param name="originalPawn"> The original pawn. </param>
        /// <param name="race"> The end race. </param>
        public static float ConvertAge([NotNull] Pawn originalPawn, [NotNull] RaceProperties race)
        {
            if (originalPawn == null) throw new ArgumentNullException(nameof(originalPawn));
            if (race == null) throw new ArgumentNullException(nameof(race));
            var age = originalPawn.ageTracker.AgeBiologicalYearsFloat;
            var originalRaceExpectancy = originalPawn.RaceProps.lifeExpectancy;
            return age * race.lifeExpectancy / originalRaceExpectancy; 
        }

        /// <summary> Generates the random human pawn from a given animal pawn. </summary>
        /// <param name="animal"> The animal. </param>
        public static PawnGenerationRequest GenerateRandomPawnFromAnimal(Pawn animal)
        {
            var convertedAge = ConvertAge(animal, ThingDefOf.Human.race);
            var gender = animal.gender;
            if (Rand.RangeInclusive(0, 100) <= 50)
            {
                switch (gender)
                {
                    
                    case Gender.Male:
                        gender = Gender.Female; 
                        break;
                    case Gender.Female:
                        gender =  Gender.Male;
                        break;
                    case Gender.None:
                    default:
                        break;
                }
            }

            var kind = PossiblePawnKinds.RandElement();

            return new PawnGenerationRequest(kind, Faction.OfPlayer, PawnGenerationContext.NonPlayer,
                                             fixedBiologicalAge: convertedAge,
                                             fixedChronologicalAge: Rand.Range(convertedAge, convertedAge + 200),
                                             fixedGender: gender, fixedMelanin: null);
        }
        
        /// <summary>Gets the transformed gender.</summary>
        /// <param name="original">The original.</param>
        /// <param name="forceGender">The force gender.</param>
        /// <param name="forceGenderChance">The force gender chance.</param>
        /// <returns></returns>
        public static Gender GetTransformedGender(Pawn original, TFGender forceGender, float forceGenderChance)
        {
            Gender animalGender = original.gender;

            // If forceGender was provided, give it a chance to be applied.
            if (forceGender != TFGender.Original && Rand.RangeInclusive(0, 100) <= forceGenderChance)
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
                    if (original.gender == Gender.Male)
                    {
                        animalGender = Gender.Female;
                    }
                    else if (original.gender == Gender.Female)
                    {
                        animalGender = Gender.Male;
                    }
                }
            }

            return animalGender;
        }

        /// <summary>
        ///     Cleans up all references to the original human pawn after creating the animal pawn. <br />
        ///     This does not call Pawn.DeSpawn.
        /// </summary>
        public static void CleanUpHumanPawnPostTf([NotNull] Pawn originalPawn,  [CanBeNull] Hediff cause)
        {
            if (originalPawn == null) throw new ArgumentNullException(nameof(originalPawn));
            HandleApparelAndEquipment(originalPawn); 
            if (cause != null)
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

            if (originalPawn.IsPrisoner)
                HandlePrisoner(originalPawn);


            Caravan caravan = originalPawn.GetCaravan();
            caravan?.RemovePawn(originalPawn);
            caravan?.Notify_PawnRemoved(originalPawn);

            // Make sure any current lords know they can't use this pawn anymore.
            originalPawn.GetLord()?.Notify_PawnLost(originalPawn, PawnLostCondition.IncappedOrKilled);

            if (originalPawn.Faction != Faction.OfPlayer) return; //past here is only relevant for colonists 

            bool IsMasterOfOriginal(Pawn animalPawn) //function to find all animals our pawn is a master of 
            {
                if (animalPawn.playerSettings != null) return animalPawn.playerSettings.Master == originalPawn;

                return false;
            }

            foreach (Pawn animalPawn in PawnsFinder.AllMapsWorldAndTemporary_Alive.Where(IsMasterOfOriginal))
                animalPawn.playerSettings.Master = null; //set to null, these animals don't have a master anymore 
        }

        static void HandlePrisoner(Pawn pawn)
        {
            pawn.guest.Released = true;
            pawn.guest.SetGuestStatus(null);
            DebugLogUtils.Assert(!pawn.guest.IsPrisoner, $"{pawn.Name} is being cleaned up but is still a prisoner");
        }

        private static void HandleApparelAndEquipment(Pawn originalPawn)
        {
            var caravan = originalPawn.GetCaravan();
            var apparelTracker = originalPawn.apparel;
            var equipmentTracker = originalPawn.equipment;

            if (originalPawn.Spawned)
            {
                apparelTracker.DropAll(originalPawn.PositionHeld); // Makes the original pawn drop all apparel...
                equipmentTracker.DropAllEquipment(originalPawn.PositionHeld); // ... and equipment (i.e. guns).
            }
            else if (caravan != null)
            {
                for (int i = apparelTracker.WornApparelCount - 1; i >= 0; i--)
                {
                    var apparel = apparelTracker.WornApparel[i];
                    apparelTracker.Remove(apparel);
                    caravan.AddPawnOrItem(apparel, false); 
                }


                for (int i = equipmentTracker.AllEquipmentListForReading.Count - 1; i >= 0; i--)
                {
                    var equipment = equipmentTracker.AllEquipmentListForReading[i];
                    equipmentTracker.Remove(equipment);
                    caravan.AddPawnOrItem(equipment, false); 
                }
            }
        }

        /// <summary> Get the "ether state" of the pawn (whether they have the ether broken or bonded hediff. </summary>
        public static EtherState GetEtherState([NotNull] this Pawn pawn)
        {
            var etherAspect = pawn.GetAspectTracker()?.GetAspect(AspectDefOf.EtherState);

            if (etherAspect != null)
            {
                return etherAspect.StageIndex == 0 ? EtherState.Broken : EtherState.Bond; 
            }


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
        /// Try to give this pawn a new memory. <br />
        /// If pawn does not have needs/mood/thoughts ect this call does nothing.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="thought">The thought.</param>
        /// <param name="otherPawn">The other pawn.</param>
        /// <param name="respectTraits">if ThoughtUtility.CanGetThought should be checked before giving the thought</param>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static void TryGainMemory([NotNull] this Pawn pawn, Thought_Memory thought, Pawn otherPawn=null, bool respectTraits=true) //move extension methods elsewhere? 
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (respectTraits && !ThoughtUtility.CanGetThought(pawn, thought.def)) return; 


            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thought, otherPawn);
            
        }

        /// <summary>
        /// Try to give this pawn a new memory. <br />
        /// If pawn does not have needs/mood/thoughts ect this call does nothing.
        /// </summary>
        public static void TryGainMemory([NotNull] this Pawn pawn, ThoughtDef thoughtDef, Pawn otherPawn = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtDef, otherPawn); 
        }
    }
}
