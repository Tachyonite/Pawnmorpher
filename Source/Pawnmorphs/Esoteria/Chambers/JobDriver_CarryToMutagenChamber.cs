// JobDriver_CarryToMutagenChamber.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 08/25/2019  7:11 PM

using System.Collections.Generic;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    /// job driver for carrying a pawn to a mutagenic chamber 
    /// </summary>
    /// <seealso cref="Verse.AI.JobDriver" />
    public class JobDriver_CarryToMutagenChamber : JobDriver
    {
        private const TargetIndex TakeeInd = TargetIndex.A;

        private const TargetIndex DropPodInd = TargetIndex.B;
        /// <summary>Gets the pawn being taken</summary>
        /// <value>The takee.</value>
        protected Pawn Takee
        {
            get
            {
                return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
            }
        }

        /// <summary>Gets the mutagenic chamber</summary>
        /// <value>The drop pod.</value>
        protected MutaChamber MutagenicChamber
        {
            get
            {
                return (MutaChamber)this.job.GetTarget(TargetIndex.B).Thing;
            }
        }
        /// <summary>Tries the make pre toil reservations.</summary>
        /// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
        /// <returns></returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.Takee;
            Job job = this.job;
            bool result;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.MutagenicChamber;
                job = this.job;
                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            else
            {
                result = false;
            }
            return result;
        }


        /// <summary>Makes the new toils.</summary>
        /// <returns></returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnDestroyedOrNull(TargetIndex.B);
            this.FailOnAggroMentalState(TargetIndex.A);
            this.FailOn(() => !this.MutagenicChamber.Accepts(this.Takee));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOn(() => this.MutagenicChamber.GetDirectlyHeldThings().Count > 0).FailOn(() => !this.Takee.Downed).FailOn(() => !this.pawn.CanReach(this.Takee, PathEndMode.OnCell, Danger.Deadly, false, TraverseMode.ByPawn)).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_General.Wait(60).WithProgressBarToilDelay(TargetIndex.A);
            Toil toil2 = new Toil();
            toil2.initAction = delegate
            {
                Thing thing = job.targetA.Thing;
                base.Map.designationManager.DesignationOn(thing, DesignationDefOf.Strip)?.Delete();
                (thing as IStrippable)?.Strip();
                pawn.records.Increment(RecordDefOf.BodiesStripped);
            };
            toil2.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil2;
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, false, false, false);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500, TargetIndex.None);
            prepare.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(TargetIndex.B, false, -0.5f);
            yield return prepare;
            yield return new Toil
            {
                initAction = delegate ()
                {
                    this.MutagenicChamber.TryAcceptThing(this.Takee, true);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield break;
        }
        /// <summary>gets the Tale parameters.</summary>
        /// <returns></returns>
        public override object[] TaleParameters()
        {
            return new object[]
            {
                this.pawn,
                this.Takee
            };
        }
    }
}