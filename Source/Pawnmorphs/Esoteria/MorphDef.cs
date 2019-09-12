// MorphDef.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 2:32 PM
// last updated 08/02/2019  2:32 PM

using System.Collections.Generic;
using System.Linq;
using AlienRace;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def class for a 'morph' 
    /// </summary>
    public class MorphDef : Def
    {
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
                    var comps = DefDatabase<HediffDef>.AllDefs
                        .Where(d => typeof(Hediff_AddedMutation).IsAssignableFrom(d.hediffClass))
                        .Select(d => d.CompProps<CompProperties_MorphInfluence>())
                        .Where(p => p != null);

                    var counter = 0.0f; 
                    foreach (var comp in comps)
                    {
                        counter += comp.influence; 
                    }

                    _totalInfluence = counter; 
                }

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