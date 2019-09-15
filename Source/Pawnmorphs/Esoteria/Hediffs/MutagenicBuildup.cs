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
    public class MutagenicBuildup : Hediff_Morph
    {
        private HediffDef _chosenMorphTf;
        private List<HediffGiver_Mutation> _mutations = new List<HediffGiver_Mutation>();
        private HediffGiver_TF _transformation;

        void PickRandomMorphTf()
        {
            bool SelectionFunc(HediffDef tfDef)
            {
                if (tfDef == def) return false;
                if (tfDef.GetTransformationType() == MorphTransformationTypes.Partial) return false; 
                var morphs = MorphUtilities.GetAssociatedMorph(tfDef);
                if (morphs.Any(m => m.categories.Contains("powerful"))) return false; //don't apply powerful morphs 



                var givers = tfDef.stages?.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_Mutation>()
                                                        ?? Enumerable.Empty<HediffGiver_Mutation>());



                if (givers == null)
                {
                    Log.Message($"excluding {tfDef.defName} for null giver");
                    return false;
                }

                if (!givers.Any())
                {
                    Log.Message($"excluding {tfDef.defName} for no givers ");
                    return false;
                }


                var tfs = tfDef.stages.SelectMany(s => s.hediffGivers?.OfType<HediffGiver_TF>()
                                                    ?? Enumerable.Empty<HediffGiver_TF>());
                return tfs.Any();



            }


            if (MP.IsInMultiplayer)
            {
                Rand.PushState(RandUtilities.MPSafeSeed); 
            }

            SetTransformationType(MorphTransformationDefOf.AllMorphsCached.Where(SelectionFunc).RandomElement());
            if (MP.IsInMultiplayer)
            {
                Rand.PopState();
            }

        }



        public override void Tick()
        {
            base.Tick();

            if (_chosenMorphTf != null && _mutations.Count > 0 && pawn.IsHashIntervalTick(60))
            {
                foreach (HediffGiver_Mutation hediffGiverMutation in _mutations)
                {
                    hediffGiverMutation.ClearHediff(this); //make sure all hediffs can be applied, like in chaotic giver 
                    hediffGiverMutation.OnIntervalPassed(pawn, this); 
                }
            }
        }


        public override void PostTick()
        {
            base.PostTick();

            if (CurStageIndex == def.stages.Count - 1)
            {
                _transformation?.TryTf(pawn, this); //this hediff will be removed if successful so we don't have to worry about multiple triggers 
            }

        }

        protected override int TransformationWarningStage => 5;

        protected override void EnterNextStage()
        {
            base.EnterNextStage();

            


            if (CurStageIndex >= 4 && _chosenMorphTf == null)
            {
                PickRandomMorphTf();
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


        public override string LabelBase
        {
            get
            {
                if (_chosenMorphTf == null) return def.label;
                return _chosenMorphTf.label; 
            }
        }


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