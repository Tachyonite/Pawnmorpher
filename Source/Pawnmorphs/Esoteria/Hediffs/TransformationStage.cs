// TransformationStage.cs created by Iron Wolf for Pawnmorph on 01/02/2020 1:44 PM
// last updated 01/02/2020  1:44 PM

using System.Collections.Generic;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     hediff stage that adds the possibility of adding mutations
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	/// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
	public class TransformationStage : TransformationStageBase
	{
		/// <summary>The mutations that this stage can add</summary>
		public List<MutationEntry> mutations;


		/// <summary>
		/// Gets the entries for the given pawn
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="source"></param>
		/// <returns></returns>
		public override IEnumerable<MutationEntry> GetEntries(Pawn pawn, Hediff source)
		{
			return mutations.MakeSafe();
		}
	}
}