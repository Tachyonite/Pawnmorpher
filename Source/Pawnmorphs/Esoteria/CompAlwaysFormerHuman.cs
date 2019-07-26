using Verse;

namespace Pawnmorph
{
    public class CompAlwaysFormerHuman : ThingComp
    // A ThingComp that sets the Thing to always have the TransformedHuman hediff.
    {
        private bool triggered = false;

        public CompProperties_AlwaysFormerHuman Props
        // Sets this ThingComp's Props to be that of CompProperties_AlwaysFormerHuman's
        {
            get
            {
                return props as CompProperties_AlwaysFormerHuman;
            }
        }

        public override void CompTick()
        // Every tick, check to see if the pawn needs to have certain hediffs applied or removed (There must be a better function for doing this).
        {
            if (!triggered)
            // If we haven't already checked to see if the pawn has the props' hediff...
            {
                triggered = true; // Set the flag so we don't constantly check for this.
                TransformerUtility.AddHediffIfNotPermanentlyFeral(parent as Pawn, Props.hediff); // If the pawn is not already permanently feral, add the props' hediff.
            }
            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, Props.hediff); // If the pawn goes permanently feral, remove the hediff.
        }
    }
}
