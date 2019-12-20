using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// comp for adding the former human hediff with a percent chance when this thing spawns 
    /// </summary>
    public class CompFormerHumanChance : ThingComp
    {
        private bool triggered = false;

        /// <summary>
        /// the properties for this comp 
        /// </summary>
        public CompProperties_FormerHumanChance Props
        {
            get
            {
                return props as CompProperties_FormerHumanChance;
            }
        }

        /// <summary>
        /// called every tick after it's parent updates 
        /// </summary>
        public override void CompTick()
        {
            RandUtilities.PushState();

            if (!triggered)
            {
                triggered = true;
                if (LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableWildFormers && Rand.RangeInclusive(0, 100) <= Props.Chance && parent.Faction == null)
                {
                    float sL = Rand.Value;
                    FormerHumanUtilities.MakeAnimalSapient((Pawn) parent, sL); 
                }
            }
            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, HediffDef.Named("TransformedHuman"));

            RandUtilities.PopState();
        }

        /// <summary>
        /// called to save/load data 
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered)); 
        }
    }
}
