// MutationUtilities.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 2:19 PM
// last updated 08/26/2019  2:19 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
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

        private static List<HediffGiver_Mutation> _allGivers;

        private static List<BodyPartDef> _allMutablePartDefs;

        private static readonly Dictionary<BodyDef, List<BodyPartRecord>> _allMutablePartsLookup =
            new Dictionary<BodyDef, List<BodyPartRecord>>();

        [NotNull] private static Dictionary<MutationDef, List<BodyPartDef>> _partLookupDict;

        private static List<ThoughtDef> _allThoughts;


        private static List<HediffDef> _allMutationsWithGraphics;

        static MutationUtilities()
        {
            StatDef stat = PMStatDefOf.MutationAdaptability;
            MinMutationAdaptabilityValue = stat.minValue;
            MaxMutationAdaptabilityValue = stat.maxValue;
            AverageMutationAdaptabilityValue = stat.defaultBaseValue;

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

            //warnings for use of the old def extension 

            foreach (HediffDef hediffDef in DefDatabase<HediffDef>.AllDefs)
            {
#pragma warning disable 618
                if (hediffDef.HasModExtension<MutationHediffExtension>())
                {
                    warningBuilder.AppendLine($"{hediffDef.defName} is still using the old MutationHediffExtension!");
                    anyWarnings = true;
                }

                if (hediffDef.HasComp(typeof(Comp_MorphInfluence)))
                {
                    warningBuilder.AppendLine($"{hediffDef.defName} is still using the old {nameof(Comp_MorphInfluence)}!");
                    anyWarnings = true;
                }
#pragma warning restore 618
            }

            if (anyWarnings) Log.Warning(warningBuilder.ToString());
            BuildLookupDicts();
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
        ///     an enumerable collection of all morph hediffs
        /// </summary>
        public static IEnumerable<HediffDef> AllMorphHediffs =>
            DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_Morph).IsAssignableFrom(d.hediffClass));

        /// <summary>
        ///     an enumerable collection of all mutation related thoughts
        /// </summary>
        public static IEnumerable<ThoughtDef> AllMutationMemories
        {
            get
            {
                if (_allThoughts == null)
                { 
                    _allThoughts = MutationDef.AllMutations.Select(m => m.mutationMemory).ToList();
                    //add in any other memories added by mutation givers 
                    foreach (HediffGiver_Mutation hediffGiverMutation in AllMorphHediffs.SelectMany(m => m.GetAllHediffGivers().OfType<HediffGiver_Mutation>()))
                    {
                        if (!_allThoughts.Contains(hediffGiverMutation.memory))
                        {
                            _allThoughts.Add(hediffGiverMutation.memory); 
                        }
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


        private static List<BodyPartDef> AllMutablePartDefs //use lazy initialization 
        {
            get
            {
                if (_allMutablePartDefs == null) _allMutablePartDefs = _mutationsByParts.Keys.ToList();

                return _allMutablePartDefs;
            }
        }

        /// <summary>
        ///     Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="countToAdd">The count to add.</param>
        /// <param name="partsAdded">The parts added.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        /// </exception>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation, int countToAdd = int.MaxValue,
                                       List<BodyPartRecord> partsAdded = null, AncillaryMutationEffects? ancillaryEffects = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            return AddMutation(pawn, mutation, mutation.parts, countToAdd, partsAdded, ancillaryEffects);
        }

        /// <summary>
        ///     Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="parts">The parts.</param>
        /// <param name="countToAdd">The count to add.</param>
        /// <param name="addedParts">The added parts.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     parts
        /// </exception>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation, [NotNull] List<BodyPartDef> parts,
                                       int countToAdd = int.MaxValue, List<BodyPartRecord> addedParts = null,
                                       AncillaryMutationEffects? ancillaryEffects = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            var addLst = new List<BodyPartRecord>();

            foreach (BodyPartRecord notMissingPart in pawn.health.hediffSet.GetNotMissingParts())
                if (parts.Contains(notMissingPart.def))
                {
                    addLst.Add(notMissingPart);
                    if (parts.Count >= countToAdd) break;
                }

            if (addLst.Count == 0) return false;
            AddMutation(pawn, mutation, addLst, addedParts, ancillaryEffects);
            return true;
        }

        /// <summary>
        ///     Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="records">The records to add mutations to</param>
        /// <param name="addedParts">The added parts.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     records
        /// </exception>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
                                       [NotNull] IEnumerable<BodyPartRecord> records, List<BodyPartRecord> addedParts = null,
                                       AncillaryMutationEffects? ancillaryEffects = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (records == null) throw new ArgumentNullException(nameof(records));
            HediffSet hSet = pawn.health?.hediffSet;
            if (hSet == null) return false;
            addedParts = addedParts ?? new List<BodyPartRecord>();
            foreach (BodyPartRecord bodyPartRecord in records)
            {
                if (bodyPartRecord.IsMissingAtAllIn(pawn)) continue;
                Hediff hediff = HediffMaker.MakeHediff(mutation, pawn, bodyPartRecord);
                hSet.AddDirect(hediff);
                addedParts.Add(bodyPartRecord);
            }

            AncillaryMutationEffects aEffects = ancillaryEffects ?? AncillaryMutationEffects.Default;
            if (addedParts.Count > 0) //only do this if we actually added any mutations 
            {
                Log.Message(aEffects.ToString());
                if (mutation.mutationMemory != null && aEffects.AddMemory) pawn.TryGainMemory(mutation.mutationMemory);

                if (PawnUtility.ShouldSendNotificationAbout(pawn) && mutation.mutationTale != null && aEffects.AddTale)
                    TaleRecorder.RecordTale(mutation.mutationTale, pawn);

                if (aEffects.AddLogEntry)
                {
                    var logEntry = new MutationLogEntry(pawn, mutation, addedParts.Select(p => p.def).Distinct());
                    Find.PlayLog?.Add(logEntry);
                }

                if (pawn.MapHeld != null && aEffects.ThrowMagicPuff)
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
            }

            return addedParts.Count > 0;
        }

        /// <summary>
        ///     Adds the mutation to the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="record">The records to add mutations to</param>
        /// <param name="addedRecords">The added records.</param>
        /// <param name="ancillaryEffects">The ancillary effects.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     records
        /// </exception>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] MutationDef mutation,
                                       [NotNull] BodyPartRecord record, List<BodyPartRecord> addedRecords = null,
                                       AncillaryMutationEffects? ancillaryEffects = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (record == null) throw new ArgumentNullException(nameof(record));
            HediffSet hSet = pawn.health?.hediffSet;
            if (hSet == null) return false;

            if (record.IsMissingAtAllIn(pawn)) return false;


            Hediff hediff = HediffMaker.MakeHediff(mutation, pawn, record);
            hSet.AddDirect(hediff);
            addedRecords?.Add(record);

            AncillaryMutationEffects aEffects = ancillaryEffects ?? AncillaryMutationEffects.Default;

            if (mutation.mutationMemory != null && aEffects.AddMemory) pawn.TryGainMemory(mutation.mutationMemory);

            if (PawnUtility.ShouldSendNotificationAbout(pawn) && mutation.mutationTale != null && aEffects.AddTale)
                TaleRecorder.RecordTale(mutation.mutationTale, pawn);

            if (aEffects.AddLogEntry)
            {
                var logEntry = new MutationLogEntry(pawn, mutation, record.def);
                Find.PlayLog?.Add(logEntry);
            }

            if (pawn.MapHeld != null && aEffects.ThrowMagicPuff)
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
            return true;
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
                if (mutation.def != mutationDef && mutationDef.parts.Contains(mutation.Part?.def))
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
        public static IEnumerable<VTuple<BodyPartDef, MutationLayer>> GetAllDefMutationSites(
            [NotNull] this MutationDef mutationDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            foreach (BodyPartDef mutationDefPart in mutationDef.parts)
                yield return new VTuple<BodyPartDef, MutationLayer>(mutationDefPart, mutationDef.RemoveComp.layer);
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
        public static IEnumerable<VTuple<BodyPartRecord, MutationLayer>> GetAllMutationSites(
            [NotNull] this MutationDef mutationDef, BodyDef bDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            foreach (BodyPartRecord bodyPartRecord in bDef.AllParts)
                if (mutationDef.parts.Contains(bodyPartRecord.def))
                    yield return new VTuple<BodyPartRecord, MutationLayer>(bodyPartRecord, mutationDef.RemoveComp.layer);
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
            if (comp == null && warnOnFail) Log.Warning($"pawn {pawn.Name} does not have a mutation tracker comp");
            return comp;
        }


        /// <summary>
        ///     get the pawn's outlook toward being mutated
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public static MutationOutlook GetOutlook([NotNull] this Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

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
                if (hediffSet.PartIsMissing(record)) return true;
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

            foreach (BodyPartDef bodyPartDef in mutationDef.parts)
                if (otherMutation.parts.Contains(bodyPartDef) == false)
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
                IEnumerable<BodyPartDef> allParts = mutation.parts;
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
            IEnumerable<string> hediffDefNames =
                bodyAddons.SelectMany(add => add.hediffGraphics ?? Enumerable.Empty<AlienPartGenerator.BodyAddonHediffGraphic>())
                          .Select(h => h.hediff);


            foreach (string hediffDef in hediffDefNames) yield return HediffDef.Named(hediffDef);
        }

        /// <summary>
        ///     simple struct to contain all options for addition actions to be taken when adding a mutation
        /// </summary>
        public readonly struct AncillaryMutationEffects
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="AncillaryMutationEffects" /> struct.
            /// </summary>
            /// <param name="addTale">if set to <c>true</c> [add tale].</param>
            /// <param name="addMemory">if set to <c>true</c> [add memory].</param>
            /// <param name="addLogEntry">if set to <c>true</c> [add log entry].</param>
            /// <param name="throwMagicPuff">if set to <c>true</c> [throw magic puff].</param>
            public AncillaryMutationEffects(bool addTale, bool addMemory, bool addLogEntry, bool throwMagicPuff)
            {
                AddTale = addTale;
                AddMemory = addMemory;
                AddLogEntry = addLogEntry;
                ThrowMagicPuff = throwMagicPuff;
            }

            /// <summary>
            ///     Initializes a new instance of the <see cref="AncillaryMutationEffects" /> struct.
            /// </summary>
            /// <param name="addTale">if set to <c>true</c> [add tale].</param>
            /// <param name="addMemory">if set to <c>true</c> [add memory].</param>
            /// <param name="addLogEntry">if set to <c>true</c> [add log entry].</param>
            public AncillaryMutationEffects(bool addTale, bool addMemory, bool addLogEntry)
            {
                AddTale = addTale;
                AddMemory = addMemory;
                AddLogEntry = addLogEntry;
                ThrowMagicPuff = true;
            }

            /// <summary>
            ///     Gets the default value for the ancillary effects
            /// </summary>
            /// <value>
            ///     The default.
            /// </value>
            public static AncillaryMutationEffects Default { get; } = new AncillaryMutationEffects(true, true, true, true);

            /// <summary>
            ///     instance representing no effects
            /// </summary>
            /// <value>
            ///     The none.
            /// </value>
            public static AncillaryMutationEffects None { get; } = new AncillaryMutationEffects(false, false, false, false);

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

            /// <summary>Returns the fully qualified type name of this instance.</summary>
            /// <returns>A <see cref="T:System.String" /> containing a fully qualified type name.</returns>
            public override string ToString()
            {
                return $@"
    {nameof(AddTale)}:{AddTale}
    {nameof(AddMemory)}:{AddMemory}
    {nameof(AddLogEntry)}:{AddLogEntry}
    {nameof(AddTale)}:{AddTale}
";
            }
        }
    }
}