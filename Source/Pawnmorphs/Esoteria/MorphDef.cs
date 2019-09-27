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
    /// <summary>
    ///     def class for a 'morph'
    /// </summary>
    public class MorphDef : Def
    {
        public List<MorphCategoryDef> categories = new List<MorphCategoryDef>();

        /// <summary>
        ///     the race of the animal this morph is to
        ///     if this is a warg morph then race should be Warg
        /// </summary>
        public ThingDef race; //the animal race of the morph 

        public ThingDef explicitHybridRace;

        public MorphGroupDef group; //the group the morph belongs to, if any 

        public HybridRaceSettings raceSettings = new HybridRaceSettings();

        public TransformSettings transformSettings = new TransformSettings();

        public List<AddedAspect> addedAspects = new List<AddedAspect>();  

        [Unsaved] public ThingDef hybridRaceDef;


        private float? _totalInfluence;


        [Unsaved] private List<HediffGiver_Mutation> _associatedMutations;

        public static IEnumerable<MorphDef> AllDefs => DefDatabase<MorphDef>.AllDefs;

        public float TotalInfluence
        {
            get
            {
                if (_totalInfluence == null)
                {
                    IEnumerable<HediffGiver_Mutation> givers = DefDatabase<HediffDef>.AllDefs
                                                                                     .Where(def => typeof(Hediff_Morph)
                                                                                               .IsAssignableFrom(def.hediffClass))
                                                                                     .SelectMany(def => def.GetAllHediffGivers()
                                                                                                           .OfType<
                                                                                                                HediffGiver_Mutation
                                                                                                            >()) //select the givers not the hediffs directly to get where they're assigned to 
                                                                                     .Where(g =>
                                                                                                g.hediff
                                                                                                 .CompProps<
                                                                                                      CompProperties_MorphInfluence
                                                                                                  >()
                                                                                                ?.morph
                                                                                             == this)
                                                                                     .GroupBy(g => g.hediff,
                                                                                              g => g) //get only distinct values 
                                                                                     .Select(g =>
                                                                                                 g.First()); //not get one of each mutation 


                    var counter = 0.0f;
                    foreach (HediffGiver_Mutation hediffGiverMutation in givers)
                    {
                        float inf = hediffGiverMutation.hediff.CompProps<CompProperties_MorphInfluence>().influence;
                        counter += inf * hediffGiverMutation.countToAffect;
                    }


                    _totalInfluence = counter;
                }


                return _totalInfluence.Value;
            }
        }


        private List<HediffGiver_Mutation> _adjacentMutations;


        /// <summary>
        /// get an enumerable collection of HediffGiver_Mutations that are either associated or 'adjacent' to this morph 
        /// </summary>
        /// an 'adjacent' HediffGiver is one that is found in the same HediffDef as another HediffGiver that gives a part associated with this morph 
        /// 
        public IEnumerable<HediffGiver_Mutation> AllAssociatedAndAdjacentMutations
        {
            get
            {
                if (_adjacentMutations == null) //use lazy initialization 
                {
                    bool Selector(HediffDef def)
                    {
                        if (!typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass)) return false; //only select morph tf hediffs 
                        if (def.CompProps<HediffCompProperties_Single>() != null) return false; //ignore partial tfs 
                        var givers = def.GetAllHediffGivers();
                        return givers.Any(g => g.hediff.CompProps<CompProperties_MorphInfluence>()?.morph == this); 
                            //make sure that the morph has at least one part associated with this morph 
                    }

                    var allGivers = DefDatabase<HediffDef>.AllDefs.Where(Selector)
                        .SelectMany(h => h.GetAllHediffGivers().OfType<HediffGiver_Mutation>())
                        .GroupBy(g => g.hediff, g => g) //group all hediff givers that give the same mutation together 
                        .Select(g => g.First()); //only keep one giver per mutation 
                    _adjacentMutations = new List<HediffGiver_Mutation>(allGivers); 

                }

                return _adjacentMutations; 
            }
        }


        /// <summary>
        ///     Gets the mutations associated with this morph.
        /// </summary>
        /// <value>
        ///     The associated mutations.
        /// </value>
        public IEnumerable<HediffGiver_Mutation> AssociatedMutations =>
            _associatedMutations ?? (_associatedMutations = GetMutations());

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors()) yield return configError;

            if (race == null)
                yield return "no race def found!";
            else if (race.race == null) yield return $"race {race.defName} has no race properties! are you sure this is a race?";
        }


        [Obsolete("This is no longer used")]
        public FoodPreferability? GetOverride(ThingDef food) //note, RawTasty is 5, RawBad is 4 
        {
            if (food?.ingestible == null) return null;
            foreach (HybridRaceSettings.FoodCategoryOverride foodOverride in raceSettings.foodSettings.foodOverrides)
                if ((food.ingestible.foodType & foodOverride.foodFlags) != 0)
                    return foodOverride.preferability;

            return null;
        }

        public override void ResolveReferences()
        {
            if (explicitHybridRace != null) hybridRaceDef = explicitHybridRace;

            //TODO patch explicit race based on hybrid race settings? 
        }

        private List<HediffGiver_Mutation> GetMutations()
        {
            IEnumerable<HediffGiver_Mutation> linq = DefDatabase<HediffDef>
                                                    .AllDefs
                                                    .Where(def => typeof(Hediff_Morph)
                                                              .IsAssignableFrom(def.hediffClass)) //get all morph hediff defs 
                                                    .SelectMany(def => def.stages
                                                                    ?? Enumerable.Empty<HediffStage>()) //get all stages 
                                                    .SelectMany(s => s.hediffGivers
                                                                  ?? Enumerable.Empty<HediffGiver>()) //get all hediff givers 
                                                    .OfType<HediffGiver_Mutation>() //keep only the mutation givers 
                                                    .Where(mut => mut.hediff.CompProps<CompProperties_MorphInfluence>()?.morph
                                                               == this); //keep only those associated with this morph 
            return linq.ToList();
        }

        /// <summary>
        ///     setting to control how this morph transforms
        /// </summary>
        public class TransformSettings
        {
            [CanBeNull] public TaleDef transformTale;
            [CanBeNull] public string transformationMessageID;
            [CanBeNull] public MessageTypeDef messageDef;
            [CanBeNull] public ThoughtDef transformationMemory; //memory added when a pawn shifts to this morph 
        }

        public class AddedAspect
        {
            public AspectDef def;
            public bool keepOnReversion; //if the affinity should be kept even if the pawn switches race 
        }
    }
}