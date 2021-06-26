using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.SlurryNet;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.IncidentWorkers
{
    class LiquidSlurry : Filth
    {
        public const int DANGER_RADIUS = 5;
        private const float MUTAGENIC_BUILDUP_RATE = 0.02f;
        private const float EPSILON = 0.0001f;

        [NotNull] private static readonly RWRaycastHit[] _buffer;

        public override void TickRare()
        {
            if(this.IsHashIntervalTick(20)) 
                DoMutagenicBuildup();
        }

        static LiquidSlurry()
        {
            _buffer = new RWRaycastHit[20];
        }

        private void DoMutagenicBuildup()
        {
            IEnumerable<Thing> things = GenRadial.RadialDistinctThingsAround(Position, Map, DANGER_RADIUS, true).MakeSafe();
            MutagenDef mutagen = MutagenDefOf.defaultMutagen;
            foreach (Thing thing in things)
            {
                if (!(thing is Pawn pawn)) continue;
                if (!mutagen.CanInfect(pawn)) return;

                TryMutatePawn(pawn);
            }
        }

        private void TryMutatePawn(Pawn pawn)
        {

            var hits = RWRaycast.RaycastAllNoAlloc(Map, Position, pawn.Position, _buffer, RaycastTargets.Impassible);


            if (hits == 0)
            {
                MutagenicBuildupUtilities.AdjustMutagenicBuildup(def, pawn, MUTAGENIC_BUILDUP_RATE);
                return;
            }

            int tHits = 0;

            for (int i = 0; i < hits; i++)
            {
                if (_buffer[i].hitThing != this) tHits++;
            }

            var p0 = Position.ToIntVec2.ToVector3(); //need to get rid of y which may be different but shouldn't be taken into account 
            var p1 = pawn.Position.ToIntVec2.ToVector3();
            var dist = (p0 - p1).magnitude + tHits * 1.5f;

            float rate = MUTAGENIC_BUILDUP_RATE / (Mathf.Pow(2, dist));


            if (rate <= EPSILON) return;

            MutagenicBuildupUtilities.AdjustMutagenicBuildup(def, pawn, MUTAGENIC_BUILDUP_RATE);

        }


    }
}
