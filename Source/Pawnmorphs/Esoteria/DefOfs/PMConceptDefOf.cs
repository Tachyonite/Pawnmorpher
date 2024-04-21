// PMConceptDefOf.cs created by Iron Wolf for Pawnmorph on 10/09/2020 11:48 AM
// last updated 10/09/2020  11:48 AM

using RimWorld;

namespace Pawnmorph
{
	/// <summary>
	/// static container for pawnmorpher concept defs 
	/// </summary>
	[DefOf]
	public static class PMConceptDefOf
	{
		/// <summary>
		/// concept def for the genebanks 
		/// </summary>
		public static ConceptDef PM_Genebanks;

		/// <summary>
		/// concept def for injectors 
		/// </summary>
		public static ConceptDef PM_Injectors;

		/// <summary>
		/// concept def for merging pawns 
		/// </summary>
		public static ConceptDef MergingPawns;

		/// <summary>
		/// concept def for the part picker 
		/// </summary>
		public static ConceptDef PM_PartPicker;

		/// <summary>
		/// concept def for tagging 
		/// </summary>
		public static ConceptDef Tagging;

		/// <summary>
		/// The chaomorphs concept def 
		/// </summary>
		public static ConceptDef Chaomorphs;

		static PMConceptDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMConceptDefOf));
		}

	}


}