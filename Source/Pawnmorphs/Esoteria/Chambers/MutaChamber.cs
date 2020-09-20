// MutaChamber.cs created by Iron Wolf for Pawnmorph on 07/26/2020 7:50 PM
// last updated 07/26/2020  7:50 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.ThingComps;
using Pawnmorph.User_Interface;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Building_Casket" />
    public class MutaChamber : Building_Casket
    {
        private const int TF_ANIMAL_DURATION = 2 * 60 * 60;
        private int _timer = 0;

        private bool _initialized;

        private ChamberState _innerState = ChamberState.WaitingForPawn;


        private CompFlickable _flickable;

        private AnimalSelectorComp _aSelector;

        private bool Occupied => innerContainer.Any;

        private CompFlickable Flickable
        {
            get
            {
                if (_flickable == null) _flickable = GetComp<CompFlickable>();

                return _flickable;
            }
        }

        private Gizmo _ppGizmo;
        private const string PART_PICKER_GIZMO_LABEL = "PMPartPickerGizmo";
        [NotNull]
        Gizmo PartPickerGizmo
        {
            get
            {
                if(_ppGizmo == null)
                {
                    _ppGizmo = new Command_Action()
                    {
                        action = OpenPartPicker,
                        defaultLabel = PART_PICKER_GIZMO_LABEL.Translate(),//TODO add an icon 
                    };
                }

                return _ppGizmo; 
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if(_innerState != ChamberState.Idle) yield break;
            yield return _ppGizmo; 
            //TODO handle merge gizmo 
        }

        private void OpenPartPicker()
        {
            var pawn = innerContainer.First() as Pawn;
            if (pawn == null)
            {
                Log.Error("unable to find pawn to open part picker for");
            }

            var dialogue = new Dialog_PartPicker(pawn);

            dialogue.WindowClosed += WindowClosed;
            Find.WindowStack.Add(dialogue); 
        }

        private void WindowClosed(Dialog_PartPicker sender, IReadOnlyAddedMutations addedmutations)
        {
            sender.WindowClosed -= WindowClosed; 

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
        /// exposes data for serialization/deserialization
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref _innerState, "state");
            Scribe_Values.Look(ref _timer, "timer", -1);
        }


        /// <summary>
        ///     Finds the Mutachamber casket for.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="traveler">The traveler.</param>
        /// <param name="ignoreOtherReservations">if set to <c>true</c> [ignore other reservations].</param>
        /// <returns></returns>
        public static MutaChamber FindMutaChamberFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
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
                    if (!mutaChamber.HasAnyContents && mutaChamber.Flickable.SwitchIsOn)
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
        /// setup after spawning in 
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
        ///     tries to accept a new thing into this chamber
        /// </summary>
        /// <param name="thing"></param>
        /// <param name="allowSpecialEffects"></param>
        /// <returns></returns>
        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                _innerState = ChamberState.Idle;
                return true;
            }

            return false;
        }

        private void AnimalChosen(PawnKindDef pawnkinddef)
        {
            _timer = TF_ANIMAL_DURATION; //TODO make this dependent on the animal chosen 
            _innerState = ChamberState.Active;

            SelectorComp.Enabled = false;
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

        private enum ChamberState
        {
            WaitingForPawn,
            Idle,
            Active
        }
    }
}