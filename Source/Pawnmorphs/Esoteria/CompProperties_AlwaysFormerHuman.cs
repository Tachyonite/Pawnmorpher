using Verse;

namespace Pawnmorph
{
    public class CompProperties_AlwaysFormerHuman : CompProperties
    {
        public HediffDef hediff;

        public CompProperties_AlwaysFormerHuman()
        {
            compClass = typeof(CompAlwaysFormerHuman);
        }
    }
}
