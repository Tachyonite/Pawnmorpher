using System.Collections.Generic;
using System.Linq;
using Verse;
using Pawnmorph;

namespace EtherGun
{
    public static class TransformPawn
    {
        public static void ApplyHediff(Pawn pawn, Map map, HediffDef hediff, float chance)
        {
            var rand = Rand.Value;
            if (rand <= chance)
            {
                var etherOnPawn = pawn.health?.hediffSet?.GetFirstHediffOfDef(hediff);
                var randomSeverity = 1f;
                if (etherOnPawn != null)
                {
                    etherOnPawn.Severity += randomSeverity;
                }
                else
                {
                    Hediff hediffOnPawn = HediffMaker.MakeHediff(hediff, pawn);
                    hediffOnPawn.Severity = randomSeverity;
                    pawn.health.AddHediff(hediffOnPawn);
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), map);
                }
            }
        }

        public static void ApplyHediff(List<Pawn> pawns, Map map, HediffDef hediff, float chance)
        {
            for (int i = 0; i < pawns.Count(); i++)
            {
                ApplyHediff(pawns[i], map, hediff, chance);
            }
        }
    }
}
