// IngestionOutcomeDoer_BoostSeverity.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 09/11/2019 2:38 PM
// last updated 09/11/2019  2:38 PM

using System;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.Noise;

namespace Pawnmorph
{
    /// <summary>
    /// ingestion outcome doer for adding severity to specific hediffs 
    /// </summary>
    public class IngestionOutcomeDoer_BoostSeverity : IngestionOutcomeDoer
    {
        public Filter<HediffDef> hediffFilter = new Filter<HediffDef>();
        public Filter<Type> hediffTypes = new Filter<Type>();
        public bool mustPassAll; //if a hediff must pass through all filters, otherwise they must pass through any filter 

        public float severityToAdd; 
        bool PassesFilters(Hediff hediff)
        {
            var pass = hediffFilter.PassesFilter(hediff.def);
            if (pass && !mustPassAll) return true;
            if (mustPassAll && !pass) return false;
            return hediffTypes.PassesFilter(hediff.GetType()); 
        }


        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            var hediffs = pawn.health.hediffSet.hediffs.Where(PassesFilters);
            foreach (var hediff in hediffs)
            {
                hediff.Severity += severityToAdd; 
            }
        }
    }
}