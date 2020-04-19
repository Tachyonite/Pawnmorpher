// PawnTransferUtilities.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 6:29 AM
// last updated 12/24/2019  6:29 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// static container for functions that transfer stuff between pawns 
    /// </summary>
    public static class PawnTransferUtilities
    {
        public enum TransferDirection 
        {
            HumanlikeToAnimal,
            AnimalToHumanlike
        }

        /// <summary>
        /// Transfers the relations from pawn1 to pawn2 
        /// </summary>
        /// <param name="pawn1">The original.</param>
        /// <param name="pawn2">The animal.</param>
        /// <param name="predicate">optional predicate to dictate which relations get transferred</param>
        public static void TransferRelations([NotNull] Pawn pawn1, [NotNull] Pawn pawn2, Predicate<PawnRelationDef> predicate=null)
        {
            if (pawn1.relations == null) return; 
            var enumerator = pawn1.relations.DirectRelations.MakeSafe().ToList();
            predicate = predicate ?? (r => true); //if no filter is set, have it pass everything 
            foreach (DirectPawnRelation directPawnRelation in enumerator.Where(d => predicate(d.def)))
            {
                if(directPawnRelation.def.implied) continue;
                pawn1.relations?.RemoveDirectRelation(directPawnRelation); //make sure we remove the relations first 
                pawn2.relations?.AddDirectRelation(directPawnRelation.def, directPawnRelation.otherPawn);//TODO restrict these to special relationships? 
            }

            foreach (Pawn pRelatedPawns in pawn1.relations.PotentiallyRelatedPawns.ToList()) //make copies so we don't  invalidate the enumerator mid way through 
            {
                foreach (PawnRelationDef pawnRelationDef in pRelatedPawns.GetRelations(pawn1).Where(d => predicate(d)).ToList())
                {
                    if(pawnRelationDef.implied) continue;
                    pRelatedPawns.relations.RemoveDirectRelation(pawnRelationDef, pawn1);
                    pRelatedPawns.relations.AddDirectRelation(pawnRelationDef, pawn2); 

                }
            }

        }

        /// <summary>
        /// move all mutation related traits from the original pawn to the transformed pawn if they are sapient
        /// </summary>
        /// <param name="originalPawn">The original pawn.</param>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <param name="selector">The selector function for determining if a trait should be transferred</param>
        /// <exception cref="ArgumentNullException">
        /// transformedPawn
        /// or
        /// selector
        /// or
        /// originalPawn
        /// </exception>
        public static void TransferTraits([NotNull] Pawn originalPawn, [NotNull] Pawn transformedPawn, [NotNull] Func<TraitDef,bool> selector)
        {
            if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (originalPawn == null) throw new ArgumentNullException(nameof(originalPawn));

            if (transformedPawn.story?.traits == null) return;
            var tTraits = originalPawn.story.traits.allTraits.Select(t => t.def).Where(selector).ToList(); //save it to a list to not invalidate the enumerator 
            foreach (TraitDef mutationTrait in tTraits)
            {
                Trait trait = originalPawn.story?.traits?.GetTrait(mutationTrait);
                if (trait == null) continue;
                var newTrait = new Trait(mutationTrait, trait.Degree, true);
                transformedPawn.story.traits.GainTrait(newTrait);
            }
        }

        /// <summary>
        ///     Transfers all transferable aspects from the original pawn to animal they turned into.
        /// </summary>
        /// <param name="pawn1">The original.</param>
        /// <param name="pawn2">The animal.</param>
        public static void TransferAspects([NotNull] Pawn pawn1, [NotNull] Pawn pawn2)
        {
            AspectTracker oTracker = pawn1.GetAspectTracker();
            AspectTracker animalTracker = pawn2.GetAspectTracker();
            if (oTracker == null) return;
            if (animalTracker == null)
            {
                Log.Warning($"animal {pawn2.Name},{pawn2.def.defName} does not have an aspect tracker");
                return;
            }


            foreach (Aspect aspect in oTracker)
            { 
                if(animalTracker.Contains(aspect.def, aspect.StageIndex)) continue;


                if (aspect.def.transferToAnimal)
                {
                    int stageIndex = aspect.StageIndex;
                    Aspect aAspect = animalTracker.GetAspect(aspect.def);
                    if (aAspect != null)
                    {
                        aAspect.StageIndex = stageIndex; //set the stage but do not re add it 
                        aspect.OnTransferToAnimal(aAspect);
                    }
                    else
                    {
                        var newAspect = aspect.def.CreateInstance();
                        aspect.OnTransferToAnimal(newAspect);
                        animalTracker.Add(newAspect, stageIndex); //add it if the animal does not have the aspect
                    }

                }

            }
        }


        [CanBeNull]
        static SkillRecord TryGetSkill([NotNull] Pawn_SkillTracker tracker, [NotNull] SkillDef def)
        {
            foreach (SkillRecord skillRecord in tracker.skills.MakeSafe())
            {
                if (skillRecord.def == def) return skillRecord; 
            }

            return null; 
        }

        /// <summary>
        /// Transfers skills from pawn1 to pawn2
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <param name="mode">The transfer mode.</param>
        /// <param name="passionTransferMode">The passion transfer mode.</param>
        public static void TransferSkills([NotNull] Pawn pawn1, [NotNull] Pawn pawn2, SkillTransferMode mode = SkillTransferMode.Set, SkillPassionTransferMode passionTransferMode=SkillPassionTransferMode.Ignore)
        {
            if (pawn2.skills == null)
            {
                Log.Warning($"sapient animal {pawn2.Name} does not have a skill tracker");
                return;
            }

            if (pawn1.skills?.skills == null) return; 

            foreach (SkillRecord skillRecord in pawn1.skills.skills)
            {
                SkillRecord p2Skill = TryGetSkill(pawn2.skills, skillRecord.def); 
                if (p2Skill == null) continue;
                var oldLevel = p2Skill.Level;
                int newLevel;

                switch (mode)
                {
                    case SkillTransferMode.Set:
                        newLevel = skillRecord.Level; 
                        break;
                    case SkillTransferMode.Min:
                        newLevel = Mathf.Min(oldLevel, skillRecord.Level); 
                        break;
                    case SkillTransferMode.Max:
                        newLevel = Mathf.Max(oldLevel, skillRecord.Level);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                
                p2Skill.Level = newLevel;


                Passion passionLevel; 
                switch (passionTransferMode)
                {
                    case SkillPassionTransferMode.Min:
                        passionLevel = (Passion) Mathf.Min((int) p2Skill.passion, (int) skillRecord.passion); 
                        break;
                    case SkillPassionTransferMode.Max:
                        passionLevel = (Passion) Mathf.Max((int) p2Skill.passion, (int) skillRecord.passion); 
                        break;
                    case SkillPassionTransferMode.Set:
                        passionLevel = skillRecord.passion; 
                        break;
                    case SkillPassionTransferMode.Ignore:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(passionTransferMode), passionTransferMode, null);
                }

                p2Skill.passion = passionLevel; 
            }
        }

        [NotNull]
        private static readonly List<Faction> _facScratchList = new List<Faction>();


        /// <summary>
        /// Transfers the favor of all factions from pawn1 to pawn2 
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <exception cref="System.ArgumentNullException">
        /// pawn1
        /// or
        /// pawn2
        /// </exception>
        public static void TransferFavor([NotNull] Pawn pawn1, [NotNull] Pawn pawn2)
        {
            if (pawn1 == null) throw new ArgumentNullException(nameof(pawn1));
            if (pawn2 == null) throw new ArgumentNullException(nameof(pawn2));
            Pawn_RoyaltyTracker rTracker1 = pawn1.royalty;
            Pawn_RoyaltyTracker rTracker2 = pawn2.royalty;
            if (rTracker1 == null) return; 
            if (rTracker2 == null)
            {
                Log.Error($"trying to transfer titles from {pawn1.Name}/{pawn1.thingIDNumber} to {pawn2.Name}/{pawn2.thingIDNumber} but {pawn2.Name} does not have a royalty tracker!");
                return; 
            }

            _facScratchList.Clear();
            _facScratchList.AddRange(rTracker1.AllTitlesForReading.MakeSafe().Select(f => f.faction).Distinct()); //make a copy so we can remove safely while transferring 


            foreach (Faction faction in _facScratchList)
            {
                var favor = rTracker1.GetFavor(faction);
                var favor2 = rTracker2.GetFavor(faction); 
                if(favor2 >= favor) continue; //don't transfer if pawn2 already has a title equal or greater to this 
                if (!rTracker1.TryRemoveFavor(faction, favor)) //try to reduce to zero 
                {
                    Log.Error($"could not reduce favor of faction {faction.Name}/{faction.def.defName} for {pawn1.Name} to 0");
                    continue;
                }

                //now add the favor to pawn2 
                rTracker2.SetFavor(faction, favor); 
            }

        }

        /// <summary>
        /// enum for the different modes of transferring skills 
        /// </summary>
        public enum SkillTransferMode
        {
            /// <summary>
            /// The target skill's level should be set to exactly that of the source skill 
            /// </summary>
            Set,
            /// <summary>
            /// target skill's level should be the min of the original and that of the source skill 
            /// </summary>
            Min,
            /// <summary>
            /// target skill's level should be the max of the original and that of the source skill 
            /// </summary>
            Max
        }

        /// <summary>
        /// the method to use when transferring skill passions 
        /// </summary>
        public enum SkillPassionTransferMode
        {
            ///do not transfer passions
            Ignore,
            /// <summary>
            /// take the minimum of the passions 
            /// </summary>
            Min,
            /// <summary>
            /// take the maximum of the passions 
            /// </summary>
            Max,
            /// <summary>
            /// just set the passion level 
            /// </summary>
            Set
        }

        
    }
}