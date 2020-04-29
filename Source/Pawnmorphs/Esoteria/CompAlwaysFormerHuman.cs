using System.Linq;
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
        private bool _finishedCheck = false;
        private CompProperties_AlwaysFormerHuman Props => props as CompProperties_AlwaysFormerHuman;

        /// <summary>
        /// called after the parent thing is spawned 
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad) return;

            if (parent.def.GetModExtension<FormerHumanSettings>()?.neverFormerHuman == true)
            {
                Log.Error($"{nameof(CompAlwaysFormerHuman)} found on {parent.def.defName} which should never be a former human!");
                triggered = true;
                return; 
            }

            if (!triggered)
            {
                triggered = true;

                if (Pawn.health.hediffSet.HasHediff(TfHediffDefOf.TransformedHuman)) return;

                float sL = Rand.Value;
                FormerHumanUtilities.MakeAnimalSapient((Pawn)parent, sL, false);
                FormerHumanUtilities.NotifyRelatedPawnsFormerHuman((Pawn)parent,
                                                                   FormerHumanUtilities.RELATED_WILD_FORMER_HUMAN_LETTER,
                                                                   FormerHumanUtilities
                                                                      .RELATED_WILD_FORMER_HUMAN_LETTER_LABEL);
            }
        }

        /// <summary>
        /// called every tick 
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();
            //wait approximately a second before having the former human join the colony 
            if (triggered && !_finishedCheck && parent.IsHashIntervalTick(60))
            {
                _finishedCheck = true;
                if (parent.Faction != null) return; //only wild former humans can automatically join 
                if (Pawn.MentalStateDef == MentalStateDefOf.Manhunter
                 || Pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent)
                    return;
                //have the former human join only if it's not part of a manhunter pack 
                if (Pawn.IsFormerHuman() && Pawn.relations?.PotentiallyRelatedPawns?.Any(p => p.IsColonist) == true)
                {
                    Pawn.SetFaction(Faction.OfPlayer);
                }

            }
        }

        /// <summary>
        ///     Posts the expose data.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered));
            Scribe_Values.Look(ref _finishedCheck, "finishedCheck");

        }

        private Pawn Pawn => (Pawn) parent; 


        /// <summary>
        /// Called when the pawn recovered from the given mental state.
        /// </summary>
        /// <param name="mentalState">State of the mental.</param>
        public void OnRecoveredFromMentalState(MentalState mentalState) //have the pawn join the colony if related and recovered from the manhunter condition 
        {
            if (mentalState.def == MentalStateDefOf.ManhunterPermanent || mentalState.def == MentalStateDefOf.Manhunter)
            {
                if (Pawn.Faction == null && Pawn.relations?.PotentiallyRelatedPawns?.Any(p => p.IsColonist) == true)
                {
                    Pawn.SetFaction(Faction.OfPlayer); 
                }
            }
        }
    }
}