using System;
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
        public static HediffDef hediff = TfDefOf.FullRandomTF; //def of classes are better, pushes errors to load time not play time 
                
        public override void TickRare()
        {
            if (Spawned && LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableMutagenMeteor)
            {
                IEnumerable<Verse.Thing> enumerable = GenRadial.RadialDistinctThingsAround(Position, Map, 2.0f, true);
                List<Pawn> pawnList = enumerable.OfType<Pawn>().ToList(); //don't need to keep the non pawns 

                if (pawnList.Count == 0) return; 

                foreach (Pawn pawn in pawnList)
                {
                    if (!this.touchingPawns.Contains(pawn) && !pawn.health.hediffSet.HasHediff(TfDefOf.StabiliserHigh))
                    {
                        touchingPawns.Add(pawn);
                    }
                }
                for (int j = 0; j < touchingPawns.Count; j++)
                {
                    Pawn pawn2 = touchingPawns[j];
                    if (!pawn2.Spawned || !pawnList.Contains(pawn2))
                    {
                        touchingPawns.Remove(pawn2);
                        var targetHediff = pawn2.health.hediffSet.hediffs.FirstOrDefault(x => x.def == hediff); //need to use FirstOrDefault, First throws InvalidOperationException when run on an empty enumerable
                        if ( targetHediff != null) 
                        {
                            pawn2.health.RemoveHediff(targetHediff);
                        }
                    }
                    else
                    {
                        if (pawn2.RaceProps.intelligence == Intelligence.Humanlike && pawn2.health.immunity?.GetImmunity(hediff) < 1f)
                        {
                            pawn2.health.AddHediff(hediff);
                            IntermittentMagicSprayer.ThrowMagicPuffDown(pawn2.Position.ToVector3(), pawn2.Map);
                        }
                    }
                }
            }
        }
    }
}
