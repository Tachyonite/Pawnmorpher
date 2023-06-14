// FormerHuman.cs created by Iron Wolf for Pawnmorph on 11/27/2019 1:12 PM
// last updated 11/27/2019  1:12 PM

using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     hediff class for the former human hediff
	/// </summary>
	/// <seealso cref="Verse.HediffWithComps" />
	public class FormerHuman : Hediff_Descriptive
	{
		private int? _lastStage;

		private bool _subscribed;

		private bool _initialized;

		private string _labelCached;

		/// <summary>
		///     Gets the label in brackets.
		/// </summary>
		/// <value>
		///     The label in brackets.
		/// </value>
		public override string LabelInBrackets
		{
			get
			{
				if (string.IsNullOrEmpty(_labelCached))
				{
					SapienceLevel? saLabel = pawn?.GetQuantizedSapienceLevel();
					_labelCached = saLabel?.GetLabel() ?? "unknown";
				}

				return _labelCached;
			}
		}

		/// <summary>Exposes the data.</summary>
		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref _lastStage, nameof(_lastStage));
			Scribe_Values.Look(ref _labelCached, nameof(_labelCached));

			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				SubscribeToEvents();
				SetLabel();
			}
		}

		/// <summary>
		///     called after the hediff is added
		/// </summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void PostAdd(DamageInfo? dinfo)
		{
			base.PostAdd(dinfo);
			if (pawn.Name == null)
				pawn.Name = new NameSingle(pawn.Label); //make sure they always have a name, is needed for sapients 
			SubscribeToEvents();
			SetLabel();
		}

		/// <summary>
		///     called when the hediff is removed
		/// </summary>
		public override void PostRemoved()
		{
			base.PostRemoved();
			if (_subscribed)
			{
				var needControl = pawn?.needs?.TryGetNeed<Need_Control>();
				if (needControl != null) needControl.SapienceLevelChanged -= SapienceLevelChanged;
			}
		}


		/// <summary>called after the pawn's tick method.</summary>
		public override void PostTick()
		{
			base.PostTick();

			if (_lastStage != CurStageIndex) OnStageChanges();

			if (pawn.needs == null) return; //dead pawns don't have needs for some reason 
			if (!_initialized) TryInit();
		}

		private void OnStageChanges()
		{
			if (CurStage is IExecutableStage exStage) exStage.EnteredStage(this);
		}

		private void SapienceLevelChanged(Need_Control sender, Pawn pawn1, SapienceLevel oldLevel, SapienceLevel currentLevel)
		{
			var idx = (int)currentLevel;
			if (idx < def.stages.Count) SetStage(idx);

			if (pawn.IsHumanlike())
			{
				FormerHumanUtilities.ResetTraining(pawn);
			}

			SetLabel(currentLevel);
		}

		private void SetLabel(SapienceLevel level)
		{
			_labelCached = level.GetLabel();
		}

		private void SetLabel()
		{
			_labelCached = pawn?.GetQuantizedSapienceLevel()?.GetLabel() ?? "unknown";
		}

		private void SetStage(int index)
		{
			if (index == 0)
			{
				Severity = (def.stages[0].minSeverity + def.minSeverity) / 2;
				return;
			}

			if (index == def.stages.Count - 1)
			{
				Severity = (def.stages[def.stages.Count - 1].minSeverity + def.maxSeverity) / 2;
				return;
			}

			Severity = (def.stages[index].minSeverity + def.stages[index + 1].minSeverity) / 2;
		}

		/// <summary>
		/// Gets a value indicating whether this instance should be removed.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance should be removed; otherwise, <c>false</c>.
		/// </value>
		public override bool ShouldRemove => false;

		private void SubscribeToEvents()
		{
			if (_subscribed) return;
			var controlNeed = pawn.needs?.TryGetNeed<Need_Control>();
			if (controlNeed != null)
			{
				_subscribed = true;
				_initialized = true;
				controlNeed.SapienceLevelChanged += SapienceLevelChanged;
				var sLevel = pawn.GetQuantizedSapienceLevel() ?? SapienceLevel.Sapient;
				var idx = Mathf.Min(def.stages.Count - 1, (int)sLevel);
				SetStage(idx);
			}
		}

		private void TryInit()
		{
			SubscribeToEvents();
			SetLabel();

		}
	}
}