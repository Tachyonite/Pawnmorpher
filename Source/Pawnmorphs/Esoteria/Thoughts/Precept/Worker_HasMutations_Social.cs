// Worker_HasMutations.cs created by Iron Wolf for Pawnmorph on 08/02/2021 6:07 PM
// last updated 08/02/2021  6:07 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	/// social precept thought worker for when a pawn has mutations
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept_Social" />
	public class Worker_HasMutations_Social : ThoughtWorker_Precept_Social
	{

		private const float JUMP_VALUE = 3f;

		[NotNull]
		private static readonly HashSet<MutationDef> _scratch = new HashSet<MutationDef>();


		/// <summary>
		/// if p should have this thought about otherPawn
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="otherPawn">The other pawn.</param>
		/// <returns></returns>
		protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			if (p == null) return false;
			if (otherPawn == null) return false;


			_scratch.Clear();
			var count = 0;
			foreach (Hediff_AddedMutation mutation in otherPawn.GetAllMutations())
				if (_scratch.Contains(mutation.Def))
					count++;
				else
					_scratch.Add(mutation.Def);

			var tStages = def.stages.Count;
			return count == 0 ? false : ThoughtState.ActiveAtStage(Mathf.Min(tStages, Mathf.CeilToInt(count / JUMP_VALUE)));

		}
	}
}