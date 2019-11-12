// Mutanite_Bed.cs created by Iron Wolf for Pawnmorph on 11/12/2019 11:06 AM
// last updated 11/12/2019  11:06 AM

using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// class for a mutanite bed 
    /// </summary>
    /// <seealso cref="RimWorld.Building_Bed" />
    public class Mutanite_Bed : Building_Bed
    {
        private const float SEVERITY_PER_LONG_TICK = 0.008f;

        float GetSeverityOffset(Pawn pawn)
        {
            if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return 0;
            return SEVERITY_PER_LONG_TICK * pawn.GetStatValue(StatDefOf.ToxicSensitivity); 
        }

        private const float EPSILON = 0.00001f;

        /// <summary>called every 2000 ticks</summary>
        public override void TickLong()
        {
            foreach (Pawn curOccupant in CurOccupants)
            {
                float sevOffset = GetSeverityOffset(curOccupant); 
                if(sevOffset < EPSILON) continue;
                HealthUtility.AdjustSeverity(curOccupant, MorphTransformationDefOf.MutagenicBuildup, sevOffset); 
            }
        }
    }
}