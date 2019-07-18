using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
    public class HediffCompProperties_Remove : HediffCompProperties
    {
        public List<HediffDef> makeImmuneTo;

        public HediffCompProperties_Remove()
        {
            compClass = typeof(HediffComp_Remove);
        }
    }
}
