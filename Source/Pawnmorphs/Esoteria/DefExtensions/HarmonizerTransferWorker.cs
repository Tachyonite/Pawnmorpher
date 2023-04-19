// HarmonizerTransferWorker.cs created by Iron Wolf for Pawnmorph on 12/13/2020 1:27 PM
// last updated 12/13/2020  1:27 PM

using System;
using Pawnmorph.Thoughts;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	/// <seealso cref="Pawnmorph.Thoughts.IThoughtTransferWorker" />
	public class HarmonizerTransferWorker : DefModExtension, IThoughtTransferWorker
	{
		/// <summary>
		/// if this thought should be transferred from the original pawn onto the target
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="target">The target.</param>
		/// <param name="thought">The thought.</param>
		/// <returns></returns>
		public bool ShouldTransfer(Pawn original, Pawn target, Thought_Memory thought)
		{
			return thought is Thought_PsychicHarmonizer;
		}

		/// <summary>
		/// Creates the new thought from the original pawn to transfer to the target pawn.
		/// </summary>
		/// <param name="original">The original.</param>
		/// <param name="target">The target.</param>
		/// <param name="originalThought">The original thought.</param>
		/// <returns></returns>
		public Thought_Memory CreateNewThought(Pawn original, Pawn target, Thought_Memory originalThought)
		{
			if (original == null) throw new ArgumentNullException(nameof(original));
			if (target == null) throw new ArgumentNullException(nameof(target));
			if (originalThought == null) throw new ArgumentNullException(nameof(originalThought));
			try
			{
				var harmonizerThought = (Thought_PsychicHarmonizer)originalThought;
				var newHarmonizerThought =
					(Thought_PsychicHarmonizer)ThoughtMaker.MakeThought(originalThought.def, originalThought.CurStageIndex);
				newHarmonizerThought.harmonizer = harmonizerThought.harmonizer;
				return newHarmonizerThought;
			}
			catch (System.Exception e)
			{
				Log.Error($"unable to transfer thought {original?.def?.label} onto {target?.Label}! caught {e.GetType().Name}!");
				throw;
			}
		}
	}
}