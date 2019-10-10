// MutationLogEntry.cs created by Iron Wolf for Pawnmorph on 10/09/2019 12:03 PM
// last updated 10/09/2019  12:03 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.Grammar;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
    /// <summary>
    ///     log entry for when a pawn mutates
    /// </summary>
    public class MutationLogEntry : LogEntry
    {
        // Rule pack constants.
        private const string PAWN_IDENTIFIER = "PAWN";
        private const string MUTATION_IDENTIFIER = "MUTATION";
        private const string RP_ROOT_RULE = "mutation_log";
        private const string PART_LABEL = "PART";

        private HediffDef _mutationDef;
        [CanBeNull] private TaleDef _mutationTale;
        private List<BodyPartDef> _mutatedRecords;
        private Pawn _pawn;

        public MutationLogEntry() : base(null)
        {
        }

        public MutationLogEntry(Pawn pawn, [CanBeNull] TaleDef taleDef, HediffDef mutationDef, IEnumerable<BodyPartDef> mutatedParts)
        {
            _mutatedRecords = mutatedParts.ToList();
            _pawn = pawn;
            _mutationTale = taleDef;
            _mutationDef = mutationDef;
        }

        public override bool Concerns(Thing t)
        {
            return t == _pawn;
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref _mutationTale, nameof(_mutationTale));
            Scribe_Defs.Look(ref _mutationDef, nameof(_mutationDef));
            Scribe_Collections.Look(ref _mutatedRecords, nameof(_mutatedRecords), LookMode.Def);
            Scribe_References.Look(ref _pawn, nameof(_pawn));

            if (Scribe.mode == LoadSaveMode.PostLoadInit) _mutatedRecords = _mutatedRecords ?? new List<BodyPartDef>();
        }

        public override IEnumerable<Thing> GetConcerns()
        {
            yield return _pawn;
        }

        public override string GetTipString()
        {
            return $"{_mutationDef.LabelCap}";
        }

        /// <summary> Returns a string that represents the current object. </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
        {
            return $"{_pawn.Name}: {string.Join(",", _mutatedRecords.Select(r => r.LabelCap).ToArray())} -> {_mutationDef.LabelCap}";
        }

        protected override string ToGameStringFromPOV_Worker(Thing pov, bool forceLog)
        {
            Assert(pov == _pawn, "pov == _pawn");

            Rand.PushState(logID); // Does not need a MP-safe seed.
            try
            {
                GrammarRequest grammarRequest = GenerateGrammarRequest();

                var rulePack = _mutationDef.GetModExtension<MutationHediffExtension>()?.mutationRulePack ?? PMRulePackDefOf.DefaultMutationRulePack;

                grammarRequest.Includes.Add(rulePack);
                IEnumerable<Rule> pawnR = GrammarUtility.RulesForPawn(PAWN_IDENTIFIER, _pawn, grammarRequest.Constants);

                BodyPartRecord partR = BodyDefOf.Human.AllParts.Where(r => _mutatedRecords.Contains(r.def)).RandomElement();
                var partRules = GrammarUtility.RulesForBodyPartRecord(PART_LABEL, partR);
                IEnumerable<Rule> mutR = GrammarUtility.RulesForHediffDef(MUTATION_IDENTIFIER, _mutationDef, partR);


                // Add the rules.
                grammarRequest.Rules.AddRange(pawnR);
                grammarRequest.Rules.AddRange(mutR);
                grammarRequest.Rules.AddRange(partRules);
                return GrammarResolver.Resolve(RP_ROOT_RULE, grammarRequest, "mutation log", forceLog);
            }
            catch (Exception exception)
            {
                Log.Error($"encountered {exception.GetType().Name} exception while generating string for mutation log\n\t{exception}");
            }
            finally
            {
                Rand.PopState(); // Make sure to always pop rand.
            }

            return _mutationDef.LabelCap; //TODO generate string 
        }
    }
}