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
    /// <summary>
    /// Class for the slurry puddles that can spawn with specific mutagenic explosions.
    /// </summary>
    class LiquidSlurry : Filth
    {
        private const float DANGER_RADIUS = 2f;
        private const float MUTAGENIC_BUILDUP_RATE = 0.015f;
        private const float EPSILON = 0.0001f;
        private const float MTTH_PLANT_MUTATE = 250;
        private static readonly float _p;

        [NotNull] private static readonly RWRaycastHit[] _buffer;

        static LiquidSlurry()
        {
            _p = RandUtilities.GetUniformProbability(MTTH_PLANT_MUTATE, 4.16f); //one long tick is ~4.16 seconds
            _buffer = new RWRaycastHit[20];
        }

        public override void TickRare()
        {
            IEnumerable<Thing> things = GenRadial.RadialDistinctThingsAround(Position, Map, DANGER_RADIUS, true);
            MutagenDef mutagen = MutagenDefOf.defaultMutagen;

            foreach (Thing thing in things)
            {
                if (thing is Pawn pawn && mutagen.CanInfect(pawn))
                    TryMutatePawn(pawn);
                else if (thing is Plant plant)
                {
                    if (Rand.Value >= _p) continue;
                        PMPlantUtilities.TryMutatePlant(plant);
                }
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
