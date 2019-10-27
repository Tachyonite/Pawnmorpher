using Verse;

namespace Pawnmorph
{
    public class CompProperties_MutagenicRadius : CompProperties
    {
        public SimpleCurve radiusPerDayCurve;
        public HediffDef hediff;
        public float harmFrequencyPerArea = 1f;

        public CompProperties_MutagenicRadius()
        {
            compClass = typeof(CompMutagenicRadius);
        }
    }
}
