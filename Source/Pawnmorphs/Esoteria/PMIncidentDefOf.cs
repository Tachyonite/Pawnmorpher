// PMIncidentDefOf.cs modified by Iron Wolf for Pawnmorph on 08/29/2019 3:34 PM
// last updated 08/29/2019  3:34 PM

using RimWorld;

#pragma warning disable 1591
namespace Pawnmorph
{
	/// <summary> Static container for incident defs. </summary>
	[DefOf]
	public static class PMIncidentDefOf
	{
		static PMIncidentDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMIncidentDefOf));
		}

		public static IncidentDef MutagenicShipPartCrash;
		public static IncidentDef MutagenicFallout;
	}
}