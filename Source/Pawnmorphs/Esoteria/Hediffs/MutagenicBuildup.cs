// MutagenicBuildup.cs modified by Iron Wolf for Pawnmorph on 08/29/2019 8:27 AM
// last updated 08/29/2019  8:27 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Multiplayer.API;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff type for mutagenic buildup 
    /// </summary>
    /// should add more and more mutations as severity increases, with a full tf at a severity of 1 
    /// <seealso cref="Pawnmorph.Hediff_Morph" />
    public class MutagenicBuildup : Hediff_Morph
    {
        private HediffDef _chosenMorphTf;
        private List<HediffGiver_Mutation> _mutations = new List<HediffGiver_Mutation>();
        private HediffGiver_TF _transformation;
        private void PickRandomMorphTf()
        {
            bool SelectionFunc(HediffDef tfDef)
            {
                if (tfDef == def) return false;
                if (tfDef.GetTransformationType() == MorphTransformationTypes.Partial) return false;
                IEnumerable<MorphDef> morphs = MorphUtilities.GetAssociatedMorph(tfDef);
                if (morphs.Any(m => m.categories.Contains(MorphCategoryDefOf.Powerful)))
                    return false; //don't apply powerful morphs 


                IEnumerable<HediffGiver_Mutation> givers =
                    tfDef.stages?.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_Mutation>()
                                               ?? Enumerable.Empty<HediffGiver_Mutation>());
                if (givers == null)
                {
                    return false;
                }

                if (!givers.Any())
                {
                    return false;
                }

                IEnumerable<HediffGiver_TF> tfs = tfDef.stages.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_TF>()
                                                                            ?? Enumerable.Empty<HediffGiver_TF>());
                return tfs.Any();
            }


            if (MP.IsInMultiplayer) Rand.PushState(RandUtilities.MPSafeSeed);

            SetTransformationType(MorphTransformationDefOf.AllMorphsCached.Where(SelectionFunc).RandomElement());
            if (MP.IsInMultiplayer) Rand.PopState();
        }


        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            if (_chosenMorphTf != null && _mutations.Count > 0 && pawn.IsHashIntervalTick(60))
            {
                foreach (HediffGiver_Mutation hediffGiverMutation in _mutations)
                {
                    float originalMtbUntis = hediffGiverMutation.mtbUnits;
                    try
                    {
                        hediffGiverMutation.mtbUnits = AverageMTBUnits;
                        hediffGiverMutation.ClearHediff(this); //make sure all hediffs can be applied, like in chaotic giver 
                        hediffGiverMutation.OnIntervalPassed(pawn, this);
                    }
                    finally
                    {
                        hediffGiverMutation.mtbUnits = originalMtbUntis; 
                    }
                   
                }
            }
        }

        /// <summary>
        /// called after all hediffs Tick() methods have been called 
        /// </summary>
        public override void PostTick()
        {
            base.PostTick();

            if (CurStageIndex == def.stages.Count - 1)
            {
                _transformation?.TryTf(pawn, this); //this hediff will be removed if successful so we don't have to worry about multiple triggers 
            }

        }
        /// <summary>
        /// Gets the transformation warning stage.
        /// </summary>
        /// <value>
        /// The transformation warning stage.
        /// </value>
        protected override int TransformationWarningStage => 5;

        /// <summary>
        /// Enters the next stage.
        /// </summary>
        protected override void EnterNextStage()
        {
            base.EnterNextStage();

            if (CurStageIndex >= 4 && _chosenMorphTf == null) PickRandomMorphTf();
        }

        private float? _averageMTBUnits;

        float AverageMTBUnits
        {
            get
            {
                if (_averageMTBUnits == null)
                {
                    _averageMTBUnits = def.GetAllHediffGivers().OfType<Giver_MutationChaotic>().Select(g => g.mtbUnits).Average(); 
                }

                return _averageMTBUnits.Value; 
            }
        }

        [SyncMethod]
        void SetTransformationType([NotNull] HediffDef tfDef)
        {
            
            _chosenMorphTf = tfDef ?? throw new ArgumentNullException(nameof(tfDef));
            var givers = tfDef.stages.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_Mutation>()
                                                   ?? Enumerable.Empty<HediffGiver_Mutation>());
            var tf = tfDef.stages.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_TF>() ?? Enumerable.Empty<HediffGiver_TF>())
                          .First();

            _transformation = tf;
            if (_mutations == null) _mutations = new List<HediffGiver_Mutation>();
            else
                _mutations.Clear();
            _mutations.AddRange(givers); 



        }

      

        /// <summary>Tries the merge with.</summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool TryMergeWith(Hediff other)
        {
            var merged = base.TryMergeWith(other);
            if (merged)
            {
                if (_chosenMorphTf == null)
                {
                    RunChaoticGivers();
                }
                else
                {
                    var giver = _chosenMorphTf.GetAllHediffGivers().OfType<HediffGiver_Mutation>().RandomElement();
                    giver.TryApply(pawn, MutagenDefOf.defaultMutagen, cause: this); 
                }
            }

            return merged; 
        }

        private void RunChaoticGivers()
        {
            if (Rand.Range(0, 1f) < 0.5f)
            {
                var giver = (CurStage?.hediffGivers?.OfType<Giver_MutationChaotic>() ?? Enumerable.Empty<Giver_MutationChaotic>())
                   .RandomElementWithFallback();
                giver?.TryApply(pawn, this, MutagenDefOf.defaultMutagen); 
            }
           
        }


        /// <summary>
        /// Gets the label base.
        /// </summary>
        /// <value>
        /// The label base.
        /// </value>
        public override string LabelBase
        {
            get
            {
                if (_chosenMorphTf == null) return def.label;
                return _chosenMorphTf.label; 
            }
        }

        /// <summary>
        /// Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref _chosenMorphTf, nameof(_chosenMorphTf));
            if (Scribe.mode == LoadSaveMode.PostLoadInit && _chosenMorphTf != null)
            {
                SetTransformationType(_chosenMorphTf); 
            }

        }
    }

  
}