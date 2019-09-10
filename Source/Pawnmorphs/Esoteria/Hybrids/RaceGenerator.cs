// RaceGenerator.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:12 PM
// last updated 08/02/2019  7:12 PM

using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hybrids
{
    /// <summary>
    ///     static class responsible for generating the implicit races
    /// </summary>
    public static class RaceGenerator
    {
        public static IEnumerable<ThingDef_AlienRace> ImplicitRaces
        {
            get
            {
                if (_lst == null)
                {
                    _lst = GenerateAllImpliedRaces().ToList(); 

                }

                return _lst; 
            }
        }
        private static List<ThingDef_AlienRace> _lst;

        private static Dictionary<ThingDef, MorphDef> _raceLookupTable = new Dictionary<ThingDef, MorphDef>();

        /// <summary>
        /// try to find the morph def associated with the given race 
        /// </summary>
        /// <param name="race"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryGetMorphOfRace(ThingDef race, out MorphDef result)
        {
            return _raceLookupTable.TryGetValue(race, out result); 
        }

        /// <summary>
        /// get the morph Def associated with this race, if any 
        /// </summary>
        /// <param name="race"></param>
        /// <returns></returns>
        public static MorphDef GetMorphOfRace(this ThingDef race)
        {
            return _raceLookupTable.TryGetValue(race); 
        }


        

        private static RaceProperties GenerateHybridProperties(RaceProperties human, RaceProperties animal)
        {
            return new RaceProperties
            {
                thinkTreeMain = human.thinkTreeMain, //most of these are just guesses, have to figure out what's safe to change and what isn't 
                thinkTreeConstant = human.thinkTreeConstant,
                intelligence = human.intelligence,
                makesFootprints = true,
                lifeExpectancy = human.lifeExpectancy,
                leatherDef = animal.leatherDef,
                nameCategory = human.nameCategory,
                body = human.body,
                baseBodySize = human.baseBodySize,
                baseHealthScale = human.baseHealthScale,
                foodType = GenerateFoodFlags(animal.foodType),
                gestationPeriodDays = human.gestationPeriodDays,
                meatColor = animal.meatColor,
                meatMarketValue = animal.meatMarketValue,
                manhunterOnDamageChance = animal.manhunterOnDamageChance,
                manhunterOnTameFailChance = animal.manhunterOnTameFailChance,
                litterSizeCurve = human.litterSizeCurve,
                lifeStageAges = human.lifeStageAges.ToList(),
                soundMeleeHitPawn = animal.soundMeleeHitPawn,
                soundMeleeHitBuilding = animal.soundMeleeHitBuilding,
                soundMeleeMiss = animal.soundMeleeMiss,
                specialShadowData = human.specialShadowData,
                soundCallIntervalRange = animal.soundCallIntervalRange,
                ageGenerationCurve = human.ageGenerationCurve,
                hediffGiverSets = human.hediffGiverSets.ToList(),
                meatDef = animal.meatDef,
                meatLabel = animal.meatLabel,
                useMeatFrom = animal.useMeatFrom,
                deathActionWorkerClass = human.deathActionWorkerClass,
                corpseDef = human.corpseDef,
            };
        }


        private static IEnumerable<ThingDef_AlienRace> GenerateAllImpliedRaces()
        {
            IEnumerable<MorphDef> morphs = DefDatabase<MorphDef>.AllDefs;
            var human = (ThingDef_AlienRace) ThingDef.Named("Human");
            foreach (MorphDef morphDef in morphs)
            {
                if (morphDef.hybridRaceDef != null)
                {
                    Log.Warning($"trying to generate race for {morphDef.defName} but it's hybrid race def is already set?");
                    continue;
                }


                Log.Message($"generating implied race for {morphDef.defName}");
                var race =  GenerateImplicitRace(human, morphDef);
                morphDef.hybridRaceDef = race;
                _raceLookupTable[race] = morphDef; 
                yield return race; 
            }
        }


        /// <summary>
        ///     generate general settings for the hybrid race given the human settings and morph def
        /// </summary>
        /// <param name="human"></param>
        /// <param name="morph"></param>
        /// <returns></returns>
        private static GeneralSettings GenerateHybridGeneralSettings(GeneralSettings human, MorphDef morph)
        {
            var traitSettings = morph.raceSettings.traitSettings;
            return new GeneralSettings
            {
                alienPartGenerator = GenerateHybridGenerator(human.alienPartGenerator, morph),
                humanRecipeImport = true ,
                forcedRaceTraitEntries = traitSettings?.forcedTraits
                //black list is not currently supported, Rimworld doesn't like it when you remove traits 
               
            };
        }

        private static AlienPartGenerator GenerateHybridGenerator(AlienPartGenerator human, MorphDef morph)
        {
            return new AlienPartGenerator
            {
                alienbodytypes = human.alienbodytypes, //this is where we'd force skin colors and stuff 
                aliencrowntypes = human.aliencrowntypes,
                bodyAddons = human.bodyAddons
            };
        }

        /// <summary>
        ///     generate the alien race restriction setting from the human default and the given morph
        /// </summary>
        /// <param name="human"></param>
        /// <param name="morph"></param>
        /// <returns></returns>
        private static RaceRestrictionSettings GenerateHybridRestrictionSettings(RaceRestrictionSettings human, MorphDef morph)
        {
            return new RaceRestrictionSettings(); //TODO restriction settings like apparel and stuff  
        }

        private static ThingDef_AlienRace.AlienSettings GenerateHybridAlienSettings(ThingDef_AlienRace.AlienSettings human,
                                                                                    MorphDef morph)
        {
            return new ThingDef_AlienRace.AlienSettings
            {
                generalSettings = GenerateHybridGeneralSettings(human.generalSettings, morph),
                graphicPaths = human.graphicPaths, //TODO put some of these in morph def or generate from the animal 
                hairSettings = human.hairSettings,
                raceRestriction =GenerateHybridRestrictionSettings(human.raceRestriction, morph),
                relationSettings = human.relationSettings,
                thoughtSettings = morph.raceSettings.GenerateThoughtSettings(human.thoughtSettings, morph)
            };
        }

        static List<StatModifier> GenerateHybridStatModifiers(List<StatModifier> humanModifiers, List<StatModifier> animalModifiers, List<StatModifier> statMods)
        {
            humanModifiers = humanModifiers ?? new List<StatModifier>();
            animalModifiers = animalModifiers ?? new List<StatModifier>();

            Dictionary<StatDef, float> valDict = new Dictionary<StatDef, float>();
            foreach (StatModifier humanModifier in humanModifiers)
            {
                valDict[humanModifier.stat] = humanModifier.value; 
            }


            //just average them for now 

            foreach (StatModifier animalModifier in animalModifiers)
            {
                float val;
                if (valDict.TryGetValue(animalModifier.stat, out val))
                {
                    val = Mathf.Lerp(val, animalModifier.value, 0.5f); //average the 2 
                }
                else val = animalModifier.value;

                valDict[animalModifier.stat] = val; 
            }

            //now handle any statMods if they exist 
            if (statMods != null)
                foreach (StatModifier statModifier in statMods)
                {
                    float v = valDict.TryGetValue(statModifier.stat) + statModifier.value; 
                    valDict[statModifier.stat] = v; 
                }




            List<StatModifier> outMods = new List<StatModifier>();
            foreach (KeyValuePair<StatDef, float> keyValuePair in valDict)
            {
                outMods.Add(new StatModifier()
                {
                    stat = keyValuePair.Key,
                    value = keyValuePair.Value
                });
            }

            


            return outMods; 


        }

        static FoodTypeFlags GenerateFoodFlags(FoodTypeFlags animalFlags)
        {
            animalFlags |= FoodTypeFlags.Meal; //make sure all hybrids can eat meals 
            //var platFlags = FoodTypeFlags.Plant | FoodTypeFlags.Tree; //make sure hybrids can't eat plants or trees, at least for now 
                        //need to figure out a way to let them graze but not pick up plants 
            //animalFlags =  animalFlags & ~platFlags;
            return animalFlags; 
        }

        private static ThingDef_AlienRace GenerateImplicitRace(ThingDef_AlienRace humanDef, MorphDef morph)
        {
            return new ThingDef_AlienRace
            {
                defName = morph.defName + "Race_Implied", //most of these are guesses, should figure out what's safe to change and what isn't 
                label = morph.label,
                race = GenerateHybridProperties(humanDef.race, morph.race.race),
                thingCategories = humanDef.thingCategories,
                thingClass = humanDef.thingClass,
                category = humanDef.category,
                selectable = humanDef.selectable,
                tickerType = humanDef.tickerType,
                altitudeLayer = humanDef.altitudeLayer,
                useHitPoints = humanDef.useHitPoints,
                hasTooltip = humanDef.hasTooltip,
                soundImpactDefault = morph.race.soundImpactDefault,
                statBases = GenerateHybridStatModifiers(humanDef.statBases, morph.race.statBases, morph.raceSettings.statModifiers),
                inspectorTabs = humanDef.inspectorTabs.ToList(), //do we want any custom tabs? 
                comps = humanDef.comps.ToList(),
                drawGUIOverlay = humanDef.drawGUIOverlay,
                description = string.IsNullOrEmpty(morph.description) ? morph.race.description : morph.description,
                alienRace = GenerateHybridAlienSettings(humanDef.alienRace, morph),
                modContentPack = morph.modContentPack,
                inspectorTabsResolved = humanDef.inspectorTabsResolved.ToList(),
                recipes = new List<RecipeDef>(humanDef.AllRecipes), //this is where the surgery operations live
                filth = morph.race.filth,
                filthLeaving = morph.race.filthLeaving,
                soundDrop = morph.race.soundDrop,
                soundInteract = morph.race.soundInteract,
                soundPickup = morph.race.soundPickup,
                socialPropernessMatters = humanDef.socialPropernessMatters,
                stuffCategories = humanDef.stuffCategories?.ToList(),
                designationCategory = humanDef.designationCategory,
                tradeTags = humanDef.tradeTags?.ToList()
                
            };
        }

        /// <summary>
        /// Determines whether this race is a morph hybrid race
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <returns>
        ///   <c>true</c> if the race is a morph hybrid race; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">raceDef</exception>
        public static bool IsMorphRace([NotNull]ThingDef raceDef)
        {
            if (raceDef == null) throw new ArgumentNullException(nameof(raceDef));
            return _raceLookupTable.ContainsKey(raceDef); 
        }
    }
}