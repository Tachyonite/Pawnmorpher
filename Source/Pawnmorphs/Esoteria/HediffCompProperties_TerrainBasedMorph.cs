using Verse;

namespace Pawnmorph
{
    public class HediffCompProperties_TerrainBasedMorph : HediffCompProperties
    {
        public HediffDef hediffDef = null;
        public TerrainDef terrain = null;

        public HediffCompProperties_TerrainBasedMorph()
        {
            compClass = typeof(HediffComp_TerrainBasedMorph);
        }
    }
}
