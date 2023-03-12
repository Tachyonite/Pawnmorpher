// PMRelationUtilities.cs created by Iron Wolf for Pawnmorph on 03/10/2020 5:20 PM
// last updated 03/10/2020  5:20 PM

using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using RimWorld;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
	/// <summary>
	/// static class for relationship utilities 
	/// </summary>
	public static class PMRelationUtilities
	{
		/// <summary>
		/// Determines whether this pawn is related to a colonist pawn by anything other than a bond.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this pawn is related to a colonist pawn; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public static bool IsRelatedToColonistPawn([NotNull] this Pawn pawn)
		{
			if (pawn is null)
				throw new System.ArgumentNullException(nameof(pawn));

			(var colonist, var relation) = pawn.GetRelatedColonistAndRelation();
			// TODO should bonds be excluded?
			if (relation != null && relation != PawnRelationDefOf.Bond)
			{
				LogMsg(LogLevel.Messages, $"{pawn.Name} is related to {colonist.Name} by relation {relation.defName}");
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the most important colonist related to this pawn, along with the relationship
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this pawn is related to a colonist pawn; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">pawn</exception>
		public static (Pawn, PawnRelationDef) GetRelatedColonistAndRelation([NotNull] this Pawn pawn)
		{
			if (pawn is null)
				throw new System.ArgumentNullException(nameof(pawn));

			var colonist = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
			var relation = colonist?.GetMostImportantRelation(pawn);

			return (colonist, relation);
		}
	}
}