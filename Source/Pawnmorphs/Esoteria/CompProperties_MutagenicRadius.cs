using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// comp properties for mutagenic radius comp 
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class CompProperties_MutagenicRadius : CompProperties
	{
		/// <summary>
		/// The radius per day curve
		/// </summary>
		public SimpleCurve radiusPerDayCurve;
		/// <summary>
		/// The hediff to add 
		/// </summary>
		public HediffDef hediff;
		/// <summary>
		/// The harm frequency per area
		/// </summary>
		public float harmFrequencyPerArea = 1f;
		/// <summary>
		/// Initializes a new instance of the <see cref="CompProperties_MutagenicRadius"/> class.
		/// </summary>
		public CompProperties_MutagenicRadius()
		{
			compClass = typeof(CompMutagenicRadius);
		}
	}
}
