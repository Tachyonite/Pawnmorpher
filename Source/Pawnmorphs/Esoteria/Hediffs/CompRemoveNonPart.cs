// Comp_RemoveNonMorphPart.cs created by Iron Wolf for Pawnmorph on 08/15/2021 8:18 AM
// last updated 08/15/2021  8:18 AM

using System;
using System.Linq;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// comp that removes mutations from a pawn while they are transforming based on the morph they
	/// are turning into 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	/// <seealso cref="ITfHediffObserverComp" />
	public class CompRemoveNonPart : HediffComp, ITfHediffObserverComp
	{
		private AnimalClassBase _currentMorph;


		/// <summary>
		/// called when the morph hediff is about to start visiting body parts.
		/// </summary>
		public void Init()
		{
			_currentMorph = (parent.CurStage as MorphTransformationStage)?.morph;
		}

		/// <summary>
		/// called when the hediff stage changes.
		/// </summary>
		public void StageChanged()
		{
			_currentMorph = (parent.CurStage as MorphTransformationStage)?.morph;
		}

		private CompProps_RemoveNonMorphPart _props;

		CompProps_RemoveNonMorphPart Props
		{
			get
			{
				if (_props == null)
				{
					try
					{
						_props = (CompProps_RemoveNonMorphPart)props;
					}
					catch (InvalidCastException e)
					{
						Log.Error($"unable to cast {props.GetType().Name} to {nameof(CompProps_RemoveNonMorphPart)}\n{e}");
					}
				}

				return _props;
			}
		}

		/// <summary>
		/// called when the morph tf observes the give body part record on the given pawn
		/// </summary>
		/// <param name="record">The record observed. if null a observing whole body hediffs</param>
		public void Observe(BodyPartRecord record)
		{
			if (_currentMorph == null) return;

			var mutations = parent?.pawn?.GetAllMutations(); //no optimized way to get all hediffs per part 
			if (mutations == null) return;//if this becomes a performance issue see about caching mutations/part in mutation tracker 
			foreach (Hediff_AddedMutation mutation in mutations)
			{
				if (mutation.Part != record) continue;

				if (!_currentMorph.GetAllMutationIn().Contains(mutation.def) && Rand.Chance(Props?.removeChance ?? 0.4f))
				{
					//should certain mutations be immune from this?
					mutation.MarkForRemoval();
				}

			}
		}

		/// <summary>
		/// called after the given mutation is added to the pawn.
		/// </summary>
		/// <param name="newMutation">The new mutation.</param>
		public void MutationAdded(Hediff_AddedMutation newMutation)
		{
			//empty 
		}
	}

	/// <summary>
	/// properties for <see cref="CompRemoveNonPart"/>
	/// </summary>
	/// <seealso cref="Verse.HediffCompProperties" />
	public class CompProps_RemoveNonMorphPart : HediffCompProperties
	{
		/// <summary>
		/// The remove chance
		/// </summary>
		public float removeChance = 0.4f;

		/// <summary>
		/// Initializes a new instance of the <see cref="CompProps_RemoveNonMorphPart"/> class.
		/// </summary>
		public CompProps_RemoveNonMorphPart()
		{
			compClass = typeof(CompRemoveNonPart);
		}
	}
}