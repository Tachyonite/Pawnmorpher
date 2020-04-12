// IngestionOutcomeDooer_Productive.cs modified by Iron Wolf for Pawnmorph on 10/05/2019 1:04 PM
// last updated 10/05/2019  1:04 PM

using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// ingestion out come doer that adds an aspect to a pawn
    /// </summary>
    /// <seealso cref="RimWorld.IngestionOutcomeDoer" />
    public class IngestionOutcomeDoer_AddRandomAspect : IngestionOutcomeDoer
    {
        /// <summary>The aspects to add</summary>
        public List<AspectDef> aspects = new List<AspectDef>();

        /// <summary>Does the ingestion outcome special.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="ingested">The ingested.</param>
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (aspects.Count == 0)
                return;

            var aspectT = pawn.GetAspectTracker();
            if (aspectT == null) return;

            try
            {
                Rand.PushState(pawn.thingIDNumber ^ 10224);
                int aspectIndex = (int)Math.Floor(Rand.Value * aspects.Count);
                AspectDef aspectDef = aspects[aspectIndex];

                var aspect = aspectT.GetAspect(aspectDef);
                if (aspect == null)
                {
                    aspectT.Add(aspectDef);
                }
            }
            finally
            {
                Rand.PopState();
            }
        }
    }
}
