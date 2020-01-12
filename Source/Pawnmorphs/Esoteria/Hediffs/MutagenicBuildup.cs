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
        private MorphDef _chosenMorphDef;
        private HediffDef _chosenMorphTf;
        private HediffGiver_TF _transformation;
        private SimpleCurve _mtbVSeverityCurve;

        

        private SimpleCurve MtbVSeverityCurve
        {
            get
            {
                float min = float.PositiveInfinity;
                if (_mtbVSeverityCurve == null)
                {
                    var points = new List<CurvePoint>();
                    foreach (HediffStage hediffStage in def.stages)
                    {
                        List<float> mtbs = hediffStage.hediffGivers.MakeSafe()
                                                      .OfType<Giver_MutationChaotic>()
                                                      .Select(g => g.mtbDays)
                                                      .ToList();
                        if (mtbs.Count == 0)
                        {
                            continue;
                        }

                        float averageMtb = mtbs.Average();
                        if (min > averageMtb) min = averageMtb; //get the min of all stages 
                        points.Add(new CurvePoint(hediffStage.minSeverity, averageMtb));
                    }

                    
                    _mtbVSeverityCurve = new SimpleCurve();
                    _mtbVSeverityCurve.SetPoints(points);
                }


                return _mtbVSeverityCurve;
            }
        }

        private float? _averageMtbUnits;


        private float MTBUnits
        {
            get
            {
                if (_averageMtbUnits == null)
                    _averageMtbUnits = def.GetAllHediffGivers()
                                          .OfType<Giver_MutationChaotic>()
                                          .Select(g => g.mtbUnits)
                                          .Average();

                return _averageMtbUnits.Value;
            }
        }


        private void PickRandomMorphTf()
        {
            bool SelectionFunc(HediffDef tfDef)
            {
                if (tfDef == def) return false;
                if (tfDef.GetTransformationType() == MorphTransformationTypes.Partial) return false;
                var morphs = MorphUtilities.GetAssociatedMorph(tfDef).ToList();
                if (morphs.Count == 0) return false; 
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

        private List<BodyPartRecord> _recordList;
        private int _curIndex;


        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public override void Tick()
        {
            if(_chosenMorphDef == null) base.Tick(); //don't use the regular hediff givers if the chosen morph is set 
            else if (Rand.MTBEventOccurs(MtbVSeverityCurve.Evaluate(Severity), AverageMTBUnits, 60))
            {
                 
                if (_recordList == null || _recordList.Count == 0)
                {
                    _recordList = new List<BodyPartRecord>();
                    _curIndex = 0;
                    pawn.RaceProps.body.RandomizedSpreadOrder(_recordList);
                }

                if (_curIndex >= _recordList.Count) return; 
                var record = _recordList[_curIndex];

                if (record.IsMissingAtAllIn(pawn)) return; //don't mutate missing parts 

                _curIndex++;
                var mutation = _chosenMorphDef.GetMutationForPart(record.def).RandomElementWithFallback();
                if (mutation != null)
                {
                    if (MutationUtilities.AddMutation(pawn, mutation, record))
                    {
                        var mutagen = this.GetMutagenDef();
                        mutagen.TryApplyAspects(pawn); 
                    } 
                }
            }

            if(pawn.IsHashIntervalTick(200) && _curIndex >= _recordList?.Count)
                _recordList.Clear();//restart every so often 

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
            var tf = tfDef.stages?.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_TF>() ?? Enumerable.Empty<HediffGiver_TF>());
            try
            {
                _transformation = tf.First();
                _chosenMorphDef = MorphUtilities.GetAssociatedMorph(tfDef).First();
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"while trying to set transformation type of {def.defName} with tfDef {tfDef.defName}",e);
            }
            
        }

      

        /// <summary>Tries the merge with.</summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool TryMergeWith(Hediff other)
        {
            if (other is MutagenicBuildup) //make sure all mutagenic buildups can merge with each other 
            {
               
                Severity += other.Severity;
                ageTicks = 0;
                foreach (HediffComp hediffComp in comps)
                {
                    hediffComp.CompPostMerged(other); 
                }

                ResetGivers();
                return true; 
            }

            return false; 

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
                if (_chosenMorphDef == null) return def.label;
                return _chosenMorphDef.label; 
            }
        }

        /// <summary>
        /// Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Defs.Look(ref _chosenMorphTf, nameof(_chosenMorphTf));
            Scribe_Collections.Look(ref _recordList, nameof(_recordList), LookMode.BodyPart);
            Scribe_Values.Look(ref _curIndex, nameof(_curIndex)); 
            if (Scribe.mode == LoadSaveMode.PostLoadInit && _chosenMorphTf != null)
            {
                SetTransformationType(_chosenMorphTf); 
            }

        }
    }

  
}