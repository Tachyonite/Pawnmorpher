using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     comp for adding the former human hediff with a percent chance when this thing spawns
    /// </summary>
    public class CompFormerHumanChance : ThingComp
    {
        private bool triggered = false;

        /// <summary>
        ///     the properties for this comp
        /// </summary>
        public CompProperties_FormerHumanChance Props => props as CompProperties_FormerHumanChance;

        /// <summary>
        ///     called every tick after it's parent updates
        /// </summary>
        public override void CompTick()
        {
            RandUtilities.PushState();

            if (!triggered)
            {
                triggered = true;
                if (CanBeFormerHuman() && Rand.RangeInclusive(0, 100) <= Props.Chance)
                {
                    float sL = Rand.Value;
                    FormerHumanUtilities.MakeAnimalSapient((Pawn) parent, sL);
                    FormerHumanUtilities.NotifyRelatedPawnsFormerHuman((Pawn) parent,
                                                                       FormerHumanUtilities.RELATED_WILD_FORMER_HUMAN_LETTER,
                                                                       FormerHumanUtilities
                                                                          .RELATED_WILD_FORMER_HUMAN_LETTER_LABEL);
                }
            }

            TransformerUtility.RemoveHediffIfPermanentlyFeral(parent as Pawn, HediffDef.Named("TransformedHuman"));

            RandUtilities.PopState();
        }

        /// <summary>
        ///     called to save/load data
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered));
        }


        private bool CanBeFormerHuman()
        {
            if (!LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableWildFormers) return false;
            if (parent.Faction != null) return false;
            var pawn = (Pawn) parent;
            if (pawn.relations == null) return true;
            if (pawn.relations.DirectRelations.Any(r => r.def == PawnRelationDefOf.Child)) return false;
            return true;
        }
    }
}