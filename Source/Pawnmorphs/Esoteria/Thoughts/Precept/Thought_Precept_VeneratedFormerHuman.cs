// Thought_Precept_VeneratedFormerHuman.cs created by Iron Wolf for Pawnmorph on 07/21/2021 12:34 PM
// last updated 07/21/2021  12:34 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	///     situational thought for former humans of an ideologie's venerated animal
	/// </summary>
	/// <seealso cref="RimWorld.Thought_Situational" />
	public class Thought_Precept_VeneratedFormerHuman : Thought_Situational
	{
		private const string ANIMAL_TAG = "ANIMALKIND";
		private const string PAWN_LABEL = "PAWN";

		//Note if this becomes a performance issue cache string per pawnkind & pawn

		/// <summary>
		///     Gets the capitalized label.
		/// </summary>
		/// <value>
		///     The label cap.
		/// </value>
		public override string LabelCap => FormatString(CurStage.LabelCap);


		/// <summary>
		///     Gets the description.
		/// </summary>
		/// <value>
		///     The description.
		/// </value>
		public override string Description => FormatString(CurStage.description) + CausedByBeliefInPrecept;

		/// <summary>
		///     gets the current internal state.
		/// </summary>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal()
		{
			return def.Worker.CurrentState(pawn);
		}


		private string FormatString(string inStr)
		{
			return inStr.Formatted(pawn.kindDef.Named(ANIMAL_TAG), pawn.Named(PAWN_LABEL)).CapitalizeFirst();
		}
	}
}