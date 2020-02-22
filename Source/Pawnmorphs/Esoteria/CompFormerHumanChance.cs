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
        /// called after the parent thing is spawned
        /// </summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (respawningAfterLoad) return; 
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
            if (pawn.relations.DirectRelations.Any(r => r.def == PawnRelationDefOf.Child || r.def == PawnRelationDefOf.Parent)) return false;

            //make sure the animal is old enough 

            var age = pawn.ageTracker.AgeBiologicalYears; 
            //convert to human years 
            var hAge = age * (ThingDefOf.Human.race.lifeExpectancy) / pawn.RaceProps.lifeExpectancy;



            return hAge > 20; //make sure their older then 20 human years 
        }
    }
}