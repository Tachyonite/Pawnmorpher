// FullTransformationStage.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 5:26 PM
// last updated 01/13/2020  5:26 PM

using System.Collections.Generic;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff stage that controls
	/// </summary>
	/// <seealso cref="Pawnmorph.Hediffs.FullTransformationStageBase" />
	public class FullTransformationStage : FullTransformationStageBase
	{

		/// The pawnKind of the animal to be transformed into.
		public List<PawnKindDef> pawnkinds;

		/// <summary>
		/// Gets the pawn kind definition to turn the given pawn into
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override PawnKindDef GetPawnKindDefFor(Pawn pawn)
		{
			Rand.PushState(pawn.thingIDNumber);
			try
			{
				return pawnkinds.RandElement();
			}
			finally
			{
				Rand.PopState();
			}
		}

		/// <summary>
		/// Gets all Configuration errors in this instance.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors()
		{
			if (pawnkinds.NullOrEmpty()) yield return "no pawnkinds set";
		}
	}
}