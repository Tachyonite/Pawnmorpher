// HybridRaceSettings.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 9:47 AM
// last updated 08/03/2019  9:47 AM

using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using RimWorld;
using UnityEngine;
using Verse;
#pragma warning disable 0612
namespace Pawnmorph.Hybrids
{
    /// <summary>
    /// class representing the hybrid race settings 
    /// </summary>
    public class HybridRaceSettings
    {
        /// <summary>unused</summary>
        [Obsolete("Doesn't do anything'")]
        public FoodSettings foodSettings = new FoodSettings();

        /// <summary>The stat modifiers</summary>
        public List<StatModifier> statModifiers;
        /// <summary>The thought settings</summary>
        public HybridThoughtSettings thoughtSettings;
        /// <summary>
        /// the race restriction settings 
        /// </summary>
        public RaceRestrictionSettings restrictionSettings;
        /// <summary>The graphics settings</summary>
        public GraphicsSettings graphicsSettings;
        /// <summary>The trait settings</summary>
        public TraitSettings traitSettings;

        /// <summary>
        /// settings for the hybrid race's thoughts 
        /// </summary>
        public class HybridThoughtSettings
        {
            /// <summary>list of thoughts that will be replaced </summary>
            public List<ThoughtReplacer> replacerList;
            /// <summary>thought given when a pawn of this hybrid race eats an animal listed in the morphDef</summary>
            public AteThought ateAnimalThought;
            /// <summary>
            /// thought given when a pawn of this hybrid race butchers an animal listed in the morph def 
            /// </summary>
            public ButcherThought butcheredAnimalThought;
            ///if true this morph will not get the cannibal thoughts 
            public bool suppressHumanlikeCannibalThoughts;
            ///if true then the AteRawFood thought will be suppressed 
            public bool canEatRaw;
            /// <summary>a list of thoughtDefs that this hybrid race cannot get </summary>
            public List<string> thoughtsBlackList;
            /// <summary>
            /// a list of thoughts when the pawn eats specific things 
            /// </summary>
            public List<AteThought> ateThoughtsSpecifics = new List<AteThought>();
            /// <summary>
            /// list of thoughts when a pawn of this race butchers specific things 
            /// </summary>
            public List<ButcherThought> butcherThoughtsSpecifics = new List<ButcherThought>();
        }

        /// <summary>
        /// class representing the graphic setting of a morph hybrid race
        /// </summary>
        public class GraphicsSettings
        {
            /// <summary>The skin color override</summary>
            public Color? skinColorOverride;

            /// <summary>
            /// The female skin color override
            /// </summary>
            public Color? femaleSkinColorOverride; 

            /// <summary>
            /// The skin color override second
            /// </summary>
            public Color? skinColorOverrideSecond;

            /// <summary>
            /// The female skin color override second
            /// </summary>
            public Color? femaleSkinColorOverrideSecond; 

            /// <summary>
            /// The female hair color override
            /// </summary>
            public Color? femaleHairColorOverride; 

            /// <summary>The hair color override</summary>
            public Color? hairColorOverride;

            /// <summary>
            /// The female hair color override second
            /// </summary>
            public Color? femaleHairColorOverrideSecond; 

            /// <summary>
            /// The hair color override second
            /// </summary>
            public Color? hairColorOverrideSecond;

            
            /// <summary>The custom draw size</summary>
            [Obsolete("Non Functional")]
            public Vector2? customDrawSize;
            /// <summary>
            /// The custom head draw size
            /// </summary>
            [Obsolete("Non Functional")]
            public Vector2? customHeadDrawSize;
        }

        /// <summary>
        /// obsolete
        /// </summary>
        [Obsolete]
        public class FoodCategoryOverride
        {
            /// <summary>The food flags</summary>
            public FoodTypeFlags foodFlags;
            /// <summary>The preferability</summary>
            public FoodPreferability preferability;
        }

        /// <summary>
        /// obsolete
        /// </summary>
        [Obsolete]
        public class FoodSettings
        {
            /// <summary>The food overrides</summary>
            public List<FoodCategoryOverride> foodOverrides = new List<FoodCategoryOverride>();
        }

        //should this be deprecated?        
        /// <summary>
        /// obsolete
        /// </summary>
        [Obsolete]
        public class TraitSettings
        {
            /// <summary>
            /// 
            /// </summary>
            //should this be deprecated? 
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
                    raceList = new List<string> { animalRace.defName }
                });

            }

            if (thoughtSettings.butcheredAnimalThought != null)
            {
                butcherSpc.Add(new ButcherThought()
                {
                    thought = thoughtSettings.butcheredAnimalThought.thought,
                    knowThought = thoughtSettings.butcheredAnimalThought.knowThought,
                    raceList = new List<string> { animalRace.defName }
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