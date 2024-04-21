// PMPrisonerInteractionModeDefOf.cs created by Iron Wolf for Pawnmorph on 10/20/2020 7:04 AM
// last updated 10/20/2020  7:04 AM

using RimWorld;

namespace Pawnmorph
{
	/// <summary>
	/// def of for prisoner interaction mode defs 
	/// </summary>
	[DefOf]
	public static class PMPrisonerInteractionModeDefOf
	{
		static PMPrisonerInteractionModeDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMPrisonerInteractionModeDefOf));
		}

		/// <summary>
		/// The interaction mode for transforming prisoners 
		/// </summary>
		public static PrisonerInteractionModeDef PM_Transform;
	}
}