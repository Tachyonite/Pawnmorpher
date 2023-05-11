// Comp_AcceleratedSeverity.cs modified by Iron Wolf for Pawnmorph on 11/04/2019 6:14 PM
// last updated 11/04/2019  6:14 PM

using Pawnmorph.Utilities;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	///     hediff comp to make the hediffs severity follow a parabolic curve over time 
	/// </summary>
	/// <seealso cref="Pawnmorph.Utilities.HediffCompBase{T}" />
	public class Comp_AcceleratedSeverity : HediffCompBase<CompProperties_AcceleratedSeverity>
	{
		private float _velocity;

		/// <summary>
		/// called every tick 
		/// </summary>
		/// <param name="severityAdjustment">The severity adjustment.</param>
		public override void CompPostTick(ref float severityAdjustment)
		{

			severityAdjustment = _velocity * 1f / 60; //use delta time here to get the time in seconds since the last update  
			_velocity += -Props.A * 1f / 60f;
			_velocity = Mathf.Clamp(_velocity, -Props.V0, Props.V0);


			base.CompPostTick(ref severityAdjustment);
		}

		/// <summary>
		/// called when this comp is first made.
		/// </summary>
		public override void CompPostMake()
		{
			_velocity = Props.V0;
		}

		/// <summary>
		/// called when the other is merged into this instance's parent
		/// </summary>
		/// <param name="other">The other.</param>
		public override void CompPostMerged(Hediff other)
		{
			_velocity = Props.V0;
			base.CompPostMerged(other);
		}

		/// <summary>
		/// exposes this instance's data.
		/// </summary>
		public override void CompExposeData()
		{
			Scribe_Values.Look(ref _velocity, "velocity");
		}
	}

	/// <summary>
	/// properties for <see cref="Comp_AcceleratedSeverity"/>
	/// </summary>
	/// <seealso cref="Pawnmorph.Utilities.HediffCompPropertiesBase{T}" />
	public class CompProperties_AcceleratedSeverity : HediffCompPropertiesBase<Comp_AcceleratedSeverity>
	{
		/// <summary>
		/// the time in seconds it takes the hediff's severity to reach it's initial value 
		/// </summary>
		public float recoveryTime = 1;

		/// <summary>
		/// the maximum severity offset this comp will cause the hediff to have 
		/// </summary>
		public float peakSeverity = 0.1f;

		/// <summary>
		/// Gets the acceleration of this comp 
		/// </summary>
		/// <value>
		/// the acceleration 
		/// </value>
		public float A => 8 * peakSeverity / (recoveryTime * recoveryTime);

		/// <summary>
		/// Gets the initial velocity of this comp.
		/// </summary>
		/// <value>
		/// The initial velocity 
		/// </value>
		public float V0 => 4 * peakSeverity / recoveryTime;

	}
}