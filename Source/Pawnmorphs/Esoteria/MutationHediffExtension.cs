// MutationHediffExtension.cs created by Iron Wolf for Pawnmorph on 09/15/2019 8:44 PM
// last updated 09/15/2019  8:44 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     Mod def extension for applying to hediffs to manually assign them a part without using a hediff giver. <br />
    ///     Note, the results will be additive, so if a mutation has this extension and is in one or more hediff givers,
    ///     the total set of parts it can be applied to is the union of this extension and the givers
    /// </summary>
    public class MutationHediffExtension : DefModExtension
    {
        /// <summary>
        ///     list of body parts this mutation can be added to
        /// </summary>
        /// note: this does not affect HediffGiver_AddedMutation, this is for adding mutations without a hediff giver
        public List<BodyPartDef> parts = new List<BodyPartDef>();

        /// <summary>the number of parts to add this mutation to</summary>
        public int countToAffect;

        /// <summary>The generation cost of this mutation. the better the mutation the higher the cost should be</summary>
        public int generationCost=1; //TODO make some sort of report or log thing to try and figure out the best costs for the mutations 

        /// <summary>
        ///     the various mutation categories this mutation belongs to
        /// </summary>
        public List<MutationCategoryDef> categories = new List<MutationCategoryDef>();

        /// <summary>
        ///     the rule pack to use when generating mutation logs for this mutation
        /// </summary>
        [CanBeNull] public RulePackDef mutationLogRulePack;

        /// <summary>Get all Configuration Errors with this instance</summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            if (generationCost <= 0)
            {
                yield return $"generationCost:{{{generationCost}}} must be greater then zero"; 
            }
        }
    }
}