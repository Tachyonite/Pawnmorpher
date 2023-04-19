// Comp_SapientAnimal.cs modified by Iron Wolf for Pawnmorph on 12/05/2019 7:04 PM
// last updated 12/05/2019  7:04 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
	/// <summary>
	///     component for controlling instinct and mental breaks of sapient animals
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class Comp_SapientAnimal : ThingComp, IMentalStateRecoveryReceiver
	{
		private SapientAnimalMentalBreaker _mentalBreaker;

		private int _instinctLevelRaw;

		/// <summary>
		///     Gets the mental breaker.
		/// </summary>
		/// <value>
		///     The mental breaker.
		/// </value>
		[NotNull]
		public SapientAnimalMentalBreaker MentalBreaker => _mentalBreaker;

		/// <summary>
		///     Gets or sets the 'instinct level'.
		/// </summary>
		/// <value>
		///     the instinct level
		/// </value>
		public int InstinctLevel
		{
			get => _instinctLevelRaw;
			set => _instinctLevelRaw = Mathf.Max(value, 0);
		}

		[NotNull]
		private Pawn Pawn
		{
			get
			{
				try
				{
					var p = (Pawn)parent;
					if (p == null) throw new ArgumentException(nameof(parent));
					return p;
				}
				catch (InvalidCastException e)
				{
					throw new InvalidCastException($"on {parent.Label}", e);
				}
			}
		}

		/// <summary>
		///     called every tick
		/// </summary>
		public override void CompTick()
		{
			base.CompTick();
			_mentalBreaker?.Tick();
		}

		/// <summary>
		///     Handles the instinct effect.
		/// </summary>
		/// <param name="instinctEffect">The instinct effect.</param>
		public void HandleInstinctEffect([NotNull] InstinctEffector instinctEffect)
		{
			var sapienceNeed = Pawn.needs.TryGetNeed<Need_Control>();
			if (sapienceNeed == null)
			{
				Log.Error($"sapient animal {Pawn.Name?.ToStringFull ?? Pawn.LabelShort} does not have the sapience need?");
				return;
			}

			sapienceNeed.AddInstinctChange(instinctEffect.baseInstinctOffset);

			if (instinctEffect.thought != null) Pawn.TryGainMemory(instinctEffect.thought);
			if (instinctEffect.taleDef != null) TaleRecorder.RecordTale(instinctEffect.taleDef, Pawn);
		}

		/// <summary>
		///     Initializes this comp
		/// </summary>
		/// <param name="props">The props.</param>
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			_mentalBreaker = _mentalBreaker ?? new SapientAnimalMentalBreaker(Pawn);
		}


		/// <summary>
		///     call this to notify the comp that the attached pawn has recovered from the given mental state
		/// </summary>
		/// <param name="state">The state.</param>
		public void OnRecoveredFromMentalState([NotNull] MentalState state)
		{
			_mentalBreaker?.NotifyRecoveredFromMentalBreak();

			var instinctEffect = state.def.GetModExtension<InstinctEffector>();
			if (instinctEffect == null)
			{
				DebugLogUtils.Warning($"mental state {state.def.defName} has no {nameof(InstinctEffector)} but is allowed on sapient animals, is this intended?");
				return;
			}

			HandleInstinctEffect(instinctEffect);
		}

		/// <summary>
		///     called to expose this instances data.
		/// </summary>
		public override void PostExposeData()
		{
			base.PostExposeData();

			Scribe_Deep.Look(ref _mentalBreaker, "mentalBreaker", Pawn);
		}
	}
}