using System.Collections.Generic;
using Pawnmorph.GraphicSys;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     ingestion outcome dooer for the reverter serum. reverts transformed pawns to their original state 
    /// </summary>
    /// <seealso cref="RimWorld.IngestionOutcomeDoer" />
    public class IngestionOutcomeDoer_EsotericRevert : IngestionOutcomeDoer
    {
        /// <summary>The black list of mutagens this instance cannot revert</summary>
        public List<MutagenDef> blackList = new List<MutagenDef>();

        /// <summary>Does the ingestion outcome special.</summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="ingested">The ingested.</param>
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            var gComp = Find.World.GetComponent<PawnmorphGameComp>();

            var status = gComp.GetPawnStatus(pawn);

            if (status == TransformedStatus.Transformed)
            {
                //revert transformations
                foreach (MutagenDef mutagenDef in DefDatabase<MutagenDef>.AllDefs)
                {
                    if (blackList.Contains(mutagenDef))
                        return; // Make it so this reverted can not revert certain kinds of transformations.
                    if (mutagenDef.MutagenCached.TryRevert(pawn))
                    {
                        return;
                    }
                }
            }
            else 
            {
                //revert mutations
                TransformerUtility.SetToRaceDefault(pawn);
                AspectTracker aT = pawn.GetAspectTracker();
                if (aT != null) RemoveAspects(aT);

                GraphicsUpdaterComp graphicsComp = pawn.GetComp<GraphicsUpdaterComp>();
                if (graphicsComp != null)
                    graphicsComp.IsDirty = true;
                pawn.RefreshGraphics();
            }
        }

        private void RemoveAspects(AspectTracker tracker)
        {
            foreach (Aspect aspect in tracker)
                if (aspect.def.removedByReverter)
                    tracker.Remove(aspect); // It's ok to remove them in a foreach loop.
        }
    }
}