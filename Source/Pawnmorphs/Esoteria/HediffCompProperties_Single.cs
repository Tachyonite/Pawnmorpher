using Verse;

namespace Pawnmorph
{
    public class HediffCompProperties_Single : HediffCompProperties
    {
        public int maxStacks = 10; 

        public HediffCompProperties_Single()
        {
            compClass = typeof(HediffComp_Single);
        }
    }
}
