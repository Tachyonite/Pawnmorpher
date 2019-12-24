// PawnTransferUtilities.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 6:29 AM
// last updated 12/24/2019  6:29 AM

using System;
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
                if (aspect.def.transferToAnimal)
                {
                    int stageIndex = aspect.StageIndex;
                    animalTracker.Add(aspect.def, stageIndex);
                }
        }

        /// <summary>
        /// Transfers skills from pawn1 to pawn2 
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <param name="mode">The transfer mode.</param>
        public static void TransferSkills([NotNull] Pawn pawn1, [NotNull] Pawn pawn2, SkillTransferMode mode = SkillTransferMode.Set)
        {
            if (pawn2.skills == null)
            {
                Log.Warning($"sapient animal {pawn2.Name} does not have a skill tracker");
                return;
            }

            if (pawn1.skills?.skills == null) return; 

            foreach (SkillRecord skillsSkill in pawn1.skills.skills)
            {
                var oldLevel = pawn1.skills.GetSkill(skillsSkill.def)?.Level ?? 0;
                int newLevel;

                switch (mode)
                {
                    case SkillTransferMode.Set:
                        newLevel = skillsSkill.Level; 
                        break;
                    case SkillTransferMode.Min:
                        newLevel = Mathf.Min(oldLevel, skillsSkill.Level); 
                        break;
                    case SkillTransferMode.Max:
                        newLevel = Mathf.Max(oldLevel, skillsSkill.Level);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
                }

                var p2Skill = pawn2.skills.GetSkill(skillsSkill.def);
                p2Skill.Level = newLevel; 

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
    }
}