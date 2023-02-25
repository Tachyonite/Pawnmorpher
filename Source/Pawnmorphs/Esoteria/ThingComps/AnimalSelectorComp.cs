// AnimalSelectorComp.cs created by Iron Wolf for Pawnmorph on 05/17/2020 7:40 PM
// last updated 05/17/2020  7:40 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.DefExtensions;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class AnimalSelectorComp : ThingComp, IEquipmentGizmo
	{
		private const string ANIMAL_PICKER_NOCHOICES = "PMAnimalPickerGizmoNoChoices";

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


		/// <summary>
		/// Simple delegate for the <see cref="OnClick"/> event 
		/// </summary>
		/// <param name="comp">The <see cref="AnimalSelectorComp" /> that triggered the event.</param>
		public delegate void OnClickHandler([NotNull] AnimalSelectorComp comp);

		/// <summary>
		/// Triggers when selector action is clicked but before anything else.
		/// </summary>
		public event OnClickHandler OnClick;

		private bool _enabled = true;
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="AnimalSelectorComp"/> is enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if enabled; otherwise, <c>false</c>.
		/// </value>
		public bool Enabled
		{
			get => _enabled;
			set => _enabled = value;
		}

		/// <summary>
		///     Gets the props.
		/// </summary>
		/// <value>
		///     The props.
		/// </value>
		public AnimalSelectorCompProperties Props => (AnimalSelectorCompProperties)props;

		/// <summary>
		/// Gets or sets a filter to specify what should (true) or shouldn't (false) be selectable.
		/// </summary>
		/// <value>
		/// The species filter.
		/// </value>
		[CanBeNull] public System.Func<PawnKindDef, bool> SpeciesFilter { get; set; } = null;

		/// <summary>
		/// Gets the kind of the chosen.
		/// </summary>
		/// <value>
		/// The kind of the chosen.
		/// </value>
		[CanBeNull] public PawnKindDef ChosenKind => _chosenKind;

		/// <summary>
		/// Gets all animals selectable according to the selection mode.
		/// </summary>
		/// <value>
		/// All animals selectable.
		/// </value>
		public IEnumerable<PawnKindDef> AllAnimalsSelectable
		{
			get
			{
				IEnumerable<PawnKindDef> animals = Props.requiresTag ? Database.TaggedAnimals : Props.AllAnimals;

				// Filter out excluded animals
				if (Props.raceFilter != null)
				{
					animals = animals.Where(x => Props.raceFilter.PassesFilter(x));
				}

				// Apply special filtering
				if (SpeciesFilter != null)
				{
					animals = animals.Where(x => SpeciesFilter(x));
				}

				// Add always available animals
				if (Props.alwaysAvailable != null)
				{
					animals = animals.Union(Props.alwaysAvailable);
				}

				return animals;
			}
		}

		private ChamberDatabase Database => Find.World.GetComponent<ChamberDatabase>();

		private Command_Action _cachedGizmo;

		private Gizmo[] _cachedGizmoArr;

		/// <inheritdoc />
		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);

			_cachedGizmo = new Command_Action()
			{
				action = GizmoAction,
				icon = PMTextures.AnimalSelectorIcon,
				defaultLabel = Props.labelKey.Translate(),
				defaultDesc = Props.descriptionKey.Translate()
			};
		}

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


			if (_enabled)
				yield return Gizmo;
		}

		Command_Action Gizmo => _cachedGizmo;

		/// <summary>
		/// Resets the selected animal
		/// </summary>
		public void ResetSelection()
		{
			_cachedGizmo.defaultLabel = Props.labelKey.Translate();
			_cachedGizmo.defaultDesc = Props.descriptionKey.Translate();
			_cachedGizmo.icon = PMTextures.AnimalSelectorIcon;
		}

		private void GizmoAction()
		{
			OnClick?.Invoke(this);
			var options = GetOptions.ToList();
			if (options.Count == 0)
			{
				var emptyOption = new FloatMenuOption(ANIMAL_PICKER_NOCHOICES.Translate(), null);
				emptyOption.Disabled = true;
				options.Add(emptyOption);
			}
			Find.WindowStack.Add(new FloatMenu(options));
		}

		private IEnumerable<FloatMenuOption> GetOptions
		{
			get
			{
				foreach (PawnKindDef kind in AllAnimalsSelectable)
				{
					var tk = kind;
					string label;
					AnimalSelectorOverrides overrides = kind.GetModExtension<AnimalSelectorOverrides>();
					if (overrides != null && string.IsNullOrWhiteSpace(overrides.label) == false)
						label = overrides.label;
					else
						label = tk.LabelCap;

					yield return new FloatMenuOption(label, () => ChoseAnimal(tk));
				}
			}
		}

		private void ChoseAnimal(PawnKindDef chosenKind)
		{
			_chosenKind = chosenKind;
			Gizmo.icon = _chosenKind.race.uiIcon;
			Gizmo.defaultLabel = _chosenKind.LabelCap;
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
				_cachedGizmoArr = new[] { Gizmo };
			}

			return _cachedGizmoArr;
		}

		/// <summary>
		/// Save/Load data.
		/// </summary>
		public override void PostExposeData()
		{
			Scribe_Defs.Look(ref _chosenKind, nameof(ChosenKind));
			Scribe_Values.Look(ref _enabled, nameof(Enabled), true);

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				if (_chosenKind != null)
					ChoseAnimal(_chosenKind);
			}
		}
	}


	/// <summary>
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class AnimalSelectorCompProperties : CompProperties
	{
		/// <summary>
		///     Only allow selection of animals which have been tagged in the database
		/// </summary>
		public bool requiresTag;

		/// <summary>
		///     Label of selector button gizmo. Localised key.
		/// </summary>
		public string labelKey;

		/// <summary>
		///     Tooltip of selector button gizmo. Localised key.
		/// </summary>
		public string descriptionKey;


		/// <summary>
		///     List of animals which will always be available for selection 
		/// </summary>
		public List<PawnKindDef> alwaysAvailable;

		/// <summary>
		///     List of animals that will be excluded from the selection
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
								 .Where(t => t.race.race?.Animal == true)
								 .ToList();

				return _allAnimals;
			}
		}
	}
}