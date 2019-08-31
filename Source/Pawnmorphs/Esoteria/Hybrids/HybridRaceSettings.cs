// HybridRaceSettings.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 9:47 AM
// last updated 08/03/2019  9:47 AM

using System.Collections.Generic;
using System.Linq;
using AlienRace;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hybrids
{
    public class HybridRaceSettings
    {
        /// <summary>
        /// settings for the hybrid race's thoughts 
        /// </summary>
        public class HybridThoughtSettings
        {


            public List<ThoughtReplacer> replacerList;
            public AteThought ateAnimalThought;
            public ButcherThought butcheredAnimalThought; 
            public bool suppressHumanlikeCannibalThoughts; //if true this morph will not get the cannibal thoughts 
            public bool canEatRaw; //if true then the AteRawFood thought will be suppressed 
            public List<string> thoughtsBlackList;
            public List<AteThought> ateThoughtsSpecifics = new List<AteThought>();
            public List<ButcherThought> butcherThoughtsSpecifics= new List<ButcherThought>();

        }

        public class GraphicsSettings
        {
            public Color? skinColorOverride;

        }


        public class FoodCategoryOverride
        {

            public FoodTypeFlags foodFlags;
            public FoodPreferability preferability; 

        }

        public class FoodSettings
        {
            public List<FoodCategoryOverride> foodOverrides = new List<FoodCategoryOverride>(); 

        }

        public FoodSettings foodSettings = new FoodSettings();

        public List<StatModifier> statModifiers; 
        public HybridThoughtSettings thoughtSettings;
        public RaceRestrictionSettings restrictionSettings;
        public GraphicsSettings graphicsSettings; 

        public TraitSettings traitSettings; 

        public class TraitSettings
        {
            public List<AlienTraitEntry> forcedTraits;
            //public List<string> disallowedTraits; removing traits not supported right now, rimworld doesn't like it when you remove them  
        }

        /// <summary>
        /// generate AlienRace thought settings with the given morph def 
        /// </summary>
        /// <param name="humanDefault"></param>
        /// <param name="morphDef"></param>
        /// <returns></returns>
        public ThoughtSettings GenerateThoughtSettings(ThoughtSettings humanDefault, MorphDef morphDef)
        {

            if (thoughtSettings == null) return humanDefault;
            var animalRace = morphDef.race;

            List<AteThought> spc = new List<AteThought>(thoughtSettings.ateThoughtsSpecifics);
            List<ButcherThought> butcherSpc = new List<ButcherThought>(thoughtSettings.butcherThoughtsSpecifics); //make copies 

            if (thoughtSettings.ateAnimalThought != null)
            {

                spc.Add(new AteThought
                {
                    thought = thoughtSettings.ateAnimalThought.thought,
                    ingredientThought = thoughtSettings.ateAnimalThought.ingredientThought,
                    raceList = new List<string> {animalRace.defName}
                }); 

            }

            if (thoughtSettings.butcheredAnimalThought != null)
            {
                butcherSpc.Add(new ButcherThought()
                {
                    thought = thoughtSettings.butcheredAnimalThought.thought,
                    knowThought = thoughtSettings.butcheredAnimalThought.knowThought,
                    raceList =  new List<string> {  animalRace.defName}
                });
            }

           
            List<string> blackList;
            if (thoughtSettings.thoughtsBlackList != null)
            {
                blackList = new List<string>(thoughtSettings.thoughtsBlackList); 
            }
            else
            {
                blackList = null; 
            }

            if (thoughtSettings.suppressHumanlikeCannibalThoughts) //add in the default cannibal thoughts to the black list 
            {
                blackList = blackList ?? new List<string>(); 

                blackList.Add("AteHumanlikeMeatDirect");
                blackList.Add("AteHumanlikeMeatAsIngredient");
                blackList.Add("ButcheredHumanlikeCorpse");
                blackList.Add("KnowButcheredHumanlikeCorpse");
            }

            if (thoughtSettings.canEatRaw)
            {
                blackList = blackList ?? new List<string>(); 
                blackList.Add("AteRawFood");
            }


            return new ThoughtSettings
            {
                replacerList = thoughtSettings.replacerList,
                butcherThoughtSpecific = butcherSpc,
                ateThoughtSpecific = spc,
                cannotReceiveThoughts = blackList
            };





        }


    }
}