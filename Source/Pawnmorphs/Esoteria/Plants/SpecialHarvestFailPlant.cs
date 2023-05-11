// SpecialHarvestFailPlant.cs created by Iron Wolf for Pawnmorph on 04/16/2021 1:03 PM
// last updated 04/16/2021  1:03 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Plants
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="RimWorld.Plant" />
	public abstract class SpecialHarvestFailPlant : Plant
	{
		/// <summary>
		/// Gets the yield now.
		/// </summary>
		/// <param name="harvester">The harvester.</param>
		/// <returns></returns>
		public abstract int GetYieldNow(Pawn harvester);
	}


	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Pawnmorph.Plants.SpecialHarvestFailPlant" />
	public class Chaobulb : SpecialHarvestFailPlant
	{
		private const string FAILED_HARVEST_MESSAGE = "PMFailedChaobulbHarvest";

		/// <summary>
		/// Gets the yield now.
		/// </summary>
		/// <param name="harvester">The harvester.</param>
		/// <returns></returns>
		public override int GetYieldNow(Pawn harvester)
		{
			var failed = harvester.RaceProps.Humanlike
					  && !Blighted
							 && Rand.Value > harvester.GetStatValue(StatDefOf.PlantHarvestYield);

			if (failed && PMUtilities.HazardousChaobulb)
			{
				Messages.Message(FAILED_HARVEST_MESSAGE.Translate(harvester.Named("PAWN")), MessageTypeDefOf.NegativeEvent);
				MutagenicBuildupUtilities.AdjustMutagenicBuildup(def, harvester, 0.1f);
				return 0;
			}

			return YieldNow();
		}
	}
}