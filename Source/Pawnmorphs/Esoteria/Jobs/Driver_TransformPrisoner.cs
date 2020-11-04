// Driver_TransformPrisoner.cs created by Iron Wolf for Pawnmorph on 10/20/2020 6:58 AM
// last updated 10/20/2020  6:58 AM

using System;
using System.Collections.Generic;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
    /// <summary>
    /// job driver for taking a prisoner to a mutachamber 
    /// </summary>
    /// <seealso cref="Verse.AI.JobDriver" />
    public class Driver_TransformPrisoner : JobDriver
    {
        private Pawn Prisoner => (Pawn)job.GetTarget(TargetIndex.A).Thing;



        private MutaChamber Chamber => (MutaChamber) job.GetTarget(TargetIndex.B).Thing;

        /// <summary>
        /// Tries to make pre toil reservations.
        /// </summary>
        /// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
        /// <returns></returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Prisoner, job, 1, -1, null, errorOnFailed)
                && pawn.Reserve(Chamber, job, 1, -1, null, errorOnFailed); 
        }

        /// <summary>
        /// Makes the new toils.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.B);
            this.FailOn(() => ((Pawn)(Thing)GetActor().CurJob.GetTarget(TargetIndex.A)).guest.interactionMode != PMPrisonerInteractionModeDefOf.PM_Transform);
            this.FailOnDowned(TargetIndex.A);
            this.FailOnAggroMentalState(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(() => !Prisoner.IsPrisonerOfColony || !Prisoner.guest.PrisonerIsSecure).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A);
            Toil carryToCell = Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            yield return carryToCell;
            Toil toil = Toils_General.Wait(500, TargetIndex.B);
            toil.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
            toil.WithProgressBarToilDelay(TargetIndex.B);
            yield return toil;
            Toil toil2 = new Toil
            {
                initAction = delegate
                {
                    Prisoner?.Strip();
                    
                    Chamber.TryAcceptThing(Prisoner);
                }, defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return toil2;
        }

       
    }
}