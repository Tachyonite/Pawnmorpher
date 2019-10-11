using Verse;

namespace Pawnmorph
{
    public class CompAlwaysFormerHuman : ThingComp
    {
        private bool triggered = false;

        public CompProperties_AlwaysFormerHuman Props
        {
            get
            {
                return props as CompProperties_AlwaysFormerHuman;
            }
        }

        public override void CompTick()
        {
            if (!triggered)
            {
                triggered = true;
                TransformerUtility.AddHediffIfNotPermanentlyFeral(parent as Pawn, Props.hediff);
            }
            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, Props.hediff);
        }
    }
}
