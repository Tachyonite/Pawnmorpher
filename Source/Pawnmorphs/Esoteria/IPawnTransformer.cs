// IPawnTransformer.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 5:31 PM
// last updated 01/13/2020  5:31 PM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// interface for a thing that can transform a pawn 
	/// </summary>
	public interface IPawnTransformer
	{
		/// <summary>
		/// Tries to transform the pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <returns></returns>
		bool TryTransform([NotNull] Pawn pawn, Hediff cause = null);

		/// <summary>
		/// transforms pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="cause">The cause.</param>
		/// <returns>if the transformation was successful or not, only false if there was an error</returns>
		bool TransformPawn([NotNull] Pawn pawn, Hediff cause = null);
	}
}