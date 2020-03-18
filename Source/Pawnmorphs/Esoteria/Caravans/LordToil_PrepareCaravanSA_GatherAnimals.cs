// LordToil_GatherAnimals.cs created by Iron Wolf for Pawnmorph on 03/18/2020 12:45 PM
// last updated 03/18/2020  12:45 PM

using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Pawnmorph.Caravans
{
    /// <summary>
    ///     toil for gathering animals in a caravan with sapient animals
    /// </summary>
    /// <seealso cref="Verse.AI.Group.LordToil" />
    public class LordToil_PrepareCaravanSA_GatherAnimals : LordToil
    {
        private readonly IntVec3 meetingPoint;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LordToil_PrepareCaravanSA_GatherAnimals" /> class.
        /// </summary>
        /// <param name="meetingPoint">The meeting point.</param>
        public LordToil_PrepareCaravanSA_GatherAnimals(IntVec3 meetingPoint)
        {
            this.meetingPoint = meetingPoint;
        }

        /// <summary>
        ///     Gets the custom wake threshold.
        /// </summary>
        /// <value>
        ///     The custom wake threshold.
        /// </value>
        public override float? CustomWakeThreshold => 0.5f;

        /// <summary>
        ///     Gets a value indicating whether pawns can rest in bed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if  pawns can rest in bed; otherwise, <c>false</c>.
        /// </value>
        public override bool AllowRestingInBed => false;

        /// <summary>
        /// called every tick
        /// </summary>
        public override void LordToilTick()
        {
            if (Find.TickManager.TicksGame % 100 == 0)
                GatherAnimalsAndSlavesForCaravanUtility.CheckArrived(lord, lord.ownedPawns, meetingPoint, "AllAnimalsGathered",
                                                                     x => x.IsAnimal(),
                                                                     GatherAnimalsAndSlavesForCaravanUtility
                                                                        .IsFollowingAnyone);
        }

        /// <summary>
        ///     Updates all duties.
        /// </summary>
        public override void UpdateAllDuties()
        {
            for (var i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                if (pawn.IsColonist || pawn.RaceProps.Animal)
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_GatherPawns, meetingPoint);
                    pawn.mindState.duty.pawnsToGather = PawnsToGather.Animals;
                }
                else
                {
                    pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait);
                }
            }
        }
    }
}