// Driver_DeliverThingToChamber.cs created by Iron Wolf for Pawnmorph on 06/25/2021 5:46 PM
// last updated 06/25/2021  5:46 PM

using System.Collections.Generic;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	///     job driver for delivering a special thing to a chamber
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public class Driver_DeliverThingToChamber : JobDriver
	{
		/// <summary>Gets the pawn being taken</summary>
		/// <value>The takee.</value>
		protected Thing DeliveredThing => job.GetTarget(TargetIndex.A).Thing;

		/// <summary>Gets the mutagenic chamber</summary>
		/// <value>The drop pod.</value>
		protected MutaChamber MutagenicChamber => (MutaChamber)job.GetTarget(TargetIndex.B).Thing;


		/// <summary>
		///     Tries the make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			LocalTargetInfo target = DeliveredThing;
			Job job = this.job;
			bool result;
			if (pawn.Reserve(target, job, 1, 1, null, errorOnFailed))
			{
				pawn = this.pawn;
				target = MutagenicChamber;
				job = this.job;
				result = pawn.Reserve(target, job, 1, 1, null, errorOnFailed);
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
			//this.FailOn(() => !this.MutagenicChamber.Accepts(this.Takee));
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell)
								   .FailOnDestroyedNullOrForbidden(TargetIndex.A)
								   .FailOnDespawnedNullOrForbidden(TargetIndex.B)
								   .FailOn(() => !pawn.CanReach(DeliveredThing, PathEndMode.OnCell, Danger.Deadly))
								   .FailOnSomeonePhysicallyInteracting(TargetIndex.A);
			yield return Toils_General.Wait(60).WithProgressBarToilDelay(TargetIndex.A);
			var toil2 = new Toil();
			toil2.initAction = delegate
			{
				Thing thing = job.targetA.Thing;
				Map.designationManager.DesignationOn(thing, DesignationDefOf.Strip)?.Delete();
				(thing as IStrippable)?.Strip();
				pawn.records.Increment(RecordDefOf.BodiesStripped);
			};
			toil2.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil2;
			yield return Toils_Haul.StartCarryThing(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.InteractionCell);
			Toil prepare = Toils_General.Wait(500);
			prepare.FailOnCannotTouch(TargetIndex.B, PathEndMode.InteractionCell);
			prepare.WithProgressBarToilDelay(TargetIndex.B);
			yield return prepare;
			yield return new Toil
			{
				initAction = delegate { MutagenicChamber.TryAcceptSpecialThing(DeliveredThing); },
				defaultCompleteMode = ToilCompleteMode.Instant
			};
		}
	}
}