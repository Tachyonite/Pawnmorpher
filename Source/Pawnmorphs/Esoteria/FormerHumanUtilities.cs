// FormerHumanUtilities.cs modified by Iron Wolf for Pawnmorph on 12/08/2019 7:56 AM
// last updated 12/08/2019  7:56 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using AlienRace;
using HugsLib.Utils;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.FormerHumans;
using Pawnmorph.TfSys;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    ///     static class containing various former human utilities
    /// </summary>
    [StaticConstructorOnStartup]
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
                
            };

            float delta = (1f) / (values.Length+1);
            float counter = 1;
            _sapienceThresholds = new float[values.Length + 2];

            foreach (SapienceLevel sapienceLevel in values
            ) //split up the level thresholds evenly between 1,0 starting at sapient 
            {
                counter -= delta;
                _sapienceThresholds[(int) sapienceLevel] = counter;
            }

            _sapienceThresholds[values.Length] = (_sapienceThresholds[values.Length - 1] + 0) / 12f; 

            _sapienceThresholds[values.Length+1] = 0;


            MutationTraits = new[] //TODO mod extension on traits to specify which ones can carry over? 
            {
                TraitDefOf.BodyPurist,
                PMTraitDefOf.MutationAffinity,
                TraitDefOf.Nudist
            };

            _cachedThresholds = new List<VTuple<SapienceLevel, float>>();
            for (var index = 0; index < _sapienceThresholds.Length; index++)
            {
                float sapienceThreshold = _sapienceThresholds[index];
                _cachedThresholds.Add(new VTuple<SapienceLevel, float>((SapienceLevel) index, sapienceThreshold));
            }


            //grab random names from factions 
            _randomNameGenerators = new List<RulePackDef>(); 
            foreach (FactionDef factionDef in DefDatabase<FactionDef>.AllDefs)
            {
                if(!factionDef.humanlikeFaction || factionDef.hidden || factionDef.pawnNameMaker == null) continue;
                _randomNameGenerators.Add(factionDef.pawnNameMaker);
            }

            Log.Message($"$$$loaded {_randomNameGenerators.Select(r => r.defName).Join(",")} for random name generation");
        }

        /// <summary>
        /// Gets the sapience level thresholds.
        /// </summary>
        /// <value>
        /// The sapience level thresholds.
        /// </value>
        [NotNull]
        public static IEnumerable<VTuple<SapienceLevel, float>> SapienceLevelThresholds => _cachedThresholds; 
       
        [NotNull]
        private static readonly List<VTuple<SapienceLevel, float>> _cachedThresholds; 

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
        /// The related wild former human letter
        /// </summary>
        public const string RELATED_WILD_FORMER_HUMAN_LETTER = "RelatedWildFormerHumanContent";
        /// <summary>
        /// The related wild former human letter label
        /// </summary>
        public const string RELATED_WILD_FORMER_HUMAN_LETTER_LABEL = "RelatedWildFormerHumanLabel";
        /// <summary>
        /// The related sold former human letter
        /// </summary>
        public const string RELATED_SOLD_FORMER_HUMAN_LETTER = "RelatedSoldFormerHumanContent";
        /// <summary>
        /// The related sold former human letter label
        /// </summary>
        public const string RELATED_SOLD_FORMER_HUMAN_LETTER_LABEL = "RelatedSoldFormerHumanLabel";
        /// <summary>
        /// generates notification letters if the given former human is related to any colonists 
        /// </summary>
        /// <param name="formerHuman">The former human.</param>
        /// <param name="letterContentID">The letter content identifier.</param>
        /// <param name="letterLabelID">The letter label identifier.</param>
        public static void NotifyRelatedPawnsFormerHuman([NotNull] Pawn formerHuman, string letterContentID, string letterLabelID)
        {

            var fRelation = formerHuman.relations;
            if (fRelation == null) return;
            var allPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners.MakeSafe();

            foreach (Pawn pawn in allPawns)
            {
                if (pawn == formerHuman) continue;
                var relation = pawn.GetMostImportantRelation(formerHuman);
                if (relation != null && relation != PawnRelationDefOf.Bond)
                {
                    SendRelationLetter(pawn, formerHuman, relation,letterContentID, letterLabelID); 
                }
            }
        }


        /// <summary>
        /// Determines whether the given pawn is a former human.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if the given pawn is former human; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFormerHuman([NotNull] this Pawn pawn)
        {
            return pawn.GetFormerHumanStatus() != null; 
        }

        private static void SendRelationLetter([NotNull] Pawn pawn, [NotNull] Pawn formerHuman, [NotNull] PawnRelationDef relation,string letterContentID, string letterLabelID)
        {
            string relationLabel;

            if (formerHuman.gender == Gender.Female && !string.IsNullOrEmpty(relation.labelFemale))
            {
                relationLabel = relation.labelFemale; 
            }
            else
            {
                relationLabel = relation.label;
            }



            var letterContent = letterContentID.Translate(formerHuman.Named("formerHuman"),
                                                                           pawn.Named("relatedPawn"),
                                                                           relationLabel.Named("relationship"));
            var letterLabel = letterLabelID.Translate(formerHuman.Named("formerHuman"),
                                                                               pawn.Named("relatedPawn"),
                                                                               relationLabel.Named("relationship"));
            Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, formerHuman, formerHuman.HostFaction);

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


        /// <summary>
        /// Gets the traits that transfer between original pawn and transformed pawn
        /// </summary>
        /// 
        /// <value>
        /// The mutation traits.
        /// </value>
        [NotNull] public static IEnumerable<TraitDef> MutationTraits { get; }

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

            return GetQuantizedSapienceLevel(sLevel.Value); 
        }

        /// <summary>
        /// Gets the quantized sapience level.
        /// </summary>
        /// <param name="sapienceLevel">The raw sapience level.</param>
        /// <returns></returns>
        public static SapienceLevel GetQuantizedSapienceLevel(float sapienceLevel)
        {
            for (var index = 0; index < _sapienceThresholds.Length; index++)
            {
                float sapienceThreshold = _sapienceThresholds[index];
                if (sapienceLevel > sapienceThreshold) return (SapienceLevel)index;
            }

            return SapienceLevel.Feral;
        }
        /// <summary>
        /// Finds the random prey for the given predator 
        /// </summary>
        /// <param name="predator">The predator.</param>
        /// <returns></returns>
        public static Pawn FindRandomPreyFor(Pawn predator)
        {
            List<Pawn> resultsList = new List<Pawn>();
            if (predator.meleeVerbs.TryGetMeleeVerb(null) == null)
            {
                return null;
            }
            bool flag = false;
            float summaryHealthPercent = predator.health.summaryHealth.SummaryHealthPercent;
            if (summaryHealthPercent < 0.25f)
            {
                flag = true;
            }
            resultsList.Clear();

            resultsList.AddRange(predator.Map.mapPawns.AllPawnsSpawned);


            Pawn pawn = null;
            var num = 0f;
            bool tutorialMode = TutorSystem.TutorialMode;
            foreach (Pawn pawn2 in resultsList)
            {
                if (predator.GetRoom() != pawn2.GetRoom()) continue;
                if (predator == pawn2) continue;
                if (flag && !pawn2.Downed) continue;
                if (!FoodUtility.IsAcceptablePreyFor(predator, pawn2)) continue;
                if (!predator.CanReach(pawn2, PathEndMode.ClosestTouch, Danger.Deadly)) continue;
                if (pawn2.IsForbidden(predator)) continue;
                if (tutorialMode && pawn2.Faction == Faction.OfPlayer) continue;
                float preyScoreFor = FoodUtility.GetPreyScoreFor(predator, pawn2);
                if (!(preyScoreFor > num) && pawn != null) continue;
                num = preyScoreFor;
                pawn = pawn2;
            }

            resultsList.Clear();
            return pawn;
        }

        /// <summary>Gets the sapience level of this pawn</summary>
        /// <param name="formerHuman">The former human.</param>
        /// <returns>the sapience level. If feral this is 0, if the given pawn is not a former human returns null</returns>
        public static float? GetSapienceLevel([NotNull] this Pawn formerHuman)
        {
            if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
            FormerHumanStatus? fHumanStatus = formerHuman.GetFormerHumanStatus();
            switch (fHumanStatus)
            {
                case FormerHumanStatus.Sapient:
                    return formerHuman.needs?.TryGetNeed<Need_Control>()?.CurLevel;
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
            PawnTransferUtilities.TransferRelations(original, animal);
            PawnTransferUtilities.TransferAspects(original, animal);
            PawnTransferUtilities.TransferSkills(original, animal);
            PawnTransferUtilities.TransferTraits(original, animal, t => MutationTraits.Contains(t));
            TryAssignBackstoryToTransformedPawn(animal, original); 
            var nC = animal.needs.TryGetNeed<Need_Control>();

            if (nC == null)
            {
                Log.Error(nameof(nC));
                return;
            }

            nC.SetInitialLevel(sapienceLevel); 
            //nC.CurLevelPercentage = sapienceLevel;

            if (animal.training == null) return;

            foreach (TrainableDef training in DefDatabase<TrainableDef>.AllDefs)
            {
                if (!animal.training.CanBeTrained(training)) continue;

                animal.training.Train(training, null, true);
            }
        }

        /// <summary>Makes the animal sapient.</summary>
        /// <param name="animal">The animal.</param>
        /// <param name="sapienceLevel">The sapience level.</param>
        public static void MakeAnimalSapient([NotNull] Pawn animal, float sapienceLevel = 1)
        {
            animal.health.AddHediff(TfHediffDefOf.TransformedHuman);
            Hediff fHumanHediff = animal.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (fHumanHediff == null)
            {
                Log.Error(nameof(fHumanHediff));
                return;
            }

            fHumanHediff.Severity = 1;

            PawnKindDef pawnKind = PawnKindDefOf.Villager; //TODO get these randomly 
            Faction ofPlayer = Faction.OfPlayer;
            
            PawnKindDef kind = pawnKind;
            Faction faction = ofPlayer;
            var convertedAge = Mathf.Max(TransformerUtility.ConvertAge(animal, ThingDefOf.Human.race), 17);
            var local = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true,
                                              false, 20f, false, true, true, false, false, false, false,
                                               null, (Predicate<Pawn>) null, null,
                                              convertedAge, null, null, null,
                                              (string) null);
            var lPawn = PawnGenerator.GeneratePawn(local);

            lPawn.equipment?.DestroyAllEquipment(); //make sure all equipment and apparel is removed so they don't spawn with it if reverted
            lPawn.apparel?.DestroyAll();

            PawnComponentsUtility.AddAndRemoveDynamicComponents(animal);

            TryAssignBackstoryToTransformedPawn(animal, lPawn); 
            PawnTransferUtilities.TransferSkills(lPawn, animal);
            PawnTransferUtilities.TransferRelations(lPawn, animal);
            PawnTransferUtilities.TransferTraits(lPawn,animal, t => MutationTraits.Contains(t));
            var inst = new TransformedPawnSingle()
            {
                original = lPawn,
                animal = animal,
                mutagenDef =  MutagenDefOf.defaultMutagen
            };


            var gameComp = Find.World.GetComponent<PawnmorphGameComp>();
            gameComp.AddTransformedPawn(inst); 

            if (animal.needs == null)
            {
                Log.Error(nameof(animal.needs));
                return;
            }

            animal.needs.AddOrRemoveNeedsAsAppropriate();
            var nC = animal.needs.TryGetNeed<Need_Control>();

            if (nC == null)
            {
                Log.Error(nameof(nC));
                return;
            }

            nC.SetInitialLevel(sapienceLevel);
            sapienceLevel = nC.SeekerLevel;

            //now give the animal a name 
            var sapienceQLevel = GetQuantizedSapienceLevel(sapienceLevel);
            if (sapienceQLevel == SapienceLevel.Sapient || sapienceQLevel == SapienceLevel.MostlySapient)
            {
                animal.Name = lPawn.Name; 
            }
            else
            {
                animal.Name = new NameSingle(animal.LabelShort);
            }

            if (animal.training == null) return;

            foreach (TrainableDef training in DefDatabase<TrainableDef>.AllDefs)
            {
                if (!animal.training.CanBeTrained(training)) continue;

                animal.training.Train(training, null, true);
            }

           
        }

        /// <summary>
        ///     Tries the get sapient variant of the specified type for the given level
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceDef">The source definition.</param>
        /// <param name="level">The level.</param>
        /// <param name="variant">The variant.</param>
        /// <returns></returns>
        public static bool TryGetSapientVariant<T>([NotNull] this Def sourceDef, SapienceLevel level, out T variant)
        {
            foreach (DefModExtension sourceDefModExtension in sourceDef.modExtensions.MakeSafe())
                if (sourceDefModExtension is ISapientVariantHolder<T> variantHolder) //can't use GetModExtension for this 
                {
                    variant = variantHolder[level];
                    return variant != null;
                }

            variant = default;
            return false;
        }

        /// <summary>
        ///     Tries the get definition variant for the given formerHuman.
        /// </summary>
        /// this also checks if the variant is valid for the formerHuman, and keeps looking until a valid variant is found or all variants are exhausted
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceDef">The source definition.</param>
        /// <param name="formerHuman">The former human.</param>
        /// <param name="variant">The variant.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     sourceDef
        ///     or
        ///     formerHuman
        /// </exception>
        public static bool TryGetSapientDefVariant<T>([NotNull] this Def sourceDef, [NotNull] Pawn formerHuman, out T variant)
            where T : Def
        {
            if (sourceDef == null) throw new ArgumentNullException(nameof(sourceDef));
            if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
            SapienceLevel? sapientLevel = formerHuman.GetQuantizedSapienceLevel();
            if (sapientLevel == null)
            {
                variant = default;
                return false;
            }

            foreach (DefModExtension sourceDefModExtension in sourceDef.modExtensions.MakeSafe())
                if (sourceDefModExtension is ISapientVariantHolder<T> variantHolder) //can't use GetModExtension for this 
                {
                    T tmp = variantHolder[sapientLevel.Value];
                    if (tmp == null || !tmp.IsValidFor(formerHuman)) continue; //keep looking if the variant is not valid 

                    variant = tmp;
                    return true;
                }

            variant = default;
            return false;
        }

        /// <summary>
        ///     Tries the get a variant for the given formerHuman.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceDef">The source definition.</param>
        /// <param name="formerHuman">The former human.</param>
        /// <param name="variant">The variant.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     sourceDef
        ///     or
        ///     formerHuman
        /// </exception>
        public static bool TryGetSapientVariant<T>([NotNull] this Def sourceDef, [NotNull] Pawn formerHuman, out T variant)
        {
            if (sourceDef == null) throw new ArgumentNullException(nameof(sourceDef));
            if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
            SapienceLevel? sapientLevel = formerHuman.GetQuantizedSapienceLevel();
            if (sapientLevel == null)
            {
                variant = default;
                return false;
            }

            foreach (DefModExtension sourceDefModExtension in sourceDef.modExtensions.MakeSafe())
                if (sourceDefModExtension is ISapientVariantHolder<T> variantHolder) //can't use GetModExtension for this 
                {
                    variant = variantHolder[sapientLevel.Value];
                    return variant != null;
                }

            variant = default;
            return false;
        }

        [NotNull]
        private static readonly List<RulePackDef> _randomNameGenerators;

        private static Name CreateRandomNameFor([NotNull] Pawn formerHumanPawn, float sapienceLevel)
        {
            Log.Message($"$$$$giving name to pawn {formerHumanPawn?.LabelShort ?? "[NULL]"}"); 


            SapienceLevel qSapience = GetQuantizedSapienceLevel(sapienceLevel);
            switch (qSapience)
            {
                case SapienceLevel.Sapient:
                case SapienceLevel.MostlySapient:
                    break;
                case SapienceLevel.Conflicted:
                case SapienceLevel.MostlyFeral:
                case SapienceLevel.Feral:
                case SapienceLevel.PermanentlyFeral:
                    return new NameSingle(formerHumanPawn.LabelShort);
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return PawnBioAndNameGenerator.TryGetRandomUnusedSolidName(formerHumanPawn.gender);
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

            var ext = pawn.def.GetModExtension<FormerHumanSettings>();

            backstoryDef = ext?.backstory ?? BackstoryDefOf.FormerHumanNormal;

            pawn.story.adulthood = backstoryDef.backstory;
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


        static void TransferRelationsToOriginal([NotNull] Pawn pawn,[CanBeNull]  Pawn oPawn, [NotNull]  Pawn_RelationsTracker aRelations)
        {
            var dRelations = aRelations.DirectRelations.MakeSafe().Where(r => !r.def.implied).ToList();

            foreach (DirectPawnRelation directPawnRelation in dRelations)
            {
                aRelations.RemoveDirectRelation(directPawnRelation);
                if (oPawn?.relations != null)
                {
                    if(directPawnRelation.def == PawnRelationDefOf.Bond ) continue;//don't transfer bond 
                    oPawn.relations.AddDirectRelation(directPawnRelation.def, directPawnRelation.otherPawn);
                }
            }

            var pRelated = aRelations.PotentiallyRelatedPawns.MakeSafe().ToList();

            foreach (Pawn pawn1 in pRelated) //move relations from potentially related pawns 
            {
                if (pawn1.relations == null) continue;
                var relations = pawn1.relations.DirectRelations.MakeSafe()
                                     .Where(r => !r.def.implied && r.otherPawn == pawn)
                                     .ToList(); //get all relations from potentially related pawns 



                foreach (DirectPawnRelation directPawnRelation in relations)
                {
                    if(directPawnRelation.def == PawnRelationDefOf.Bond) continue; //don't transfer bond
                    pawn1.relations.RemoveDirectRelation(directPawnRelation);
                    if (oPawn?.relations != null)
                    {
                        pawn1.relations.AddDirectRelation(directPawnRelation.def, oPawn);
                    }
                }

            }
        }

        /// <summary>
        /// Makes the pawn permanently feral.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <exception cref="NotImplementedException"></exception>
        public static void MakePermanentlyFeral(Pawn pawn)
        {
            Hediff fHediff = pawn.health.hediffSet.GetFirstHediffOfDef(TfHediffDefOf.TransformedHuman);
            if (fHediff == null) return;
           

            //transfer relationships back if possible 
            var gComp = Find.World.GetComponent<PawnmorphGameComp>();
            var oPawn = gComp.GetTransformedPawnContaining(pawn)?.First?.OriginalPawns.FirstOrDefault();
            if (oPawn == pawn)
            {
                oPawn = null; 
            }

            Pawn_RelationsTracker aRelations = pawn.relations;
            if (aRelations != null)
            {
                TransferRelationsToOriginal(pawn, oPawn, aRelations);
            }

            pawn.health.AddHediff(TfHediffDefOf.PermanentlyFeral);
            pawn.health.RemoveHediff(fHediff); 

            var loader = Find.World.GetComponent<PawnmorphGameComp>();
            var inst = loader.GetTransformedPawnContaining(pawn)?.First;
            var singleInst = inst as TransformedPawnSingle; //hacky, need to come up with a better solution 
            foreach (var instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>())//needed to handle merges correctly 
            {
                ReactionsHelper.OnPawnPermFeral(instOriginalPawn, pawn, singleInst?.reactionStatus ?? FormerHumanReactionStatus.Wild);
            }

            //remove the original and destroy the pawns 
            foreach (var instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>())
            {
                instOriginalPawn.Destroy();
            }

            if (inst != null)
            {
                loader.RemoveInstance(inst);
            }

            if (inst != null || pawn.Faction == Faction.OfPlayer)
                Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(pawn.LabelShort).CapitalizeFirst(), "LetterHediffFromPermanentTF".Translate(pawn.LabelShort).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn, null, null);

            pawn.needs?.AddOrRemoveNeedsAsAppropriate(); //make sure any comps get added/removed as appropriate 
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn); 




        }

        /// <summary>
        /// Initializes the work settings for the given sapient animal
        /// </summary>
        /// <param name="sapientAnimal">The sapient animal.</param>
        /// <param name="workSettings">The pawn work settings.</param>
        /// <exception cref="ArgumentNullException">
        /// sapientAnimal
        /// or
        /// workSettings
        /// </exception>
        public static void InitializeWorkSettingsFor([NotNull] Pawn sapientAnimal, [NotNull] Pawn_WorkSettings workSettings)
        {
            if (sapientAnimal == null) throw new ArgumentNullException(nameof(sapientAnimal));
            if (workSettings == null) throw new ArgumentNullException(nameof(workSettings));
            var formerHumanExt = sapientAnimal.def.GetModExtension<FormerHumanSettings>();
            var backstoryDef = formerHumanExt?.backstory ?? BackstoryDefOf.FormerHumanNormal;
            var bkStory = backstoryDef.backstory; 
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"for {sapientAnimal.Name}");
            foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
            {
                if (bkStory.DisabledWorkTypes.Contains(workTypeDef))
                {
                    workSettings.SetPriority(workTypeDef, 0); 
                }
                else
                {
                    workSettings.SetPriority(workTypeDef, 3); 
                }
            }

            
        }
    }
}