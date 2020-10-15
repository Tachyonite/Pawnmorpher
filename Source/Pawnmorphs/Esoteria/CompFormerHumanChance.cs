using System.Linq;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    ///     comp for adding the former human hediff with a percent chance when this thing spawns
    /// </summary>
    public class CompFormerHumanChance : ThingComp, IMentalStateRecoveryReceiver
    {
        private bool triggered = false;
        private bool _finishedCheck = false;

        private bool _isRelatedToColonist; 

        /// <summary>
        ///     the properties for this comp
        /// </summary>
        public CompProperties_FormerHumanChance Props => props as CompProperties_FormerHumanChance;

        /// <summary>
        /// called after the parent thing is spawned
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad) return; 
        }

        /// <summary>
        /// called every tick 
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();


            if (!triggered)
            {
                triggered = true;
                if (CanBeFormerHuman() && Rand.RangeInclusive(0, 100) <= Props.Chance)
                {
                    float sL = Rand.Value;
                    FormerHumanUtilities.MakeAnimalSapient((Pawn)parent, sL);
                    FormerHumanUtilities.NotifyRelatedPawnsFormerHuman((Pawn)parent,
                                                                       FormerHumanUtilities.RELATED_WILD_FORMER_HUMAN_LETTER,
                                                                       FormerHumanUtilities
                                                                          .RELATED_WILD_FORMER_HUMAN_LETTER_LABEL);
                    _isRelatedToColonist = Pawn.IsRelatedToColonistPawn();
                }
            }


          
        }

        /// <summary>
        ///     called to save/load data
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref triggered, nameof(triggered));
            Scribe_Values.Look(ref _finishedCheck, "finishedCheck"); 
            Scribe_Values.Look(ref _isRelatedToColonist, "isRelatedToColonists");
        }


        private bool CanBeFormerHuman()
        {
            if (!LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>().enableWildFormers) return false;
            if (parent.def.GetModExtension<FormerHumanSettings>()?.neverFormerHuman == true) return false; 
            if (parent.Faction != null) return false;
            var pawn = (Pawn) parent;
            if (pawn.relations == null) return true;
            if (pawn.relations.DirectRelations.Any(r => r.def == PawnRelationDefOf.Child || r.def == PawnRelationDefOf.Parent)) return false;

            //make sure the animal is old enough 

            var age = pawn.ageTracker.AgeBiologicalYears; 
            //convert to human years 
            var hAge = age * (ThingDefOf.Human.race.lifeExpectancy) / pawn.RaceProps.lifeExpectancy;

            //don't let manhunters be former humans 
            if (Pawn.MentalStateDef == MentalStateDefOf.Manhunter
             || Pawn.MentalStateDef == MentalStateDefOf.ManhunterPermanent)
                return false;


            return hAge > 20; //make sure their older then 20 human years 
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
                if (Pawn.Faction == null  && Pawn.IsFormerHuman() && _isRelatedToColonist)
                {
                    Pawn.SetFaction(Faction.OfPlayer);
                }
            }
        }
    }
}