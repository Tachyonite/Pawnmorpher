// AdditionalPlantInfo.cs created by Iron Wolf for Pawnmorph on 07/25/2021 6:34 PM
// last updated 07/25/2021  6:34 PM

using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// def extension for giving plants additional variability 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class AdditionalPlantInfo : DefModExtension
	{
		/// <summary>
		/// The minimum growth temperature
		/// </summary>
		public float minGrowthTemperature;
		/// <summary>
		/// The minimum optimal growth temperature
		/// </summary>
		public float minOptimalGrowthTemperature = 6f;
		/// <summary>
		/// The maximum optimal growth temperature
		/// </summary>
		public float maxOptimalGrowthTemperature = 42f;
		/// <summary>
		/// The maximum growth temperature
		/// </summary>
		public float maxGrowthTemperature = 58f;

		/// <summary>
		/// The minimum leafless temperature
		/// </summary>
		public float minLeaflessTemperature = -10;

		/// <summary>
		/// The maximum leafless temperature
		/// </summary>
		public float maxLeaflessTemperature = -2f;

	}
}