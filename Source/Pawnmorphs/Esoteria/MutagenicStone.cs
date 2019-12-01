using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Pawnmorph.Hediffs;

namespace Pawnmorph
{
    /// <summary>
    /// class for mutanite 
    /// </summary>
    /// <seealso cref="RimWorld.Mineable" />
    public class MutagenicStone : Mineable
    {
        
        /// <summary>called every once and a while</summary>
        public override void TickRare()
        {
            if (!Spawned || !LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableMutagenMeteor) return;

            IEnumerable<Thing> enumerable = GenRadial.RadialDistinctThingsAround(Position, Map, 2.0f, true);
            var pawnList = enumerable.OfType<Pawn>(); // Don't need to keep the non-pawns.

            var mutagen = MutagenDefOf.defaultMutagen;
            foreach (Pawn pawn in pawnList)
            {
                HediffSet hediffSet = pawn.health.hediffSet;

                if (hediffSet.HasHediff(MorphTransformationDefOf.StabiliserHigh)) continue;
                if (!pawn.Spawned) continue;
                if (!mutagen.CanInfect(pawn)) continue;

                if (pawn.health.immunity?.GetImmunity(MorphTransformationDefOf.FullRandomTF) < 1f)
                {
                    pawn.health.AddHediff(MorphTransformationDefOf.FullRandomTF);
                    IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.Map);
                }
            }
        }
    }
}
