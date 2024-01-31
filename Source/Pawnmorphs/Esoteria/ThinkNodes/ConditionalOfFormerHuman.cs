// ConditionalOfFormerHuman.cs modified by Iron Wolf for Pawnmorph on 12/19/2019 12:45 PM
// last updated 12/19/2019  12:45 PM

using Pawnmorph.Utilities;
using Verse;
using Verse.AI;

namespace Pawnmorph.ThinkNodes
{

	/// <summary>
	/// think node that restricts things to former humans 
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_Conditional" />
	public class ConditionalOfFormerHuman : ThinkNode_Conditional
	{
		/// <summary>
		/// The sapience filter
		/// </summary>
		public Filter<SapienceLevel> filter = new Filter<SapienceLevel>();

		/// <summary>
		/// checks if the condition is satisfied by the specified pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override bool Satisfied(Pawn pawn)
		{
			SapienceLevel? saLevel = pawn?.GetQuantizedSapienceLevel();
			if (saLevel == null)
				saLevel = SapienceLevel.PermanentlyFeral;
			return filter.PassesFilter(saLevel.Value);
		}
	}
}