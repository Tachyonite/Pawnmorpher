// MutagenicFallout.cs created by Iron Wolf for Pawnmorph on 02/29/2020 2:29 PM
// last updated 02/29/2020  2:29 PM

using RimWorld;

namespace Pawnmorph.IncidentWorkers
{
	/// <summary>
	/// incident worker for the mutagenic fallout event 
	/// </summary>
	/// <seealso cref="RimWorld.IncidentWorker_MakeGameCondition" />
	public class MutagenicFallout : IncidentWorker_MakeGameCondition
	{
		/// <summary>
		/// Determines whether the incident can happen now 
		/// </summary>
		/// <param name="parms">The parms.</param>
		/// <returns>
		///   <c>true</c> if this incident can occur now otherwise, <c>false</c>.
		/// </returns>
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!PMUtilities.GetSettings().enableFallout) return false;

			return base.CanFireNowSub(parms);
		}
	}
}