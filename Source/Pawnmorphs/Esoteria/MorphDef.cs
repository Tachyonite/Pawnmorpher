// MorphDef.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:32 PM
// last updated 08/02/2019  2:32 PM

using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def class for a 'morph' 
    /// </summary>
    public class MorphDef : Def
    {
        public static IEnumerable<MorphDef> AllDefs => DefDatabase<MorphDef>.AllDefs; 

        /// <summary>
        /// setting to control how this morph transforms 
        /// </summary>
        public class TransformSettings
        {
            public TaleDef transformTale;
            public string transformationMessageID;
            public MessageTypeDef messageDef; 
        }

        /// <summary>
        /// all categories the morph belongs to (canid, carnivore, ect) 
        /// </summary>
        public List<string> categories = new List<string>(); 
        /// <summary>
        /// the race of the animal this morph is to
        /// if this is a warg morph then race should be Warg
        /// </summary>
        public ThingDef race; //the animal race of the morph 

        public MorphGroupDef group; //the group the morph belongs to, if any 


        private float? _totalInfluence;

        public float TotalInfluence
        {
            get
            {
                if (_totalInfluence == null)
                {
                    var givers = DefDatabase<HediffDef>.AllDefs
                                                       .Where(def => typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass))
                                                       .SelectMany(def => def.GetAllHediffGivers()
                                                                             .OfType<HediffGiver_Mutation
                                                                              >()) //select the givers not the hediffs directly to get where they're assigned to 
                                                       .Where(g => g.hediff.CompProps<CompProperties_MorphInfluence>()?.morph
                                                                == this)
                                                       .GroupBy(g => g.hediff, g => g) //get only distinct values 
                                                       .Select(g => g.First()); //not get one of each mutation 


                    var counter = 0.0f; 
                    foreach (HediffGiver_Mutation hediffGiverMutation in givers)
                    {
                        var inf = hediffGiverMutation.hediff.CompProps<CompProperties_MorphInfluence>().influence;
                        counter += inf * hediffGiverMutation.countToAffect; 
                    }


                    _totalInfluence = counter; 
                }

                Log.Message($"$$$$ {nameof(TotalInfluence)} for {defName} is {_totalInfluence}");

                return _totalInfluence.Value;
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors())
            {
                yield return configError;
            }

            if (race == null)
            {
                yield return $"no race def found!"; 
            }else if (race.race == null)
            {
                yield return $"race {race.defName} has no race properties! are you sure this is a race?"; 
            }
        }

        public HybridRaceSettings raceSettings = new HybridRaceSettings(); 

        public TransformSettings transformSettings = new TransformSettings();


        [Unsaved]
        private List<HediffGiver_Mutation> _associatedMutations;

        /// <summary>
        /// Gets the mutations associated with this morph.
        /// </summary>
        /// <value>
        /// The associated mutations.
        /// </value>
        public IEnumerable<HediffGiver_Mutation> AssociatedMutations => _associatedMutations ?? (_associatedMutations = GetMutations());

        private List<HediffGiver_Mutation> GetMutations()
        {
            var linq = DefDatabase<HediffDef>.AllDefs.Where(def => typeof(Hediff_Morph).IsAssignableFrom(def.hediffClass)) //get all morph hediff defs 
                                             .SelectMany(def => def.stages ?? Enumerable.Empty<HediffStage>()) //get all stages 
                                             .SelectMany(s => s.hediffGivers ?? Enumerable.Empty<HediffGiver>()) //get all hediff givers 
                                             .OfType<HediffGiver_Mutation>() //keep only the mutation givers 
                                             .Where(mut => mut.hediff.CompProps<CompProperties_MorphInfluence>()?.morph == this); //keep only those associated with this morph 
            return linq.ToList();



        }

        [Unsaved] public ThingDef_AlienRace hybridRaceDef;



        [Obsolete("This is no longer used")]
        public FoodPreferability? GetOverride(ThingDef food) //note, RawTasty is 5, RawBad is 4 
        {
            if (food?.ingestible == null) return null; 
            foreach (HybridRaceSettings.FoodCategoryOverride foodOverride in raceSettings.foodSettings.foodOverrides)
            {
                if ((food.ingestible.foodType & foodOverride.foodFlags) != 0)
                    return foodOverride.preferability;
            }

            return null; 

        }
        
    }
}