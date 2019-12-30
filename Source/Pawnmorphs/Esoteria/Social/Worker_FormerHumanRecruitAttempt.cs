// Worker_FormerHumanRecruitAttempt.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 8:59 AM
// last updated 12/22/2019  8:59 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Social
{
    /// <summary>
    /// interaction worker for recruiting former humans 
    /// </summary>
    /// <seealso cref="RimWorld.InteractionWorker_RecruitAttempt" />
    public class Worker_FormerHumanRecruitAttempt : InteractionWorker_RecruitAttempt
    {
        [NotNull]
        private static readonly List<RulePackDef> _scratchList = new List<RulePackDef>();

        /// <summary>
        ///  called when the initiator interacts with the specified recipient.
        /// </summary>
        /// <param name="initiator">The initiator.</param>
        /// <param name="recipient">The recipient.</param>
        /// <param name="extraSentencePacks">The extra sentence packs.</param>
        /// <param name="letterText">The letter text.</param>
        /// <param name="letterLabel">The letter label.</param>
        /// <param name="letterDef">The letter definition.</param>
        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel,
                                        out LetterDef letterDef)
        {
            base.Interacted(initiator, recipient, extraSentencePacks, out letterText, out letterLabel, out letterDef);
            if (extraSentencePacks == null) return;
            var sapientLevel = recipient?.GetQuantizedSapienceLevel();
            if (sapientLevel == null) return;
            RulePackDef variant;
            if (extraSentencePacks.Count != 0)
            {
                _scratchList.Clear();
                _scratchList.AddRange(extraSentencePacks);
                extraSentencePacks.Clear(); //we need to substitute any variants if any exist 

                foreach (RulePackDef rulePackDef in _scratchList)
                {
                    
                    if (rulePackDef.TryGetSapientDefVariant(recipient, out variant))
                    {
                        
                        extraSentencePacks.Add(variant); //add the variant if a variant is found 
                    }
                    else
                    {
                        extraSentencePacks.Add(rulePackDef); //add the original if no variant is found 
                    }
                }
            }

            //now get additions from the interaction def 
            if (interaction.TryGetSapientDefVariant(recipient, out variant))
            {
                extraSentencePacks.Add(variant); 
            }

            if (recipient.Faction == Faction.OfPlayer)
            {
                recipient.TryGainMemory(PMThoughtDefOf.FormerHumanTameThought);
            }

        }


        /// <summary>
        ///     gets the selection weight
        /// </summary>
        /// <param name="initiator"></param>
        /// <param name="recipient"></param>
        /// <returns></returns>
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (!interaction.IsValidFor(recipient)) return 0;

            return base.RandomSelectionWeight(initiator, recipient);
        }
    }
}