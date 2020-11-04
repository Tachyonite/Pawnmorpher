// MutationUtilities.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 2:19 PM
// last updated 08/26/2019  2:19 PM

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using static Pawnmorph.DebugUtils.DebugLogUtils;
using Pawnmorph.Hediffs;
using Pawnmorph.User_Interface;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;


namespace Pawnmorph
{
    /// <summary>
    ///     static class containing mutation related utility functions
    /// </summary>
    [StaticConstructorOnStartup]
    public static class MutationUtilities
    {
        [NotNull] private static Dictionary<BodyPartDef, List<MutationDef>> _mutationsByParts;

    
        private static List<BodyPartDef> _allMutablePartDefs;

        private static readonly Dictionary<BodyDef, List<BodyPartRecord>> _allMutablePartsLookup =
            new Dictionary<BodyDef, List<BodyPartRecord>>();

        [NotNull] private static Dictionary<MutationDef, List<BodyPartDef>> _partLookupDict;

        private static List<ThoughtDef> _allThoughts;


        private static List<HediffDef> _allMutationsWithGraphics;

        /// <summary>
        /// Gets a list of all non restricted mutations.
        /// </summary>
        /// <value>
        /// All non restricted mutations.
        /// </value>
        [NotNull]
        public static IReadOnlyList<MutationDef> AllNonRestrictedMutations { get; }

        static MutationUtilities()
        {
            StatDef stat = PMStatDefOf.MutationAdaptability;
            MinMutationAdaptabilityValue = stat.minValue;
            MaxMutationAdaptabilityValue = stat.maxValue;
            AverageMutationAdaptabilityValue = stat.defaultBaseValue;

            AllNonRestrictedMutations = MutationDef.AllMutations.Where(m => !m.IsRestricted).ToList(); 


            //generate some warning for missing mutation defs 
            var warningBuilder = new StringBuilder();
            var anyWarnings = false;
            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefs)
            {
                if (!typeof(Hediff_AddedMutation).IsAssignableFrom(hediffDef.hediffClass)) continue;
                if (hediffDef is MutationDef) continue;
                warningBuilder.AppendLine($"{hediffDef.defName} is a mutation but does not use {nameof(MutationDef)}!");
                anyWarnings = true;
            }

            if (anyWarnings) Log.Warning(warningBuilder.ToString());
            BuildLookupDicts();


            //check for parts with 'Animal' influence, these might be because of a mistake 

            var mutationDefs =
                MutationDef.AllMutations.Where(m => m.classInfluence == null || m.classInfluence == AnimalClassDefOf.Animal)
                           .ToList();
            if (mutationDefs.Count > 0)
            {
                warningBuilder.Clear();

                warningBuilder.AppendLine($"found {mutationDefs.Count} mutations with null or animal influence");
                warningBuilder.AppendLine(mutationDefs.Join(d => d.defName, "\n"));
                Log.Warning(warningBuilder.ToString()); 
            } 

            


        }


        /// <summary>
        ///     Gets the minimum mutation adaptability stat value.
        /// </summary>
        /// <value>
        ///     The minimum mutation adjust value.
        /// </value>
        public static float MinMutationAdaptabilityValue { get; }

        /// <summary>
        ///     Gets the maximum mutation adaptability value.
        /// </summary>
        /// <value>
        ///     The maximum mutation adaptability value.
        /// </value>
        public static float MaxMutationAdaptabilityValue { get; }

        /// <summary>
        ///     Gets the average mutation adaptability value.
        /// </summary>
        /// <value>
        ///     The average mutation adaptability value.
        /// </value>
        public static float AverageMutationAdaptabilityValue { get; }
        
        /// <summary>
        ///     an enumerable collection of all mutation related thoughts
        /// </summary>
        [NotNull]
        public static IEnumerable<ThoughtDef> AllMutationMemories
        {
            get
            {
                if (_allThoughts == null)
                {
                    var mutationsToCheck = MutationDef.AllMutations
                                                      .Where(m =>
                                                                 !m.memoryIgnoresLimit); //if true, the memory should act like a normal memory not a mutation memory, thus not respecting the limit 

                    _allThoughts = new List<ThoughtDef>();
                    foreach (MutationDef mutationDef in mutationsToCheck)
                    {
                        if(mutationDef.mutationMemory != null)
                            _allThoughts.AddDistinct(mutationDef.mutationMemory);
                    }

                } 

                return _allThoughts;
            }
        }


        /// <summary>Gets all mutations with graphics.</summary>
        /// <value>All mutations with graphics.</value>
        public static IEnumerable<HediffDef> AllMutationsWithGraphics
        {
            get
            {
                if (_allMutationsWithGraphics == null) _allMutationsWithGraphics = GetAllMutationsWithGraphics().ToList();

                return _allMutationsWithGraphics;
            }
        }
        [NotNull]
        private static readonly List<BodyPartRecord> _recordCache = new List<BodyPartRecord>();


        private const float MARKET_VALUE_PER_VALUE = 1;


        /// <summary>
        /// Gets the market value for this mutation.
        /// </summary>
        /// this can be negative for bad mutations
        /// <param name="mDef">The m definition.</param>
        /// <returns></returns>
        public static float GetMarketValueFor([NotNull] this MutationDef mDef)
        {
            return mDef.value * MARKET_VALUE_PER_VALUE; 
        }

        /// <summary>
        /// Adds all morph mutations.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="morph">The morph.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <exception cref="ArgumentNullException">pawn
        /// or
        /// morph</exception>
        public static void AddAllMorphMutations([NotNull] Pawn pawn, [NotNull] MorphDef morph, AncillaryMutationEffects? ancillaryEffects = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (morph == null) throw new ArgumentNullException(nameof(morph));
            var hediffSet = pawn.health.hediffSet; 
            _recordCache.Clear();
            _recordCache.AddRange(hediffSet.GetNotMissingParts());
            foreach (MutationDef mutation in morph.AllAssociatedMutations)
            {
                if (mutation.parts != null)
                {
                    foreach (BodyPartDef mutationPart in mutation.parts)
                    foreach (BodyPartRecord bodyPartRecord in _recordCache.Where(r => r.def == mutationPart))
                    {
                        if (hediffSet.HasHediff(mutation, bodyPartRecord)) continue;
                        AddMutation(pawn, mutation, bodyPartRecord, ancillaryEffects:ancillaryEffects);
                    }
                }
                else
                {
                    if (!hediffSet.HasHediff(mutation))
                    {
                        AddMutation(pawn, mutation, ancillaryEffects:ancillaryEffects); 
                    }
                }
            }
        }


        /// <summary>
        /// Tries to add a mutation thought.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutationMemory">The mutation memory.</param>
        /// <param name="ignoreLimit">if set to <c>true</c> ignore the mutation memory limit in the mod settings.</param>
        public static void TryAddMutationThought([NotNull] this Pawn pawn, [NotNull] ThoughtDef mutationMemory, bool ignoreLimit=false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutationMemory == null) throw new ArgumentNullException(nameof(mutationMemory));
            var memoryHandler = pawn.needs?.mood?.thoughts?.memories;
            if (memoryHandler == null) return;

            if (ignoreLimit || !AllMutationMemories.Contains(mutationMemory))//if the memory isn't a mutation memory just add it 
            {
                memoryHandler.TryGainMemory(mutationMemory);
                return; 
            }

            int counter = 0;
            Thought_Memory firstAdded = null;

            foreach (Thought_Memory memory in memoryHandler.Memories)
            {
                if(!AllMutationMemories.Contains(memory.def)) continue;

                counter++; 
                if (firstAdded == null || firstAdded.age < memory.age)
                {

                    firstAdded = memory; 
                }

            }

            var limit = PMUtilities.GetSettings().maxMutationThoughts;
            if (counter >= limit)
            {
                //if we'er at the limit remove the first thought that was added before adding the new one 
                if (firstAdded != null)
                    memoryHandler.RemoveMemory(firstAdded); 
            }

            memoryHandler.TryGainMemory(mutationMemory); 
        }


        private static List<BodyPartDef> AllMutablePartDefs //use lazy initialization 
        {
            get
            {
                if (_allMutablePartDefs == null) _allMutablePartDefs = _mutationsByParts.Keys.ToList();

                return _allMutablePartDefs;
            }
        }

        /// <summary>
        /// Determines whether the specified hediff is prosthetic.
        /// </summary>
        /// <param name="hediff">The hediff.</param>
        /// <returns>
        ///   <c>true</c> if the specified hediff is prosthetic; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">hediff</exception>
        public static bool IsProsthetic([NotNull] this Hediff hediff)
        {
            if (hediff == null) throw new ArgumentNullException(nameof(hediff));
            if (hediff is Hediff_AddedMutation || hediff.def is Hediffs.MutationDef) return false;

            ThingDef prosthetic = hediff.def?.spawnThingOnRemoved;
            bool isProsthetic; 
            if (prosthetic != null)
            {
                isProsthetic = hediff.def.addedPartProps?.solid == true; 
            }
            else
            {
                isProsthetic = false; 
            }
            

            return isProsthetic && hediff is Hediff_AddedPart; 
        }

        /// <summary>
        /// Gets the mutations for the given race and preGenerated pawn
        /// </summary>
        /// <param name="retrievers">The retrievers.</param>
        /// <param name="race">The race.</param>
        /// <param name="preGeneratedPawn">The pre generated pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// retrievers
        /// or
        /// race
        /// </exception>
        [NotNull]
        public static IEnumerable<MutationDef> GetMutationsFor([NotNull] this IEnumerable<IRaceMutationRetriever> retrievers,
                                                               [NotNull] ThingDef race, [CanBeNull] Pawn preGeneratedPawn)
        {
            if (retrievers == null) throw new ArgumentNullException(nameof(retrievers));
            if (race == null) throw new ArgumentNullException(nameof(race));

            return retrievers.SelectMany(r => r.GetMutationsFor(race, preGeneratedPawn)).Distinct();
        }


        /// <summary>
        /// Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="countToAdd">The count to add.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <param name="force">if set to <c>true</c> the mutation will be added regardless if it is valid for the given pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn
        /// or
        /// mutation</exception>
        public static MutationResult AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation, int countToAdd = int.MaxValue, AncillaryMutationEffects? ancillaryEffects = null, bool force=false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            return AddMutation(pawn, mutation, mutation.parts, countToAdd,  ancillaryEffects, force);
        }


        /// <summary>
        /// Applies the mutation retrievers.
        /// </summary>
        /// <param name="retrievers">The retrievers.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="effects">The effects.</param>
        /// <returns></returns>
        public static MutationResult ApplyMutationRetrievers([NotNull] this IEnumerable<IRaceMutationRetriever> retrievers,
                                                             [NotNull] Pawn pawn, AncillaryMutationEffects? effects = null)
        {
            var mutations = retrievers.GetMutationsFor(pawn.def, pawn);

            List<Hediff_AddedMutation> mutationsAdded = new List<Hediff_AddedMutation>();

            foreach (MutationDef mutationDef in mutations)
            {
                mutationsAdded.AddRange(AddMutation(pawn, mutationDef, ancillaryEffects:effects));
            }

            return new MutationResult(mutationsAdded); 

        }

        /// <summary>
        /// Applies the mutation data.
        /// </summary>
        /// <param name="mData">The m data.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// mData
        /// or
        /// pawn
        /// </exception>
        public static MutationResult ApplyMutationData([NotNull] this IReadOnlyMutationData mData, [NotNull] Pawn pawn, AncillaryMutationEffects? ancillaryEffects = null)
        {
            
            if (mData == null) throw new ArgumentNullException(nameof(mData));
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mData.Removing)
            {

                var mutation = pawn.health?.hediffSet?.hediffs.Where(h => h.def == mData.Mutation && h.Part == mData.Part)
                                   .OfType<Hediff_AddedMutation>();
                foreach (Hediff_AddedMutation hediffAddedMutation in mutation.MakeSafe())
                {
                    hediffAddedMutation.MarkForRemoval();
                }

                return MutationResult.Empty;
            }
            
            var mutations = AddMutation(pawn, mData.Mutation, mData.Part, ancillaryEffects);

            foreach (Hediff_AddedMutation mutation in mutations)
            {
                mutation.Severity = mData.Severity;
                if (mutation.SeverityAdjust != null)
                {
                    mutation.SeverityAdjust.Halted = mData.IsHalted; 
                }
            }

            return mutations; 
        }

        /// <summary>
        /// Gets all non missing without prosthetics.
        /// </summary>
        /// <param name="hSet">The h set.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">hSet</exception>
        public static IEnumerable<BodyPartRecord> GetAllNonMissingWithoutProsthetics([NotNull] this HediffSet hSet)
        {
            if (hSet == null) throw new ArgumentNullException(nameof(hSet));

            foreach (BodyPartRecord notMissingPart in hSet.GetNotMissingParts().MakeSafe())
            {
                if(hSet.hediffs.Any(h => h.Part== notMissingPart && h.IsProsthetic())) continue;
                yield return notMissingPart; 
            }

        }

        /// <summary>
        /// Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="parts">The parts.</param>
        /// <param name="countToAdd">The count to add.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <param name="force">if set to <c>true</c> the mutation will be added regardless if it is valid for the given pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn
        /// or
        /// mutation</exception>
        public static MutationResult AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation, [CanBeNull] List<BodyPartDef> parts,
                                       int countToAdd = int.MaxValue, 
                                       AncillaryMutationEffects? ancillaryEffects = null, bool force=false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));

            var addLst = new List<BodyPartRecord>();

            if (parts != null)
            {
                foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetAllNonMissingWithoutProsthetics())
                    if (parts.Contains(notMissingPart.def))
                    {
                        addLst.Add(notMissingPart);
                        if (parts.Count >= countToAdd) break;
                    }

                if (addLst.Count == 0) return MutationResult.Empty;
                return AddMutation(pawn, mutation, addLst, ancillaryEffects, force);
            }

            if(!force && !mutation.IsValidFor(pawn)) return MutationResult.Empty;

            var existingHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(m => m.def == mutation && m.Part == null);
            if (existingHediff != null)
            {
                (existingHediff as Hediff_AddedMutation)?.ResumeAdaption();
                return MutationResult.Empty;
            }

            if (!(HediffMaker.MakeHediff(mutation, pawn) is Hediff_AddedMutation hDef))
            {
                Log.Error($"{mutation.defName} is not a mutation but is being added like one!");
                return MutationResult.Empty;
            }

            pawn.health.AddHediff(hDef);

            DoAncillaryMutationEffects(pawn, mutation, hDef, ancillaryEffects ?? AncillaryMutationEffects.Default); 

            return new MutationResult(hDef);

        }

        /// <summary>
        /// Resumes the adjustment process for this hediff if it is a mutation, does nothing 
        /// </summary>
        /// Resumes the adjustment process for this hediff if it is a mutation, does nothing if the hediff is not a mutation, the mutation is not halted
        /// or the process is complete 
        /// <param name="hediff">The hediff.</param>
        public static void ResumeAdjustment(this Hediff hediff)
        {
            (hediff as Hediff_AddedMutation)?.ResumeAdaption();
        }

        /// <summary>
        /// Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="records">The records to add mutations to</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <param name="force">if set to <c>true</c> the mutation is added regardless if the mutation is valid for the given pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn
        /// or
        /// mutation
        /// or
        /// records</exception>
        public static MutationResult AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
                                       [NotNull] IEnumerable<BodyPartRecord> records, 
                                       AncillaryMutationEffects? ancillaryEffects = null, bool force=false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (records == null) throw new ArgumentNullException(nameof(records));
            HediffSet hSet = pawn.health?.hediffSet;
            if (hSet == null) return MutationResult.Empty; 
            if(!force && !mutation.IsValidFor(pawn)) return MutationResult.Empty;
            List<Hediff_AddedMutation> lst = new List<Hediff_AddedMutation>();
            foreach (BodyPartRecord bodyPartRecord in records)
            {
                if (bodyPartRecord.IsMissingAtAllIn(pawn))
                {
                    LogMsg(LogLevel.Pedantic, $"could not add {mutation.defName} to {pawn.Name} on {bodyPartRecord.Label} because it is missing or has a prosthetic");
                    continue;
                }

                if(!force && HasAnyBlockingMutations(pawn, mutation, bodyPartRecord)) continue;

                var existingMutation = hSet.hediffs.FirstOrDefault(h => h.def == mutation && h.Part == bodyPartRecord);
                if (existingMutation != null) //resume adaption for mutations that are already added instead of re adding them
                {
                    LogMsg(LogLevel.Pedantic, $"could not add {mutation.defName} to {pawn.Name} on {bodyPartRecord.Label} because it already has that mutation");

                    existingMutation.ResumeAdjustment(); //don't do count restarted mutations as new ones 
                    continue;
                }

                var hediff = HediffMaker.MakeHediff(mutation, pawn, bodyPartRecord) as Hediff_AddedMutation;
                if (hediff == null)
                {
                    Log.Error($"{mutation.defName} is not a mutation but is being added like one!");
                    continue;
                }

                lst.Add(hediff); 
                hSet.AddDirect(hediff);
            }

            AncillaryMutationEffects aEffects = ancillaryEffects ?? AncillaryMutationEffects.Default;
            if (lst.Count > 0) //only do this if we actually added any mutations 
            {
                DoAncillaryMutationEffects(pawn, mutation, lst, aEffects);
            }

            return new MutationResult(lst);
        }



        private static void DoAncillaryMutationEffects(Pawn pawn, MutationDef mutation, List<Hediff_AddedMutation> addedParts, in AncillaryMutationEffects aEffects)
        {

            if (mutation.mutationMemory != null && aEffects.AddMemory)
            {
                TryAddMutationThought(pawn, mutation.mutationMemory);
            }

            if (PawnUtility.ShouldSendNotificationAbout(pawn) && mutation.mutationTale != null && aEffects.AddTale)
                TaleRecorder.RecordTale(mutation.mutationTale, pawn);

            if (aEffects.AddLogEntry && !addedParts.NullOrEmpty())
            {
                var logEntry = new MutationLogEntry(pawn, mutation, addedParts.MakeSafe().Where(p => p?.Part != null).Select(p => p.Part.def).Distinct());
                Find.PlayLog?.Add(logEntry);
            }

            if (pawn.MapHeld != null && aEffects.ThrowMagicPuff)
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
        }

        private static void DoAncillaryMutationEffects(Pawn pawn, MutationDef mutation, Hediff_AddedMutation addedParts, in AncillaryMutationEffects aEffects)
        {
            if (mutation.mutationMemory != null && aEffects.AddMemory)
            {
                TryAddMutationThought(pawn, mutation.mutationMemory);
            }

            if (PawnUtility.ShouldSendNotificationAbout(pawn) && mutation.mutationTale != null && aEffects.AddTale)
                TaleRecorder.RecordTale(mutation.mutationTale, pawn);

            if (aEffects.AddLogEntry && addedParts?.Part != null)
            {
                var logEntry = new MutationLogEntry(pawn, mutation, addedParts.Part.def);
                Find.PlayLog?.Add(logEntry);
            }

            if (pawn.MapHeld != null && aEffects.ThrowMagicPuff)
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
        }


        /// <summary>
        ///     Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="record">The records to add mutations to</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     records
        /// </exception>
        public static MutationResult AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
                                       [NotNull] BodyPartRecord record,
                                       AncillaryMutationEffects? ancillaryEffects = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (record == null) throw new ArgumentNullException(nameof(record));

            var debugLog = pawn.GetMutationTracker()?.DebugMessagesEnabled == true; 
            HediffSet hSet = pawn.health?.hediffSet;
            if (hSet == null)
            {
                if (debugLog)
                {
                    Log.Message($"{pawn.Label} has no hediffSet!");
                }
                return MutationResult.Empty;
            }

            if (record.IsMissingAtAllIn(pawn))
            {
                if (debugLog)
                {
                    Log.Message($"{pawn.Label} is missing all parts of {record.Label}");
                }
                
                return MutationResult.Empty;
            }


           

            Hediff existingMutation = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == mutation && h.Part == record);
            if (existingMutation != null)
            {
                existingMutation.ResumeAdjustment();
                if (debugLog)
                {
                    Log.Message($"{pawn.Label} already has {mutation.defName}");

                }
                return MutationResult.Empty;
            }

            if (HasAnyBlockingMutations(pawn, mutation, record))
            {
                if (debugLog)
                {
                    Log.Message($"{pawn.Label} has blocking mutations!");

                }
                return MutationResult.Empty;
            }

            var hediff = HediffMaker.MakeHediff(mutation, pawn, record) as Hediff_AddedMutation;
            if (hediff == null)
            {
                Log.Error($"{mutation.defName} is not a mutation but is being added like one!");
                return MutationResult.Empty;
            }

            hSet.AddDirect(hediff);
            

            AncillaryMutationEffects aEffects = ancillaryEffects ?? AncillaryMutationEffects.Default;

            DoAncillaryMutationEffects(pawn, mutation, hediff, aEffects);
            return new MutationResult(hediff);

        }

       
        private static bool HasAnyBlockingMutations(Pawn pawn, MutationDef mutation, BodyPartRecord record)
        {
            foreach (Hediff_AddedMutation curMutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
            {
                if (curMutation.Blocks(mutation, record)) return true; 
            }

            return false; 
        }


        /// <summary>
        ///     Determines whether this instance can apply mutations to the specified pawn.
        /// </summary>
        /// <param name="mutationGiver">The mutation giver.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///     <c>true</c> if this instance can apply mutations to the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanApplyMutations([NotNull] this HediffGiver_Mutation mutationGiver, [NotNull] Pawn pawn)
        {
            if (mutationGiver.partsToAffect == null) return false;

            IEnumerable<BodyPartRecord> allRecordsToCheck = pawn.health.hediffSet.GetNotMissingParts()
                                                                .Where(p => mutationGiver.partsToAffect.Contains(p.def));


            List<BodyPartRecord> mutatedParts = pawn.health.hediffSet.hediffs.Where(h => h.def == mutationGiver.hediff)
                                                    .Select(h => h.Part)
                                                    .Distinct()
                                                    .ToList();

            return
                allRecordsToCheck.Any(p => !mutatedParts
                                              .Contains(p)); //if there are any non missing part missing mutations then the hediff_giver can be applied 
        }

        /// <summary>
        ///     Clears the overlapping mutations.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <exception cref="ArgumentNullException">
        ///     mutationDef
        ///     or
        ///     pawn
        /// </exception>
        public static void ClearOverlappingMutations([NotNull] this MutationDef mutationDef, [NotNull] Pawn pawn)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            var rmLst = new List<Hediff_AddedMutation>();
            foreach (Hediff_AddedMutation mutation in pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>())
                //make a list of all the stuff to remove 
                if (mutation.def != mutationDef && mutationDef.parts?.Contains(mutation.Part?.def)==true)
                    rmLst.Add(mutation);

            //no remove all the mutations 
            foreach (Hediff_AddedMutation hediffAddedMutation in rmLst) pawn.health.RemoveHediff(hediffAddedMutation);
        }

        /// <summary>
        ///     Gets all part def mutation sites.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">mutationDef</exception>
        [NotNull]
        public static IEnumerable<(BodyPartDef bodyPart, MutationLayer layer)> GetAllDefMutationSites(
            [NotNull] this MutationDef mutationDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if(mutationDef.parts == null) yield break;
            if (mutationDef.RemoveComp == null)
            {
                yield break;
            }
            foreach (BodyPartDef mutationDefPart in mutationDef.parts)
                yield return (mutationDefPart, mutationDef.RemoveComp.layer);
        }


        /// <summary>
        ///     Gets all mutable part on this body def
        /// </summary>
        /// <param name="bodyDef">The body definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">bodyDef</exception>
        public static IEnumerable<BodyPartRecord> GetAllMutableParts([NotNull] this BodyDef bodyDef)
        {
            if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));

            if (_allMutablePartsLookup.TryGetValue(bodyDef, out List<BodyPartRecord> recordList)
            ) //see if we already calculated the list previously 
                return recordList;
            recordList = new List<BodyPartRecord>();

            foreach (BodyPartRecord bodyPartRecord in bodyDef.AllParts)
                if (AllMutablePartDefs.Contains(bodyPartRecord.def))
                    recordList.Add(bodyPartRecord);

            _allMutablePartsLookup[bodyDef] = recordList; //cache the result so we only have to do this once 
            return recordList;
        }

        /// <summary>
        ///     Gets all mutation sites.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="bDef">The b definition.</param>
        /// <returns></returns>
        public static IEnumerable<MutationSite> GetAllMutationSites(
            [NotNull] this MutationDef mutationDef, [NotNull] BodyDef bDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (bDef == null) throw new ArgumentNullException(nameof(bDef));
            if(mutationDef.RemoveComp == null) yield break;

            IEnumerable<BodyPartRecord> bodyPartRecords;
            if (bDef.AllParts.NullOrEmpty())
            {
                Log.Error($"body def \"{bDef.defName}\" has no body parts or has not been initialized correctly");
                bodyPartRecords = TreeUtilities.Preorder(bDef.corePart, r => r.parts.MakeSafe()); 
            }else 
                bodyPartRecords = bDef.AllParts.MakeSafe();
            foreach (BodyPartRecord bodyPartRecord in bodyPartRecords)
                if (mutationDef.parts.MakeSafe().Contains(bodyPartRecord?.def))
                    yield return new MutationSite(bodyPartRecord, mutationDef.RemoveComp.layer);
        }


        /// <summary>Gets the mutations by part def.</summary>
        /// <param name="bodyPartDef">The body part definition.</param>
        /// <returns></returns>
        public static IEnumerable<MutationDef> GetMutationsByPart([NotNull] BodyPartDef bodyPartDef)
        {
            return _mutationsByParts.TryGetValue(bodyPartDef) ?? Enumerable.Empty<MutationDef>();
        }

        /// <summary>
        ///     try to get the mutation tracker on this pawn, null if the pawn does not have a tracker
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="warnOnFail">if the pawn does not have a mutation tracker, display a warning message</param>
        /// <returns></returns>
        [CanBeNull]
        public static MutationTracker GetMutationTracker([NotNull] this Pawn pawn, bool warnOnFail = true)
        {
            var comp = pawn.GetComp<MutationTracker>();
            if (comp == null && warnOnFail) Warning($"pawn {pawn.Name} does not have a mutation tracker comp");
            return comp;
        }


        /// <summary>
        ///     get the pawn's outlook toward being mutated
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static MutationOutlook GetMutationOutlook([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            var aspectTracker = pawn.GetAspectTracker();
            if (aspectTracker?.Contains(AspectDefOf.PrimalWish) == true) return MutationOutlook.PrimalWish; 
            TraitSet traits = pawn.story?.traits;
            if (traits == null) return MutationOutlook.Neutral;
            if (traits.HasTrait(PMTraitDefOf.MutationAffinity)) return MutationOutlook.Furry;
            if (traits.HasTrait(TraitDefOf.BodyPurist)) return MutationOutlook.BodyPurist;

            

            return MutationOutlook.Neutral;
        }


        /// <summary>Gets the part to add hediffs to.</summary>
        /// <param name="giver">The giver.</param>
        /// <returns></returns>
        [NotNull]
        public static IEnumerable<BodyPartDef> GetPartsToAddTo([NotNull] this HediffGiver giver)
        {
            return giver.partsToAffect ?? Enumerable.Empty<BodyPartDef>();
        }


        /// <summary>
        ///     get the production hediffs of the pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static IEnumerable<Hediff> GetProductionMutations([NotNull] this Pawn pawn)
        {
            MutationTracker comp = pawn.GetMutationTracker();
            if (comp == null) yield break;
            foreach (Hediff_AddedMutation mutation in comp.AllMutations)
                if (mutation.TryGetComp<HediffComp_Production>() != null)
                    yield return mutation;
        }

        /// <summary>
        ///     Determines whether this part or any of it's parent is missing at all in the specified pawn.
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///     <c>true</c> if this part or any of it's parents is missing at all in the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     record
        ///     or
        ///     pawn
        /// </exception>
        public static bool IsMissingAtAllIn([NotNull] this BodyPartRecord record, [NotNull] Pawn pawn)
        {
            if (record is null) throw new ArgumentNullException(nameof(record));

            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            HediffSet hediffSet = pawn.health.hediffSet;
            while (record != null)
            {
                //check if the part is missing or it has a prosthetic attached to it 
                BodyPartRecord record1 = record;
                if (hediffSet.PartIsMissing(record) || hediffSet.hediffs.Any(h => h.Part == record1 && h.IsProsthetic())) return true;
                record = record.parent;
            }

            return false;
        }

        /// <summary>Determines whether this instance is obsolete.</summary>
        /// <param name="def">The definition.</param>
        /// <returns>
        ///     <c>true</c> if the specified definition is obsolete; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">def</exception>
        public static bool IsObsolete([NotNull] this HediffDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            return def.GetType().HasAttribute<ObsoleteAttribute>() || def.hediffClass.HasAttribute<ObsoleteAttribute>();
        }

        /// <summary>
        ///     checks if this mutation overlaps with the given mutation
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="otherMutation">The other mutation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     mutationDef
        ///     or
        ///     otherMutation
        /// </exception>
        [Pure]
        public static bool OverlapsWith([NotNull] this MutationDef mutationDef, [NotNull] MutationDef otherMutation)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));

            //make sure this mutation def has all the same parts as other mutation 

            foreach (BodyPartDef bodyPartDef in mutationDef.parts.MakeSafe())
                if (otherMutation.parts.MakeSafe().Contains(bodyPartDef) == false)
                    return false; //if mutation Def has any part that other mutation does not they do not overlap 

            RemoveFromPartCompProperties thRmComp = mutationDef.RemoveComp;
            RemoveFromPartCompProperties othRmComp = otherMutation.RemoveComp;

            //make sure the layers overlap 
            return thRmComp.layer != othRmComp.layer;
        }

        private static void BuildLookupDicts()
        {
            //build the lookup table of part sorted by the mutations that affect them 
            _mutationsByParts = new Dictionary<BodyPartDef, List<MutationDef>>();
            _partLookupDict = new Dictionary<MutationDef, List<BodyPartDef>>();
            foreach (MutationDef mutation in MutationDef.AllMutations)
            {
                IEnumerable<BodyPartDef> allParts = mutation.parts.MakeSafe();
                _partLookupDict[mutation] = allParts.ToList();
            }
            //now build the reverse lookup table 

            foreach (KeyValuePair<MutationDef, List<BodyPartDef>> kvp in _partLookupDict)
            foreach (BodyPartDef bodyPartDef in kvp.Value)
            {
                List<MutationDef> mutations;
                if (!_mutationsByParts.TryGetValue(bodyPartDef, out mutations))
                {
                    mutations = new List<MutationDef>
                    {
                        kvp.Key
                    };
                    _mutationsByParts[bodyPartDef] = mutations;
                }
                else
                {
                    mutations.Add(kvp.Key);
                }
            }
        }

        private static IEnumerable<HediffDef> GetAllMutationsWithGraphics()
        {
            List<AlienPartGenerator.BodyAddon> bodyAddons =
                ((ThingDef_AlienRace) ThingDefOf.Human).alienRace.generalSettings.alienPartGenerator.bodyAddons;
            var hediffDefs =
                bodyAddons.SelectMany(add => add.hediffGraphics ?? Enumerable.Empty<AlienPartGenerator.BodyAddonHediffGraphic>())
                          .Select(h => h.hediff);

            return hediffDefs; 

        }

        /// <summary>
        /// Determines whether this instance can be applied to the specified pawn 
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutagen">The mutagen.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be applied to the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// mutationDef
        /// or
        /// pawn
        /// </exception>
        [Pure]
        public static bool CanApplyMutations([NotNull] this MutationDef mutationDef, [NotNull] Pawn pawn, MutagenDef mutagen= null)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            mutagen = mutagen ?? MutagenDefOf.defaultMutagen;
            if (!mutagen.CanInfect(pawn)) return false;
           
            return mutationDef.IsValidFor(pawn); 
        }


        /// <summary>
        /// Determines whether this instance can be applied to the specified pawn
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="addPart">The add part.</param>
        /// <param name="mutagen">The mutagen.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be applied to the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">mutationDef
        /// or
        /// pawn</exception>
        [Pure]
        public static bool CanApplyMutations([NotNull] this MutationDef mutationDef, [NotNull] Pawn pawn, BodyPartRecord addPart, MutagenDef mutagen = null)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            mutagen = mutagen ?? MutagenDefOf.defaultMutagen;
            if (!mutagen.CanInfect(pawn)) return false;

            var hediffs = (pawn.health?.hediffSet?.hediffs).MakeSafe();
            foreach (Hediff_AddedMutation mutation in hediffs.OfType<Hediff_AddedMutation>())
            {
                if (mutation.Blocks(mutationDef, addPart)) return false; 
            }

            return mutationDef.IsValidFor(pawn);
        }

        /// <summary>
        /// checks if this mutation blocks the addition of the other mutation at the given site 
        /// </summary>
        /// <param name="mutation">The mutation.</param>
        /// <param name="otherMutation">The other mutation.</param>
        /// <param name="addPart">The add part.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// mutation
        /// or
        /// otherMutation
        /// or
        /// addPart
        /// </exception>
        [Pure]
        public static bool BlocksMutation([NotNull] this Hediff_AddedMutation mutation, [NotNull] MutationDef otherMutation,
                                         [NotNull] BodyPartRecord addPart)
        {
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));
            if (addPart == null) throw new ArgumentNullException(nameof(addPart));

            foreach (MutationDef.BlockEntry blockEntry in mutation.Def.blockList.MakeSafe())
            {
                if (blockEntry.Blocks(mutation, otherMutation, addPart)) return true; 
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified pawn has the given mutation.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <returns>
        ///   <c>true</c> if the specified pawn has the given mutation; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasMutation([NotNull] this Pawn pawn, [NotNull] MutationDef mutation)
        {
            foreach (Hediff hediff in (pawn.health?.hediffSet?.hediffs).MakeSafe())
            {
                if (hediff.def == mutation) return true; 
            }

            return false; 
        }

        //TODO change this to enum flag 
        /// <summary>
        ///     simple struct to contain all options for addition actions to be taken when adding a mutation
        /// </summary>
        public readonly struct AncillaryMutationEffects
        {
            

            /// <summary>
            /// Initializes a new instance of the <see cref="AncillaryMutationEffects" /> struct.
            /// </summary>
            /// <param name="addTale">if set to <c>true</c> [add tale].</param>
            /// <param name="addMemory">if set to <c>true</c> [add memory].</param>
            /// <param name="addLogEntry">if set to <c>true</c> [add log entry].</param>
            /// <param name="throwMagicPuff">if set to <c>true</c> [throw magic puff].</param>
            /// <param name="memoryIgnoresLimit">if set to <c>true</c> [memory ignores limit].</param>
            public AncillaryMutationEffects(bool addTale, bool addMemory, bool addLogEntry, bool throwMagicPuff, bool memoryIgnoresLimit=false)
            {
                AddTale = addTale;
                AddMemory = addMemory;
                AddLogEntry = addLogEntry;
                ThrowMagicPuff = throwMagicPuff;
                MemoryIgnoresLimit = memoryIgnoresLimit; 
            }

            

            /// <summary>
            ///     Gets the default value for the ancillary effects
            /// </summary>
            /// <value>
            ///     The default.
            /// </value>
            public static AncillaryMutationEffects Default { get; } = new AncillaryMutationEffects(true, true, true, true,false);

            /// <summary>
            ///     instance representing no effects
            /// </summary>
            /// <value>
            ///     The none.
            /// </value>
            public static AncillaryMutationEffects None { get; } = new AncillaryMutationEffects(false, false, false, false,false);

            /// <summary>
            /// Gets the no smoke instance
            /// </summary>
            /// <value>
            /// The no smoke.
            /// </value>
            public static AncillaryMutationEffects NoSmoke { get; } =
                new AncillaryMutationEffects(true, true, true, false, false);


            /// <summary>
            /// Gets the history only.
            /// </summary>
            /// <value>
            /// The history only.
            /// </value>
            public static AncillaryMutationEffects HistoryOnly { get; } =
                new AncillaryMutationEffects(true, false, true, false, false); 

            /// <summary>
            ///     Gets a value indicating whether the  tale should be added.
            /// </summary>
            /// <value>
            ///     <c>true</c> if the tale should be added; otherwise, <c>false</c>.
            /// </value>
            public bool AddTale { get; }

            /// <summary>
            ///     Gets a value indicating whether the memory should be added.
            /// </summary>
            /// <value>
            ///     <c>true</c> if the memory should be added; otherwise, <c>false</c>.
            /// </value>
            public bool AddMemory { get; }

            /// <summary>
            ///     Gets a value indicating whether the log entry should be added log.
            /// </summary>
            /// <value>
            ///     <c>true</c> if [add log entry]; otherwise, <c>false</c>.
            /// </value>
            public bool AddLogEntry { get; }

            /// <summary>
            ///     Gets a value indicating whether throw magic puff.
            /// </summary>
            /// <value>
            ///     <c>true</c> if magic puffs should be thrown; otherwise, <c>false</c>.
            /// </value>
            public bool ThrowMagicPuff { get; }

            /// <summary>
            /// Gets a value indicating whether the mutation memory should ignore the mod setting's max mutation thought limit 
            /// </summary>
            /// <value>
            ///   <c>true</c> if [memory ignores limit]; otherwise, <c>false</c>.
            /// </value>
            public bool MemoryIgnoresLimit { get; }


            /// <summary>Returns the fully qualified type name of this instance.</summary>
            /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
            public override string ToString()
            {
                return $@"{{
    {nameof(AddTale)}:{AddTale},
    {nameof(AddMemory)}:{AddMemory},
    {nameof(AddLogEntry)}:{AddLogEntry},
    {nameof(AddTale)}:{AddTale},
    {nameof(MemoryIgnoresLimit)}:{MemoryIgnoresLimit}
}}
";
            }
        }
    }
}