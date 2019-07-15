using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Pawnmorph;

namespace EtherGun
{
    //Custom class to allow me to re-use code. Made it static so I didn't have to instantiate it every time I wanted to use it.
    public static class TransformPawn
    {
        //Applies a hediff to a single pawn.
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

        //Overridde to apply hediffs to a list of pawns.
        public static void ApplyHediff(List<Pawn> pawns, Map map, HediffDef hediff, float chance)
        {
            for (int i = 0; i < pawns.Count(); i++)
            {
                ApplyHediff(pawns[i], map, hediff, chance);
            }
        }
    }
}
