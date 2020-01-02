// MorphTf.cs created by Iron Wolf for Pawnmorph on 01/02/2020 1:53 PM
// last updated 01/02/2020  1:53 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    ///     simple implementation of TransformationBase
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.TransformationBase" />
    public class MorphTf : TransformationBase
    {
        private const float BASE_MTB = 0.4f;

        private float _mbt = BASE_MTB;

        private List<HediffDef> _allMutations;

        /// <summary>Gets the mean time between mutations</summary>
        /// <value>The mean time between mutation.</value>
        public override float MTBMutations => _mbt;

        /// <summary>Gets the available mutations.</summary>
        /// <value>The available mutations.</value>
        public override IEnumerable<HediffDef> AllAvailableMutations => _allMutations.MakeSafe();

        /// <summary>called after this hediff is added to the pawn</summary>
        /// <param name="dinfo">The dinfo.</param>
        public override void PostAdd(DamageInfo? dinfo)
        {
            base.PostAdd(dinfo);
            _allMutations = def.stages.OfType<TransformationStage>().SelectMany(t => t.mutations.MakeSafe()).ToList();
        }

        /// <summary>Ticks this instance.</summary>
        public override void Tick()
        {
            base.Tick();

            if (pawn.IsHashIntervalTick(200))
            {
                float statDef = pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
                _mbt = BASE_MTB / Mathf.Max(0.01f, statDef);
            }
        }

        /// <summary>Fills the part check list.</summary>
        /// the check list is a list of all parts in the parents body def in the order mutations should be added
        /// <param name="checkList">The check list.</param>
        protected override void FillPartCheckList(List<BodyPartRecord> checkList)
        {
            pawn.RaceProps.body.RandomizedSpreadOrder(checkList);
        }

        /// <summary>Gets the available the mutations from the given stage.</summary>
        /// <param name="currentStage">The current stage.</param>
        /// <returns></returns>
        protected override IEnumerable<HediffDef> GetAvailableMutations(HediffStage currentStage)
        {
            if (currentStage is TransformationStage stage) return stage.mutations.MakeSafe();
            return Enumerable.Empty<HediffDef>();
        }
    }
}