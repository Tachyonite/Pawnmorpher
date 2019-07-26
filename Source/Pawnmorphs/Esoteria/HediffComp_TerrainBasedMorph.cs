using Verse;

namespace Pawnmorph
{
    public class HediffComp_TerrainBasedMorph : HediffComp
    {
        public HediffCompProperties_TerrainBasedMorph Props
        {
            get
            {
                return (HediffCompProperties_TerrainBasedMorph)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            if (parent.pawn.Position.GetTerrain(parent.pawn.Map) == Props.terrain)
            {
                Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, parent.pawn, null);
                hediff.Severity = 1f;
                parent.pawn.health.AddHediff(hediff, null, null, null);
            }
        }
    }
}
