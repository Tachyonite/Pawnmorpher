// MutaChamber.cs created by Iron Wolf for Pawnmorph on 07/26/2020 7:50 PM
// last updated 07/26/2020  7:50 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Pawnmorph.User_Interface;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Building_Casket" />
    public class MutaChamber : Building_Casket
    {
        private const int REFUEL_CHECK_TIMER = 100; 
        
        private const float TF_ANIMAL_DURATION = 1.5f; //units are in days 
        private const float MIN_TRANSFORMATION_TIME = 0.5f * 60000; //minimum transformation time in ticks
        private const string PART_PICKER_GIZMO_LABEL = "PMPartPickerGizmo";
        private const string MERGING_GIZMO_LABEL = "PMMergeGizmo";

        private static List<PawnKindDef> _randomAnimalCache;
        private int _timer = 0;
        int _curMutationIndex = -1; 
        private bool _initialized;

        private ChamberState _innerState = ChamberState.WaitingForPawn;

        private ChamberUse _currentUse;


        

        private CompRefuelable _refuelable;

        private CompFlickable _flickable;

        private AnimalSelectorComp _aSelector;

        private Gizmo _ppGizmo;

        private Gizmo _mergingGizmo;

        private PawnKindDef _targetAnimal;
        private int _lastTotal = 0;


        private Gizmo _debugFinishGizmo;


        private ColorInt Clear { get; } = new ColorInt(0,0,0,0);
        private ColorInt GlowColor { get; } = new ColorInt(0,255,0,255); 

        [NotNull]
        private IReadOnlyList<MutationDef> AnimalMutations => _targetAnimal?.GetAllMutationsFrom() ?? Array.Empty<MutationDef>();


        /// <summary>
        /// Ejects the contents.
        /// </summary>
        public override void EjectContents()
        {
            base.EjectContents();
            ResetChamber();
        }

        private CompPowerTrader _power;

        bool HasPower
        {
            get { return PowerCompTrader.PowerOn; }
        }

        CompPowerTrader  PowerCompTrader
        {
            get
            {
                if (_power == null)
                {
                    _power = GetComp<CompPowerTrader>();
                }

                return _power; 
            }
        }


        private CompGlower _glower;

        [CanBeNull]
        CompGlower Glower
        {
            get
            {
                if (_glower == null)
                {
                    _glower = GetComp<CompGlower>();
                }

                return _glower; 
            }
        }

        [NotNull]
        Gizmo DebugFinishGizmo
        {
            get
            {
                if (_debugFinishGizmo == null)
                {
                    _debugFinishGizmo = new Command_Action()
                    {
                        defaultLabel = "Debug Finish Chamber",
                        action = DebugFinishChamber
                    };
                }

                return _debugFinishGizmo; 
            }
        }

        private void DebugFinishChamber()
        {
            _timer = 0; 
        }


        /// <summary>
        ///     Gets the current use.
        /// </summary>
        /// <value>
        ///     The current use.
        /// </value>
        public ChamberUse CurrentUse => _currentUse;

        /// <summary>
        ///     Gets a value indicating whether this chamber is waiting for a pawn.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this chamber is waiting for a pawn; otherwise, <c>false</c>.
        /// </value>
        public bool WaitingForPawn =>
            _innerState == ChamberState.WaitingForPawn || _innerState == ChamberState.WaitingForPawnMerging;

        [NotNull]
        private CompRefuelable Refuelable
        {
            get
            {
                if (_refuelable == null) _refuelable = GetComp<CompRefuelable>();
                if (_refuelable == null) Log.ErrorOnce("unable to find refuelable comp on mutachamber!", thingIDNumber);

                return _refuelable;
            }
        }

        private bool Occupied => innerContainer.Any;

        private CompFlickable Flickable
        {
            get
            {
                if (_flickable == null) _flickable = GetComp<CompFlickable>();

                return _flickable;
            }
        }

        private FillableBarDrawer _fillableDrawer;

        [CanBeNull]
        FillableBarDrawer FillableDrawer
        {
            get
            {
                if (_fillableDrawer == null)
                {
                    _fillableDrawer = GetComp<FillableBarDrawer>();
                }

                return _fillableDrawer; 
            }
        }

        private bool HasFuel => Refuelable.HasFuel; 

        /// <summary>
        /// Gets a value indicating whether this instance can accept pawns.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can accept pawns; otherwise, <c>false</c>.
        /// </value>
        public bool CanAcceptPawns => WaitingForPawn && HasPower && HasFuel; 

        [NotNull]
        private Gizmo PartPickerGizmo
        {
            get
            {
                if (_ppGizmo == null)
                    _ppGizmo = new Command_Action
                    {
                        action = OpenPartPicker,
                        icon = PMTextures.PartPickerIcon,
                        defaultLabel = PART_PICKER_GIZMO_LABEL.Translate()
                    };

                return _ppGizmo;
            }
        }

        private Gizmo MergingGizmo
        {
            get
            {
                if (_mergingGizmo == null)
                    _mergingGizmo = new Command_Action
                    {
                        icon = PMTextures.MergingIcon,
                        action = EnterMergingIdle,
                        defaultLabel = MERGING_GIZMO_LABEL.Translate()
                    };

                return _mergingGizmo;
            }
        }


        [NotNull]
        private static List<PawnKindDef> RandomAnimalCache
        {
            get
            {
                if (_randomAnimalCache == null)
                    _randomAnimalCache = DefDatabase<PawnKindDef>
                                        .AllDefsListForReading.Where(p => p.race.IsValidAnimal())
                                        .ToList();

                return _randomAnimalCache;
            }
        }

        [NotNull]
        private AnimalSelectorComp SelectorComp
        {
            get
            {
                if (_aSelector == null) _aSelector = GetComp<AnimalSelectorComp>();
                if (_aSelector == null) Log.ErrorOnce("unable to find animal selector on mutachamber!", thingIDNumber);

                return _aSelector;
            }
        }

        /// <summary>
        ///     Draws this instance.
        /// </summary>
        public override void Draw()
        {
            
            FillableDrawer?.PreDraw();
            base.Draw();
            
        }

        /// <summary>
        ///     exposes data for serialization/deserialization
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _currentUse, "currentUse"); 
            Scribe_Values.Look(ref _innerState, "state");
            Scribe_Values.Look(ref _timer, "timer", -1);
            Scribe_Values.Look(ref _lastTotal, "lastTotal");
            Scribe_Defs.Look(ref _targetAnimal, "targetAnimal");
            Scribe_Deep.Look(ref _addedMutationData, "addedMutationData"); 
            Scribe_Values.Look(ref _curMutationIndex, "curMutationIndex");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (PowerCompTrader != null)
                {
                    PowerCompTrader.PowerOn = _innerState == ChamberState.Active; 
                }

                if (Glower != null)
                {
                    Glower.Props.glowColor =_innerState == ChamberState.Active ? GlowColor : Clear;
                }
            }
        }

        [CanBeNull]
        CompRefuelable FindMutagenTankComp()
        {
            var room = Position.GetRoom(Map);

            var tanks = room?.ContainedThings(PMThingDefOf.PM_MutagenTank)
                            ?.Select(c => c.TryGetComp<CompRefuelable>())
                             .Where(c => c != null);


            return tanks?.FirstOrDefault(c => c.HasFuel); 

        }

        void TryRefillFromTank()
        {
            var tank = FindMutagenTankComp();
            if (tank == null) return;

            var refuelAmount = Mathf.Min(Refuelable.TargetFuelLevel - Refuelable.Fuel, tank.Fuel);
            Refuelable.Refuel(refuelAmount); 
            tank.ConsumeFuel(refuelAmount);
        }
    


        /// <summary>
        /// Finds the Mutachamber casket for.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="traveler">The traveler.</param>
        /// <param name="ignoreOtherReservations">if set to <c>true</c> [ignore other reservations].</param>
        /// <param name="use">The use.</param>
        /// <returns></returns>
        public static MutaChamber FindMutaChamberFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false, ChamberUse? use = null)
        {
            IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
                                               where typeof(MutaChamber).IsAssignableFrom(def.thingClass)
                                               select def;
            foreach (ThingDef item in enumerable)
            {
                bool Validator(Thing x)
                {
                    int result;
                    var mutaChamber = (MutaChamber) x;
                    if (mutaChamber.CanAcceptPawns && mutaChamber.Flickable.SwitchIsOn && (use == null || use == mutaChamber.CurrentUse))
                    {
                        Pawn p2 = traveler;
                        LocalTargetInfo target = x;
                        bool ignoreOtherReservations2 = ignoreOtherReservations;
                        result = p2.CanReserve(target, 1, -1, null, ignoreOtherReservations2) ? 1 : 0;
                    }
                    else
                    {
                        result = 0;
                    }

                    return result != 0;
                }

                var building_MutagenChamber =
                    (MutaChamber) GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(item),
                                                                   PathEndMode.InteractionCell, TraverseParms.For(traveler),
                                                                   9999f, Validator);
                if (building_MutagenChamber != null) return building_MutagenChamber;
            }

            return null;
        }

        /// <summary>
        ///     Gets the float menu options.
        /// </summary>
        /// <param name="myPawn">My pawn.</param>
        /// <returns></returns>
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            foreach (FloatMenuOption floatMenuOption in base.GetFloatMenuOptions(myPawn)) yield return floatMenuOption;
            if (!MutagenDefOf.MergeMutagen.CanTransform(myPawn))
                yield break;

            if (WaitingForPawn)
            {
                if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
                {
                    yield return new FloatMenuOption("CannotUseNoPath".Translate(), null);
                    yield break;
                }

                JobDef jobDef = PMJobDefOf.EnterMutagenChamber;
                string jobStr;

                switch (_currentUse)
                {
                    case ChamberUse.Mutation:
                        
                    case ChamberUse.Tf:
                        jobStr = "EnterMutagenChamber".Translate();
                        break;
                    case ChamberUse.Merge:
                        jobStr = "PMEnterMutagenChamberMerge".Translate(); 
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                   
                Action jobAction = delegate
                {
                    var job = new Job(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction), myPawn, this);
            }
        }

        /// <summary>
        ///     Gets the gizmos.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) yield return gizmo;

            if (DebugSettings.godMode && _innerState == ChamberState.Active)
            {
                yield return DebugFinishGizmo; 
            }


            if (_innerState != ChamberState.Idle) yield break;
            yield return PartPickerGizmo;
            yield return MergingGizmo;
        }

        private float PercentDone => 1f - ((float) _timer) / _lastTotal; 

        /// <summary>
        ///     Gets the inspect string.
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            base.GetInspectString();
            var stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            stringBuilder.Append(("PM" + _innerState).Translate() + " ");
            stringBuilder.AppendLine(inspectString);

            if (_innerState == ChamberState.Active)
            {
                float pDone = 1f - (float)_timer / _lastTotal;
                string insString = "MutagenChamberProgress".Translate() + ": " + pDone.ToStringPercent() + " ";
                insString = GetPawnInspectionString(insString);

                stringBuilder.AppendLine(insString);
            }


            return stringBuilder.ToString().TrimEndNewlines();
        }

        private string GetPawnInspectionString(string insString)
        {
            var pawn = (Pawn)innerContainer.First();
            switch (_currentUse)
            {
                case ChamberUse.Mutation:
                    insString += "PMChamberAddingMutations".Translate();
                    break;
                case ChamberUse.Merge:
                case ChamberUse.Tf:
                    insString += "PMChamberTransforming".Translate(pawn, _targetAnimal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return insString;
        }

        /// <summary>
        ///     setup after spawning in
        /// </summary>
        /// <param name="map"></param>
        /// <param name="respawningAfterLoad"></param>
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            LessonAutoActivator.TeachOpportunity(PMConceptDefOf.MergingPawns, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(PMConceptDefOf.PM_PartPicker, OpportunityType.Important);
            LessonAutoActivator.TeachOpportunity(PMConceptDefOf.Tagging, OpportunityType.Important);
        }

        private bool _hasFuelLast; 

        /// <summary>
        ///     Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (!Refuelable.HasFuel)
            {
                if (_hasFuelLast)
                {
                    TryRefillFromTank();
                }
                else if (this.IsHashIntervalTick(REFUEL_CHECK_TIMER))
                {
                    TryRefillFromTank();
                }
                _hasFuelLast = false;
                return;
            }
            else
            {
                _hasFuelLast = true;
            }

            if (_innerState != ChamberState.Active) return;
            if (_timer <= 0)
            {
                try
                {
                    EjectPawn();
                }
                catch (Exception e)
                {
                    Log.Error($"unable to eject pawns from chamber!\ncaught exception {e.GetType().Name}\n{e}");
                }//make sure an exception while ejecting a pawn doesn't put the chamber in a bad state 
                _currentUse = ChamberUse.Tf;//this should be the default 
                _innerState = ChamberState.WaitingForPawn;
                _timer = 0;
                return;
            }

            if (PowerCompTrader?.PowerOn == false) return; 

            
            if (!Flickable.SwitchIsOn) return;
            Refuelable.Notify_UsedThisTick();
            _timer -= 1;
            switch (_currentUse)
            {
                case ChamberUse.Mutation:
                    CheckMutationProgress();
                    break;
                case ChamberUse.Tf:
                    CheckTfMutationProgress(); 
                    break;
                case ChamberUse.Merge: default:
                    break;
            }


        }

        private void CheckTfMutationProgress()
        {
            if (AnimalMutations.Count == 0) return;

            var mx = AnimalMutations.Count;
            var idx = Mathf.FloorToInt(Mathf.Clamp(PercentDone * mx, 0, mx));
            if (idx != _curMutationIndex)
            {
                _curMutationIndex = idx; 
                var pawn = innerContainer?.FirstOrDefault() as Pawn;
                if (pawn == null) return;
                var mut = AnimalMutations[idx];
                var muts =  MutationUtilities.AddMutation(pawn, mut, ancillaryEffects: MutationUtilities.AncillaryMutationEffects.None);
                foreach (Hediff_AddedMutation hediffAddedMutation in muts)
                {
                    var adjComp = hediffAddedMutation.SeverityAdjust; 
                    if(adjComp != null)
                    {
                        hediffAddedMutation.Severity = adjComp.NaturalSeverityLimit; 
                    }
                }
            }

        }

        void FinalizeMutations()
        {
            if (_addedMutationData == null) return;
            var pawn = innerContainer?.FirstOrDefault() as Pawn;
            if (pawn == null) return;


            for (int i = _curMutationIndex + 1; i < _addedMutationData.Count; i++)
            {
                var mut = _addedMutationData[i];
                ApplyMutationData(pawn, mut); 
            }
        }

        private void CheckMutationProgress()
        {
            if (_addedMutationData == null) return;

            int mx = _addedMutationData.Count;
            int idx = Mathf.FloorToInt(Mathf.Clamp(PercentDone * mx, 0, mx));
            if (idx != _curMutationIndex)
            {
                var pawn = innerContainer?.FirstOrDefault() as Pawn;
                if (pawn == null) return;

                _curMutationIndex = idx;
                IReadOnlyMutationData mutationData = _addedMutationData[_curMutationIndex];
                ApplyMutationData(pawn, mutationData);
            }
        }

        private void ApplyMutationData([NotNull] Pawn pawn, [NotNull] IReadOnlyMutationData mutationData)
        {
            mutationData.ApplyMutationData(pawn, MutationUtilities.AncillaryMutationEffects.HistoryOnly);
        }


        /// <summary>
        ///     tries to accept a new thing into this chamber
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="allowSpecialEffects"></param>
        /// <returns></returns>
        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {


            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                var p = thing as Pawn;
                if (p == null)
                {
                    Log.Error($"{ThingID} accepted non pawn {p.ThingID}/{p.GetType().Name}! this should never happen");
                    return true;
                }

                var food = p.needs?.food;
                if (food != null)
                {
                    food.CurLevel = food.MaxLevel; 
                }

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (_innerState)
                {
                    case ChamberState.WaitingForPawn:
                        _innerState = ChamberState.Idle;
                        SelectorComp.Enabled = true;
                        break;
                    case ChamberState.WaitingForPawnMerging:
                        _innerState = ChamberState.Active;
                        SetActive(); 
                        StartMerging();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_innerState), _innerState.ToString());
                }

                return true;
            }

            return false;
        }

        private void SetActive()
        {
            FillableDrawer?.Trigger();
            PowerCompTrader.PowerOn = !PowerCompTrader.PowerOn;
            Glower?.UpdateLit(Map);

            PowerCompTrader.PowerOn = true; 
            if (Glower != null)
            {
                Glower.Props.glowColor = GlowColor;
                Glower.UpdateLit(Map);
                Log.Message($"{ThingID} {Glower.Glows}|{PowerCompTrader.PowerOn}");
            }
        }

        void SetInactive()
        {
            PowerCompTrader.PowerOn = !PowerCompTrader.PowerOn;
            Glower?.UpdateLit(Map); 


            PowerCompTrader.PowerOn = false; 
            if (Glower != null)
            {
                Glower.Props.glowColor = Clear;
                Glower.UpdateLit(Map);
                Log.Message($"{ThingID} {Glower.Glows}|{PowerCompTrader.PowerOn}");
            }
        }

        private void AnimalChosen(PawnKindDef pawnkinddef)
        {
            _timer = GetTransformationTime(pawnkinddef);
            _lastTotal = _timer;
            _innerState = ChamberState.Active;
            _currentUse = ChamberUse.Tf;
            _targetAnimal = pawnkinddef;

            SetActive();

            SelectorComp.Enabled = false;
        }

      

        [DebugOutput(category = "Pawnmorpher")]
        private static void DBGGetAnimalTransformationTimes()
        {
            var builder = new StringBuilder();
            foreach (PawnKindDef pawnKindDef in DefDatabase<PawnKindDef>.AllDefsListForReading)
            {
                float size = pawnKindDef.RaceProps.baseBodySize;

                builder.AppendLine($"{pawnKindDef.defName},{size}");
            }

            Log.Message(builder.ToString());
        }

        [NotNull]
        private readonly
            static List<Pawn> _scratchList = new List<Pawn>();  

        private void EjectPawn()
        {
            
            _scratchList.Clear();
            _scratchList.AddRange(innerContainer.OfType<Pawn>());
            var pawn = _scratchList[0] as Pawn;
            if (pawn == null)
            {
                Log.Error("trying to eject empty muta chamber!");
                return;
            }

            TransformationRequest tfRequest;
            Mutagen mutagen = null;
            SetInactive();
            switch (_currentUse)
            {
                case ChamberUse.Mutation:
                    tfRequest = null;
                    FinalizeMutations();
                    break;
                case ChamberUse.Merge:
                    var otherPawn = (Pawn)_scratchList[1];
                    if (otherPawn == null)
                    {
                        Log.Error("merging but cannot find other pawn! aborting!");
                        tfRequest = null;
                        break;
                    }

                    tfRequest = new TransformationRequest(_targetAnimal, pawn, otherPawn)
                    {
                        addMutationToOriginal = false,
                        factionResponsible = Faction,
                        forcedFaction = Faction,
                        forcedSapienceLevel = 1,
                        manhunterSettingsOverride = ManhunterTfSettings.Never
                    };
                    mutagen = MutagenDefOf.MergeMutagen.MutagenCached;
                    break;
                case ChamberUse.Tf:
                    PawnKindDef animal = SelectorComp.ChosenKind;
                    if (animal == null) animal = GetRandomAnimal();

                    tfRequest = new TransformationRequest(animal, pawn)
                    {
                        addMutationToOriginal = true,
                        factionResponsible = Faction,
                        forcedFaction = Faction,
                        forcedGender = TFGender.Original,
                        forcedSapienceLevel = 1,
                        manhunterSettingsOverride = ManhunterTfSettings.Never
                    };
                    mutagen = MutagenDefOf.defaultMutagen.MutagenCached;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            EjectContents();

            if (tfRequest == null) return;


            TransformedPawn tfPawn = mutagen.Transform(tfRequest);

            if (tfPawn == null)
            {
                Log.Error($"unable to transform pawn(s)! {_currentUse} {_innerState}");
                return;
            }

            var gComp = Find.World.GetComponent<PawnmorphGameComp>();
            gComp.AddTransformedPawn(tfPawn);
            foreach (Pawn oPawn in tfPawn.OriginalPawns)
            {
                if (oPawn.Spawned)
                    oPawn.DeSpawn();
            }

        }

        private void ResetChamber()
        {
            FillableDrawer?.Clear();
            SelectorComp.Enabled = false;
            _innerState = ChamberState.WaitingForPawn;
            _currentUse = ChamberUse.Tf;
            _addedMutationData = null;
            _curMutationIndex = -1; 
        }

        private void EnterMergingIdle()
        {
            _innerState = ChamberState.WaitingForPawnMerging;
            _currentUse = ChamberUse.Merge;
            SelectorComp.Enabled = false; 
        }

        private PawnKindDef GetRandomAnimal()
        {
            return RandomAnimalCache.RandomElement();
        }


        private int GetTransformationTime(PawnKindDef pawnKindDef)
        {
            float baseTime = TF_ANIMAL_DURATION * 60000; //convert from days to ticks 
            float szFactor =
                Mathf.Pow(pawnKindDef.RaceProps.baseBodySize, 1f / 3); //want to reduce the time of extreme body size values 
            float time = baseTime * szFactor;
            return Mathf.RoundToInt(Mathf.Max(time, MIN_TRANSFORMATION_TIME));
        }

        private void Initialize()
        {
           if(_innerState == ChamberState.Active) SetActive();
           else SetInactive();


            SelectorComp.Enabled = Occupied && _innerState == ChamberState.Idle;
            if (_innerState == ChamberState.Active && _timer == -1)
            {
                Log.Error("timer on mutachamber is -1 while active, ending");
                _timer = 0;
            }

            SelectorComp.AnimalChosen += AnimalChosen;
        }

        private void OpenPartPicker()
        {
            var pawn = innerContainer.First() as Pawn;
            if (pawn == null) Log.Error("unable to find pawn to open part picker for");

            var dialogue = new Dialog_PartPicker(pawn);

            dialogue.WindowClosed += WindowClosed;
            Find.WindowStack.Add(dialogue);
        }

        private void StartMerging()
        {
            ThingDef merge = ChaomorphUtilities.GetRandomChaomorph(ChaomorphType.Merge);
            if (merge == null)
            {
                Log.Error("unable to get random merge, instead generating chaomorph!");
                merge = ChaomorphUtilities.GetRandomChaomorph(ChaomorphType.Chaomorph);
            }

            _targetAnimal = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(pk => pk.race == merge);
            if (_targetAnimal == null)
            {
                Log.Error($"unable to find pawnkind def for {merge.defName}! aborting!");
                EjectContents();
                return;
            }

            _timer = GetTransformationTime(_targetAnimal);
            _lastTotal = _timer;
            _innerState = ChamberState.Active;
            _currentUse = ChamberUse.Merge;
            SelectorComp.Enabled = false;
        }


        private IReadOnlyAddedMutations _addedMutationData;

        private void WindowClosed(Dialog_PartPicker sender, IReadOnlyAddedMutations addedmutations)
        {
            sender.WindowClosed -= WindowClosed;

            if (_innerState != ChamberState.Idle)
            {
                Log.Message("state is not idle!");

                return;
            }

            if (addedmutations?.Any() != true) return;
            
            
            _addedMutationData = new AddedMutations(addedmutations);
            _timer = GetMutationDuration(addedmutations);
            _currentUse = ChamberUse.Mutation;
            _innerState = ChamberState.Active;
            SetActive();
            _lastTotal = _timer;
            sender.Reset();

            //remove any mutations left over 
            var pawn = innerContainer.FirstOrDefault() as Pawn;
            if (pawn == null) return; 
            foreach (IReadOnlyMutationData mData in _addedMutationData.Where(m => !m.Removing))
            {
                var hediff =
                    pawn.health?.hediffSet?.hediffs?.FirstOrDefault(h => h.def == mData.Mutation && h.Part == mData.Part) as
                        Hediff_AddedMutation; 
                hediff?.MarkForRemoval();
            }

            SelectorComp.Enabled = false; 
        }

        private int GetMutationDuration(IReadOnlyAddedMutations addedMutations)
        {
            const float averageValue = 10;
            const float averageMaxPossible = 30;
            float maxTime = 60000 * TF_ANIMAL_DURATION;
            float minTime = 60000 * TF_ANIMAL_DURATION / 10; 
            float tValue = 0;

            foreach (IReadOnlyMutationData mutation in addedMutations)
            {
                tValue += mutation.Mutation.value; 
            }

            tValue /= (averageValue * averageMaxPossible);

            var t = Mathf.Clamp(tValue * maxTime, minTime, maxTime);
            return Mathf.RoundToInt(t);
        }

        private enum ChamberState
        {
            WaitingForPawn,
            WaitingForPawnMerging,
            Idle,
            Active
        }
    }
}