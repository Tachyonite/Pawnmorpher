// Worker_BodyPuristDisgust.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:36 PM
// last updated 09/18/2019  2:36 PM

using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	///     thought worker for pawns that have the body purist to add opinions about other pawns with mutations
	/// </summary>
	public class Worker_BodyPuristDisgust : ThoughtWorker
	{
		/// <summary>gets the current state of the thought with regards to the given pawns</summary>
		/// <param name="p">The pawn that has the thought</param>
		/// <param name="otherPawn">The pawn the thought is about</param>
		/// <returns></returns>
		protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
		{
			if (!p.RaceProps.Humanlike) return false;
			if (otherPawn?.RaceProps?.Humanlike != true) return false; //make sure only humanlike pawns are affected by this 
			if (p.story?.traits?.HasTrait(TraitDefOf.BodyPurist) != true) return false;
			if (!RelationsUtility.PawnsKnowEachOther(p, otherPawn)) return false; //the pawns have to know each other 

			MutationTracker tracker = otherPawn.GetMutationTracker();
			if (tracker == null) return false;
			if (tracker.MutationsCount == 0) return false;


			//check for aliens that naturally spawn with parts 

			RaceMutationSettingsExtension raceExt = p.TryGetRaceMutationSettings();
			if (raceExt != null) return CalculateAlienBP(p, tracker, raceExt);


			int n = Mathf.FloorToInt(tracker.TotalNormalizedInfluence * def.stages.Count);
			n = Mathf.Clamp(n, 0, def.stages.Count - 1);

			return ThoughtState.ActiveAtStage(n);
		}

		private ThoughtState CalculateAlienBP([NotNull] Pawn pawn, [NotNull] MutationTracker tracker,
											  [NotNull] RaceMutationSettingsExtension raceExt)
		{
			float inf = tracker.TotalInfluence;
			foreach (Hediff_AddedMutation mutation in tracker.AllMutations)
			{
				var isNatural = false;
				foreach (IRaceMutationRetriever retriever in raceExt.mutationRetrievers.MakeSafe())
					if (retriever.CanGenerate(mutation.Def))
					{
						isNatural = true;
						break;
					}

				if (isNatural) inf -= 1;
			}

			float nInf = Mathf.Max(inf, 0) / MorphUtilities.GetMaxInfluenceOfRace(pawn.def);
			int n = Mathf.FloorToInt(nInf * def.stages.Count);
			n = Mathf.Clamp(n, 0, def.stages.Count - 1);
			return ThoughtState.ActiveAtStage(n);
		}
	}
}