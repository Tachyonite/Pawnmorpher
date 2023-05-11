// FormerHumanPalsWorker.cs modified by Iron Wolf for Pawnmorph on 01/21/2020 8:48 PM
// last updated 01/21/2020  8:48 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// situational thought worker for when a former human is bonded to one or more former humans 
	/// </summary>
	/// <seealso cref="Pawnmorph.Thoughts.FormerHumanSituationalWorkerBase" />
	public class FormerHumanPalsWorker : AnimalPalsWorkerBase
	{

		[NotNull] private static readonly List<string> _formerHumanCache = new List<string>();

		/// <summary>
		/// gets the current state of the thought 
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal([NotNull] Pawn p)
		{
			if (!IsValidFor(p))
				return false;

			_formerHumanCache.Clear();

			GetAnimalBonds(p, _formerHumanCache, pawn => pawn?.IsFormerHuman() == true);

			if (_formerHumanCache.Count > 0)
			{
				return SetState(p, _formerHumanCache);
			}

			return false;
		}
	}

	/// <summary>
	/// thought worker for when a former human is bonded to a regular animal 
	/// </summary>
	/// <seealso cref="Pawnmorph.Thoughts.AnimalPalsWorkerBase" />
	public class AnimalPalsWorker : AnimalPalsWorkerBase
	{
		[NotNull]
		private static readonly List<string> _animalsList = new List<string>();

		/// <summary>
		/// gets the current state of the thought 
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (!IsValidFor(p))
				return false;

			_animalsList.Clear();

			GetAnimalBonds(p, _animalsList, pawn => pawn?.IsFormerHuman() == false && pawn.RaceProps.Animal);

			if (_animalsList.Count > 0)
			{
				return SetState(p, _animalsList);
			}

			return false;
		}
	}


	/// <summary>
	/// base worker for all 'animal pal' thoughts 
	/// </summary>
	/// <seealso cref="Pawnmorph.Thoughts.FormerHumanSituationalWorkerBase" />
	public abstract class AnimalPalsWorkerBase : FormerHumanSituationalWorkerBase
	{

		/// <summary>
		/// Determines whether this thought is valid for the specified pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if this thought is valid for the specified pawn; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pawn</exception>
		protected bool IsValidFor([NotNull] Pawn pawn)
		{
			if (pawn == null) throw new ArgumentNullException(nameof(pawn));
			return def.IsValidFor(pawn) && pawn.IsFormerHuman(); //only valid for former humans 
		}

		/// <summary>
		/// Gets the former human bonded to this
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		/// <param name="allFHBonded">All fh bonded.</param>
		/// <param name="selectorFunc">The selector function.</param>
		/// <exception cref="ArgumentNullException">
		/// formerHuman
		/// or
		/// allFHBonded
		/// </exception>
		protected void GetAnimalBonds([NotNull] Pawn formerHuman, [NotNull] List<string> allFHBonded,
									  Func<Pawn, bool> selectorFunc = null)
		{
			if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
			if (allFHBonded == null) throw new ArgumentNullException(nameof(allFHBonded));
			List<DirectPawnRelation> dRelations = formerHuman.relations?.DirectRelations;

			if (dRelations == null) return;

			foreach (DirectPawnRelation directPawnRelation in dRelations)
			{
				if (directPawnRelation.def != PawnRelationDefOf.Bond) //only check bond relations 
					continue;
				if (directPawnRelation.otherPawn?.RaceProps?.Animal != true)
					continue; //only check animals 

				if (selectorFunc?.Invoke(directPawnRelation.otherPawn) ?? true) //if no selector func is defined grab everyone 
					allFHBonded.Add(directPawnRelation.otherPawn.LabelShort);
			}
		}

		/// <summary>
		/// Sets the state of this thought for the given pawn for the given reasons.
		/// </summary>
		/// <param name="formerHuman">The former human.</param>
		/// <param name="reasonList">The reason list.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// formerHuman
		/// or
		/// reasonList
		/// </exception>
		protected ThoughtState SetState([NotNull] Pawn formerHuman, [NotNull] List<string> reasonList)
		{
			if (formerHuman == null) throw new ArgumentNullException(nameof(formerHuman));
			if (reasonList == null) throw new ArgumentNullException(nameof(reasonList));
			var qSapience = formerHuman.GetQuantizedSapienceLevel();

			if (qSapience == null)
			{
				Log.Error($"trying to set the thought state of {def.defName} for {formerHuman.Name} but they are not a former human! this should not be possible");
				return false;
			}


			var index = GetStageIndex(qSapience.Value);
			return ThoughtState.ActiveAtStage(index, reasonList.ToCommaList(true));
		}
	}

}