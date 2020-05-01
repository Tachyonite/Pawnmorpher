// HybridRaceSettings.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 9:47 AM
// last updated 08/03/2019  9:47 AM

using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
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
        [Obsolete("use " + nameof(ColorGenerator) + " instead")]
        public GraphicsSettings graphicsSettings;
        /// <summary>The trait settings</summary>
        public TraitSettings traitSettings;

        private IMorphColorGenerator colorGenerator;


        /// <summary>
        /// Gets the color generator.
        /// </summary>
        /// <value>
        /// The color generator.
        /// </value>
        public IMorphColorGenerator ColorGenerator 
        {
            get 
            {
                if (colorGenerator != null)
                    return colorGenerator;
#pragma warning disable 618
                else if (graphicsSettings != null) //attempt fallback to obsolete graphicsSettings
                    return graphicsSettings;
#pragma warning restore 618
                else
                {
                    colorGenerator = new GraphicsSettings();
                    return colorGenerator;
                }
            }
        }


        /// <summary>
        /// The explicit hybrid race
        /// </summary>
        public ThingDef explicitHybridRace;

        /// <summary>
        /// if true and explicitHybridRace is set, human hediff graphics will be added onto the explicit hybrid race 
        /// </summary>
        public bool transferHumanBodyAddons;

        private Type partTransformer = default;
        private IPartTransformer _transformer;


        /// <summary>
        /// a list of mutations that will be added to a pawn when they become a hybrid if they do not have them already 
        /// </summary>
        public List<MutationDef> requiredMutations = new List<MutationDef>(); 

        /// <summary>
        /// Gets the transformer.
        /// </summary>
        /// <value>
        /// The transformer.
        /// </value>
        /// <exception cref="InvalidCastException">tried to cast {partTransformer.Name} to {nameof(IPartTransformer)}</exception>
        [NotNull]
        public IPartTransformer Transformer
        {
            get
            {
                if (_transformer == null)
                {
                    if (partTransformer == null)
                    {
                        _transformer = new DefaultPartTransformer(); 
                    }
                    else
                    {
                        try
                        {
                            _transformer = (IPartTransformer) Activator.CreateInstance(partTransformer);
                            if (_transformer == null)
                            {
                                throw new InvalidCastException($"could not create type from {partTransformer.Name}");
                            }
                        }
                        catch (InvalidCastException e)
                        {
                            throw new InvalidCastException($"tried to cast {partTransformer.Name} to {nameof(IPartTransformer)}",e); 
                        }
                    }
                }

                return _transformer; 
            }
        }


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
        public class GraphicsSettings : IMorphColorGenerator
        {
            /// <summary>
            /// The skin color override.
            /// </summary>
            public Color? skinColorOverride;

            /// <summary>
            /// The female skin color override.
            /// </summary>
            public Color? femaleSkinColorOverride; 

            /// <summary>
            /// The skin color override second.
            /// </summary>
            public Color? skinColorOverrideSecond;

            /// <summary>
            /// The female skin color override second.
            /// </summary>
            public Color? femaleSkinColorOverrideSecond; 

            /// <summary>
            /// The female hair color override.
            /// </summary>
            public Color? femaleHairColorOverride; 

            /// <summary>
            /// The hair color override.
            /// </summary>
            public Color? hairColorOverride;

            /// <summary>
            /// The female hair color override second.
            /// </summary>
            public Color? femaleHairColorOverrideSecond; 

            /// <summary>
            /// The hair color override second.
            /// </summary>
            public Color? hairColorOverrideSecond;

            /// <summary>
            /// The custom draw size.
            /// </summary>
            public Vector2? customDrawSize;

            /// <summary>
            /// The custom head draw size.
            /// </summary>
            public Vector2? customHeadDrawSize;


            /// <summary>
            /// Gets all available channels in this .
            /// </summary>
            /// <value>
            /// The available channels.
            /// </value>
            IEnumerable<string> IMorphColorGenerator.AvailableChannels { get; } = new[] {"skin", "hair"};

            /// <summary>
            /// Gets a generated color channel for a specific pawn.
            /// </summary>
            /// <param name="pawn">The pawn.</param>
            /// <param name="channelID">The channel identifier.</param>
            /// <returns>the generated channel if possible, else null</returns>
            ColorChannel? IMorphColorGenerator.GetChannel(Pawn pawn, string channelID)
            {
                bool isFemale = pawn.gender == Gender.Female;

                if (channelID == "skin")
                {
                    Color? skinOverride = isFemale ? femaleSkinColorOverride ?? skinColorOverride : skinColorOverride;

                    if (skinOverride != null)
                    {
                        Color? skin2O = isFemale ? femaleSkinColorOverride ?? skinColorOverrideSecond : skinColorOverrideSecond;
                        return new ColorChannel(skinOverride.Value, skin2O);
                    }
                }
                else if (channelID == "hair")
                {
                    Color? hairO = isFemale ? femaleHairColorOverride ?? hairColorOverride : hairColorOverride;
                    if (hairO != null)
                        return new ColorChannel(hairO.Value,
                                                isFemale
                                                    ? femaleHairColorOverrideSecond ?? hairColorOverrideSecond
                                                    : hairColorOverrideSecond);
                }

                return null; 
            }
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