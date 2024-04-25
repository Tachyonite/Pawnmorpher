// Driver_UseMutationScanner.cs created by Iron Wolf for Pawnmorph on 12/13/2020 2:07 PM
// last updated 12/13/2020  2:07 PM

using System.Collections.Generic;
using Pawnmorph.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public class Driver_UseMutationScanner : JobDriver
	{
		/// <summary>
		/// Tries to make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		/// <summary>
		/// Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{
			var scannerComp = job.targetA.Thing.TryGetComp<MutationSequencerComp>();
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOn(() => !scannerComp.CanUseNow);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
			Toil work = new Toil();
			work.tickAction = delegate
			{
				Pawn actor = work.actor;
				_ = (Building)actor.CurJob.targetA.Thing;
				scannerComp.Used(actor);
				actor.skills?.Learn(SkillDefOf.Intellectual, 0.035f);
				actor.GainComfortFromCellIfPossible(chairsOnly: true);
			};
			work.PlaySustainerOrSound(scannerComp.Props.soundWorking);
			work.AddFailCondition(() => !scannerComp.CanUseNow);
			work.defaultCompleteMode = ToilCompleteMode.Never;
			work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
			work.activeSkill = () => SkillDefOf.Intellectual;
			yield return work;
		}
	}
}