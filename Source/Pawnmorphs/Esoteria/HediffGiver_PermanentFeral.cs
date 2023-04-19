using System.Linq;
using Pawnmorph.TfSys;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// hediff giver that gives the permanently feral hediff 
	/// </summary>
	/// <seealso cref="Verse.HediffGiver" />
	public class HediffGiver_PermanentFeral : HediffGiver
	{
		/// <summary>
		/// The mean time between days
		/// </summary>
		public float mtbDays;
		/// <summary>
		/// Called when the interval passed
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (TryApply(pawn, null))
			{
				MakeFeral(pawn);
			}
		}

		private static void MakeFeral(Pawn pawn)
		{
			var loader = Find.World.GetComponent<PawnmorphGameComp>();
			var inst = loader.GetTransformedPawnContaining(pawn)?.pawn;
			var singleInst = inst as TransformedPawnSingle; //hacky, need to come up with a better solution eventually 
			foreach (var instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>())
			{
				ReactionsHelper.OnPawnPermFeral(instOriginalPawn, pawn, singleInst?.reactionStatus ?? FormerHumanReactionStatus.Wild);
			}

			//remove the original and destroy the pawns 
			foreach (var instOriginalPawn in inst?.OriginalPawns ?? Enumerable.Empty<Pawn>())
			{
				instOriginalPawn.Destroy();
			}

			if (inst != null)
			{
				loader.RemoveInstance(inst);
			}

			if (inst != null || pawn.Faction == Faction.OfPlayer)
				Find.LetterStack.ReceiveLetter("LetterHediffFromPermanentTFLabel".Translate(pawn.LabelShort).CapitalizeFirst(), "LetterHediffFromPermanentTF".Translate(pawn.LabelShort).CapitalizeFirst(), LetterDefOf.NegativeEvent, pawn, null, null);
		}
	}
}
