// VeneratedAnimalTransferWorkerBase.cs created by Iron Wolf for Pawnmorph on 08/03/2021 4:09 PM
// last updated 08/03/2021  4:09 PM

using System;
using Pawnmorph.Thoughts;
using Pawnmorph.Thoughts.Precept;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// thought transfer worker for the venerated animal mutation thoughts 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	/// <seealso cref="Pawnmorph.Thoughts.IThoughtTransferWorker" />
	public class VeneratedAnimalMutationThought_TransferWorker : DefModExtension, IThoughtTransferWorker
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
			return thought is MutationMemory_VeneratedAnimal;
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
			try
			{
				var oThought = (MutationMemory_VeneratedAnimal)originalThought;
				var newThought =
					(MutationMemory_VeneratedAnimal)ThoughtMaker.MakeThought(originalThought.def, originalThought.sourcePrecept);
				newThought.veneratedAnimalLabel = oThought.veneratedAnimalLabel;
				return newThought;
			}
			catch (InvalidCastException)
			{
				Log.Error($"unable to create new thought of {originalThought.def.defName}! unable to cast {originalThought.GetType().Name} to {nameof(MutationMemory_VeneratedAnimal)}");
				throw;
			}
		}
	}
}