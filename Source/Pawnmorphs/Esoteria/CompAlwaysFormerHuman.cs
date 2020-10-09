using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    ///     comp for ensuring the parent always spawns with the former human hediff
    /// </summary>
    public class CompAlwaysFormerHuman : ThingComp, IMentalStateRecoveryReceiver
    {
        private bool triggered = false;
        private CompProperties_AlwaysFormerHuman Props => props as CompProperties_AlwaysFormerHuman;

        private Pawn Pawn => (Pawn) parent;

        /// <summary>
        ///     called every tick
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();


            if (parent.def.GetModExtension<FormerHumanSettings>()?.neverFormerHuman == true)
            {
                Log.Error($"{nameof(CompAlwaysFormerHuman)} found on {parent.def.defName} which should never be a former human!");
                triggered = true;
                return;
            }

            if (!triggered)
            {
                triggered = true;

                if (Pawn.IsFormerHuman()) return;
                bool isManhunter = Pawn.MentalStateDef == MentalStateDefOf.Manhunter
                 || Pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent;
                    
                float sL = Rand.Value;
                FormerHumanUtilities.MakeAnimalSapient((Pawn) parent, sL, !isManhunter);
                FormerHumanUtilities.NotifyRelatedPawnsFormerHuman((Pawn) parent,
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

        /// <summary>
        ///     called after the parent thing is spawned
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (respawningAfterLoad) return;

            if(parent?.def?.IsChaomorph() == true)
                LessonAutoActivator.TeachOpportunity(PMConceptDefOf.Chaomorphs, OpportunityType.GoodToKnow); 

        }

        /// <summary>
        /// Called when the pawn recovered from the given mental state.
        /// </summary>
        /// <param name="mentalState">State of the mental.</param>
        public void OnRecoveredFromMentalState(MentalState mentalState)
        {
            if (Pawn.IsRelatedToColonistPawn() && Pawn.Faction != Faction.OfPlayer)
            {
                Pawn.SetFaction(Faction.OfPlayer); 
            }
        }
    }
}