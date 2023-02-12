using System.Collections.Generic;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary> Base class for productive mutation's job driver. </summary>
	public abstract class Driver_ProduceThing : JobDriver
	{
		private const int WAIT_TIME = 250;

		/// <summary>
		/// Tries the make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> error on failed.</param>
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
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
			yield return Toils_General.Wait(WAIT_TIME);
			yield return Toils_General.Do(Produce);
		}
		/// <summary>
		/// Produce whatever resources this driver is producing.
		/// </summary>
		public abstract void Produce();
	}
}
