using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.TfSys;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Pawnmorph
{
    /// A class full of useful methods.
    public static class TransformerUtility
    {
        private const string ETHER_BOND_DEF_NAME = "EtherBond";
        private const string ETHER_BROKEN_DEF_NAME = "EtherBroken";
        private static readonly PawnKindDef[] PossiblePawnKinds;


        private static List<HediffDef> _allMorphTfs;

        /// <summary>
        /// Scales all injuries on the current pawn to a new race with a different base health scale 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="newRace">The new race.</param>
        /// <exception cref="ArgumentNullException">
        /// pawn
        /// or
        /// newRace
        /// </exception>
        public static void ScaleInjuriesToNewRace([NotNull] Pawn pawn, [NotNull] ThingDef newRace)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (newRace == null) throw new ArgumentNullException(nameof(newRace));

            var hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null) return;

            var healthScaleAmount = Mathf.Min(1, newRace.race.baseHealthScale / pawn.RaceProps.baseHealthScale);

            foreach (Hediff_Injury injury in hediffs.OfType<Hediff_Injury>())
            {
                injury.Severity = injury.Severity * healthScaleAmount; 
            }

        }


        /// <summary>
        /// Gets all morph tf hediffs.
        /// </summary>
        /// <value>
        /// All morph TFS.
        /// </value>
        [NotNull]
        public static IReadOnlyList<HediffDef> AllMorphTfs
        {
            get
            {
                if (_allMorphTfs == null)
                {
                    _allMorphTfs = new List<HediffDef>(); 
                    foreach (MorphDef morphDef in MorphDef.AllDefs)
                    {
                        if(morphDef.fullTransformation != null)
                            _allMorphTfs.AddDistinct(morphDef.fullTransformation);
                        if(morphDef.partialTransformation != null)
                            _allMorphTfs.AddDistinct(morphDef.partialTransformation);
                    } 
                }

                return _allMorphTfs; 
            }
        }


        /// <summary>
        /// Gets the former human reaction status of the given original pawn 
        /// </summary>
        /// <param name="originalPawn">The original pawn.</param>
        /// <returns></returns>
        public static FormerHumanReactionStatus GetFormerHumanReactionStatus([NotNull] this Pawn originalPawn)
        {
            if (originalPawn.IsColonist)
                return FormerHumanReactionStatus.Colonist;
            if (originalPawn.IsPrisonerOfColony)
                return FormerHumanReactionStatus.Prisoner;
            if (originalPawn.guest?.HostFaction == Faction.OfPlayer)
                return FormerHumanReactionStatus.Guest;
            return FormerHumanReactionStatus.Wild; 
        }

        /// <summary>
        /// Gets all pawn transformers in this stage 
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<IPawnTransformer> GetAllTransformers([NotNull] this HediffStage stage)
        {
            if (stage is IPawnTransformer stageTf) yield return stageTf;
            foreach (IPawnTransformer pawnTransformer in stage.hediffGivers.MakeSafe().OfType<IPawnTransformer>())
            {
                yield return pawnTransformer; 
            }
        }

        /// <summary>
        /// Gets all pawn transformers in this hediff def.
        /// </summary>
        /// <param name="hediff">The hediff.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">hediff</exception>
        [NotNull]
        public static IEnumerable<IPawnTransformer> GetAllTransformers([NotNull] this HediffDef hediff)
        {
            if (hediff == null) throw new ArgumentNullException(nameof(hediff));

            foreach (IPawnTransformer pawnTransformer in hediff.GetAllHediffGivers().OfType<IPawnTransformer>())
            {
                yield return pawnTransformer; 
            }


            foreach (IPawnTransformer pawnTransformer in hediff.stages.MakeSafe().OfType<IPawnTransformer>())
            {
                yield return pawnTransformer;
            }


        }

        [NotNull]
        private static MutagenDamageProperties DefaultDamageValues { get; }

        private const float EPSILON = 0.001f;
        /// <summary>
        /// The maximum apparel percent difference. percent differences between old and new size will cause apparel to be completely destroyed 
        /// </summary>
        private const float MAX_APPAREL_PDIFF = 0.35f;

        private const float APPAREL_PDIFF_OFFSET = 0.15f;

        private const int MAX_APPAREL_DAMAGE_PRODUCT_PER_APPAREL = 50; 

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
            int totalStuffProduced=0; 
            foreach (Apparel apparel in cachedApparel)
            {
                int damage = Mathf.CeilToInt(apparel.MaxHitPoints * percentDamage * damageProps.apparelDamageMultiplier)
                           + damageProps.apparelDamageOffset;
                int newHitPoints = Mathf.Max(apparel.HitPoints - damage, 0);
                var damageDone = apparel.HitPoints - newHitPoints; //save the actual damage done 
                
                apparel.HitPoints = newHitPoints;
                if (apparel.HitPoints == 0)
                {
                    apparelTracker.Remove(apparel);
                    apparelTracker.Notify_ApparelRemoved(apparel); 
                    apparel.Destroy();
                }

                var stuffProduced = Mathf.FloorToInt(damageDone * damageProps.spawnedBiproductMult);
                totalStuffProduced += Mathf.Min(stuffProduced, MAX_APPAREL_DAMAGE_PRODUCT_PER_APPAREL);
            }

            if (damageProps.biproduct != null && damageProps.spawnedBiproductMult > EPSILON)
            {
                Thing thing = ThingMaker.MakeThing(damageProps.biproduct);
                thing.stackCount = totalStuffProduced;

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

           
            DefaultDamageValues= new MutagenDamageProperties();
            
        }

        /// <summary>
        /// Sets to race default.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static void SetToRaceDefault([NotNull] Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            RemoveAllMutations(pawn);
            var ext = pawn.def.GetModExtension<RaceMutationSettingsExtension>();
            if (ext != null && ext.mutationRetrievers?.Count > 0)
            {

                foreach (Hediff_AddedMutation mutation in ext.mutationRetrievers.ApplyMutationRetrievers(pawn))
                {
                    var adjComp = mutation.SeverityAdjust;
                    if (adjComp != null)
                    {
                        mutation.Severity = adjComp.NaturalSeverityLimit; 
                    }
                }

            }

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

            foreach (MorphTf hediffMorph in hS2.OfType<MorphTf>()) //do this second so the morph hediff can cleanup properly 
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

                FormerHumanUtilities.TryAssignBackstoryToTransformedPawn(pawn, null); //try to assign them a background if they're sapient 

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
            if (originalPawn?.RaceProps == null) throw new ArgumentNullException(nameof(originalPawn));
            if (race == null) throw new ArgumentNullException(nameof(race));

            return ConvertAge(originalPawn.RaceProps, race, originalPawn.ageTracker.AgeBiologicalYears);
        }

        

        /// <summary>
        /// Converts the age from one race to the equivalent age of another race 
        /// </summary>
        /// <param name="originalRace">The original race.</param>
        /// <param name="endRace">The end race.</param>
        /// <param name="originalAge">The original age.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// originalRace
        /// or
        /// endRace
        /// </exception>
        public static float ConvertAge([NotNull] RaceProperties originalRace, [NotNull] RaceProperties endRace, float originalAge)
        {
            if (originalRace == null) throw new ArgumentNullException(nameof(originalRace));
            if (endRace == null) throw new ArgumentNullException(nameof(endRace));
            return ConvertAge(originalAge, originalRace.lifeExpectancy, endRace.lifeExpectancy); 
        }

        /// <summary>
        /// Converts the age from one life expectancy range to another 
        /// </summary>
        /// <param name="originalAge">The original age.</param>
        /// <param name="originalLifeExpectancy">The original life expectancy.</param>
        /// <param name="newLifeExpectancy">The new life expectancy.</param>
        /// <returns></returns>
        public static float ConvertAge(float originalAge, float originalLifeExpectancy, float newLifeExpectancy)
        {
            return originalAge * newLifeExpectancy / originalLifeExpectancy;
        }

        /// <summary> Generates the random human pawn from a given animal pawn. </summary>
        /// <param name="animal"> The animal. </param>
        public static PawnGenerationRequest GenerateRandomPawnFromAnimal(Pawn animal)
        {
            var convertedAge = Mathf.Max(ConvertAge(animal, ThingDefOf.Human.race),17);
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

            //remove any jobs the pawn may be doing 
            if (originalPawn.jobs != null && originalPawn.Map != null && originalPawn.thinker != null)
            {
                originalPawn.jobs.ClearQueuedJobs();
                originalPawn.jobs.EndCurrentJob(JobCondition.InterruptForced); 
            }




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
                apparelTracker?.DropAll(originalPawn.PositionHeld); // Makes the original pawn drop all apparel...
                equipmentTracker?.DropAllEquipment(originalPawn.PositionHeld); // ... and equipment (i.e. guns).
            }
            else if (caravan != null)
            {

                if (apparelTracker != null)
                {
                    for (int i = apparelTracker.WornApparelCount - 1; i >= 0; i--)
                    {
                        var apparel = apparelTracker.WornApparel[i];
                        apparelTracker.Remove(apparel);
                        caravan.AddPawnOrItem(apparel, false); 
                    }
                }


                if (equipmentTracker != null)
                {
                    for (int i = equipmentTracker.AllEquipmentListForReading.Count - 1; i >= 0; i--)
                    {
                        var equipment = equipmentTracker.AllEquipmentListForReading[i];
                        equipmentTracker.Remove(equipment);
                        caravan.AddPawnOrItem(equipment, false); 
                    }
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
            if (respectTraits && !ThoughtUtility.CanGetThought_NewTemp(pawn, thought.def)) return; 


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
