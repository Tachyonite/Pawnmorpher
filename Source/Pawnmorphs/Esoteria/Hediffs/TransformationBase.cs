// TransformationBase.cs created by Iron Wolf for Pawnmorph on 01/02/2020 12:54 PM
// last updated 01/02/2020  12:54 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.Hediffs
{
	/// <summary>abstract base class for all transformation hediffs</summary>
	/// <seealso cref="Pawnmorph.IDescriptiveHediff" />
	/// <seealso cref="Verse.Hediff" />
	public abstract class TransformationBase : Hediff_Descriptive, IMutagenicHediff
	{


		[NotNull]
		private readonly Dictionary<BodyPartDef, List<MutationEntry>> _mutationPerPartCache =
			new Dictionary<BodyPartDef, List<MutationEntry>>();

		private List<BodyPartRecord> _checkList;
		private int _curPartIndex;
		private int _curMutationIndex;

		private List<ITfHediffObserverComp> _observerCompsCache;


		/// <summary>
		/// all observer comps to notify when adding mutations and visiting parts to add mutations onto.
		/// </summary>
		/// <value>
		/// The observer comps.
		/// </value>
		[NotNull]
		protected IReadOnlyList<ITfHediffObserverComp> ObserverComps
		{
			get
			{
				if (_observerCompsCache == null)
				{
					_observerCompsCache = comps.MakeSafe().OfType<ITfHediffObserverComp>().ToList();
				}

				return _observerCompsCache;
			}
		}
		/// <summary>
		/// Gets a value indicating whether this transformation hediff blocks the race checking 
		/// </summary>
		/// <value>
		///   <c>true</c> if this transformation hediff blocks the race checking; otherwise, <c>false</c>.
		/// </value>
		public virtual bool BlocksRaceCheck => AnyMutationsInStage(CurStage);

		/// <summary>
		/// Creates a debug string for this hediff 
		/// </summary>
		/// <returns></returns>
		public override string DebugString()
		{
			StringBuilder builder = new StringBuilder(base.DebugString());
			builder.AppendLine($"{nameof(TransformationBase)}: ");
			if (FinishedAddingMutations)
				builder.AppendLine("\tFinished");
			if (_checkList == null || _checkList.Count == 0)
			{
				builder.AppendLine($"\tno parts to check");
			}
			else
			{
				if (_curPartIndex < _checkList.Count)
				{
					var nextPart = _checkList[_curPartIndex];
					builder.AppendLine($"going to check {nextPart.Label} next");
				}

				builder.AppendLine($"\t{(((float)_curPartIndex) / _checkList.Count).ToStringPercent()} done");
			}
			return builder.ToString();
		}

		/// <summary>
		/// the expected number of mutations to happen in a single day 
		/// </summary>
		public abstract float MeanMutationsPerDay { get; }

		/// <summary>Gets the available mutations.</summary>
		/// <value>The available mutations.</value>
		[NotNull]
		public abstract IEnumerable<MutationEntry> AllAvailableMutations { get; }


		/// <summary>
		///     Gets a value indicating whether this instance has finished adding mutations or not.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance has finished adding mutations; otherwise, <c>false</c>.
		/// </value>
		public bool FinishedAddingMutations
		{
			get
			{
				if (_checkList == null) return false;
				return _checkList.Count == _curPartIndex;
			}
		}

		/// <summary>Gets the minimum mutations per check.</summary>
		/// if greater then 1, every-time a mutation is possible don't stop iterating over the parts until a body part that can be mutated is found 
		/// <value>The minimum mutations per check.</value>
		protected virtual int MinMutationsPerCheck => 1;

		/// <summary>Gets a value indicating whether this instance can reset.</summary>
		/// <value><c>true</c> if this instance can reset; otherwise, <c>false</c>.</value>
		protected virtual bool CanReset => FinishedAddingMutations;

		/// <summary>Exposes the data.</summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref _checkList, nameof(_checkList), LookMode.BodyPart);
			Scribe_Values.Look(ref _curPartIndex, nameof(_curPartIndex));
			Scribe_Values.Look(ref _lastStageIndex, nameof(_lastStageIndex), -1);
			Scribe_Values.Look(ref _curMutationIndex, nameof(_curMutationIndex));
			Scribe_Values.Look(ref forceRemove, nameof(forceRemove));

			if (Scribe.mode == LoadSaveMode.PostLoadInit && _checkList == null)
			{
				ResetMutationOrder();
				ResetPossibleMutations();
			}
		}

		private bool? _canMutateCache;

		/// <summary>
		/// Gets a value indicating whether this instance can mutate the pawn.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance can mutate the pawn; otherwise, <c>false</c>.
		/// </value>
		/// this is meant as an optimization, if false the hediff won't bother looking for parts to mutate  
		protected bool CanMutatePawn
		{
			get
			{
				if (_canMutateCache == null)
				{
					var mutagen = def.GetMutagenDef();
					_canMutateCache = mutagen.CanInfect(pawn);
				}

				return _canMutateCache.Value;
			}
		}



		/// <summary>called after this hediff is added to the pawn</summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			_checkList = new List<BodyPartRecord>();
			FillPartCheckList(_checkList);

			_curPartIndex = 0;

			var mutagen = def.GetMutagenDef();
			if (!mutagen.CanInfect(pawn)) //if we somehow got a pawn that can't be mutated just remove the hediff
				forceRemove = true;         //this is because AndroidTiers was giving android mutations because reasons 


			RestartAllMutations();

		}

		private void RestartAllMutations()
		{
			var mutations = pawn?.health?.hediffSet?.hediffs?.OfType<Hediff_AddedMutation>();
			var allMutations = AllAvailableMutations.Select(m => m.mutation).Distinct().ToList();


			foreach (Hediff_AddedMutation mutation in mutations.MakeSafe())
			{

				if (!allMutations.Contains(mutation.def as MutationDef)) continue;
				var adjComp = mutation.TryGetComp<Comp_MutationSeverityAdjust>();
				adjComp?.Restart();
			}

			foreach (ITfHediffObserverComp comp in ObserverComps)
			{
				comp.Init();
			}
		}

		private int _lastStageIndex = -1;

		/// <summary>Ticks this instance.</summary>
		public override void Tick()
		{
			;
			base.Tick();
			if (CurStageIndex != _lastStageIndex)
			{
				_lastStageIndex = CurStageIndex;
				OnStageChanged(CurStage);
			}
			var mtb = 1 / Mathf.Max(0.0001f, MeanMutationsPerDay);
			if (pawn.IsHashIntervalTick(60) && Rand.MTBEventOccurs(mtb, 60000, 60)) AddMutations();

			if (pawn.IsHashIntervalTick(10000) && CanReset) ResetMutationOrder();
		}





		/// <summary>
		/// set to true if this instance should be removed before severity reaches 0 
		/// </summary>
		protected bool forceRemove;

		/// <summary>
		/// Gets a value indicating whether this instance should be removed.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance should be removed; otherwise, <c>false</c>.
		/// </value>
		public override bool ShouldRemove
		{
			get { return base.ShouldRemove || forceRemove; }
		}


		/// <summary>Fills the part check list.</summary>
		/// the check list is a list of all parts in the parents body def in the order mutations should be added
		/// <param name="checkList">The check list.</param>
		protected abstract void FillPartCheckList([NotNull] List<BodyPartRecord> checkList);

		/// <summary>Gets the available the mutations from the given stage.</summary>
		/// <param name="currentStage">The current stage.</param>
		/// <returns></returns>
		[NotNull]
		protected virtual IEnumerable<MutationEntry> GetAvailableMutations([NotNull] HediffStage currentStage)
		{
			return AllAvailableMutations;
		}

		/// <summary>Notifies this instance that the available mutations have changed.</summary>
		//was made protected under the assumption that most configuration would be done through inheritance 
		//when reworking to prefer composition, this or ResetPossibleMutations should be made public 
		protected void Notify_AvailableMutationsChanged()
		{
			ResetPossibleMutations();
		}

		/// <summary>
		///     Called when the stage changes.
		/// </summary>
		/// <param name="currentStage">The last stage.</param>
		protected virtual void OnStageChanged([CanBeNull] HediffStage currentStage)
		{
			ResetPossibleMutations();
			//TryGiveTransformations();
			if (CurStage is IExecutableStage execStage)
			{
				Pedantic($"Executing {execStage.GetType().Name} on {def.defName}");
				execStage.EnteredStage(this);
			}

			foreach (ITfHediffObserverComp comp in ObserverComps)
			{
				comp.StageChanged();
			}
		}

		/// <summary>
		/// called when the hediff is removed.
		/// </summary>
		public override void PostRemoved()
		{
			base.PostRemoved();
		}




		/// <summary>
		/// returns true if there are ny mutations in this stage 
		/// </summary>
		/// <param name="stage"></param>
		/// <returns></returns>
		protected abstract bool AnyMutationsInStage(HediffStage stage);

		/// <summary>
		/// Gets a value indicating whether there are any mutations in the current stage.
		/// </summary>
		/// <value>
		///   <c>true</c> if there are any mutations in the current stage; otherwise, <c>false</c>.
		/// </value>
		public virtual bool CurrentStageHasMutations => AnyMutationsInStage(CurStage);

		/// <summary>
		/// Gets a value indicating whether there are any transformations in the current stage.
		/// </summary>
		/// <value>
		///   <c>true</c> if there are any transformations in the current stage; otherwise, <c>false</c>.
		/// </value>
		public virtual bool CurrentStageHasTransformation => CurStage is FullTransformationStageBase;

		private void AddMutations()
		{

			if (FinishedAddingMutations)
			{
				return;
			}
			if (!AnyMutationsInStage(CurStage)) return;

			if (_mutationPerPartCache.Count == 0)
			{
				ResetPossibleMutations();
			}



			if (_curPartIndex < _checkList.Count)
				AddPartMutations();
			else
			{
				//add whole body mutations 
				int mutationsAdded = 0;

				foreach (ITfHediffObserverComp comp in ObserverComps)
				{
					comp.Observe(null); //call observers in case there is anything they do with whole body slots 
				}

				for (var index = 0; index < _wholeBodyParts.Count; index++)
				{
					MutationEntry mutationEntry = _wholeBodyParts[index];
					if (Rand.Value < mutationEntry.addChance)
					{
						MutationResult m = MutationUtilities.AddMutation(pawn, mutationEntry.mutation);
						if (m) NotifyObserversMutationsAdded(m);
						MutagenDef mutagen = def.GetMutagenDef();
						mutagen.TryApplyAspects(pawn);
						mutationsAdded++;
					}

					if (mutationsAdded >= MinMutationsPerCheck) break;
				}

				if (mutationsAdded > 0)
					OnMutationsAdded(mutationsAdded);
			}

		}

		private void NotifyObserversMutationsAdded(MutationResult m)
		{
			foreach (ITfHediffObserverComp comp in ObserverComps)
			{
				foreach (Hediff_AddedMutation mutation in m)
				{
					comp.MutationAdded(mutation);
				}
			}
		}

		/// <summary>
		/// Tries the merge with the other hediff
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public override bool TryMergeWith(Hediff other)
		{
			if (base.TryMergeWith(other))
			{
				ResetMutationOrder();
				return true;
			}

			return false;
		}


		/// <summary>
		/// Called when mutations are added the pawn.
		/// </summary>
		/// <param name="mutationsAdded">The mutations added.</param>
		protected virtual void OnMutationsAdded(int mutationsAdded)
		{

		}

		private void AddPartMutations()
		{
			var mutationsAdded = 0;
			while (_curPartIndex < _checkList.Count)
			{
				BodyPartRecord part = _checkList[_curPartIndex];
				if (!pawn.RaceProps.body.AllParts.Contains(part))
				{
					//if the pawn's race changes the mutation order may no longer be valid 
					//need to reset it and try again later 
					break;
				}

				//call the observers before anything else. they may add/remove/change mutations on this part on their own 
				foreach (ITfHediffObserverComp comp in ObserverComps)
				{
					comp.Observe(part);
				}

				var lst = _mutationPerPartCache.TryGetValue(part.def);


				if (lst == null) //end check early if there are no parts that can be added 
				{
					_curPartIndex++;
					_curMutationIndex = 0;
					continue;
				}
				//make sure we try each mutation per part 

				while (_curMutationIndex < lst.Count)
				{
					var mutation = lst[_curMutationIndex];

					//check if the mutation can actually be added 
					if (!mutation.mutation.CanApplyMutations(pawn, part))
					{
						_curMutationIndex++;
						continue;
					}
					else if (Rand.Value < mutation.addChance)
					{
						MutationResult result = MutationUtilities.AddMutation(pawn, mutation.mutation, part);
						if (result) //make sure the mutation was actually added before doing this 
						{
							NotifyObserversMutationsAdded(result);

							mutationsAdded = NewMethod(mutationsAdded);
						}
					}
					else if (mutation.blocks)
					{
						return; //wait here until the blocking mutation is added 
					}

					_curMutationIndex++;

					//check if we added enough mutations to break early 
					if (mutationsAdded >= MinMutationsPerCheck)
						goto loopBreak;//break both loops 

				}

				_curPartIndex++; //increment after so mutations can 'block'
				_curMutationIndex = 0; //reset the mutation index every time we move up a part 

				//check if we have added enough mutations to end the loop early 
				if (mutationsAdded >= MinMutationsPerCheck) break;
			}

		loopBreak: //label so we can break both loops 

			if (mutationsAdded > 0)
			{
				OnMutationsAdded(mutationsAdded);
			}
		}

		private int NewMethod(int mutationsAdded)
		{
			var mutagen = def.GetMutagenDef();
			mutagen.TryApplyAspects(pawn);
			mutationsAdded++;
			return mutationsAdded;
		}

		/// <summary>
		/// Resets the mutation order.
		/// </summary>
		protected void ResetMutationOrder()
		{
			_checkList = _checkList ?? new List<BodyPartRecord>();
			_checkList.Clear();
			_curPartIndex = 0;
			FillPartCheckList(_checkList);
		}

		[NotNull]
		private readonly List<MutationEntry> _wholeBodyParts = new List<MutationEntry>();

		private void ResetPossibleMutations()
		{
			try
			{
				IEnumerable<MutationEntry> mutations = GetAvailableMutations(CurStage);
				_mutationPerPartCache.Clear();
				foreach (var entry in mutations) //fill a lookup dict 
					foreach (BodyPartDef possiblePart in entry.mutation.parts.MakeSafe())
						if (_mutationPerPartCache.TryGetValue(possiblePart, out var lst))
						{
							lst.Add(entry);
						}
						else
						{
							lst = new List<MutationEntry> { entry };
							_mutationPerPartCache[possiblePart] = lst;
						}
				_wholeBodyParts.Clear();
				foreach (MutationEntry availableMutation in GetAvailableMutations(CurStage))
				{
					if (availableMutation.mutation.parts == null)
					{
						_wholeBodyParts.Add(availableMutation);
					}
				}

				foreach (ITfHediffObserverComp comp in ObserverComps)
				{
					comp.Init();
				}
			}
			catch (NullReferenceException e)
			{
				throw new NullReferenceException($"null reference exception while trying reset mutations! are all mutations set in {def.defName}?", e);
			}
		}

		/// <summary>
		/// Marks this hediff removal.
		/// </summary>
		/// this is needed because Rimworld is touchy about removing hediffs. best to not do it manually and call this,
		/// the HediffTracker will then remove this hediff next tick once all hediffs are no longer running any code 
		public void MarkForRemoval()
		{
			forceRemove = true;
		}
	}
}