using RimWorld;
using Verse;

namespace Pawnmorph.Letters
{
	/// <summary>
	/// LetterDef references
	/// </summary>
	[DefOf]
	public static class PMLetterDefOf
	{
		/// <summary>
		/// The letter for former humans attempting to join the colony
		/// </summary>
		public static LetterDef PMFormerHumanJoinRequest;

		static PMLetterDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMLetterDefOf));
		}
	}
}
