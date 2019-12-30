// MorphDef.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:32 PM
// last updated 08/02/2019  2:32 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary> Def class for a morph. Used to generate the morph's implicit race. </summary>
    public class MorphDef : Def
    {
        /// <summary>
        ///     The categories that the morph belongs to. <br />
        ///     For example, a Pigmorph belongs to the Farm and Production morph groups.
        /// </summary>
        public List<MorphCategoryDef> categories = new List<MorphCategoryDef>();

        /// <summary>
        ///     The creature this race is a morph of.<br />
        ///     For example, a Wargmorph's race should be Warg.
        /// </summary>
        public ThingDef race;

        /// <summary> If specified, the race to use in place of the implicit one.</summary>
        public ThingDef explicitHybridRace;

        /// <summary>
        ///     The group the morph belongs to. <br />
        ///     For example, a Huskymorph belongs to the pack, while a Cowmorph is a member of the herd.
        /// </summary>
        public MorphGroupDef group;

        /// <summary> Various settings for the morph's implied race.</summary>
        public HybridRaceSettings raceSettings = new HybridRaceSettings();

        /// <summary> Various settings determining what happens when a pawn is transformed or reverted.</summary>
        public TransformSettings transformSettings = new TransformSettings();

        /// <summary> Aspects that a morph of this race get.</summary>
        public List<AddedAspect> addedAspects = new List<AddedAspect>();

        /// <summary> The morph's implicit race.</summary>
        [Unsaved] public ThingDef hybridRaceDef;


        [Unsaved] private readonly Dictionary<BodyDef, float> _maxInfluenceCached = new Dictionary<BodyDef, float>();

        /// <summary> The percent influence this morph has upon the pawn.</summary>
        private float? _totalInfluence;

        /// <summary> Any mutations directly associated with this morph (the hediff specifies this MorphDef).</summary>
        [Unsaved] private List<HediffGiver_Mutation> _associatedMutations;

        /// <summary> Any mutations indirectly associated with this morph (they share a TF hediff with an associated mutation).</summary>
        private List<HediffGiver_Mutation> _adjacentMutationGivers;

        [Unsaved] private Dictionary<BodyPartDef, List<HediffDef>> _mutationsByParts;

        [Unsaved] private List<HediffDef> _allAssociatedMutations;

        [CanBeNull] [Unsaved] private HashSet<HediffDef> _associatedMutationsLookup;

        /// <summary> Gets an enumerable collection of all the morph type's defs.</summary>
        public static IEnumerable<MorphDef> AllDefs => DefDatabase<MorphDef>.AllDefs;

        /// <summary> Gets the mutations associated with this morph. </summary>
        public IEnumerable<HediffGiver_Mutation> AssociatedMutations =>
            _associatedMutations ?? (_associatedMutations = GetMutations());

        /// <summary>Gets the collection of all mutations associated with this morph def</summary>
        /// <value>All associated mutations.</value>
        [NotNull]
        public IEnumerable<HediffDef> AllAssociatedMutations
        {
            get
            {
                if (_allAssociatedMutations == null)
                {
                    _allAssociatedMutations = new List<HediffDef>();
                    _allAssociatedMutations.AddRange(AllAssociatedAndAdjacentMutationGivers.Select(g => g.hediff).Distinct());
                }

                return _allAssociatedMutations;
            }
        }

        /// <summary>
        ///     Gets an enumerable collection of HediffGiver_Mutations that are either associated with or 'adjacent' to this morph.
        ///     <br />
        ///     An adjacent HediffGiver is one that is found in the same HediffDef as another HediffGiver that gives a part
        ///     associated with this morph.
        /// </summary>
        [NotNull]
        public IEnumerable<HediffGiver_Mutation> AllAssociatedAndAdjacentMutationGivers
        {
            get //TODO remove this and just use AllAssociateMutations 
            {
                if (_adjacentMutationGivers == null) //use lazy initialization 
                {
                    bool Selector(HediffDef def)
                    {
                        if (!typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass)) return false; //only select morph tf hediffs 
                        if (def.CompProps<HediffCompProperties_Single>() != null) return false; //ignore partial tfs 
                        IEnumerable<HediffGiver> givers = def.GetAllHediffGivers();
                        return givers.Any(g => g.hediff.CompProps<CompProperties_MorphInfluence>()?.morph == this);
                        //make sure that the morph has at least one part associated with this morph 
                    }

                    IEnumerable<HediffGiver_Mutation> allGivers = DefDatabase<HediffDef>.AllDefs.Where(Selector)
                                                                                        .SelectMany(h => h.GetAllHediffGivers()
                                                                                                          .OfType<
                                                                                                               HediffGiver_Mutation
                                                                                                           >())
                                                                                        .GroupBy(g => g.hediff,
                                                                                                 g =>
                                                                                                     g) //group all hediff givers that give the same mutation together 
                                                                                        .Select(g =>
                                                                                                    g.First()); //only keep one giver per mutation 
                    _adjacentMutationGivers = new List<HediffGiver_Mutation>(allGivers);
                }

                return _adjacentMutationGivers;
            }
        }

 
        //simply a cached hash Set of all hediffs added by the HediffGivers in AllAssociatedAndAdjacentMutationGivers 
        [NotNull]
        private HashSet<HediffDef> AssociatedMutationsLookup
        {
            get
            {
                if (_associatedMutationsLookup == null)
                    _associatedMutationsLookup =
                        new HashSet<HediffDef>(AllAssociatedAndAdjacentMutationGivers
                                              .Select(g => g.hediff)
                                              .Where(h => h != null));

                return _associatedMutationsLookup;
            }
        }


        /// <summary>
        ///     get all configuration errors with this instance
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors()) yield return configError;

            if (race == null)
                yield return "No race def found!";
            else if (race.race == null) yield return $"Race {race.defName} has no race properties! Are you sure this is a race?";
        }

        /// <summary>
        ///     Gets the maximum possible influence this morph has on a given body
        /// </summary>
        /// <param name="bodyDef">The body definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">bodyDef</exception>
        public float GetMaximumInfluence([NotNull] BodyDef bodyDef)
        {
            if (bodyDef == null) throw new ArgumentNullException(nameof(bodyDef));

            if (_maxInfluenceCached.TryGetValue(bodyDef, out float accum)) return accum; //see if we calculated this before 

            foreach (HediffGiver_Mutation giver in AllAssociatedAndAdjacentMutationGivers)
            {
                int mutationCount =
                    bodyDef.GetAllMutableParts()
                           .Count(m => giver.partsToAffect?
                                          .Contains(m.def)
                                    ?? false); //get the total number of unique mutations the body can have at once 
                float influence = giver.hediff.GetInfluenceOf(this); //get the influence this mutation gives 
                accum += influence * mutationCount;
            }

            _maxInfluenceCached[bodyDef] = accum; //cache the result so we only have to do this once per body def
            return accum;
        }

        /// <summary>Gets the mutation that affect the given part from this morph def</summary>
        /// <param name="partDef">The part definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">partDef</exception>
        [NotNull]
        public IEnumerable<HediffDef> GetMutationForPart([NotNull] BodyPartDef partDef)
        {
            if (partDef == null) throw new ArgumentNullException(nameof(partDef));
            if (_mutationsByParts == null)
            {
                _mutationsByParts = new Dictionary<BodyPartDef, List<HediffDef>>();
                foreach (HediffDef mutation in AllAssociatedMutations) //build the lookup dict here 
                foreach (BodyPartDef part in mutation.GetPossibleParts()) //gets a list of all parts this mutation affects 
                {
                    List<HediffDef> lst;
                    if (_mutationsByParts.TryGetValue(part, out lst))
                    {
                        lst.Add(mutation);
                    }
                    else
                    {
                        lst = new List<HediffDef> {mutation};
                        _mutationsByParts[part] = lst;
                    }
                }
            }

            return _mutationsByParts.TryGetValue(partDef) ?? Enumerable.Empty<HediffDef>();
        }


        /// <summary>
        ///     obsolete, does nothing
        /// </summary>
        /// <param name="food"></param>
        /// <returns></returns>
        [Obsolete("This is no longer used")]
        public FoodPreferability? GetOverride(ThingDef food) //note, RawTasty is 5, RawBad is 4 
        {
            if (food?.ingestible == null) return null;
            foreach (HybridRaceSettings.FoodCategoryOverride foodOverride in raceSettings.foodSettings.foodOverrides)
                if ((food.ingestible.foodType & foodOverride.foodFlags) != 0)
                    return foodOverride.preferability;

            return null;
        }

        /// <summary>
        ///     Determines whether the specified hediff definition is an associated mutation .
        /// </summary>
        /// <param name="hediffDef">The hediff definition.</param>
        /// <returns>
        ///     <c>true</c> if the specified hediff definition is an associated mutation; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">hediffDef</exception>
        public bool IsAnAssociatedMutation([NotNull] HediffDef hediffDef)
        {
            if (hediffDef == null) throw new ArgumentNullException(nameof(hediffDef));

            return AssociatedMutationsLookup.Contains(hediffDef);
        }

        /// <summary>
        ///     Determines whether the given hediff is an associated mutation.
        /// </summary>
        /// <param name="hediff">The hediff.</param>
        /// <returns>
        ///     <c>true</c> if the specified hediff is an associated mutation ; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">hediff</exception>
        public bool IsAnAssociatedMutation([NotNull] Hediff hediff)
        {
            if (hediff?.def == null) throw new ArgumentNullException(nameof(hediff));
            return IsAnAssociatedMutation(hediff.def);
        }

        /// <summary>
        ///     resolves all references after DefOfs are loaded
        /// </summary>
        public override void ResolveReferences()
        {
            if (explicitHybridRace != null)
            {
                hybridRaceDef = explicitHybridRace;
                Log.Warning($"MorphDef {defName} is using an explicit hybrid {explicitHybridRace.defName} for {race.defName}. This has not been tested yet");
            }

            //TODO patch explicit race based on hybrid race settings? 
        }

        private List<HediffGiver_Mutation> GetMutations()
        {
            IEnumerable<HediffGiver_Mutation> linq =
                DefDatabase<HediffDef>.AllDefs
                                      .Where(def => typeof(Hediff_Morph)
                                                .IsAssignableFrom(def.hediffClass)) // Get all morph hediff defs.
                                      .SelectMany(def => def.stages ?? Enumerable.Empty<HediffStage>()) // Get all stages.
                                      .SelectMany(s => s.hediffGivers
                                                    ?? Enumerable.Empty<HediffGiver>()) // Get all hediff givers.
                                      .OfType<HediffGiver_Mutation>() // Keep only the mutation givers.
                                      .Where(mut => mut.hediff.CompProps<CompProperties_MorphInfluence>()?.morph
                                                 == this); // Keep only those associated with this morph.
            return linq.ToList();
        }

        /// <summary> Settings to control what happens when a pawn changes race to this morph type.</summary>
        public class TransformSettings
        {
            /// <summary> The TaleDef that should be used in art that occurs whenever a pawn shifts to this morph.</summary>
            [CanBeNull] public TaleDef transformTale;

            /// <summary> The ID of the message that should be spawned when a pawn shifts to this morph.</summary>
            [CanBeNull] public string transformationMessageID;

            /// <summary> The message type that should be used when a pawn shifts to this morph.</summary>
            [CanBeNull] public MessageTypeDef messageDef;

            /// <summary> Memory added when a pawn shifts to this morph.</summary>
            [CanBeNull] public ThoughtDef transformationMemory;

            /// <summary> Memory added when the pawn reverts from this morph back to human if they have the furry trait.</summary>
            [CanBeNull] public ThoughtDef revertedMemoryFurry;

            /// <summary> Memory added when the pawn reverts from this morph back to human if they have the body purist trait.</summary>
            [CanBeNull] public ThoughtDef revertedMemoryBP;

            /// <summary>
            ///     Memory added when the pawn reverts from this morph back to human if they have neither the body purist or
            ///     furry traits.
            /// </summary>
            [CanBeNull] public ThoughtDef revertedMemory;

            /// <summary> Gets the memory for when a pawn is reverted based on their outlook.</summary>
            /// <param name="outlook"> The mutation outlook of the pawn (i.e. Whether they are a body purist, a furry, or nothing).</param>
            /// <returns> The ThoughtDef of the memory associated with their outlook.</returns>
            public ThoughtDef GetReversionMemory(MutationOutlook outlook)
            {
                switch (outlook)
                {
                    case MutationOutlook.Furry:
                        return revertedMemoryFurry;
                    case MutationOutlook.BodyPurist:
                        return revertedMemoryBP;
                    case MutationOutlook.Neutral:
                        return revertedMemory;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(outlook), outlook, null);
                }
            }
        }

        /// <summary> Aspects to add when a pawn changes race to this morph type and settings asociated with them.</summary>
        public class AddedAspect
        {
            /// <summary> The Def of the aspect to add.</summary>
            public AspectDef def;

            /// <summary> Whether or not the aspect should be kept even if the pawn switches race.</summary>
            public bool keepOnReversion;
        }
    }
}