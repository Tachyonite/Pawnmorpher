// IThoughtTransferWorker.cs created by Iron Wolf for Pawnmorph on 12/13/2020 1:18 PM
// last updated 12/13/2020  1:18 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// interface for something that helps transfer thoughts from one pawn onto another
	/// </summary>
	/// interface to help with thoughts that need special handling with regards to transforming pawns and thought transfer 
	public interface IThoughtTransferWorker
	{
		/// <summary>
		/// if this thought should be transferred from the original pawn onto the target
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="target">The target.</param>
		/// <param name="thought">The thought.</param>
		/// <returns></returns>
		bool ShouldTransfer([NotNull] Pawn original, [NotNull] Pawn target, [NotNull] Thought_Memory thought);


		/// <summary>
		/// Creates the new thought from the original pawn to transfer to the target pawn.
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="target">The target.</param>
		/// <param name="originalThought">The original thought.</param>
		/// <returns></returns>
		[NotNull]
		Thought_Memory CreateNewThought([NotNull] Pawn original, [NotNull] Pawn target, [NotNull] Thought_Memory originalThought);
	}
}