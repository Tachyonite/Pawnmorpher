// Worker_Precept_FormerHuman.cs created by Iron Wolf for Pawnmorph on 07/24/2021 12:00 PM
// last updated 07/24/2021  12:00 PM

using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	/// abstract class for all precept thoughts that pertain only to former humans 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept" />
	public class PWorker_FormerHuman : ThoughtWorker_Precept
	{
		/// <summary>
		/// The minimum sapience level
		/// </summary>
		public SapienceLevel minSapienceLevel = SapienceLevel.PermanentlyFeral;


		/// <summary>
		/// if the given pawn should have this thought .
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected sealed override ThoughtState ShouldHaveThought(Pawn p)
		{
			if (!def.IsValidFor(p)) return false;
			//sapience levels are inverted. so permently feral has the highest value
			if (!p.IsFormerHuman() || p.GetQuantizedSapienceLevel() > minSapienceLevel) return false;
			return ShouldHaveThought_Internal(p);
		}

		/// <summary>
		/// if the given pawn should have this thought.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected virtual ThoughtState ShouldHaveThought_Internal(Pawn p)
		{
			return ThoughtState.ActiveAtStage(GetStageForSapienceLevel(p.GetQuantizedSapienceLevel() ?? SapienceLevel.PermanentlyFeral));
		}

		/// <summary>
		/// Gets the stage for sapience level.
		/// </summary>
		/// <param name="sapience">The sapience.</param>
		/// <returns></returns>
		protected virtual int GetStageForSapienceLevel(SapienceLevel sapience)
		{
			return Mathf.Min((int)sapience, def.stages.Count - 1);
		}
	}


	/// <summary>
	/// abstract class for all precept thoughts that pertain only to former humans 
	/// </summary>
	/// <seealso cref="RimWorld.ThoughtWorker_Precept" />
	public class PWorker_FormerHumanSocial : ThoughtWorker_Precept_Social
	{

		/// <summary>
		/// if the observer must be a former human
		/// </summary>
		public bool observerFormerHuman = false;
		/// <summary>
		/// if the observed must be a former human
		/// </summary>
		public bool observedFormerHuman = true;

		/// <summary>
		/// The minimum observer sapience level
		/// </summary>
		public SapienceLevel minObserverSapienceLevel;

		/// <summary>
		/// The minimum observed sapience level
		/// </summary>
		public SapienceLevel minObservedSapienceLevel;

		/// <summary>
		/// if the given pawn should have this thought .
		/// </summary>
		/// <param name="p">The p.</param>
		/// <param name="otherPawn">The other pawn.</param>
		/// <returns></returns>
		protected sealed override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			if (p == null || otherPawn == null) return false;
			if (!def.IsValidFor(p)) return false;
			//sapience levels are inverted. so permently feral has the highest value
			if (observerFormerHuman && (!p.IsFormerHuman() || p.GetQuantizedSapienceLevel() > minObserverSapienceLevel)) return false;
			if (observedFormerHuman && (!otherPawn.IsFormerHuman() || p.GetQuantizedSapienceLevel() > minObservedSapienceLevel))
				return false;
			return ShouldHaveThought_Internal(p, otherPawn);
		}

		/// <summary>
		/// if the given pawn should have this thought.
		/// </summary>
		/// <param name="observer">The observer.</param>
		/// <param name="observed">The observed.</param>
		/// <returns></returns>
		protected virtual ThoughtState ShouldHaveThought_Internal([NotNull] Pawn observer, [NotNull] Pawn observed)
		{
			return ThoughtState.ActiveAtStage(GetStageForSapienceLevel(observer.GetQuantizedSapienceLevel() ?? (observer.IsFormerHuman() ? SapienceLevel.PermanentlyFeral : SapienceLevel.Sapient)));
		}

		/// <summary>
		/// Gets the stage for sapience level.
		/// </summary>
		/// <param name="sapience">The sapience.</param>
		/// <returns></returns>
		protected virtual int GetStageForSapienceLevel(SapienceLevel sapience)
		{
			return Mathf.Min((int)sapience, def.stages.Count - 1);
		}
	}




}