// RaceShiftUtilities.cs modified by Iron Wolf for Pawnmorph on 08/02/2019 7:34 PM
// last updated 08/02/2019  7:34 PM

using System;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.Hybrids
{
    /// <summary>
    /// a collection of utilities around changing a pawn's race 
    /// </summary>
    public static class RaceShiftUtilities
    {
        /// <summary>
        /// The race change message identifier (used in the keyed translation file)
        /// </summary>
        public const string RACE_CHANGE_MESSAGE_ID = "RaceChangeMessage";

        private const string RACE_REVERT_MESSAGE_ID = "HumanAgainMessage";
        // private static string RaceRevertLetterLabel => RACE_REVERT_LETTER + "Label";
        //private static string RaceRevertLetterContent => RACE_REVERT_LETTER + "Content";

        private static LetterDef RevertToHumanLetterDef => LetterDefOf.PositiveEvent;

        /// <summary>
        /// Determines whether this pawn is a morph hybrid 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if the specified pawn is a morph hybrid; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static bool IsMorphHybrid([NotNull] Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            return RaceGenerator.IsMorphRace(pawn.def);

        }


        /// <summary>
        /// safely change the pawns race
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="race">The race.</param>
        /// <param name="reRollTraits">if race related traits should be reRolled</param>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static void ChangePawnRace([NotNull] Pawn pawn, ThingDef race, bool reRollTraits = false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            MorphDef oldMorph = pawn.def.GetMorphOfRace();
            ThingDef oldRace = pawn.def;

            AspectTracker aTracker = pawn.GetAspectTracker();

            AspectDef oldMorphAspectDef = oldMorph?.group?.aspectDef;
            if (oldMorphAspectDef != null && aTracker != null)
            {
                Aspect aspect = aTracker.GetAspect(oldMorphAspectDef);
                if (aspect != null) aTracker.Remove(aspect);
            }

            //var pos = pawn.Position;
            Faction faction = pawn.Faction;
            Map map = pawn.Map;

            if (map != null)
                RegionListersUpdater.DeregisterInRegions(pawn, map);
            var removed = false;

            if (map != null)
                if (map.listerThings.Contains(pawn))
                {
                    map.listerThings.Remove(pawn); //make sure to update the lister things or else dying will break 
                    removed = true;
                }

            pawn.def = race;

            if (removed && !map.listerThings.Contains(pawn))
                map.listerThings.Add(pawn);

            if (map != null)
                RegionListersUpdater.RegisterInRegions(pawn, map);

            map?.mapPawns.UpdateRegistryForPawn(pawn);

            //add the group hediff if applicable 
            AspectDef aspectDef = race.GetMorphOfRace()?.group?.aspectDef;
            if (aspectDef != null) aTracker?.Add(aspectDef);

            if (map != null)
            {
                var comp = map.GetComponent<MorphTracker>();
                comp.NotifyPawnRaceChanged(pawn, oldMorph);
            }

            if (race == ThingDefOf.Human)
                ValidateReversion(pawn); 
            else 
                ValidateExplicitRaceChange(pawn, race, oldRace);

            

            //no idea what HarmonyPatches.Patch.ChangeBodyType is for, not listed in pasterbin 
            pawn.RefreshGraphics();

            if (reRollTraits && race is ThingDef_AlienRace alienDef) ReRollRaceTraits(pawn, alienDef);

            //save location 
            if (Current.ProgramState == ProgramState.Playing)
                pawn.ExposeData();
            if (pawn.Faction != faction) pawn.SetFaction(faction);
            foreach (IRaceChangeEventReceiver raceChangeEventReceiver in pawn.AllComps.OfType<IRaceChangeEventReceiver>())
            {
                raceChangeEventReceiver.OnRaceChange(oldRace);
            }
        }

        private static void ValidateReversion(Pawn pawn)
        {
            var graphicsComp = pawn.GetComp<InitialGraphicsComp>();
            var alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            var story = pawn.story; 
            if (alienComp == null)
            {
                Log.Error($"trying to validate the graphics of {pawn.Name} but they don't have an {nameof(AlienPartGenerator.AlienComp)}!");
                return;
            }

            if (graphicsComp == null)
            {
                Log.Error($"trying to validate the graphics of {pawn.Name} but they don't have an {nameof(InitialGraphicsComp)}!");

            }

            story.bodyType = graphicsComp.BodyType;
            alienComp.crownType = graphicsComp.CrownType;
            story.hairDef = graphicsComp.HairDef; 
        }


        private static void ValidateExplicitRaceChange(Pawn pawn, ThingDef race, ThingDef oldRace)
        {
            if (oldRace is ThingDef_AlienRace oldARace)
            {
                if (race is ThingDef_AlienRace aRace)
                {
                    ValidateGraphicsPaths(pawn, oldARace, aRace);
                } //validating the graphics paths only works for aliens 
                else
                {
                    Log.Warning($"trying change the race of {pawn.Name} to {race.defName} which is not {nameof(ThingDef_AlienRace)}!");

                }
            }
            else
            {
                Log.Warning($"trying change the race of {pawn.Name} from {oldRace.defName} which is not {nameof(ThingDef_AlienRace)}!");
            }
        }

        private static void ValidateGraphicsPaths([NotNull] Pawn pawn, [NotNull] ThingDef_AlienRace oldRace, [NotNull] ThingDef_AlienRace race)
        {
            //this implimentation is a work in progress 
            //currently, when shifting to an explicit race the body and head types will come out 'shuffled'
            //
            var alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            var graphicsComp = pawn.GetComp<InitialGraphicsComp>();
            var story = pawn.story; 
            if (alienComp == null)
            {
                Log.Error($"trying to validate the graphics of {pawn.Name} but they don't have an {nameof(AlienPartGenerator.AlienComp)}!");
                return; 
            }

            if(graphicsComp == null)
            {
                Log.Error($"trying to validate the graphics of {pawn.Name} but they don't have an {nameof(InitialGraphicsComp)}!");

            }

           

            var oldGen = oldRace.alienRace.generalSettings.alienPartGenerator;
            var newGen = race.alienRace.generalSettings.alienPartGenerator; 

            //get the new head type 
            var oldHIndex = oldGen.aliencrowntypes.FindIndex(s => s == alienComp.crownType);
            float hRatio = newGen.aliencrowntypes.Count / ((float) oldGen.aliencrowntypes.Count) ; 

            int newHIndex; 
            if (oldHIndex == -1) //-1 means not found 
            {
                newHIndex = Rand.Range(0, newGen.aliencrowntypes.Count); //just pick a random head 
            }
            else
            {
                newHIndex = Mathf.FloorToInt(oldHIndex * hRatio); //squeeze the old index into the range of the new crow type 
            }


            //now get the new body type 

            var bRatio = newGen.alienbodytypes.Count / ((float) oldGen.alienbodytypes.Count);
            var oldBIndex = oldGen.alienbodytypes.FindIndex(b => b == story.bodyType);

            int newBIndex; 
            if (oldBIndex == -1 )
            {
                newBIndex = Rand.Range(0, newGen.alienbodytypes.Count); 
            }
            else
            {
                newBIndex = Mathf.FloorToInt(oldBIndex * bRatio);
            }

            Log.Message($"{nameof(newHIndex)}:{newHIndex}/Count:{newGen.aliencrowntypes.Count}");
            Log.Message($"{nameof(newBIndex)}:{newBIndex}/Count:{newGen.alienbodytypes.Count}");


            //now set the body and head type 

            var newHeadType = newGen.aliencrowntypes[newHIndex];
            
            alienComp.crownType = newHeadType;

            if (newGen.alienbodytypes.Count > 0)
            {
                var newBType = newGen.alienbodytypes[newBIndex];
                story.bodyType = newBType; 
            }

            //TODO make hair disaper if explicit race doesn't have hair 
            //if (oldRace.alienRace.hairSettings?.hasHair == true && race.alienRace.hairSettings?.hasHair == false)
            //{
            //    story.hairDef = null; 
            //}
        }

        static void ReRollRaceTraits(Pawn pawn, ThingDef_AlienRace newRace)
        {
            var traitSet = pawn.story?.traits;
            if (traitSet == null) return;
            var allAlienTraits = newRace.alienRace.generalSettings?.forcedRaceTraitEntries;
            if (allAlienTraits == null || allAlienTraits.Count == 0) return;
            //removing traits not supported right now, Rimworld doesn't like it when you remove traits 


            var traitsToAdd = allAlienTraits;
            foreach (AlienTraitEntry alienTraitEntry in traitsToAdd)
            {
                var def = TraitDef.Named(alienTraitEntry.defName);
                if (traitSet.HasTrait(def)) continue; //don't add traits that are already added 

                var add = (Rand.RangeInclusive(0, 100) <= alienTraitEntry.chance);

                if (add && pawn.gender == Gender.Male && alienTraitEntry.commonalityMale > 0)
                {
                    add = Rand.RangeInclusive(0, 100) <= alienTraitEntry.commonalityMale;
                }
                else if (add && pawn.gender == Gender.Female && alienTraitEntry.commonalityFemale > 0) //only check gender chance if the add roll has passed 
                {                                                                                        //this is consistent with how the alien race framework handles it  
                    add = Rand.RangeInclusive(0, 100) <= alienTraitEntry.commonalityMale;
                }


                if (add)
                {
                    var degree = def.degreeDatas[alienTraitEntry.degree];

                    traitSet.GainTrait(new Trait(def, alienTraitEntry.degree, true));
                    if (degree.skillGains != null)
                        UpdateSkillsPostAdd(pawn, degree.skillGains); //need to update the skills manually
                }
            }
        }

        static void UpdateSkillsPostAdd(Pawn pawn, Dictionary<SkillDef, int> skillDict)
        {
            var skills = pawn.skills;
            if (skills == null) return;

            foreach (KeyValuePair<SkillDef, int> keyValuePair in skillDict)
            {
                var skRecord = skills.GetSkill(keyValuePair.Key);
                skRecord.Level += keyValuePair.Value;
            }
        }

        /// <summary>
        /// change the given pawn to the hybrid race of the desired morph
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="morph">the morph to change the pawn to</param>
        /// <param name="addMissingMutations">if true, any missing mutations will be applied to the pawn</param>
        /// <param name="displayNotifications">if set to <c>true</c> display race shit notifications.</param>
        /// <exception cref="ArgumentNullException">
        /// pawn
        /// or
        /// morph
        /// </exception>
        public static void ChangePawnToMorph([NotNull] Pawn pawn, [NotNull] MorphDef morph, bool addMissingMutations=true, bool displayNotifications=true)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (morph == null) throw new ArgumentNullException(nameof(morph));
            if (morph.hybridRaceDef == null)
                Log.Error($"tried to change pawn {pawn.Name.ToStringFull} to morph {morph.defName} but morph has no hybridRace!");
            if (pawn.def != ThingDefOf.Human && !pawn.IsHybridRace())
            {
                
                return;
            }

            var oldRace = pawn.def; 
            //apply mutations 
            if (addMissingMutations)
                SwapMutations(pawn, morph);

            var hRace = morph.hybridRaceDef;
            MorphDef.TransformSettings tfSettings = morph.transformSettings;
            HandleGraphicsChanges(pawn, morph);
            ChangePawnRace(pawn, hRace, true);

            if (pawn.IsColonist)
            {
                PortraitsCache.SetDirty(pawn);
            }

            if (displayNotifications && (pawn.IsColonist || pawn.IsPrisonerOfColony))
                SendHybridTfMessage(pawn, tfSettings);

     

            if (tfSettings?.transformTale != null) TaleRecorder.RecordTale(tfSettings.transformTale, pawn);
            pawn.TryGainMemory(tfSettings?.transformationMemory ?? PMThoughtDefOf.DefaultMorphTfMemory);

            if (oldRace.race.body != pawn.RaceProps.body)
            {
                FixHediffs(pawn, oldRace, morph); 
            }

        }

        [NotNull]
        private static readonly List<Hediff> _rmList = new List<Hediff>(); 

        private static void FixHediffs([NotNull] Pawn pawn, [NotNull] ThingDef oldRace, [NotNull] MorphDef morph)
        {
            var transformer = morph.raceSettings.Transformer;


            _rmList.Clear();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                if(hediff.Part == null) continue;
                var newRecord = transformer.Transform(hediff.Part, pawn.RaceProps.body);
                if (newRecord != null)
                {
                    hediff.Part = newRecord; 
                }
                else
                {
                    _rmList.Add(hediff);
                }
            }

            foreach (Hediff hediff in _rmList)
            {
                pawn.health.RemoveHediff(hediff);
            }

        }

        private static void SwapMutations([NotNull] Pawn pawn,[NotNull] MorphDef morph)
        {
            if (pawn.health?.hediffSet?.hediffs == null)
            {
                Log.Error($"pawn {pawn.Name} has null health or hediffs?");
                return;
            }

            var partDefsToAddTo = pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>() //only want to count mutations 
                                      .Where(m => m.Part != null && !m.def.HasComp(typeof(SpreadingMutationComp)) && !morph.IsAnAssociatedMutation(m))
                                      //don't want mutations without a part or mutations that spread past the part they were added to 
                                      .Select(m => m.Part)
                                      .ToList(); //needs to be a list because we're about to modify hediffs 

            List<BodyPartRecord> addedRecords = new List<BodyPartRecord>();
            
            foreach (BodyPartRecord bodyPartRecord in partDefsToAddTo)
            {
                if(addedRecords.Contains(bodyPartRecord)) continue; //if a giver already added to the record don't add it twice 
                
                // ReSharper disable once AssignNullToNotNullAttribute
                var mutation = morph.GetMutationForPart(bodyPartRecord.def).RandomElementWithFallback();
                if (mutation != null)
                {
                    var result = MutationUtilities.AddMutation(pawn, mutation, bodyPartRecord);
                    foreach (Hediff_AddedMutation addedMutation in result)
                    {
                        addedRecords.Add(addedMutation.Part); 
                    }
                }
            }
        }

        private static void SendHybridTfMessage(Pawn pawn, MorphDef.TransformSettings tfSettings)
        {
            string label;

            label = string.IsNullOrEmpty(tfSettings?.transformationMessage)
                        ? RACE_CHANGE_MESSAGE_ID.Translate(pawn.LabelShort)
                        : tfSettings.transformationMessage.Formatted(pawn.LabelShort);

            label = label.CapitalizeFirst(); 

         

            var messageDef = tfSettings.messageDef ?? MessageTypeDefOf.NeutralEvent;
            Messages.Message(label, pawn, messageDef);
        }

        private static void HandleGraphicsChanges(Pawn pawn, MorphDef morph)
        {
            var comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            comp.skinColor = comp.ColorChannels["skin"].first = morph.GetSkinColorOverride(pawn) ?? comp.skinColor;
            comp.skinColorSecond = comp.ColorChannels["skin"].second = morph.GetSkinColorSecondOverride(pawn) ?? comp.skinColorSecond;
            comp.hairColorSecond = comp.ColorChannels["hair"].second = morph.GetHairColorOverrideSecond(pawn) ?? comp.hairColorSecond;
            pawn.story.hairColor = comp.ColorChannels["hair"].first = morph.GetHairColorOverride(pawn) ?? pawn.story.hairColor;
        }

        /// <summary>
        /// change the race of the pawn back to human 
        /// </summary>
        /// <param name="pawn"></param>
        public static void RevertPawnToHuman([NotNull] Pawn pawn)
        {
            var race = pawn.def;

            var human = ThingDefOf.Human;
            if (race == human) return; //do nothing 


            var oldMorph = pawn.def.GetMorphOfRace();
            bool isHybrid = oldMorph != null;


            DebugLogUtils.Assert(isHybrid, "pawn.IsHybridRace()");
            if (!isHybrid) return;

            var storedGraphics = pawn.GetComp<GraphicSys.InitialGraphicsComp>();
            storedGraphics.RestoreGraphics();

            ChangePawnRace(pawn, human);

            
            var morphRThought = oldMorph.transformSettings?.revertedMemory;
            morphRThought = morphRThought ?? PMThoughtDefOf.DefaultMorphRevertsToHuman; 
            
            if(morphRThought != null)
                pawn.TryGainMemory(morphRThought);
            var messageStr = RACE_REVERT_MESSAGE_ID.Translate(pawn.LabelShort).CapitalizeFirst();
            Messages.Message(messageStr, pawn, MessageTypeDefOf.NeutralEvent);
        }
    }
}
