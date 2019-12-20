using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// comp for ensuring the parent always spawns with the former human hediff 
    /// </summary>
    public class CompAlwaysFormerHuman : ThingComp
    {
        private bool triggered = false;


        CompProperties_AlwaysFormerHuman Props
        {
            get
            {
                return props as CompProperties_AlwaysFormerHuman;
            }
        }

        /// <summary>
        /// called every tick after it's parent updates 
        /// </summary>
        public override void CompTick()
        {
            if (!triggered)
            {
                triggered = true;
                float sL = Rand.Value;
                FormerHumanUtilities.MakeAnimalSapient((Pawn)parent, sL);
            }
            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, Props.hediff);
        }
    }
}
