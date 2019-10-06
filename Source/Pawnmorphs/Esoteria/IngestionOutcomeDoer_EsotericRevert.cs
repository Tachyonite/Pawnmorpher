using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Hybrids;
using Pawnmorph.TfSys;
using Pawnmorph.GraphicSys;
using Pawnmorph.Utilities;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_EsotericRevert : IngestionOutcomeDoer
    {
        public float mtbDays;
        public List<HediffDef> defsToRevert;
        public List<HediffDef> revertThoughts;
        public List<HediffDef> mergeRevertThoughts;
        public List<MutagenDef> blackList = new List<MutagenDef>();
        public string transformedHuman = "TransformedHuman";

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            var comp = Find.World.GetComponent<PawnmorphGameComp>();
            Tuple<TransformedPawn, TransformedStatus> tuple = comp.GetTransformedPawnContaining(pawn);

            foreach (MutagenDef mutagenDef in DefDatabase<MutagenDef>.AllDefs)
            {
                if (blackList.Contains(mutagenDef))
                    return; //make it so this reverted can not revert certain kinds of transformations 

                if (mutagenDef.MutagenCached.TryRevert(pawn))
                {
                    TransformedPawn inst = tuple?.First;
                    if (inst != null) comp.RemoveInstance(inst);

                    return;
                }
            }

            TransformerUtility.RemoveAllMutations(pawn);
            MorphGraphicsUtils.RefreshGraphics(pawn);
            var aT = pawn.GetAspectTracker();
            if (aT != null) RemoveAspects(aT);
        }

        private void RemoveAspects(AspectTracker tracker)
        {
            foreach (Aspect aspect in tracker)
            {
                if (aspect.def.removedByReverter) tracker.Remove(aspect); //it's ok to remove them in a foreach loop 
            }
        }
    }
}
