// MutaniteCentrifuge.cs created by Iron Wolf for Pawnmorph on 03/25/2020 6:14 AM
// last updated 03/25/2020  6:14 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.Buildings
{
    /// <summary>
    ///     building class for the mutanite centrifuge
    /// </summary>
    /// <seealso cref="Verse.Building" />
    public class MutaniteCentrifuge : Building
    {
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

        private const int TICKS_TO_PRODUCE = TimeMetrics.TICKS_PER_REAL_SECOND * 110;

        private const string MUTANITE_CENTRIFUGE_MODE_DESCRIPTION = "MutaniteCentrifugeRunModeDesc";
        private const string MUTANITE_CENTRIFUGE_MODE_LABEL = "MutaniteCentrifugeRunModeLabel";

        private const float BASE_MUTANITE_REQUIRED = 1.1f;

        private const float EPSILON = 0.0001f;

        private const int DANGER_RADIUS = 3;

        private const float MUTAGENIC_BUILDUP_RATE = 0.01f;

        [NotNull] private readonly List<Building_Storage> _hoppers = new List<Building_Storage>();

        [NotNull] private readonly List<(Thing thing, int rmCount)> _rmCache = new List<(Thing thing, int rmCount)>();
        private bool _running;
        private int _timeCounter;


        private float _initialRadius;

        private RunningMode _mode;

        private List<IntVec3> _cachedAdjCellsCardinal;

        private bool _producing;

        private CompFlickable _flickable;

        private CompPowerTrader _trader;

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
            Scribe_Values.Look(ref _running, "running");
            Scribe_Values.Look(ref _timeCounter, "timeCounter");
            Scribe_Values.Look(ref _mode, nameof(CurrentMode));
            Scribe_Values.Look(ref _producing, "producing");
        }


        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;

            var toggleGizmo = new Command_Action
            {
                action = ToggleRunMode,
                defaultDesc = MUTANITE_CENTRIFUGE_MODE_DESCRIPTION.Translate(),
                defaultLabel = MUTANITE_CENTRIFUGE_MODE_LABEL.Translate()
            };
            yield return toggleGizmo;
        }


        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            _initialRadius = Glower?.Props?.glowRadius ?? 0;
            _flickable = GetComp<CompFlickable>();
        }

        /// <summary>
        ///     called every tick
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            if (!ShouldBeOn) return;
            if (_producing)
            {
                if (!IsOn) return;
                if (CurrentMode == RunningMode.HighYield && this.IsHashIntervalTick(20)) DoMutagenicBuildup();

                _timeCounter++;
                if (_timeCounter >= GetTimeNeeded()) ProduceMutanite();
                return;
            }


            if (this.IsHashIntervalTick(30)) _hoppers.Clear();

            //introduce a delay between starting and ending production 
            if (this.IsHashIntervalTick(20))
                if (TryStartProduction())
                    StartProduction();
        }

        private void DoMutagenicBuildup()
        {
            IEnumerable<Thing> things = GenRadial.RadialDistinctThingsAround(Position, Map, DANGER_RADIUS, true).MakeSafe();
            MutagenDef mutagen = MutagenDefOf.defaultMutagen;
            foreach (Thing thing in things)
            {
                if (!(thing is Pawn pawn)) continue;
                if (!mutagen.CanInfect(pawn)) return;
                float stat = pawn.GetMutagenicBuildupMultiplier();
                if (stat * MUTAGENIC_BUILDUP_RATE < EPSILON) continue;
                HealthUtility.AdjustSeverity(pawn, MorphTransformationDefOf.MutagenicBuildup, stat * MUTAGENIC_BUILDUP_RATE);
            }
        }

        private void EndProduction()
        {
            if (PowerComp != null) PowerComp.PowerOn = false;

            _producing = false;
        }


        [NotNull]
        private IEnumerable<Thing> GetFeed()
        {
            foreach (Building_Storage buildingStorage in _hoppers)
            foreach (Thing thing in (buildingStorage.slotGroup?.HeldThings).MakeSafe())
                if (IsAcceptableFeedstock(thing))
                    yield return thing;
        }


        [NotNull]
        private IEnumerable<Building> GetHoppers([NotNull] Pawn reacher)
        {
            for (var i = 0; i < AdjCellsCardinalInBounds.Count; i++)
            {
                Building edifice = AdjCellsCardinalInBounds[i].GetEdifice(Map);
                if (edifice != null
                 && (edifice.def == PMThingDefOf.MutagenHopper || edifice.def == ThingDefOf.Hopper)
                 && reacher.CanReach(edifice, PathEndMode.Touch, Danger.Deadly)) yield return edifice;
            }
        }

        [NotNull]
        private IEnumerable<Building> GetHoppers()
        {
            for (var i = 0; i < AdjCellsCardinalInBounds.Count; i++)
            {
                Building edifice = AdjCellsCardinalInBounds[i].GetEdifice(Map);
                if (edifice != null && edifice.def == PMThingDefOf.MutagenHopper) yield return edifice;
            }
        }

        private float GetRequiredMutaniteCount(RunningMode currentMode)
        {
            switch (currentMode)
            {
                case RunningMode.Normal:
                    return BASE_MUTANITE_REQUIRED;
                    break;
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

        private bool IsAcceptableFeedstock([NotNull] Thing thing)
        {
            return thing.GetStatValue(PMStatDefOf.MutaniteConcentration) > EPSILON;
        }

        private RunningMode NextMode(RunningMode mode)
        {
            const int max = 1;
            return (RunningMode) (((int) mode + 1) % (max + 1));
        }

        private void ProduceMutanite()
        {
            Thing thing = ThingMaker.MakeThing(PMThingDefOf.Mutanite);
            GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
            EndProduction();
        }

        private void SearchForHoppers()
        {
            _hoppers.Clear();
            _hoppers.AddRange(GetHoppers().OfType<Building_Storage>());
        }

        private void SetRunningMode(RunningMode value)
        {
            float radius;
            CompGlower glower = Glower;
            if (glower == null) return;
            switch (value)
            {
                case RunningMode.Normal:
                    radius = _initialRadius;
                    break;
                case RunningMode.HighYield:
                    radius = DANGER_RADIUS;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            glower.Props.glowRadius = radius;
        }

        private void StartProduction()
        {
            if (PowerComp != null)
                PowerComp.PowerOn = true;

            _producing = true;
            _timeCounter = 0;
        }

        private void ToggleRunMode()
        {
            CurrentMode = NextMode(CurrentMode);
        }

        private bool TryStartProduction()
        {
            if (_hoppers.Count == 0) SearchForHoppers();
            _rmCache.Clear();

            float minAmount = GetRequiredMutaniteCount(CurrentMode);
            var curAmount = 0f;
            var canProduce = false;
            foreach (Thing thing in GetFeed())
            {
                float concentration = thing.GetStatValue(PMStatDefOf.MutaniteConcentration);
                float needed = minAmount - curAmount;
                int take = Mathf.FloorToInt(concentration * thing.stackCount / needed);
                take = Mathf.Min(take, thing.stackCount);
                _rmCache.Add((thing, take));
                curAmount += concentration * take;
                if (minAmount - curAmount < EPSILON)
                {
                    canProduce = true;
                    break;
                }
            }

            if (canProduce)
                foreach ((Thing thing, int rmCount) in _rmCache)
                    thing.SplitOff(rmCount);
            _rmCache.Clear();

            return canProduce;
        }
    }
}