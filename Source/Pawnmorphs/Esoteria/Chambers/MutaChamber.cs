// MutaChamber.cs created by Iron Wolf for Pawnmorph on 07/26/2020 7:50 PM
// last updated 07/26/2020  7:50 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using Pawnmorph.ThingComps;
using Pawnmorph.User_Interface;
using RimWorld;
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
        private const float TF_ANIMAL_DURATION = 1.5f; //units are in days 
        private const float MIN_TRANSFORMATION_TIME = 0.5f * 60000; //minimum transformation time in ticks
        private const string PART_PICKER_GIZMO_LABEL = "PMPartPickerGizmo";
        private const string MERGING_GIZMO_LABEL = "PMMergeGizmo";

        private static List<PawnKindDef> _randomAnimalCache;
        private int _timer = 0;

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
            //TODO draw pawn

            Comps_PostDraw();
        }

        /// <summary>
        ///     exposes data for serialization/deserialization
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _innerState, "state");
            Scribe_Values.Look(ref _timer, "timer", -1);
            Scribe_Values.Look(ref _lastTotal, "lastTotal");
            Scribe_Defs.Look(ref _targetAnimal, "targetAnimal");
        }

        /// <summary>
        ///     Finds the Mutachamber casket for.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="traveler">The traveler.</param>
        /// <param name="ignoreOtherReservations">if set to <c>true</c> [ignore other reservations].</param>
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
                    if (mutaChamber.WaitingForPawn && mutaChamber.Flickable.SwitchIsOn && (use == null || use == mutaChamber.CurrentUse))
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

        /// <summary>
        ///     Gets the inspect string.
        /// </summary>
        /// <returns></returns>
        public override string GetInspectString()
        {
            base.GetInspectString();
            var stringBuilder = new StringBuilder();
            string inspectString = base.GetInspectString();
            stringBuilder.Append(_innerState.ToString());
            stringBuilder.AppendLine(inspectString);

            if (_innerState == ChamberState.Active)
            {
                float pDone = 1f - (float) _timer / _lastTotal;
                var pawn = (Pawn) innerContainer.First();
                string insString = "MutagenChamberProgress".Translate() + ": " + pDone.ToStringPercent() + " ";

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


                stringBuilder.AppendLine(insString);
            }


            return stringBuilder.ToString().TrimEndNewlines();
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
        }


        /// <summary>
        ///     Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

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


            if (!Refuelable.HasFuel) return;
            if (!Flickable.SwitchIsOn) return;
            Refuelable.Notify_UsedThisTick();
            _timer -= 1;
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
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (_innerState)
                {
                    case ChamberState.WaitingForPawn:
                        _innerState = ChamberState.Idle;
                        SelectorComp.Enabled = true;
                        break;
                    case ChamberState.WaitingForPawnMerging:
                        _innerState = ChamberState.Active;
                        StartMerging();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_innerState), _innerState.ToString());
                }

                return true;
            }

            return false;
        }

        private void AnimalChosen(PawnKindDef pawnkinddef)
        {
            _timer = GetTransformationTime(pawnkinddef); //TODO make this dependent on the animal chosen 
            _lastTotal = _timer;
            _innerState = ChamberState.Active;
            _currentUse = ChamberUse.Tf;
            _targetAnimal = pawnkinddef;
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

            EjectContents();
            SelectorComp.Enabled = false;
            TransformationRequest tfRequest;
            Mutagen mutagen = null;
            switch (_currentUse)
            {
                case ChamberUse.Mutation:
                    tfRequest = null;
                    break;
                case ChamberUse.Merge:
                    var otherPawn = (Pawn) _scratchList[1];
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
                if(oPawn.Spawned)
                    oPawn.DeSpawn(); 
            }
            
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

        private void WindowClosed(Dialog_PartPicker sender, IReadOnlyAddedMutations addedmutations)
        {
            sender.WindowClosed -= WindowClosed;

            if (_innerState != ChamberState.Idle)
            {
                Log.Message("state is not idle!");

                return;
            }

            if (addedmutations?.Any() != true) return;

            //TODO get wait time based on number of mutations added/removed 
            _timer = Mathf.RoundToInt(TF_ANIMAL_DURATION * 60000);
            _currentUse = ChamberUse.Mutation;
            _innerState = ChamberState.Active;
            _lastTotal = _timer;
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