using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    public class CompFormerHumanChance : ThingComp
    {
        private bool triggered = false;

        public CompProperties_FormerHumanChance Props
        {
            get
            {
                return props as CompProperties_FormerHumanChance;
            }
        }

        public override void CompTick()
        {
            RandUtilities.PushState();

            if (!triggered)
            {
                triggered = true;
                if (LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableWildFormers && Rand.RangeInclusive(0, 100) <= Props.Chance && parent.Faction == null)
                {
                    TransformerUtility.AddHediffIfNotPermanentlyFeral(parent as Pawn, HediffDef.Named("TransformedHuman"));
                }
            }
            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, HediffDef.Named("TransformedHuman"));

            RandUtilities.PopState();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered)); 
        }
    }
}
