// Worker_VeneratedMutation_Social.cs created by Iron Wolf for Pawnmorph on 08/02/2021 5:39 PM
// last updated 08/02/2021  5:39 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	///     thought worker for a pawn having thoughts about another pawn with venerated mutations
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept_Social" />
	public class Worker_VeneratedMutation_Social : ThoughtWorker_Precept_Social
	{
		private const float JUMP_VALUE = 3f;

		[NotNull] private static readonly HashSet<MutationDef> _scratch = new HashSet<MutationDef>();

		/// <summary>
		///     if the pawn 'p' should have this thought about the given other pawn
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="otherPawn">The other pawn.</param>
		/// <returns></returns>
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			if (otherPawn == null) return false;
			Ideo ideo = p?.Ideo;
			if (ideo == null) return false;

			var count = 0;
			_scratch.Clear();
			foreach (Hediff_AddedMutation mutation in otherPawn.GetAllMutations())
				if (mutation.IsVeneratedBy(ideo))
				{
					if (!_scratch.Contains(mutation.Def))
						count++;
					else
						_scratch.Add(mutation.Def); //only want to count unique values 
				}


			int tStages = def.stages.Count;

			return count <= 0 ? false : ThoughtState.ActiveAtStage(Mathf.Min(tStages, Mathf.CeilToInt(count / JUMP_VALUE)));
		}
	}
}