using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// properties for the hediff comp single
	/// </summary>
	/// <seealso cref="Verse.HediffCompProperties" />
	public class HediffCompProperties_Single : HediffCompProperties
	{
		/// <summary>
		/// the maximum times the parent hediff can 'stack'
		/// </summary>
		public int maxStacks = 10;

		/// <summary>
		/// The mutation rate multiplier
		/// </summary>
		public float mutationRateMultiplier = 1.5f;

		/// <summary>
		/// Initializes a new instance of the <see cref="HediffCompProperties_Single"/> class.
		/// </summary>
		public HediffCompProperties_Single()
		{
			compClass = typeof(HediffComp_Single);
		}
	}
}
