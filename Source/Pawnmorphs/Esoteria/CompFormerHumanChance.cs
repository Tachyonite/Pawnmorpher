using Verse;

namespace Pawnmorph
{
    public class CompFormerHumanChance : ThingComp
    // A ThingComp that gives a Thing a chance to have the TransformedHuman hediff.
    {
        private bool triggered = false;

        public CompProperties_FormerHumanChance Props
        // Sets this ThingComp's Props to be that of CompProperties_FormerHumanChance's
        {
            get
            {
                return props as CompProperties_FormerHumanChance;
            }
        }

        public override void CompTick()
        // Every tick, check if the Thing needs to have the TransformHuman hediff set (There is probably a better method to put this in).
        {
            if (!triggered)
            // If we haven't checked already if the pawn is a former human...
            {
                triggered = true; // Set the flag so that we don't endlessly check.
                if (LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableWildFormers && Rand.RangeInclusive(0, 100) <= Props.Chance && parent.Faction == null)
                // Give the pawn a chance to be a former human if the setting is enabled and it doesn't already belong to a faction (i.e. a wild animal).
                {
                    TransformerUtility.AddHediffIfNotPermanentlyFeral(parent as Pawn, HediffDef.Named("TransformedHuman")); // Add the TransformedHuman hediff if the pawn is not permanently feral.
                }
            }
            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, HediffDef.Named("TransformedHuman")); // Remove the TransformedHuman hediff if the pawn becomes permanently feral.
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered)); 
        }
    }
}
