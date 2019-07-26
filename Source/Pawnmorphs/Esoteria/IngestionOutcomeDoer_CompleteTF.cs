using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDoer_CompleteTF : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs) // Loop through all the hediffs on the pawn.
            {
                if (hediff is Hediff_Morph morph && morph.CurStage == morph.def.stages[0]) // When you find one that is a pawnmorph in the final stage...
                {
                    foreach (HediffStage stage in morph.def.stages) // ...loop through its stages...
                    {
                        foreach (HediffGiver giver in stage.hediffGivers) // ...and their hediffGivers...
                        {
                            if (giver is HediffGiver_TF giverTF) // ...until you find one that is of type HediffGiver_TF.
                            {
                                TransformerUtility.Transform(pawn, morph, giverTF.hediff, giverTF.pawnkind, giverTF.tale); // When you do, use it's infor to transform the pawn.
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
