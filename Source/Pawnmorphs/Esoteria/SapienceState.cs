// SapienceState.cs created by Iron Wolf for Pawnmorph on 04/24/2020 7:37 AM
// last updated 04/24/2020  7:37 AM

using JetBrains.Annotations;
using Pawnmorph.ThingComps;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     a specific state c pawn's 'sapience/mind' can be in, like FormerHuman, Animalistic, ect.
	/// </summary>
	/// <seealso cref="Verse.IExposable" />
	public abstract class SapienceState : IExposable
	{
		private SapienceStateDef _def;

		/// <summary>
		/// Gets the pawn this state is for 
		/// </summary>
		/// <value>
		/// The pawn.
		/// </value>
		public Pawn Pawn => Tracker?.Pawn;

		/// <summary>
		/// Gets a value indicating whether this state makes the pawn count as a 'former human'.
		/// </summary>
		/// <value>
		///   <c>true</c> if this state makes the pawn count as a 'former human'; otherwise, <c>false</c>.
		/// </value>
		public abstract bool IsFormerHuman { get; }

		void IExposable.ExposeData()
		{
			Scribe_Defs.Look(ref _def, nameof(StateDef));
			ExposeData();
		}


		/// <summary>
		/// Adds the or remove dynamic components.
		/// </summary>
		public abstract void AddOrRemoveDynamicComponents();

		/// <summary>
		/// Gets the current sapience level
		/// </summary>
		/// <value>
		/// The current sapience.
		/// </value>
		protected SapienceLevel CurrentSapience
		{
			get { return Tracker?.SapienceLevel ?? SapienceLevel.PermanentlyFeral; }
		}

		/// <summary>
		/// called after every tick 
		/// </summary>
		public abstract void Tick();

		/// <summary>
		///     Gets the current intelligence.
		/// </summary>
		/// <value>
		///     The current intelligence.
		/// </value>
		public abstract Intelligence CurrentIntelligence { get; }


		/// <summary>
		///     Gets the state definition.
		/// </summary>
		/// <value>
		///     The state definition.
		/// </value>
		[NotNull]
		public SapienceStateDef StateDef => _def;

		/// <summary>
		///     Gets the tracker.
		/// </summary>
		/// <value>
		///     The tracker.
		/// </value>
		public SapienceTracker Tracker { get; internal set; }


		/// <summary>
		///     called when a pawn enters this sapience state
		/// </summary>
		public virtual void Enter()
		{
			if (StateDef.forcedHediff != null)
			{
				Hediff hediff = HediffMaker.MakeHediff(StateDef.forcedHediff, Pawn);
				hediff.Severity = 1;

				Pawn.health.AddHediff(hediff);
			}
		}



		/// <summary>
		/// called when the pawn exits this state 
		/// </summary>
		public virtual void Exit()
		{
			if (StateDef.forcedHediff != null)
			{
				Hediff hediff = Pawn.health.hediffSet.GetFirstHediffOfDef(StateDef.forcedHediff);
				if (hediff != null)
					Pawn.health.RemoveHediff(hediff);
			}
		}

		/// <summary>
		///     Initializes this instance with the specified sapience tracker.
		/// </summary>
		/// <param name="sapienceTracker">The sapience tracker.</param>
		public void Init([NotNull] SapienceTracker sapienceTracker)
		{
			Tracker = sapienceTracker;
			Init();
		}

		/// <summary>
		///     called to save/load all data.
		/// </summary>
		protected abstract void ExposeData();

		/// <summary>
		///     Initializes this instance.
		/// </summary>
		/// this is always called before enter and after loading a pawn
		protected abstract void Init();

		/// <summary>
		/// clean up the pawn when making them feral .
		/// </summary>
		protected void MakeFeral()
		{
			var restriction = Pawn.foodRestriction;
			var curRestriction = restriction?.CurrentFoodPolicy;
			if (curRestriction == null) return;
			restriction.CurrentFoodPolicy = null;
		}

		internal void SetDef([NotNull] SapienceStateDef def)
		{
			_def = def;
		}
	}
}