// MutaniteCentrifuge.cs created by Iron Wolf for Pawnmorph on 03/25/2020 6:14 AM
// last updated 03/25/2020  6:14 AM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Buildings
{
	/// <summary>
	///     building class for the mutanite centrifuge
	/// </summary>
	/// <seealso cref="Verse.Building" />
	public class MutaniteCentrifuge : Building
	{

		static MutaniteCentrifuge()
		{
			_buffer = new RWRaycastHit[20];
		}

		/// <summary>
		///     the running mode of the centrifuge
		/// </summary>
		public enum RunningMode
		{
			/// <summary>
			///     normal production
			/// </summary>
			Normal,

			/// <summary>
			///     more efficient production at a cost of mutagenic buildup
			/// </summary>
			HighYield
		}

		private const int TICKS_TO_PRODUCE = TimeMetrics.TICKS_PER_REAL_SECOND * 510;

		private const string MUTANITE_CENTRIFUGE_MODE_DESCRIPTION = "MutaniteCentrifugeRunModeDesc";
		private const string MUTANITE_CENTRIFUGE_MODE_LABEL = "MutaniteCentrifugeRunModeLabel";

		private const float BASE_MUTANITE_REQUIRED = 2.7f;

		private const float EPSILON = 0.0001f;

		/// <summary>
		/// The danger radius
		/// </summary>
		public const int DANGER_RADIUS = 5;
		private const float DEFAULT_GLOW_RADIUS = 1;

		private const float MUTAGENIC_BUILDUP_RATE = 0.02f;

		[NotNull] private readonly List<Building_Storage> _hoppers = new List<Building_Storage>();

		[NotNull] private readonly List<(Thing thing, int rmCount)> _rmCache = new List<(Thing thing, int rmCount)>();
		private int _timeCounter;


		private RunningMode _mode;

		private List<IntVec3> _cachedAdjCellsCardinal;

		private bool _producing;

		private CompFlickable _flickable;

		private CompPowerTrader _trader;

		private ColorInt _initialColor;

		private PipeSystem.CompResource _drawer;

		private float _glowRadius;

		private string _cachedInactiveString;

		private float _storedSlurry;

		private Command_Toggle _highYieldCommand;


		/// <summary>
		///     Gets or sets the current mode.
		/// </summary>
		/// <value>
		///     The current mode.
		/// </value>
		public RunningMode CurrentMode
		{
			get => _mode;
			set
			{
				if (_mode != value) SetRunningMode(value);
			}
		}

		/// <summary>
		///     Gets a value indicating whether this <see cref="MutaniteCentrifuge" /> is enabled.
		/// </summary>
		/// <value>
		///     <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled => PowerComp?.PowerOn == true && FlickableComp?.SwitchIsOn != false;

		/// <summary>
		///     Gets the adjacent cells cardinal in bounds.
		/// </summary>
		/// <value>
		///     The adjacent cells cardinal in bounds.
		/// </value>
		[NotNull]
		public List<IntVec3> AdjCellsCardinalInBounds
		{
			get
			{
				if (_cachedAdjCellsCardinal == null)
					_cachedAdjCellsCardinal = (from c in GenAdj.CellsAdjacentCardinal(this)
											   where c.InBounds(Map)
											   select c).ToList();
				return _cachedAdjCellsCardinal;
			}
		}

		private static ColorInt Clear { get; } = new ColorInt(0, 0, 0, 0);

		[CanBeNull] private CompFlickable FlickableComp => _flickable ?? (_flickable = GetComp<CompFlickable>());

		private new CompPowerTrader PowerComp => _trader ?? (_trader = GetComp<CompPowerTrader>());

		private bool ShouldBeOn => FlickableComp?.SwitchIsOn != false && !this.IsBrokenDown();

		private bool IsOn => ShouldBeOn && PowerComp?.PowerOn != false;

		private CompGlower Glower => GetComp<CompGlower>();


		/// <summary>
		///     Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _timeCounter, "timeCounter");
			Scribe_Values.Look(ref _mode, nameof(CurrentMode));
			Scribe_Values.Look(ref _producing, "producing");
			Scribe_Values.Look(ref _storedSlurry, "storedSlurry");
		}

		/// <summary>
		///     Gets the gizmos.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;

			if (Faction == Faction.OfPlayer)
			{
				if (_highYieldCommand == null)
				{
					_highYieldCommand = new Command_Toggle
					{
						hotKey = KeyBindingDefOf.Command_TogglePower,
						defaultLabel = MUTANITE_CENTRIFUGE_MODE_LABEL.Translate(),
						defaultDesc = MUTANITE_CENTRIFUGE_MODE_DESCRIPTION.Translate(),
						isActive = () => CurrentMode == RunningMode.HighYield,
						toggleAction = ToggleRunMode,
						icon = PMTextures.MutagenicHazardHigh,
					};
				}

				yield return _highYieldCommand;
			}
		}

		/// <summary>
		///     Gets the inspect string.
		/// </summary>
		/// <returns></returns>
		public override string GetInspectString()
		{
			var builder = new StringBuilder();
			builder.AppendLine(base.GetInspectString());
			builder.Append(GetModeString(CurrentMode));

			if (_producing)
				builder.Append("\n"
							 + "CentrifugeRunningText".Translate(((float)_timeCounter / GetTimeNeeded()).ToStringPercent()));
			else
				builder.Append("\n" + GetInactiveString());

			return builder.ToString();
		}

		/// <summary>
		///     set up the object on spawn
		/// </summary>
		/// <param name="map">The map.</param>
		/// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);

			_flickable = GetComp<CompFlickable>();
			_initialColor = Glower?.Props?.glowColor ?? Clear;
			_glowRadius = GetGlowRadius(CurrentMode);
			_drawer = GetComp<PipeSystem.CompResource>();

			if (Glower != null)
			{
				if (!_producing)
				{
					if (PowerComp != null) PowerComp.PowerOn = false;
					Glower.Props.glowRadius = 0;
				}
				else
				{
					Glower.Props.glowRadius = _glowRadius;
				}

				Glower.UpdateLit(Map);
			}
		}

		/// <summary>
		///     called every tick
		/// </summary>
		public override void Tick()
		{
			base.Tick();
			if (!IsOn)
				return;

			if (_producing)
			{
				if (CurrentMode == RunningMode.HighYield && this.IsHashIntervalTick(20))
					DoMutagenicBuildup();

				_timeCounter++;

				if (_timeCounter >= GetTimeNeeded())
					ProduceMutanite();
			}
			else if (this.IsHashIntervalTick(100))
			{

				if (_drawer != null)
				{
					float val;
					switch (_mode)
					{
						case RunningMode.Normal:
							val = 1;
							break;

						case RunningMode.HighYield:
							val = 2;
							break;

						default:
							return;
					}

					float neededAmount = GetRequiredMutaniteCount(CurrentMode);
					// Is there enough slurry in the network to top up?
					if (_drawer.PipeNet.Stored > val)
					{
						_drawer.PipeNet.DrawAmongStorage(val, _drawer.PipeNet.storages);
						_storedSlurry += val;

						if (neededAmount < _storedSlurry)
							StartProduction();

						_cachedInactiveString = null;
					}
				}
			}
		}

		private void DoMutagenicBuildup()
		{
			IEnumerable<Thing> things = GenRadial.RadialDistinctThingsAround(Position, Map, DANGER_RADIUS, true).MakeSafe();
			MutagenDef mutagen = MutagenDefOf.defaultMutagen;
			foreach (Thing thing in things)
			{
				if (!(thing is Pawn pawn)) continue;
				if (!mutagen.CanInfect(pawn)) return;

				TryMutatePawn(pawn);
			}
		}

		[NotNull] private static readonly RWRaycastHit[] _buffer;

		private void TryMutatePawn(Pawn pawn)
		{

			var hits = RWRaycast.RaycastAllNoAlloc(Map, Position, pawn.Position, _buffer, RaycastTargets.Impassible);


			if (hits == 0)
			{
				MutagenicBuildupUtilities.AdjustMutagenicBuildup(def, pawn, MUTAGENIC_BUILDUP_RATE);
				return;
			}

			int tHits = 0;

			for (int i = 0; i < hits; i++)
			{
				if (_buffer[i].hitThing != this) tHits++;
			}

			var p0 = Position.ToIntVec2.ToVector3(); //need to get rid of y which may be different but shouldn't be taken into account 
			var p1 = pawn.Position.ToIntVec2.ToVector3();
			var dist = (p0 - p1).magnitude + tHits * 1.5f;

			float rate = MUTAGENIC_BUILDUP_RATE / (Mathf.Pow(2, dist));


			if (rate <= EPSILON) return;

			MutagenicBuildupUtilities.AdjustMutagenicBuildup(def, pawn, MUTAGENIC_BUILDUP_RATE);

		}

		private void EndProduction()
		{
			if (PowerComp != null) PowerComp.PowerOn = false;
			if (Glower != null)
				Glower.Props.glowColor = Clear;
			_producing = false;
			UpdateGlower();
		}


		[NotNull]
		private IEnumerable<Thing> GetFeed()
		{
			foreach (Building_Storage buildingStorage in _hoppers)
				foreach (Thing thing in (buildingStorage.slotGroup?.HeldThings).MakeSafe())
					if (IsAcceptableFeedstock(thing))
						yield return thing;
		}

		private float GetGlowRadius(RunningMode value)
		{
			float radius;
			switch (value)
			{
				case RunningMode.Normal:
					radius = DEFAULT_GLOW_RADIUS;
					break;
				case RunningMode.HighYield:
					radius = DANGER_RADIUS;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
			}

			const float scalar = 2.3f;
			float r = radius * scalar;
			return r;
		}


		private string GetInactiveString()
		{
			if (_cachedInactiveString != null) return _cachedInactiveString;
			float needed = GetRequiredMutaniteCount(CurrentMode);
			float total = GetTotalMutaniteFeed();
			ThingDef repThing = null;
			var mxCount = 0;
			foreach (Thing thing in GetFeed())
			{
				if (thing.GetStatValue(PMStatDefOf.MutaniteConcentration) <= 0) continue;
				if (mxCount < thing.stackCount)
				{
					repThing = thing.def;
					mxCount = thing.stackCount;
				}
			}

			repThing = repThing ?? PMThingDefOf.MechaniteSlurry;
			needed -= total;
			needed = Mathf.Max(0, needed);
			if (needed <= 0)
			{
				_cachedInactiveString = "MutaniteCentrifugeReadyToStart".Translate();
				return _cachedInactiveString;
			}

			var percentFilled = _storedSlurry / GetRequiredMutaniteCount(CurrentMode);

			_cachedInactiveString =
				"PMMutaniteCentrifugeNotActive".Translate(percentFilled.ToStringByStyle(ToStringStyle.PercentTwo));
			return _cachedInactiveString;
		}

		private string GetModeString(RunningMode mode)
		{
			switch (mode)
			{
				case RunningMode.Normal:
					return "CentrifugeNormalYield".Translate();
				case RunningMode.HighYield:
					return "CentrifugeHighYield".Translate();
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		private float GetRequiredMutaniteCount(RunningMode currentMode)
		{
			switch (currentMode)
			{
				case RunningMode.Normal:
					return BASE_MUTANITE_REQUIRED;
				case RunningMode.HighYield:
					return Mathf.Max(1, BASE_MUTANITE_REQUIRED / 1.5f);
				default:
					throw new ArgumentOutOfRangeException(nameof(currentMode), currentMode, null);
			}
		}

		private int GetTimeNeeded()
		{
			switch (CurrentMode)
			{
				case RunningMode.Normal:
					return TICKS_TO_PRODUCE;
				case RunningMode.HighYield:
					return Mathf.CeilToInt(TICKS_TO_PRODUCE * 2f / 3);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private float GetTotalMutaniteFeed()
		{
			float count = 0;
			foreach (Thing thing in GetFeed()) count += thing.GetStatValue(PMStatDefOf.MutaniteConcentration) * thing.stackCount;

			return count;
		}

		private bool IsAcceptableFeedstock([NotNull] Thing thing)
		{
			return thing.GetStatValue(PMStatDefOf.MutaniteConcentration) > EPSILON;
		}

		private RunningMode NextMode(RunningMode mode)
		{
			const int max = 1;
			return (RunningMode)(((int)mode + 1) % (max + 1));
		}

		private void ProduceMutanite()
		{
			Thing thing = ThingMaker.MakeThing(PMThingDefOf.Mutanite);
			GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
			EndProduction();
		}

		private void SetRunningMode(RunningMode value)
		{
			_mode = value;

			CompGlower glower = Glower;
			if (glower == null) return;
			_glowRadius = GetGlowRadius(value);
			if (_producing)
			{
				if (FlickableComp?.SwitchIsOn != false) PowerComp.PowerOn = true;

				glower.Props.glowRadius = _glowRadius; //increase size so it shows the danger zone 
				UpdateGlower();
			}
			else
			{
				glower.Props.glowRadius = 0;
			}
		}



		private void StartProduction()
		{
			_storedSlurry -= GetRequiredMutaniteCount(_mode);
			_producing = true;
			_timeCounter = 0;
			if (Glower != null)
			{
				Glower.Props.glowColor = _initialColor;
				Glower.Props.glowRadius = _glowRadius;
			}

			UpdateGlower();
		}


		private void ToggleRunMode()
		{
			CurrentMode = NextMode(CurrentMode);
		}




		private void UpdateGlower()
		{
			if (PowerComp.PowerOn)
				PowerComp.PowerOn = false;
			else return;

			if (Glower == null) return;
			Glower.UpdateLit(Map);
			PowerComp.PowerOn = true;
		}
	}
}