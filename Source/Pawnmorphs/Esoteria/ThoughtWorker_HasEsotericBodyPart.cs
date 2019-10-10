using System.Collections.Generic;
using Verse;
using RimWorld;

namespace Pawnmorph
{
    public class ThoughtWorker_HasEsotericBodyPart : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            HediffSet hs = p.health.hediffSet;
            int num = 0;
            List<Hediff> hediffs = hs.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[i] is Hediff_AddedMutation)
                {
                    num++;
                }
            }

            if (num > 0)
            {
                return ThoughtState.ActiveAtStage(num - 1);
            }
            return false;
        }
    }
}
