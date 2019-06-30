using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;
using static RimWorld.MoteMaker;
using RimWorld;

namespace Pawnmorph
{
    public class MutagenicStone : Mineable
    {

        private List<Pawn> touchingPawns = new List<Pawn>();

        public static HediffDef hediff = HediffDef.Named("FullRandomTF");
                
        public override void TickRare()
        {

            if (base.Spawned && LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableMutagenMeteor)
            {
                IEnumerable<Verse.Thing> enumerable = GenRadial.RadialDistinctThingsAround(base.Position, base.Map, 2.0f, true);
                List<Thing> thingList = enumerable.ToList();


                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null && !this.touchingPawns.Contains(pawn))
                    {
                        this.touchingPawns.Add(pawn);
                    }
                }
                for (int j = 0; j < this.touchingPawns.Count; j++)
                {
                    Pawn pawn2 = this.touchingPawns[j];
                    if (!pawn2.Spawned || !thingList.Contains(pawn2))
                    {
                        this.touchingPawns.Remove(pawn2);
                        if (pawn2.health.hediffSet.hediffs.Where(x => x.def == hediff).First() != null)
                        {
                            pawn2.health.RemoveHediff(pawn2.health.hediffSet.hediffs.Where(x => x.def == hediff).First());
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
