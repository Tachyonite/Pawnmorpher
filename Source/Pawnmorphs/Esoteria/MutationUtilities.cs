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
        private const float EPSILON = 0.01f;

        [NotNull] private static Dictionary<BodyPartDef, List<HediffDef>> _mutationsByParts;

        private static List<HediffGiver_Mutation> _allGivers;

        private static List<BodyPartDef> _allMutablePartDefs;


        private static readonly Dictionary<HediffDef, List<VTuple<MorphDef, float>>> _influenceLookupTable =
            new Dictionary<HediffDef, List<VTuple<MorphDef, float>>>();


        private static readonly Dictionary<BodyDef, List<BodyPartRecord>>
            _allMutablePartsLookup = new Dictionary<BodyDef, List<BodyPartRecord>>();

        [NotNull] private static Dictionary<HediffDef, List<BodyPartDef>> _partLookupDict;

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
        ///     returns an enumerable collection of all hediffGiver_Mutations active
        ///     note, this does <i>not</i> check for givers that give the same hediff
        /// </summary>
        public static IEnumerable<HediffGiver_Mutation> AllGivers
        {
            get
            {
                if (_allGivers == null)
                    _allGivers = AllMorphHediffs.SelectMany(def => def.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
                                                .ToList();

                return _allGivers;
            }
        }


        /// <summary>
        ///     an enumerable collection of all mutations
        /// </summary>
        [NotNull]
        public static IEnumerable<HediffDef> AllMutations =>
            DefDatabase<HediffDef>.AllDefs.Where(d => typeof(Hediff_AddedMutation).IsAssignableFrom(d.hediffClass));

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
                    _allThoughts = DefDatabase<HediffDef>.AllDefs.SelectMany(d => d.GetAllHediffGivers()
                                                                                   .OfType<HediffGiver_Mutation>())
                                                         .Select(g => g.memory)
                                                         .Where(t => t != null)
                                                         .Distinct()
                                                         .ToList();

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

        /// <summary>Adds the mutation to the pawn without a hediff giver.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="parts">The part.</param>
        /// <param name="addParts">if not null, this will be filled with all part that mutations were added to</param>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     part
        /// </exception>
        /// <returns>if any mutations were added</returns>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] HediffDef mutation,
                                       [NotNull] IEnumerable<BodyPartRecord> parts, List<BodyPartRecord> addParts = null)
        {
            if (pawn?.health?.hediffSet == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (parts == null) throw new ArgumentNullException(nameof(parts));

            Pawn_HealthTracker health = pawn.health;
            var addedRecords = new List<BodyPartRecord>();
            foreach (BodyPartRecord bodyPartRecord in parts)
            {
                if (health.hediffSet.PartIsMissing(bodyPartRecord))
                    //make sure none of the part are missing 
                    continue;
                if (health.hediffSet.HasHediff(mutation, bodyPartRecord)) continue;

                Hediff hediff = HediffMaker.MakeHediff(mutation, pawn, bodyPartRecord);
                health.AddHediff(hediff, bodyPartRecord);
                addedRecords.Add(bodyPartRecord);
                addParts?.Add(bodyPartRecord);
                var mutationDef = mutation as MutationDef;
                if (mutationDef == null)
                {
                    Log.Warning($"{mutation.defName} does not use {nameof(MutationDef)}");
                    continue;
                }

                if (mutationDef.mutationMemory != null) pawn.TryGainMemory(mutationDef.mutationMemory);

                if (PawnUtility.ShouldSendNotificationAbout(pawn) && mutationDef.mutationTale != null)
                    TaleRecorder.RecordTale(mutationDef.mutationTale, pawn);
            }

            if (addedRecords.Count > 0) //only do this if we actually added any mutations 
            {
                var logEntry = new MutationLogEntry(pawn, mutation, addedRecords.Select(p => p.def).Distinct());
                Find.PlayLog?.Add(logEntry);
                if (pawn.MapHeld != null) IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
            }

            return addedRecords.Count > 0;
        }

        /// <summary>Adds the mutation to the pawn without a hediff giver.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="part">The part.</param>
        /// <param name="addParts">if not null, this will be filled with all part that mutations were added to</param>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     part
        /// </exception>
        /// <returns>if any mutations were added</returns>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] HediffDef mutation,
                                       [NotNull] BodyPartRecord part, List<BodyPartRecord> addParts = null)
        {
            if (pawn?.health?.hediffSet == null) throw new ArgumentNullException(nameof(pawn));
            if (mutation == null) throw new ArgumentNullException(nameof(mutation));
            if (part == null) throw new ArgumentNullException(nameof(part));
            if (pawn.health.hediffSet.HasHediff(mutation, part)) return false;
            Pawn_HealthTracker health = pawn.health;
            var addedRecords = new List<BodyPartRecord>();
            if (health.hediffSet.PartIsMissing(part))
                //make sure none of the part are missing 
                return false;

            Hediff hediff = HediffMaker.MakeHediff(mutation, pawn, part);
            health.AddHediff(hediff, part);
            addedRecords.Add(part);
            addParts?.Add(part);

            var mDef = mutation as MutationDef;

            var logEntry = new MutationLogEntry(pawn, mutation, addedRecords.Select(p => p.def).Distinct());
            Find.PlayLog?.Add(logEntry);
            if (pawn.MapHeld != null) IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);


            if (mDef == null)
            {
                Log.Warning($"{mutation.defName} is does not use {nameof(MutationDef)} as a def!");
                return true;
            }

            if (mDef.mutationMemory != null) pawn.TryGainMemory(mDef.mutationMemory);

            if (PawnUtility.ShouldSendNotificationAbout(pawn) && mDef.mutationTale != null)
                TaleRecorder.RecordTale(mDef.mutationTale, pawn);


            return addedRecords.Count > 0;
        }

        /// <summary>Adds the mutation to the pawn without a hediff giver.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutation">The mutation.</param>
        /// <param name="parts">The part.</param>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     mutation
        ///     or
        ///     part
        /// </exception>
        /// <returns>if any mutations were added</returns>
        public static bool AddMutation([NotNull] Pawn pawn, [NotNull] HediffDef mutation, [NotNull] params BodyPartRecord[] parts)
        {
            return AddMutation(pawn, mutation, (IEnumerable<BodyPartRecord>) parts);
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
        /// checks if this mutation overlaps with the given mutation 
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="otherMutation">The other mutation.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// mutationDef
        /// or
        /// otherMutation
        /// </exception>
        [Pure]
        public static bool OverlapsWith([NotNull] this MutationDef mutationDef, [NotNull] MutationDef otherMutation)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (otherMutation == null) throw new ArgumentNullException(nameof(otherMutation));

            //make sure this mutation def has all the same parts as other mutation 

            foreach (BodyPartDef bodyPartDef in mutationDef.parts)
            {
                if (otherMutation.parts.Contains(bodyPartDef) == false) return false; //if mutation Def has any part that other mutation does not they do not overlap 
            }

            var thRmComp = mutationDef.RemoveComp;
            var othRmComp = otherMutation.RemoveComp; 

            //make sure the layers overlap 
            return thRmComp.layer != othRmComp.layer; 


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
        ///     Gets all non zero morph influences the given hediff def gives to a pawn
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">def</exception>
        public static IEnumerable<VTuple<MorphDef, float>> GetAllNonZeroInfluences([NotNull] HediffDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            if (_influenceLookupTable.TryGetValue(def, out List<VTuple<MorphDef, float>> lst)
            ) //check if we calculated the value already 
                return lst;

            lst = new List<VTuple<MorphDef, float>>();
            _influenceLookupTable[def] = lst; //make sure the list is saved so we don't have to calculate this more then once 

            foreach (MorphDef morphDef in DefDatabase<MorphDef>.AllDefs)
            {
                float influence = GetInfluenceOf(def, morphDef);
                if (influence > EPSILON)
                    lst.Add(new VTuple<MorphDef, float>(morphDef, influence));
            }

            return lst;
        }


        /// <summary>
        ///     Gets the influence of the given morph this mutationDef provides.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <param name="morph">The morph.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     mutationDef
        ///     or
        ///     morph
        /// </exception>
        [Obsolete]
        public static float GetInfluenceOf([NotNull] this HediffDef mutationDef, [NotNull] MorphDef morph)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (morph == null) throw new ArgumentNullException(nameof(morph));

            if (!typeof(Hediff_AddedMutation).IsAssignableFrom(mutationDef.hediffClass))
                return 0; //if not a mutation just return 0 

            if (mutationDef.HasComp(typeof(SpreadingMutationComp)))
                return 0; //spreading mutations shouldn't give influence, too messy to deal with 

            var comp = mutationDef.CompProps<CompProperties_MorphInfluence>();
            if (comp != null && comp.morph == morph)
                return comp.influence; //if it has a morph influence comp return the influence value

            if (morph.AllAssociatedAndAdjacentMutationGivers.Select(g => g.hediff).Contains(mutationDef))
                return 0.0f; //might want to let these guys give influence 
            return 0;
        }


        /// <summary>Gets the mutations by part def.</summary>
        /// <param name="bodyPartDef">The body part definition.</param>
        /// <returns></returns>
        public static IEnumerable<HediffDef> GetMutationsByPart([NotNull] BodyPartDef bodyPartDef)
        {
            return _mutationsByParts.TryGetValue(bodyPartDef) ?? Enumerable.Empty<HediffDef>();
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
        ///     get the body part this hediff can be assigned to
        /// </summary>
        /// <param name="def"></param>
        /// <returns>
        ///     an enumerable collection of all part this hediff can be assigned to, Note the elements can contain duplicates
        ///     and null
        /// </returns>
        [NotNull]
        public static IEnumerable<BodyPartDef> GetPossibleParts([NotNull] this HediffDef def)
        {
            if (def == null) throw new ArgumentNullException(nameof(def));
            List<BodyPartDef> lst;
            if (_partLookupDict.TryGetValue(def, out lst)) return lst;
            return Enumerable.Empty<BodyPartDef>();
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

        private static void BuildLookupDicts()
        {
            //build the lookup table of part sorted by the mutations that affect them 
            _mutationsByParts = new Dictionary<BodyPartDef, List<HediffDef>>();
            _partLookupDict = new Dictionary<HediffDef, List<BodyPartDef>>();
            foreach (HediffDef mutation in AllMutations)
            {
                IEnumerable<BodyPartDef> allParts = GetAffectedParts(mutation).Distinct();
                _partLookupDict[mutation] = allParts.ToList();
            }
            //now build the reverse lookup table 

            foreach (KeyValuePair<HediffDef, List<BodyPartDef>> kvp in _partLookupDict)
            foreach (BodyPartDef bodyPartDef in kvp.Value)
            {
                List<HediffDef> mutations;
                if (!_mutationsByParts.TryGetValue(bodyPartDef, out mutations))
                {
                    mutations = new List<HediffDef>
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

        /// <summary>internal function for getting the part a mutation affects </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <returns></returns>
        [NotNull]
        private static IEnumerable<BodyPartDef> GetAffectedParts([NotNull] HediffDef mutationDef)
        {
            List<BodyPartDef> extParts;
            if (mutationDef is MutationDef mDef)
                extParts = mDef.parts;
            else
                extParts = null;


            foreach (BodyPartDef bodyPartDef in extParts.MakeSafe()) yield return bodyPartDef;

            //for backwards compatibility 
            foreach (HediffDef hDef in DefDatabase<HediffDef>.AllDefs)
            {
                if (hDef == mutationDef) continue;
                IEnumerable<HediffGiver> allGivers = hDef.GetAllHediffGivers().Where(g => g.hediff == mutationDef);
                foreach (HediffGiver hediffGiver in allGivers)
                foreach (BodyPartDef bodyPartDef in hediffGiver.partsToAffect.MakeSafe())
                    yield return bodyPartDef;
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
    }
}