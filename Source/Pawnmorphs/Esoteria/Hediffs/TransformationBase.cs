// TransformationBase.cs created by Iron Wolf for Pawnmorph on 01/02/2020 12:54 PM
// last updated 01/02/2020  12:54 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>abstract base class for all transformation hediffs</summary>
    /// <seealso cref="Pawnmorph.IDescriptiveHediff" />
    /// <seealso cref="Verse.Hediff" />
    public abstract class TransformationBase : Hediff, IDescriptiveHediff
    {
        [NotNull] private readonly Dictionary<BodyPartDef, List<HediffDef>> _scratchDict =
            new Dictionary<BodyPartDef, List<HediffDef>>();

        private List<BodyPartRecord> _checkList;
        private int _curIndex;

        /// <summary>
        ///     Gets the description.
        /// </summary>
        /// <value>
        ///     The description.
        /// </value>
        public virtual string Description
        {
            get
            {
                if (CurStage is IDescriptiveStage dStage)
                    return string.IsNullOrEmpty(dStage.DescriptionOverride) ? def.description : dStage.DescriptionOverride;

                return def.description;
            }
        }

        /// <summary>Gets the mean time between mutations</summary>
        /// <value>The mean time between mutation.</value>
        public abstract float MTBMutations { get; }

        /// <summary>Gets the available mutations.</summary>
        /// <value>The available mutations.</value>
        [NotNull]
        public abstract IEnumerable<HediffDef> AllAvailableMutations { get; }


        /// <summary>
        ///     Gets a value indicating whether this instance has finished adding mutations or not.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance has finished adding mutations; otherwise, <c>false</c>.
        /// </value>
        public bool FinishedAddingMutations
        {
            get
            {
                if (_checkList == null) return false;
                return _checkList.Count == _curIndex;
            }
        }

        /// <summary>Gets the label base.</summary>
        /// <value>The label base.</value>
        public override string LabelBase
        {
            get
            {
                if (CurStage is IDescriptiveStage dStage)
                    return string.IsNullOrEmpty(dStage.LabelOverride) ? base.LabelBase : dStage.LabelOverride;

                return base.LabelBase;
            }
        }

        /// <summary>Gets the minimum mutations per check.</summary>
        /// if greater then 1, every-time a mutation is possible don't stop iterating over the parts until a mutable part is found
        /// <value>The minimum mutations per check.</value>
        protected virtual int MinMutationsPerCheck => 1;

        /// <summary>Gets a value indicating whether this instance can reset.</summary>
        /// <value><c>true</c> if this instance can reset; otherwise, <c>false</c>.</value>
        protected virtual bool CanReset => FinishedAddingMutations;

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _checkList, nameof(_checkList), LookMode.BodyPart);
            Scribe_Values.Look(ref _curIndex, nameof(_curIndex));
        }


        /// <summary>called after this hediff is added to the pawn</summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            _checkList = new List<BodyPartRecord>();
            FillPartCheckList(_checkList);
            _curIndex = 0;
        }

        /// <summary>Ticks this instance.</summary>
        public override void Tick()
        {
            HediffStage cStage = CurStage;
            base.Tick();
            if (CurStage != cStage) OnStageChanged(cStage);

            if (Rand.MTBEventOccurs(MTBMutations, 6000, 60)) AddMutations();

            if (pawn.IsHashIntervalTick(10000) && CanReset) ResetMutations();
        }

        /// <summary>Fills the part check list.</summary>
        /// the check list is a list of all parts in the parents body def in the order mutations should be added
        /// <param name="checkList">The check list.</param>
        protected abstract void FillPartCheckList([NotNull] List<BodyPartRecord> checkList);

        /// <summary>Gets the available the mutations from the given stage.</summary>
        /// <param name="currentStage">The current stage.</param>
        /// <returns></returns>
        [NotNull]
        protected virtual IEnumerable<HediffDef> GetAvailableMutations([NotNull] HediffStage currentStage)
        {
            return AllAvailableMutations;
        }

        /// <summary>Notifies this instance that the available mutations have changed.</summary>
        protected void Notify_AvailableMutationsChanged()
        {
            ResetPossibleMutations();
        }

        /// <summary>
        ///     Called when the stage changes.
        /// </summary>
        /// <param name="lastStage">The last stage.</param>
        protected virtual void OnStageChanged([CanBeNull] HediffStage lastStage)
        {
            ResetPossibleMutations();
            TryGiveTransformations();
            if(CurStage is IExecutableStage execStage)
            {
                execStage.EnteredStage(this); 
            }
        }

        /// <summary>Tries to give transformations</summary>
        protected virtual void TryGiveTransformations()
        {
            if (CurStage.hediffGivers == null) return;

            RandUtilities.PushState();

            foreach (HediffGiver_TF tfGiver in CurStage.hediffGivers.OfType<HediffGiver_TF>())
                if (tfGiver.TryTf(pawn, this))
                    break; //try each one, one by one. break at first one that succeeds  

            RandUtilities.PopState();
        }

        private void AddMutations()
        {
            if (FinishedAddingMutations) return;

            var mutationsAdded = 0;
            while (_curIndex < _checkList.Count)
            {
                BodyPartRecord part = _checkList[_curIndex];
                _curIndex++;
                List<HediffDef> lst = _scratchDict.TryGetValue(part.def);

                HediffDef mutationToAdd = lst?.RandomElementWithFallback();
                if (mutationToAdd != null)
                {
                    MutationUtilities.AddMutation(pawn, mutationToAdd, part);
                    var mutagen = def.GetMutagenDef();
                    mutagen.TryApplyAspects(pawn); 
                    mutationsAdded++;

                }

                //check if we have added enough mutations to end the loop early 
                if (mutationsAdded >= MinMutationsPerCheck) break;
            }
        }


        private void ResetMutations()
        {
            _checkList = _checkList ?? new List<BodyPartRecord>();
            _checkList.Clear();
            _curIndex = 0;
        }

        private void ResetPossibleMutations()
        {
            IEnumerable<HediffDef> mutations = GetAvailableMutations(CurStage);
            _scratchDict.Clear();
            foreach (HediffDef hediffDef in mutations) //fill a lookup dict 
            foreach (BodyPartDef possiblePart in hediffDef.GetPossibleParts())
                if (_scratchDict.TryGetValue(possiblePart, out List<HediffDef> lst))
                {
                    lst.Add(hediffDef);
                }
                else
                {
                    lst = new List<HediffDef> {hediffDef};
                    _scratchDict[possiblePart] = lst;
                }
        }
    }
}