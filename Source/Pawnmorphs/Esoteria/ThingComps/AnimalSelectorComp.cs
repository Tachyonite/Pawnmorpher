// AnimalSelectorComp.cs created by Iron Wolf for Pawnmorph on 05/17/2020 7:40 PM
// last updated 05/17/2020  7:40 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class AnimalSelectorComp : ThingComp, IEquipmentGizmo
    {
        private PawnKindDef _chosenKind;

        /// <summary>
        /// delegate for the Animal Chosen event 
        /// </summary>
        /// <param name="pawnKindDef">The pawn kind definition.</param>
        public delegate void AnimalChosenHandler([CanBeNull] PawnKindDef pawnKindDef);
        /// <summary>
        /// Occurs when an animal is chosen.
        /// </summary>
        public event AnimalChosenHandler AnimalChosen; 


        private bool _enabled = true; 
        public bool Enabled
        {
            get
            {
                return _enabled; 
                
            }
            set { _enabled = value; }
        }

        /// <summary>
        ///     Gets the props.
        /// </summary>
        /// <value>
        ///     The props.
        /// </value>
        public AnimalSelectorCompProperties Props => (AnimalSelectorCompProperties) props;

        /// <summary>
        /// Gets the kind of the chosen.
        /// </summary>
        /// <value>
        /// The kind of the chosen.
        /// </value>
        [CanBeNull] public PawnKindDef ChosenKind => _chosenKind;

        /// <summary>
        /// Gets all animals selectable.
        /// </summary>
        /// <value>
        /// All animals selectable.
        /// </value>
        public IEnumerable<PawnKindDef> AllAnimalsSelectable
        {
            get
            {
                if (Props.requiresTag)
                {
                    var comp = PMComp;

                    return Props.AllAnimals.Where(t => comp.TaggedAnimals.Contains(t));
                }

                return Props.AllAnimals;
            }
        }

        private ChamberDatabase PMComp => Find.World.GetComponent<ChamberDatabase>();


        private Command_Action _cachedGizmo;

        private Gizmo[] _cachedGizmoArr;

        /// <summary>
        /// Comps the get gizmos extra.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo; 
            }

            yield return Gizmo; 
        }

        Command_Action Gizmo
        {
            get
            {
                if (_cachedGizmo == null)
                {
                    _cachedGizmo = new Command_Action()
                    {
                        action = GizmoAction,
                        defaultLabel = "none"
                    };
                }

                return _cachedGizmo; 
            }
        }

        private void GizmoAction()
        {
            var options = GetOptions.ToList();
            if (options.Count == 0) return; 
            Find.WindowStack.Add(new FloatMenu(options)); 
        }

        private IEnumerable<FloatMenuOption> GetOptions
        {
            get
            {
                foreach (PawnKindDef kind in AllAnimalsSelectable)
                {
                    var tk = kind; 
                    yield return new FloatMenuOption(tk.label, () => ChoseAnimal(tk));
                }


            }
        }

        /// <summary>
        ///     Posts the expose data.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _chosenKind, nameof(ChosenKind));
            Scribe_Values.Look(ref _enabled, nameof(Enabled), true); 
        }


        private void ChoseAnimal(PawnKindDef chosenKind)
        {
            _chosenKind = chosenKind;
            Gizmo.icon = _chosenKind.race.uiIcon;
            Gizmo.defaultLabel = _chosenKind.label;
            AnimalChosen?.Invoke(chosenKind); 
        }

        /// <summary>
        /// Gets the gizmos.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Gizmo> GetGizmos()
        {

            if (!_enabled) return Enumerable.Empty<Gizmo>();
            
            if (_cachedGizmoArr == null)
            {
                _cachedGizmoArr = new[] {Gizmo};
            }

            return _cachedGizmoArr; 
        }
    }


    /// <summary>
    /// </summary>
    /// <seealso cref="Verse.CompProperties" />
    public class AnimalSelectorCompProperties : CompProperties
    {
        /// <summary>
        ///     if an animal must be tagged by the tagging rifle
        /// </summary>
        public bool requiresTag;


        /// <summary>
        ///     The race filter
        /// </summary>
        public Filter<PawnKindDef> raceFilter;

        [Unsaved] private List<PawnKindDef> _allAnimals;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AnimalSelectorCompProperties" /> class.
        /// </summary>
        public AnimalSelectorCompProperties()
        {
            compClass = typeof(AnimalSelectorComp);
        }

        /// <summary>
        ///     Gets all animals that can be selected
        /// </summary>
        /// <value>
        ///     All animals.
        /// </value>
        public IReadOnlyList<PawnKindDef> AllAnimals
        {
            get
            {
                if (_allAnimals == null)
                    _allAnimals = DefDatabase<PawnKindDef>
                                 .AllDefsListForReading
                                 .Where(t => t.race.race?.Animal == true && raceFilter?.PassesFilter(t) != false)
                                 .ToList();

                return _allAnimals;
            }
        }
    }
}