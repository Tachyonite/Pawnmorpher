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

namespace Pawnmorph.Hybrids
{
    public static class RaceShiftUtilities
    {
        //public const string RACE_CHANGE_LETTER_LABEL = "LetterRaceChangeToMorphLabel";
        //public const string RACE_CHANGE_LETTER_CONTENT = "LetterRaceChangeToMorphContent";

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
        /// <param name="pawn"></param>
        /// <param name="race"></param>
        /// <param name="reRollTraits">if race related traits should be reRolled</param>
        public static void ChangePawnRace([NotNull] Pawn pawn, ThingDef race, bool reRollTraits=false)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            var oldMorph = pawn.def.GetMorphOfRace();

            HediffDef oldGroupHediff = oldMorph?.group?.hediff;
            if (oldGroupHediff != null)
            {
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(oldGroupHediff);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff); 
                }
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
            var hediffDef = race.GetMorphOfRace()?.@group?.hediff;
            if (hediffDef != null)
            {
                pawn.health.AddHediff(hediffDef);
            }

            if (map != null)
            {
                var comp = map.GetComponent<MorphTracker>();
                comp.NotifyPawnRaceChanged(pawn, oldMorph); 
            }

            //no idea what HarmonyPatches.Patch.ChangeBodyType is for, not listed in pasterbin 
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();

            if (reRollTraits && race is ThingDef_AlienRace alienDef) ReRollRaceTraits(pawn, alienDef);

            //save location 
            pawn.ExposeData();
            if (pawn.Faction != faction) pawn.SetFaction(faction);
        }

        /// <summary>
        /// got through the mutations the pawn has and try to trigger any
        /// that change if the pawn's race changes 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="def"></param>
        static void TryTriggerMutations(Pawn pawn, MorphDef def)
        {
            var comps = pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>()
                            .Select(h => h.TryGetComp<Comp_MorphTrigger>())
                            .Where(c => c != null);
            foreach (Comp_MorphTrigger trigger in comps)
            {
                trigger.TryTrigger(def); 
            }
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
               }else if ( add && pawn.gender == Gender.Female && alienTraitEntry.commonalityFemale > 0) //only check gender chance if the add roll has passed 
               {                                                                                        //this is consistent with how the alien race framework handles it  
                   add =  Rand.RangeInclusive(0, 100) <= alienTraitEntry.commonalityMale; 
               }


               if (add)
               {
                   var degree = def.degreeDatas[alienTraitEntry.degree]; 
                   
                   traitSet.GainTrait(new Trait(def, alienTraitEntry.degree, true));
                   if(degree.skillGains != null)
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
                var skRecord= skills.GetSkill(keyValuePair.Key);
                skRecord.Level += keyValuePair.Value; 
            }
        }

        /// <summary>
        /// change the given pawn to the hybrid race of the desired morph 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="morph">the morph to change the pawn to</param>
        public static void ChangePawnToMorph([NotNull] Pawn pawn, [NotNull] MorphDef morph)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (morph == null) throw new ArgumentNullException(nameof(morph));
            if (morph.hybridRaceDef == null)
                Log.Error($"tried to change pawn {pawn.Name.ToStringFull} to morph {morph.defName} but morph has no hybridRace!");
            if (pawn.def != ThingDefOf.Human && !pawn.IsHybridRace())
            {
                Log.Warning($"hybrids of non human pawns are currently not supported");
                return;
            }

            //apply mutations 
            foreach (HediffGiver_Mutation morphAssociatedMutation in morph.AssociatedMutations)
            {
                morphAssociatedMutation.TryApply(pawn, MutagenDefOf.defaultMutagen);
            }

            var hRace = morph.hybridRaceDef;
            MorphDef.TransformSettings tfSettings = morph.transformSettings;
            HandleGraphicsChanges(pawn, morph);
            ChangePawnRace(pawn, hRace, true);

            if (pawn.IsColonist)
            {
                PortraitsCache.SetDirty(pawn);
            }

            if(pawn.IsColonist || pawn.IsPrisonerOfColony)
                SendHybridTfMessage(pawn, tfSettings);

            //now try to trigger any mutations
            if (pawn.health?.hediffSet?.hediffs != null)
                TryTriggerMutations(pawn, morph);

            if (tfSettings.transformTale != null) TaleRecorder.RecordTale(tfSettings.transformTale, pawn);
        }

        private static void SendHybridTfMessage(Pawn pawn, MorphDef.TransformSettings tfSettings)
        {
            string labelId = string.IsNullOrEmpty(tfSettings.transformationMessageID)
                ? RACE_CHANGE_MESSAGE_ID
                : tfSettings.transformationMessageID;//assign the correct default values if none are present 
            //string contentID = string.IsNullOrEmpty(tfSettings.transformLetterContentId)
            //    ? RACE_CHANGE_LETTER_CONTENT
            //    : tfSettings.transformLetterContentId; 

            string label = labelId.Translate(pawn.LabelShort).CapitalizeFirst();
            //string content = contentID.Translate(pawn.LabelShort).CapitalizeFirst();
            //LetterDef letterDef = tfSettings.letterDef ?? LetterDefOf.PositiveEvent;
            //Find.LetterStack.ReceiveLetter(label, content, letterDef, pawn);

            var messageDef = tfSettings.messageDef ?? MessageTypeDefOf.NeutralEvent;
            Messages.Message(label, pawn, messageDef);
        }

        private static void HandleGraphicsChanges(Pawn pawn,MorphDef morph)
        {
            var comp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            comp.skinColor = morph.GetSkinColorOverride() ?? comp.skinColor;
            comp.skinColorSecond = morph.GetSkinColorSecondOverride() ?? comp.skinColorSecond;
            comp.hairColorSecond = morph.GetHairColorOverrideSecond() ?? comp.hairColorSecond;
            pawn.story.hairColor = morph.GetHairColorOverride() ?? pawn.story.hairColor; 
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

            var isHybrid = pawn.IsHybridRace();
            DebugLogUtils.Assert(isHybrid, "pawn.IsHybridRace()");
            if (!isHybrid) return;

            var storedGraphics = pawn.GetComp<GraphicSys.InitialGraphicsComp>(); 
            storedGraphics.RestoreGraphics();

            ChangePawnRace(pawn, human); 

            //TODO traits or something? 





            //var letterLabel = RaceRevertLetterLabel.Translate(pawn.LabelShort).CapitalizeFirst();
            //var letterContent = RaceRevertLetterContent.Translate(pawn.LabelShort).CapitalizeFirst();

            //Find.LetterStack.ReceiveLetter(letterLabel, letterContent, RevertToHumanLetterDef, pawn); 

            var messageStr = RACE_REVERT_MESSAGE_ID.Translate(pawn.LabelShort).CapitalizeFirst();
            Messages.Message(messageStr, pawn, MessageTypeDefOf.NeutralEvent); 


        }

    }
}