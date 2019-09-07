﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;
using Multiplayer.API;
using Pawnmorph.Hediffs;

namespace Pawnmorph
{
    public class MutagenicStone : Mineable
    {
        private List<Pawn> touchingPawns = new List<Pawn>();
        public static HediffDef hediff = MorphTransformationDefOf.FullRandomTF; //def of classes are better, pushes errors to load time not play time 
                
        public override void TickRare()
        {
            if (!Spawned
             || !LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableMutagenMeteor) return;



            IEnumerable<Verse.Thing> enumerable = GenRadial.RadialDistinctThingsAround(Position, Map, 2.0f, true);
            var pawnList = enumerable.OfType<Pawn>(); //don't need to keep the non pawns 

            //if (pawnList.Count == 0) return;
            var mutagen = MutagenDefOf.defaultMutagen; 
            foreach (Pawn pawn in pawnList)
            {
                HediffSet hediffSet = pawn.health.hediffSet;
                if (hediffSet.HasHediff(MorphTransformationDefOf.FullRandomTF) || hediffSet.HasHediff(MorphTransformationDefOf.StabiliserHigh))
                {
                    continue;
                }

                if(!pawn.Spawned) continue;
                if(!mutagen.CanInfect(pawn)) continue;
                if (pawn.health.immunity?.GetImmunity(MorphTransformationDefOf.FullRandomTF) < 1f)
                {
                    pawn.health.AddHediff(MorphTransformationDefOf.FullRandomTF);
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.Map);
                }

            }



            //foreach (Pawn pawn in pawnList)
            //{
            //    if (!this.touchingPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(MorphTransformationDefOf.StabiliserHigh))
            //    {
            //        touchingPawns.Add(pawn);
            //    }
            //}
            //for (int j = touchingPawns.Count - 1; j > 0; j--)
            //{
            //    Pawn pawn2 = touchingPawns[j];
            //    if (!pawn2.Spawned || !pawnList.Contains(pawn2))
            //    {
            //        touchingPawns.Remove(pawn2);
            //        var targetHediff = pawn2.health.hediffSet.hediffs.FirstOrDefault(x => x.def == hediff); //need to use FirstOrDefault, First throws InvalidOperationException when run on an empty enumerable
            //        if ( targetHediff != null) 
            //        {
            //            pawn2.health.RemoveHediff(targetHediff);
            //        }
            //    }
            //    else
            //    {
            //        if (pawn2.RaceProps.intelligence == Intelligence.Humanlike && pawn2.health.immunity?.GetImmunity(hediff) < 1f)
            //        {
            //            pawn2.health.AddHediff(hediff);
            //            IntermittentMagicSprayer.ThrowMagicPuffDown(pawn2.Position.ToVector3(), pawn2.Map);
            //        }
            //    }
            //}
        }
    }
}
