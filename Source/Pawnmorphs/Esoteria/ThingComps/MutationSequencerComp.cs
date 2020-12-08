// MutationSequencerComp.cs created by Iron Wolf for Pawnmorph on 11/14/2020 8:28 AM
// last updated 11/14/2020  8:28 AM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    /// </summary>
    public class MutationSequencerComp : CompScanner
    {
        private const string MUTATION_GATHERED_LABEL = "PMMutationTagged";

        [NotNull] private readonly List<MutationDef> _scratchList = new List<MutationDef>();

        private Command_Action _cachedGizmo;

        [CanBeNull] private PawnKindDef _chosenAnimalToScan;

        private ChamberDatabase _db;


        /// <summary>
        ///     Gets a value indicating whether this instance can use now.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can use now; otherwise, <c>false</c>.
        /// </value>
        public new bool CanUseNow => base.CanUseNow && _chosenAnimalToScan != null && DB.CanTag;

        private MutationSequencerProps SequencerProps => (MutationSequencerProps) props;

        private IEnumerable<FloatMenuOption> GetOptions
        {
            get
            {
                foreach (PawnKindDef kind in AllAnimalsSelectable)
                {
                    PawnKindDef tk = kind;
                    yield return new FloatMenuOption(tk.label, () => ChoseAnimal(tk));
                }
            }
        }

        [NotNull]
        private Command_Action Gizmo
        {
            get
            {
                if (_cachedGizmo == null)
                    _cachedGizmo = new Command_Action
                    {
                        action = GizmoAction,
                        defaultLabel = "none",
                        icon = PMTextures.AnimalSelectorIcon
                    };

                return _cachedGizmo;
            }
        }

        [NotNull]
        private IEnumerable<PawnKindDef> AllAnimalsSelectable
        {
            get
            {
                var db = Find.World.GetComponent<ChamberDatabase>();
                foreach (PawnKindDef taggedAnimal in db.TaggedAnimals)
                    if (taggedAnimal.GetAllMutationsFrom().Any(m => !db.StoredMutations.Contains(m)))
                        yield return taggedAnimal;
            }
        }

        [NotNull]
        private ChamberDatabase DB
        {
            get
            {
                if (_db == null) _db = Find.World.GetComponent<ChamberDatabase>();

                return _db;
            }
        }


        /// <summary>
        ///     Comps the get gizmos extra.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra()) yield return gizmo;


            yield return Gizmo;
        }

        /// <summary>
        ///     Posts the expose data.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref _chosenAnimalToScan, "chosenAnimal");
        }

        /// <summary>
        ///     Does the find.
        /// </summary>
        /// <param name="worker">The worker.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void DoFind(Pawn worker)
        {
            if (_chosenAnimalToScan == null)
            {
                Log.Error($"calling DoFind on {parent.ThingID} which does not have a chosen animal!");
                return;
            }

            _scratchList.Clear();
            _scratchList.AddRange(_chosenAnimalToScan.GetAllMutationsFrom().Where(m => !DB.StoredMutations.Contains(m)));

            if (_scratchList.Count == 0)
            {
                Log.Warning("unable to find mutation to give!");
                _chosenAnimalToScan = null;
                return;
            }

            MutationDef mutation = _scratchList.RandomElement();

            TaggedString msg = MUTATION_GATHERED_LABEL.Translate(mutation.Named("mutation"),
                                                                 _chosenAnimalToScan.Named("animal")
                                                                );
            Messages.Message(msg, MessageTypeDefOf.PositiveEvent);
            if(_scratchList.Count - 1 == 0)
                _chosenAnimalToScan = null;
        }

        private void ChoseAnimal(PawnKindDef tk)
        {
            _chosenAnimalToScan = tk;
            if (_chosenAnimalToScan?.race?.uiIcon == null)
                Gizmo.icon = _chosenAnimalToScan?.race?.uiIcon;
            else
                Gizmo.icon = PMTextures.AnimalSelectorIcon;
            Gizmo.defaultLabel = _chosenAnimalToScan?.label ?? "none";
        }

        private void GizmoAction()
        {
            List<FloatMenuOption> options = GetOptions.ToList();
            if (options.Count == 0) return;
            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}