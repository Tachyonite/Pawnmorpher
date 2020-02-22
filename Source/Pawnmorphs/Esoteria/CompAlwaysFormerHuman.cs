using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     comp for ensuring the parent always spawns with the former human hediff
    /// </summary>
    public class CompAlwaysFormerHuman : ThingComp
    {
        private bool triggered = false;


        private CompProperties_AlwaysFormerHuman Props => props as CompProperties_AlwaysFormerHuman;

        /// <summary>
        /// called after the parent thing is spawned 
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad) return;

            if (!triggered)
            {
                triggered = true;

                if (((Pawn)parent).health.hediffSet.HasHediff(TfHediffDefOf.TransformedHuman)) return;

                float sL = Rand.Value;
                FormerHumanUtilities.MakeAnimalSapient((Pawn)parent, sL);
                FormerHumanUtilities.NotifyRelatedPawnsFormerHuman((Pawn)parent,
                                                                   FormerHumanUtilities.RELATED_WILD_FORMER_HUMAN_LETTER,
                                                                   FormerHumanUtilities
                                                                      .RELATED_WILD_FORMER_HUMAN_LETTER_LABEL);
            }
        }
        
        /// <summary>
        ///     Posts the expose data.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered));
        }
    }
}