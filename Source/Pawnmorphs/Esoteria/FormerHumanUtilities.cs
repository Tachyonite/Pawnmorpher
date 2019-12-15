// FormerHumanUtilities.cs modified by Iron Wolf for Pawnmorph on 12/08/2019 7:56 AM
// last updated 12/08/2019  7:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static class containing various former human utilities
    /// </summary>
    public static class FormerHumanUtilities
    {
        [NotNull]
        private static readonly float[]
            _sapienceThresholds; //these are the minimum sapience levels needed to fall withing a given enum level 

        static FormerHumanUtilities()
        {
            var values = new[]
            {
                SapienceLevel.Sapient,
                SapienceLevel.MostlySapient,
                SapienceLevel.Conflicted,
                SapienceLevel.MostlyFeral,
                SapienceLevel.Feral
            };

            float delta = 1f / values.Length;
            float counter = 1;
            _sapienceThresholds = new float[values.Length];
            foreach (SapienceLevel sapienceLevel in values
            ) //split up the level thresholds evenly between 1,0 starting at sapient 
            {
                counter -= delta;
                _sapienceThresholds[(int) sapienceLevel] = counter;
            }

            MutationTraits = new[] //TODO mod extension on traits to specify which ones can carry over? 
            {
                TraitDefOf.BodyPurist,
                PMTraitDefOf.MutationAffinity
            };
        }

        /// <summary>
        ///     Gets all former humans on all maps
        /// </summary>
        /// <value>
        ///     All maps player former humans.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllMaps_FormerHumans
        {
            get
            {
                foreach (Pawn allMap in PawnsFinder.AllMaps)
                    if (allMap.GetFormerHumanStatus() != null)
                        yield return allMap;
            }
        }

        /// <summary>
        ///     Gets all former humans on all maps, caravans and traveling transport pods that are alive
        /// </summary>
        /// <value>
        ///     all former humans on all maps, caravans and traveling transport pods that are alive
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive
        {
            get
            {
                foreach (Pawn pawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive)
                    if (pawn.GetFormerHumanStatus() != null)
                        yield return pawn;
            }
        }

        /// <summary>
        ///     Gets all former humans belonging to the player
        /// </summary>
        /// <value>
        ///     All player former humans.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllPlayerFormerHumans
        {
            get
            {
                foreach (Pawn pawn in AllMapsCaravansAndTravelingTransportPods_Alive)
                    if (pawn.Faction == Faction.OfPlayer)
                        yield return pawn;
            }
        }


        /// <summary>
        ///     Gets all sapient animals that are at risk of a minor break .
        /// </summary>
        /// <value>
        ///     All sapient animals minor break risk.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllSapientAnimalsMinorBreakRisk
        {
            get
            {
                foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
                {
                    Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
                    if (saComp?.MentalBreaker?.BreakMinorIsImminent == true) yield return allPlayerFormerHuman;
                }
            }
        }

        /// <summary>
        ///     Gets all sapient animals that are at risk of a major break .
        /// </summary>
        /// <value>
        ///     All sapient animals that are at risk of a major break .
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllSapientAnimalsMajorBreakRisk
        {
            get
            {
                foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
                {
                    Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
                    if (saComp?.MentalBreaker?.BreakMajorIsImminent == true) yield return allPlayerFormerHuman;
                }
            }
        }


        /// <summary>
        ///     Gets all sapient animals at risk of an extreme break.
        /// </summary>
        /// <value>
        ///     All sapient animals at risk of an extreme break.
        /// </value>
        [NotNull]
        public static IEnumerable<Pawn> AllSapientAnimalsExtremeBreakRisk
        {
            get
            {
                foreach (Pawn allPlayerFormerHuman in AllPlayerFormerHumans)
                {
                    Comp_SapientAnimal saComp = allPlayerFormerHuman.GetSapientAnimalComp();
                    if (saComp?.MentalBreaker?.BreakExtremeIsImminent == true) yield return allPlayerFormerHuman;
                }
            }
        }

        /// <summary>
        ///     Gets the break alert label for sapient animals
        /// </summary>
        /// <value>
        ///     The break alert label.
        /// </value>
        [NotNull]
        public static string BreakAlertLabel
        {
            get
            {
                int num = AllSapientAnimalsExtremeBreakRisk.Count();
                string text;
                if (num > 0)
                {
                    text = "BreakRiskExtreme".Translate();
                }
                else
                {
                    num = AllSapientAnimalsMajorBreakRisk.Count();
                    if (num > 0)
                    {
                        text = "BreakRiskMajor".Translate();
                    }
                    else
                    {
                        num = AllSapientAnimalsMinorBreakRisk.Count();
                        text = "BreakRiskMinor".Translate();
                    }
                }

                if (num > 1) text = text + " x" + num.ToStringCached();
                return text;
            }
        }

        /// <summary>
        ///     Gets the break alert explanation for sapient animals .
        /// </summary>
        /// <value>
        ///     The break alert explanation.
        /// </value>
        [NotNull]
        public static string BreakAlertExplanation
        {
            get
            {
                var stringBuilder = new StringBuilder();
                if (AllSapientAnimalsExtremeBreakRisk.Any())
                {
                    var stringBuilder2 = new StringBuilder();
                    foreach (Pawn current in AllSapientAnimalsExtremeBreakRisk)
                        stringBuilder2.AppendLine("    " + current.LabelShort);
                    stringBuilder.Append("BreakRiskExtremeDesc".Translate(stringBuilder2));
                }

                if (AllSapientAnimalsMajorBreakRisk.Any())
                {
                    if (stringBuilder.Length != 0) stringBuilder.AppendLine();
                    var stringBuilder3 = new StringBuilder();
                    foreach (Pawn current2 in AllSapientAnimalsMajorBreakRisk)
                        stringBuilder3.AppendLine("    " + current2.LabelShort);
                    stringBuilder.Append("BreakRiskMajorDesc".Translate(stringBuilder3));
                }

                if (AllSapientAnimalsMinorBreakRisk.Any())
                {
                    if (stringBuilder.Length != 0) stringBuilder.AppendLine();
                    var stringBuilder4 = new StringBuilder();
                    foreach (Pawn current3 in AllSapientAnimalsMinorBreakRisk)
                        stringBuilder4.AppendLine("    " + current3.LabelShort);
                    stringBuilder.Append("BreakRiskMinorDesc".Translate(stringBuilder4));
                }

                stringBuilder.AppendLine();
                stringBuilder.Append("BreakRiskDescEnding".Translate());
                return stringBuilder.ToString();
            }
        }

        [NotNull] private static IEnumerable<TraitDef> MutationTraits { get; }

        /// <summary>
        ///     get the former human status of the given pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns>the former human status, null if the given pawn is not a former human </returns>
        public static FormerHumanStatus? GetFormerHumanStatus([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            Hediff formerHumanHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (formerHumanHediff == null)
            {
                bool hasPFeralHediff = pawn.health?.hediffSet?.GetFirstHediffOfDef(TfHediffDefOf.PermanentlyFeral) != null;

                if (hasPFeralHediff) return FormerHumanStatus.PermanentlyFeral;
                return null;
            }

            if (formerHumanHediff.CurStageIndex >= 2) return FormerHumanStatus.Sapient;
            return FormerHumanStatus.Feral;
        }

        /// <summary>
        ///     Gets the original pawn of the given former human.
        /// </summary>
        /// <param name="formerHuman">The former human.</param>
        /// <returns>the original pawn if it exists, otherwise null</returns>
        [CanBeNull]
        public static Pawn GetOriginalPawnOfFormerHuman([NotNull] Pawn formerHuman)
        {
            foreach (TransformedPawn tfPawn in Find.World.GetComponent<PawnmorphGameComp>().TransformedPawns)
                if (tfPawn.TransformedPawns.Contains(formerHuman))
                    return tfPawn.OriginalPawns.FirstOrDefault();

            return null;
        }


        /// <summary>Gets the quantized sapience level.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>returns null if the pawn isn't a former human</returns>
        public static SapienceLevel? GetQuantizedSapienceLevel([NotNull] this Pawn pawn)
        {
            float? sLevel = GetSapienceLevel(pawn);
            if (sLevel == null) return null;
            if (pawn.GetFormerHumanStatus() == FormerHumanStatus.PermanentlyFeral) return SapienceLevel.PermanentlyFeral;
            for (var index = 0; index < _sapienceThresholds.Length; index++)
            {
                float sapienceThreshold = _sapienceThresholds[index];
                if (sLevel > sapienceThreshold) return (SapienceLevel) index;
            }

            return SapienceLevel.Feral;
        }

        /// <summary>Gets the sapience level of this pawn</summary>
        /// <param name="formerHuman">The former human.</param>
        /// <returns>the sapience level. If feral this is 0, if the given pawn is not a former human returns null</returns>
        public static float? GetSapienceLevel([NotNull] this Pawn formerHuman)
        {
            FormerHumanStatus? fHumanStatus = formerHuman.GetFormerHumanStatus();
            switch (fHumanStatus)
            {
                case FormerHumanStatus.Sapient:
                    return formerHuman.needs.TryGetNeed<Need_Control>()?.CurLevel;
                case FormerHumanStatus.Feral:
                    return 0;
                case FormerHumanStatus.PermanentlyFeral:
                    return 0;
                case null:
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Gets the sapient animal comp.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        [CanBeNull]
        public static Comp_SapientAnimal GetSapientAnimalComp([NotNull] this Pawn pawn)
        {
            return pawn.TryGetComp<Comp_SapientAnimal>();
        }

        /// <summary>Makes the animal sapient. including adding necessary comps, need, training, etc  </summary>
        /// <param name="original">The original.</param>
        /// <param name="animal">The animal.</param>
        /// <param name="sapienceLevel">The sapience level.</param>
        public static void MakeAnimalSapient([NotNull] Pawn original, [NotNull] Pawn animal, float sapienceLevel = 1)
        {
            animal.health.AddHediff(TfHediffDefOf.TransformedHuman);
            Hediff fHumanHediff = animal.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (fHumanHediff == null)
            {
                Log.Error(nameof(fHumanHediff));
                return;
            }

            fHumanHediff.Severity = 1;

            if (original.Faction == Faction.OfPlayer) animal.SetFaction(original.Faction);

            PawnComponentsUtility.AddAndRemoveDynamicComponents(animal);
            if (animal.needs == null)
            {
                Log.Error(nameof(animal.needs));
                return;
            }

            animal.needs.AddOrRemoveNeedsAsAppropriate();
            TransferRelationsToAnimal(original, animal); 
            TransferAspectsToAnimal(original, animal);
            TransferSkillsToAnimal(original, animal);
            var nC = animal.needs.TryGetNeed<Need_Control>();

            if (nC == null)
            {
                Log.Error(nameof(nC));
                return;
            }

            nC.CurLevel = sapienceLevel;

            if (animal.training == null) return;

            foreach (TrainableDef training in DefDatabase<TrainableDef>.AllDefs)
            {
                if (!animal.training.CanBeTrained(training)) continue;

                animal.training.Train(training, null, true);
            }
        }

        private static void TransferRelationsToAnimal([NotNull] Pawn original, [NotNull] Pawn animal)
        {
            if (original.relations == null) return; 
            var enumerator = original.relations.DirectRelations.ToList();
            foreach (DirectPawnRelation directPawnRelation in enumerator)
            {
                if(directPawnRelation.def.implied) continue;
                original.relations?.RemoveDirectRelation(directPawnRelation); //make sure we remove the relations first 
                animal.relations?.AddDirectRelation(directPawnRelation.def, directPawnRelation.otherPawn);//TODO restrict these to special relationships? 
            }

            foreach (Pawn pRelatedPawns in original.relations.PotentiallyRelatedPawns.ToList()) //make copies so we don't  invalidate the enumerator mid way through 
            {
                foreach (PawnRelationDef pawnRelationDef in pRelatedPawns.GetRelations(original).ToList())
                {
                    if(pawnRelationDef.implied) continue;
                    pRelatedPawns.relations.RemoveDirectRelation(pawnRelationDef, original);
                    pRelatedPawns.relations.AddDirectRelation(pawnRelationDef, animal); 

                }
            }

        }

        /// <summary>
        ///     move all mutation related traits from the original pawn to the transformed pawn if they are sapient
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
                Trait trait = originalPawn.story?.traits?.GetTrait(mutationTrait);
                if (trait == null) continue;
                var newTrait = new Trait(mutationTrait, trait.Degree, true);
                transformedPawn.story.traits.GainTrait(newTrait);
            }
        }

        /// <summary>
        ///     Transfers all transferable aspects from the original pawn to animal they turned into.
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="animal">The animal.</param>
        public static void TransferAspectsToAnimal([NotNull] Pawn original, [NotNull] Pawn animal)
        {
            AspectTracker oTracker = original.GetAspectTracker();
            AspectTracker animalTracker = animal.GetAspectTracker();
            if (oTracker == null) return;
            if (animalTracker == null)
            {
                Log.Warning($"animal {animal.Name},{animal.def.defName} does not have an aspect tracker");
                return;
            }


            foreach (Aspect aspect in oTracker)
                if (aspect.def.transferToAnimal)
                {
                    int stageIndex = aspect.StageIndex;
                    animalTracker.Add(aspect.def, stageIndex);
                }
        }

        /// <summary>
        ///     Tries the assign the correct backstory to transformed pawn.
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
                pawn.Name = originalPawn.Name;
            else if (pawn.Name == null) pawn.Name = new NameSingle(pawn.LabelShort);


            BackstoryDef backstoryDef;
            if (pawn.def.defName.ToLower().StartsWith("chao")
            ) //TODO mod extension or something to add specific backgrounds for different animals 
                backstoryDef = BackstoryDefOf.FormerHumanChaomorph;
            else
                backstoryDef = BackstoryDefOf.FormerHumanNormal;

            Log.Message($"adding {backstoryDef.defName} to {pawn.Name}");

            pawn.story.adulthood = backstoryDef.backstory;
        }

        private static void TransferSkillsToAnimal([NotNull] Pawn original, [NotNull] Pawn animal)
        {
            if (animal.skills == null)
            {
                Log.Warning($"sapient animal {animal.Name} does not have a skill tracker");
                return;
            }

            foreach (SkillRecord skillsSkill in original.skills.skills)
                animal.skills.Learn(skillsSkill.def, skillsSkill.XpTotalEarned, true);
        }

        /// <summary>
        /// Gives the sapient animal the hunting thought.
        /// </summary>
        /// <param name="sapientAnimal">The sapient animal.</param>
        /// <param name="prey">The prey.</param>
        public static void GiveSapientAnimalHuntingThought([NotNull] Pawn sapientAnimal, [NotNull] Pawn prey)
        {
            sapientAnimal.TryGainMemory(PMThoughtDefOf.SapientAnimalHuntingMemory); 
        }
    }
}