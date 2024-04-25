using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// class representing a production boost
	/// </summary>
	public class ProductionBoost
	{
		/// <summary>filter to specify which hediffs to boost production to</summary>
		public Filter<HediffDef> hediffFilter = new Filter<HediffDef>();
		/// <summary>is a increase/decrease in production Hediff's severity</summary>
		public float productionBoost;


		/// <summary>Gets the boost to the specific hediff.</summary>
		/// <param name="hediff">The hediff.</param>
		/// <returns></returns>
		public float GetBoost(HediffDef hediff)
		{
			return hediffFilter.PassesFilter(hediff) ? productionBoost : 0;
		}
	}
}
