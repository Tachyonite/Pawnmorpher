// Worker_FormerHuman.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 4:21 PM
// last updated 12/15/2019  4:21 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	///     thought worker to give former humans a constant 'i'm an animal now' thought
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker" />
	public class Worker_FormerHuman : ThoughtWorker
	{
		/// <summary>
		///     gets the current thought state for the pawn
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (ModsConfig.IdeologyActive)
				if (p?.ideo?.Ideo?.IsVeneratedAnimal(p) == true)
					return false; // delegate this to a different worker 

			int? stage = GetStage(p);

			return stage == null ? false : ThoughtState.ActiveAtStage(stage.Value);
		}

		/// <summary>
		///     Gets the correct stage based on the pawns sapience level
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected virtual int? GetStage(Pawn p)
		{
			if (!def.IsValidFor(p)) return null;
			SapienceLevel? sapientLevel = p.GetQuantizedSapienceLevel();
			if (sapientLevel == null || p.GetSapienceState()?.StateDef != SapienceStateDefOf.FormerHuman) return null;
			return Mathf.Min(def.stages.Count - 1, (int)sapientLevel.Value);
		}
	}
}