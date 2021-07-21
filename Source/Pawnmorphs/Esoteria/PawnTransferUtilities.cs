// PawnTransferUtilities.cs modified by Iron Wolf for Pawnmorph on 12/24/2019 6:29 AM
// last updated 12/24/2019  6:29 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Thoughts;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static container for functions that transfer stuff between pawns
    /// </summary>
    public static class PawnTransferUtilities
    {
        /// <summary>
        ///     the method to use when transferring skill passions
        /// </summary>
        public enum SkillPassionTransferMode
        {
            ///do not transfer passions
            Ignore,

            /// <summary>
            ///     take the minimum of the passions
            /// </summary>
            Min,

            /// <summary>
            ///     take the maximum of the passions
            /// </summary>
            Max,

            /// <summary>
            ///     just set the passion level
            /// </summary>
            Set
        }

        /// <summary>
        ///     enum for the different modes of transferring skills
        /// </summary>
        public enum SkillTransferMode
        {
            /// <summary>
            ///     The target skill's level should be set to exactly that of the source skill
            /// </summary>
            Set,

            /// <summary>
            ///     target skill's level should be the min of the original and that of the source skill
            /// </summary>
            Min,

            /// <summary>
            ///     target skill's level should be the max of the original and that of the source skill
            /// </summary>
            Max
        }

        [NotNull] private static readonly List<Faction> _facScratchList = new List<Faction>();

        /// <summary>
        ///     Merges the skills from the given original pawns into the given meld
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="meld">The meld.</param>
        public static void MergeSkills([NotNull] IEnumerable<Pawn> originals, [NotNull] Pawn meld)
        {
            if (originals == null) throw new ArgumentNullException(nameof(originals));
            if (meld == null) throw new ArgumentNullException(nameof(meld));
            Pawn_SkillTracker mSkills = meld.skills;
            if (mSkills == null) return;
            var tmpDict = new Dictionary<SkillDef, int>();
            var passionDict = new Dictionary<SkillDef, int>();
            var count = 0;
            foreach (Pawn original in originals)
            {
                Pawn_SkillTracker skills = original.skills;
                if (skills == null) continue;
                foreach (SkillRecord skill in skills.skills)
                {
                    tmpDict[skill.def] = tmpDict.TryGetValue(skill.def) + skill.Level;
                    passionDict[skill.def] = (int) skill.passion + passionDict.TryGetValue(skill.def);
                }

                count++;
            }

            count = Mathf.Max(1, count);
            float scaleVal = 1 / (count * 0.7f);

            foreach (KeyValuePair<SkillDef, int> keyValuePair in tmpDict)
            {
                int skVal = Mathf.Min(10, Mathf.RoundToInt(keyValuePair.Value * scaleVal));
                var passion = (Passion) Mathf.Min(passionDict.TryGetValue(keyValuePair.Key) / count, 2);
                SkillRecord sk = mSkills.GetSkill(keyValuePair.Key);
                sk.Level = skVal;
                sk.passion = passion;
            }
        }

        /// <summary>
        /// Transfers the quest relations from the original pawn onto the transfer pawn 
        /// </summary>
        /// <param name="original">The original.</param>
        /// <param name="transferPawn">The transfer pawn.</param>
        /// <exception cref="ArgumentNullException">
        /// original
        /// or
        /// transferPawn
        /// </exception>
        public static void TransferQuestRelations([NotNull] Pawn original, [NotNull] Pawn transferPawn)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (transferPawn == null) throw new ArgumentNullException(nameof(transferPawn));

            if (original.questTags != null)
            {
                transferPawn.questTags = transferPawn.questTags ?? new List<string>();
                foreach (string originalQuestTag in original.questTags)
                {
                    transferPawn.questTags.Add(originalQuestTag); 
                }
                original.questTags.Clear();
            }
            
            var qM = Find.QuestManager;
            
            
            
            foreach (Quest quest in qM.QuestsListForReading)
            {
                var qPs = quest.PartsListForReading;

                foreach (QuestPart questPart in qPs) questPart?.ReplacePawnReferences(original, transferPawn);
            }

        }

        /// <summary>
        /// tries to get the equivalent body part record in the other body def 
        /// </summary>
        /// <param name="record">The record.</param>
        /// <param name="otherDef">The other definition.</param>
        /// <returns>the equivalent body part record in the other body def if it exists, null otherwise</returns>
        /// <exception cref="ArgumentNullException">
        /// record
        /// or
        /// otherDef
        /// </exception>
        [CanBeNull]
        public static BodyPartRecord GetRecord([NotNull] BodyPartRecord record, [NotNull] BodyDef otherDef)
        {
            if (record == null) throw new ArgumentNullException(nameof(record));
            if (otherDef == null) throw new ArgumentNullException(nameof(otherDef));
            var pAddr = record.GetAddress();
            return otherDef.GetRecordAt(pAddr); 
        }

        /// <summary>
        /// Transfers the hediffs from pawn1 onto pawn2 
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <param name="selector">The selector.</param>
        /// <param name="transferFunc">The transfer function.</param>
        public static void TransferHediffs([NotNull] Pawn pawn1, [NotNull] Pawn pawn2,  [NotNull] Func<Hediff, bool> selector, [NotNull] Func<BodyPartRecord, BodyPartRecord> transferFunc)
        {
            if (pawn1 == null) throw new ArgumentNullException(nameof(pawn1));
            if (pawn2 == null) throw new ArgumentNullException(nameof(pawn2));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (transferFunc == null) throw new ArgumentNullException(nameof(transferFunc));

            var health1 = pawn1.health;
            var health2 = pawn2.health;

            var tHediffs = health1?.hediffSet?.hediffs?.Where(selector);
            foreach (Hediff hediff in tHediffs.MakeSafe())
            {

                BodyPartRecord otherRecord;
                if (hediff.Part == null) otherRecord = null;
                else
                    otherRecord = transferFunc(hediff.Part); 

                if(otherRecord == null && hediff.Part != null) continue;

                if(health2.hediffSet.HasHediff(hediff.def, otherRecord)) continue;
                
                var newHediff = HediffMaker.MakeHediff(hediff.def, pawn2, otherRecord);
                health2.AddHediff(newHediff); 


            }


        }

        /// <summary>
        /// Transfers the hediffs from pawn1 onto pawn2 
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <param name="selector">The selector.</param>
        /// <exception cref="ArgumentNullException">
        /// pawn1
        /// or
        /// pawn2
        /// </exception>
        public static void TransferHediffs([NotNull] Pawn pawn1, [NotNull] Pawn pawn2, [NotNull] Func<Hediff, bool> selector)
        {
            if (pawn1 == null) throw new ArgumentNullException(nameof(pawn1));
            if (pawn2 == null) throw new ArgumentNullException(nameof(pawn2));
            var otherDef = pawn2.RaceProps.body;

            TransferHediffs(pawn1, pawn2, selector, r => GetRecord(r, otherDef)); 

        }

        static bool DefaultThoughtSelector(Pawn pawn, Thought_Memory mem)
        {
            return mem.def.IsValidFor(pawn); 
        }


        /// <summary>
        /// Transfers thoughts from pawn1 onto pawn2.
        /// </summary>
        /// <param name="pawn1">The pawn to transfer thoughts from.</param>
        /// <param name="pawn2">The pawn to transfer thoughts onto.</param>
        /// <param name="selector">The selector function. default just checks that the memory is valid for pawn2</param>
        /// <exception cref="ArgumentNullException">
        /// pawn1
        /// or
        /// pawn2
        /// </exception>
        public static void TransferThoughts([NotNull] Pawn pawn1, [NotNull] Pawn pawn2,
                                            [CanBeNull] Func<Thought_Memory, bool> selector = null)
        {
            if (pawn1 == null) throw new ArgumentNullException(nameof(pawn1));
            if (pawn2 == null) throw new ArgumentNullException(nameof(pawn2));
            selector = selector ?? (t => DefaultThoughtSelector(pawn2, t)); 
            var thoughtHandler1 = pawn1.needs?.mood?.thoughts;
            var thoughtHandler2 = pawn2.needs?.mood?.thoughts;

            if (thoughtHandler2?.memories == null || thoughtHandler1?.memories == null) return;

            foreach (Thought_Memory memory in thoughtHandler1.memories.Memories.MakeSafe())
            {
                if(!selector(memory)) continue;

                var sameMemory = thoughtHandler2.memories.Memories.MakeSafe().FirstOrDefault(m => m.def == memory.def); 
                if(sameMemory != null) continue;

                var worker = memory.def?.modExtensions?.OfType<IThoughtTransferWorker>().FirstOrDefault();

                Thought_Memory newMemory;
                if (worker != null)
                {
                    if (!worker.ShouldTransfer(pawn1, pawn2, memory)) continue;
                    newMemory = worker.CreateNewThought(pawn1, pawn2, memory);
                }
                else
                {
                    newMemory = ThoughtMaker.MakeThought(memory.def, memory.CurStageIndex);
                }

                
                if(memory.otherPawn == null)
                    thoughtHandler2.memories.TryGainMemory(newMemory, memory.otherPawn);

                newMemory.age = memory.age;
                newMemory.moodPowerFactor = memory.moodPowerFactor; 
            }


        }


        /// <summary>
        ///     Transfers all transferable aspects from pawn1 to pawn2
        /// </summary>
        /// <param name="pawn1">The source pawn.</param>
        /// <param name="pawn2">The destination pawn.</param>
        public static void TransferAspects([NotNull] Pawn pawn1, [NotNull] Pawn pawn2)
        {
            AspectTracker originalTracker = pawn1.GetAspectTracker();
            AspectTracker animalTracker = pawn2.GetAspectTracker();
            if (originalTracker == null) return;
            if (animalTracker == null)
            {
                Log.Warning($"pawn {pawn2.Name},{pawn2.def.defName} does not have an aspect tracker");
                return;
            }


            foreach (Aspect aspect in originalTracker)
            {
                if (animalTracker.Contains(aspect.def, aspect.StageIndex))
                {
                    aspect.PostTransfer(animalTracker.GetAspect(aspect.def));
                    continue;
                }


                if (aspect.def.transferToAnimal)
                {
                    int stageIndex = aspect.StageIndex;
                    Aspect aAspect = animalTracker.GetAspect(aspect.def);
                    if (aAspect != null)
                    {
                        aAspect.StageIndex = stageIndex; //set the stage but do not re add it 
                        aspect.PostTransfer(aAspect);
                    }
                    else
                    {
                        Aspect newAspect = aspect.def.CreateInstance();
                        animalTracker.Add(newAspect, stageIndex); //add it if the animal does not have the aspect
                        aspect.PostTransfer(newAspect);
                    }
                }
            }
        }


        /// <summary>
        ///     Transfers the favor of all factions from pawn1 to pawn2
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     pawn1
        ///     or
        ///     pawn2
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
            _facScratchList.AddRange(rTracker1.AllTitlesForReading.MakeSafe()
                                              .Select(f => f.faction)
                                              .Distinct()); //make a copy so we can remove safely while transferring 


            foreach (Faction faction in _facScratchList)
            {
                int favor = rTracker1.GetFavor(faction);
                int favor2 = rTracker2.GetFavor(faction);
                if (favor2 >= favor) continue; //don't transfer if pawn2 already has a title equal or greater to this 
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
        ///     Transfers the relations from pawn1 to pawn2
        /// </summary>
        /// <param name="pawn1">The original.</param>
        /// <param name="pawn2">The animal.</param>
        /// <param name="predicate">optional predicate to dictate which relations get transferred</param>
        public static void TransferRelations([NotNull] Pawn pawn1, [NotNull] Pawn pawn2,
                                             Predicate<PawnRelationDef> predicate = null)
        {
            if (pawn1.relations == null) return;
            List<DirectPawnRelation> enumerator = pawn1.relations.DirectRelations.MakeSafe().ToList();
            predicate = predicate ?? (r => true); //if no filter is set, have it pass everything 
            foreach (DirectPawnRelation directPawnRelation in enumerator.Where(d => predicate(d.def)))
            {
                if (directPawnRelation.def.implied) continue;
                pawn1.relations?.RemoveDirectRelation(directPawnRelation); //make sure we remove the relations first 
                pawn2.relations?.AddDirectRelation(directPawnRelation.def,
                                                   directPawnRelation.otherPawn); //TODO restrict these to special relationships? 
            }

            foreach (Pawn pRelatedPawns in pawn1.relations.PotentiallyRelatedPawns.ToList()
            ) //make copies so we don't  invalidate the enumerator mid way through 
            foreach (PawnRelationDef pawnRelationDef in pRelatedPawns.GetRelations(pawn1).Where(d => predicate(d)).ToList())
            {
                if (pawnRelationDef.implied) continue;
                pRelatedPawns.relations.RemoveDirectRelation(pawnRelationDef, pawn1);
                pRelatedPawns.relations.AddDirectRelation(pawnRelationDef, pawn2);
            }
        }

        /// <summary>
        ///     Transfers the relations from pawn1 to pawn2
        /// </summary>
        /// <param name="pawn1">The original.</param>
        /// <param name="pawn2">The animal.</param>
        /// <param name="predicate">optional predicate to dictate which relations get transferred</param>
        public static void TransferRelations([NotNull] Pawn pawn1, [NotNull] Pawn pawn2, Predicate<DirectPawnRelation> predicate)
        {
            if (pawn1.relations == null) return;
            List<DirectPawnRelation> enumerator = pawn1.relations.DirectRelations.MakeSafe().ToList();
            foreach (DirectPawnRelation directPawnRelation in enumerator.Where(d => predicate?.Invoke(d) != false))
            {
                if (directPawnRelation.def.implied) continue;
                pawn1.relations?.RemoveDirectRelation(directPawnRelation); //make sure we remove the relations first 
                pawn2.relations?.AddDirectRelation(directPawnRelation.def,
                                                   directPawnRelation.otherPawn); //TODO restrict these to special relationships? 
            }

            foreach (Pawn pRelatedPawns in pawn1.relations.PotentiallyRelatedPawns.MakeSafe().ToList()
            ) //make copies so we don't  invalidate the enumerator mid way through 
            {
                Pawn_RelationsTracker rel2 = pRelatedPawns.relations;
                if (rel2 == null) continue;

                foreach (DirectPawnRelation pawnRelationDef in rel2
                                                              .DirectRelations.MakeSafe()
                                                              .Where(d => predicate?.Invoke(d) != false && d.otherPawn == pawn1)
                                                              .ToList())
                {
                    if (pawnRelationDef.def.implied) continue;
                    
                    rel2.RemoveDirectRelation(pawnRelationDef.def, pawn1);
                    rel2.AddDirectRelation(pawnRelationDef.def, pawn2);
                }
            }
        }


        /// <summary>
        ///     Transfers the relations from the original pawns to the given meld
        /// </summary>
        /// <param name="originals">The originals.</param>
        /// <param name="meld">The meld.</param>
        /// <param name="filter">The filter.</param>
        /// <exception cref="ArgumentNullException">
        ///     originals
        ///     or
        ///     meld
        /// </exception>
        public static void TransferRelations([NotNull] IReadOnlyList<Pawn> originals, [NotNull] Pawn meld,
                                             Func<PawnRelationDef, bool> filter = null)
        {
            if (originals == null) throw new ArgumentNullException(nameof(originals));
            if (meld == null) throw new ArgumentNullException(nameof(meld));
            Pawn_RelationsTracker mRelations = meld.relations;
            if (mRelations == null) return;

            bool InnerFilter(DirectPawnRelation relation, Pawn rPawn)
            {
                return !originals.Contains(rPawn) && filter?.Invoke(relation.def) != false;
            }


            foreach (Pawn original in originals) TransferRelations(original, meld, r => InnerFilter(r, original));
        }


        /// <summary>
        ///     Transfers skills from pawn1 to pawn2
        /// </summary>
        /// <param name="pawn1">The pawn1.</param>
        /// <param name="pawn2">The pawn2.</param>
        /// <param name="mode">The transfer mode.</param>
        /// <param name="passionTransferMode">The passion transfer mode.</param>
        public static void TransferSkills([NotNull] Pawn pawn1, [NotNull] Pawn pawn2,
                                          SkillTransferMode mode = SkillTransferMode.Set,
                                          SkillPassionTransferMode passionTransferMode = SkillPassionTransferMode.Ignore)
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
                int oldLevel = p2Skill.Level;
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

        /// <summary>
        ///     move all mutation related traits from the original pawn to the transformed pawn if they are sapient
        /// </summary>
        /// <param name="originalPawn">The original pawn.</param>
        /// <param name="transformedPawn">The transformed pawn.</param>
        /// <param name="selector">The selector function for determining if a trait should be transferred</param>
        /// <exception cref="ArgumentNullException">
        ///     transformedPawn
        ///     or
        ///     selector
        ///     or
        ///     originalPawn
        /// </exception>
        public static void TransferTraits([NotNull] Pawn originalPawn, [NotNull] Pawn transformedPawn,
                                          [NotNull] Func<TraitDef, bool> selector)
        {
            if (transformedPawn == null) throw new ArgumentNullException(nameof(transformedPawn));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            if (originalPawn == null) throw new ArgumentNullException(nameof(originalPawn));

            if (transformedPawn.story?.traits == null) return;
            if (originalPawn.story?.traits?.allTraits == null) return; 
            List<TraitDef>
                tTraits = originalPawn.story.traits.allTraits.Select(t => t.def)
                                      .Where(selector)
                                      .ToList(); //save it to a list to not invalidate the enumerator 
            foreach (TraitDef mutationTrait in tTraits)
            {
                Trait trait = originalPawn.story?.traits?.GetTrait(mutationTrait);
                if (trait == null) continue;
                var newTrait = new Trait(mutationTrait, trait.Degree, true);
                transformedPawn.story.traits.GainTrait(newTrait);
            }
        }


        [CanBeNull]
        private static SkillRecord TryGetSkill([NotNull] Pawn_SkillTracker tracker, [NotNull] SkillDef def)
        {
            foreach (SkillRecord skillRecord in tracker.skills.MakeSafe())
                if (skillRecord.def == def)
                    return skillRecord;

            return null;
        }
    }
}