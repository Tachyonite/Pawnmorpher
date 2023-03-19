using RimWorld;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// Thought for a former human relative being accepted into the colony
	/// </summary>
	public class Thought_RelatedFormerHuman_Accepted : Thought_Memory
	{
		/// <summary>
		/// Whether this thought should be discarded
		/// </summary>
		/// <value><c>true</c> if should discard; otherwise, <c>false</c>.</value>
		public override bool ShouldDiscard
		{
			get
			{
				if (base.ShouldDiscard)
					return true;

				// Stop thinking about this if we banish them
				return otherPawn.Faction != pawn.Faction;
			}
		}
	}

	/// <summary>
	/// Thought for a former human relative being rejected by the colony
	/// </summary>
	public class Thought_RelatedFormerHuman_Rejected : Thought_Memory
	{
		/// <summary>
		/// Whether this thought should be discarded
		/// </summary>
		/// <value><c>true</c> if should discard; otherwise, <c>false</c>.</value>
		public override bool ShouldDiscard
		{
			get
			{
				if (base.ShouldDiscard)
					return true;

				// Stop thinking about this if we recruit them
				return otherPawn.Faction == pawn.Faction;
			}
		}
	}
}
