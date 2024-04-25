// Driver_FindMushrooms.cs modified by Iron Wolf for Pawnmorph on 11/09/2019 7:33 AM
// last updated 11/09/2019  7:33 AM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	///     job driver for the 'find mushrooms' job
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public class Driver_TerrainProduction : JobDriver
	{
		/// <summary>
		///     Tries to make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}
		/// <summary>
		/// Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => !JoyUtility.EnjoyableOutsideNow(pawn));
			Toil goToil = Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			goToil.tickAction = GotToToil;

			var tComps = pawn.health.hediffSet.hediffs.Select(h => h.TryGetComp<Comp_TerrainProduction>())
							 .Where(c => c != null)
							 .ToList();


			yield return goToil;

			void OnEndToil()
			{
				if (job.targetQueueA.Count > 0)
				{
					DebugLogUtils.LogMsg(LogLevel.Messages, "checking production now");

					if (tComps.Count > 0 && Rand.Range(0, 1f) < 0.4f)
						tComps.RandomElement().ProduceNow();

					LocalTargetInfo targetA = job.targetQueueA[0];
					job.targetQueueA.RemoveAt(0);
					job.targetA = targetA;
					JumpToToil(goToil);
				}
			}

			yield return new Toil() { initAction = OnEndToil };

		}

		void ProduceStuff()
		{

		}

		private void GotToToil()
		{
			if (Find.TickManager.TicksGame > startTick + job.def.joyDuration)
			{
				EndJobWith(JobCondition.Succeeded);
				return;
			}

			JoyUtility.JoyTickCheckEnd(pawn);
		}
	}
}