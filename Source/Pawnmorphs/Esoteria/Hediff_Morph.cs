using System.Collections.Generic;
using System.Linq;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class Hediff_Morph : HediffWithComps
    {
        protected virtual int TransformationWarningStage => 1;
        private const string TRANSFORMATION_WARNING_LETTER_ID = "TransformationStageWarning";

        private HediffComp_Single _comp;
        private MorphDef _morph;
        private List<HediffGiver_Mutation> _allGivers;
        private List<HediffGiver_Mutation> _givers;
        [Unsaved] private int _lastStage = -1; // ToDo can we save this?

        [CanBeNull]
        public HediffComp_Single SingleComp
        {
            get { return _comp ?? (_comp = this.TryGetComp<HediffComp_Single>()); }
        }

        List<HediffGiver_Mutation> AllGivers
        {
            get
            {
                if (_allGivers == null)
                {
                    _allGivers = def.GetAllHediffGivers().OfType<HediffGiver_Mutation>().ToList();
                }

                return _allGivers;
            }
        }

        MorphDef AssociatedMorph
        {
            get
            {
                if (_morph == null)
                {
                    _morph = MorphUtilities.GetAssociatedMorph(def).FirstOrDefault();
                }

                return _morph; 
            }
        }

        public override string LabelBase
        {
            get
            {
                var labelB = base.LabelBase;
                if (SingleComp != null)
                {
                    return $"{labelB} x{SingleComp.stacks}";
                }

                return labelB;
            }
        }

        public IEnumerable<HediffGiver_Mutation> MutationGivers
        {
            get
            {
                if (_givers == null)
                {
                    var givers = def.stages?.SelectMany(g => g.hediffGivers ?? Enumerable.Empty<HediffGiver>())
                                    .OfType<HediffGiver_Mutation>()
                              ?? Enumerable.Empty<HediffGiver_Mutation>();
                    _givers = givers.ToList();
                }

                return _givers;
            }
        }

        private void UpdateGraphics()
        {

            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            if(pawn.IsColonist)
                PortraitsCache.SetDirty(pawn);
        }

        private bool UpdateMorphSkinColor()
        {
            if (AssociatedMorph == pawn.def.GetMorphOfRace()) return false;

            var startColor = GetStartSkinColor();
            var endColor = AssociatedMorph.raceSettings?.graphicsSettings?.skinColorOverride ??
                           pawn.GetComp<GraphicSys.InitialGraphicsComp>().SkinColor;

            float counter = 0;
            foreach (var hediffGiverMutation in AllGivers) //count how many mutations the pawn has now 
            {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(hediffGiverMutation.hediff) != null)
                {
                    counter++; 
                }
            }

            float lerpVal = counter / AllGivers.Count;
            var col = Color.Lerp(startColor, endColor, lerpVal);

            var alComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
            alComp.skinColor = col;
            return true; 
        }

        private Color GetStartSkinColor()
        {
            var pM = pawn.def.GetMorphOfRace();
            var col = pM?.raceSettings?.graphicsSettings?.skinColorOverride;
            if (col == null)
            {
                var comp = pawn.GetComp<GraphicSys.InitialGraphicsComp>();
                col = comp.SkinColor; 
            }

            return col.Value; 
        }

        public override void PostTick()
        {
            base.PostTick();

            if (_lastStage != CurStageIndex)
            {
                _lastStage = CurStageIndex;
                EnterNextStage();
            }
        }

        protected virtual void EnterNextStage()
        {
            if (_lastStage == TransformationWarningStage && (pawn.IsColonist || pawn.IsPrisonerOfColony))
            {
                SendLetter();
            }
            
            TryGiveTransformations();
        }

        protected virtual void TryGiveTransformations()
        {
            if (CurStage.hediffGivers == null) return;

            RandUtilities.PushState();

            foreach (HediffGiver_TF tfGiver in CurStage.hediffGivers.OfType<HediffGiver_TF>())
            {
                if (tfGiver.TryTf(pawn, this)) break; //try each one, one by one. break at first one that succeeds  
            }

            RandUtilities.PopState();
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (!base.TryMergeWith(other)) return false;


            foreach (HediffGiver_Mutation hediffGiverMutation in MutationGivers) //make sure mutations can be re rolled 
            {
                hediffGiverMutation.ClearHediff(this); 
            }

            return true; 
        }

        private void SendLetter()
        {
            var mutagen = this.GetMutagenDef();
            if (!mutagen.CanTransform(pawn)) return;

            var letterLabel = (TRANSFORMATION_WARNING_LETTER_ID + "Label").Translate(pawn);
            var letterContent = (TRANSFORMATION_WARNING_LETTER_ID + "Content").Translate(pawn);
            Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent);
        }

        /// <summary>
        /// This is called when the transformation hediff is removed naturally (after reaching a severity of 0). <br />
        /// This is only called when the hediff is removed after reaching a severity of zero, not when the pawn it's self is removed.
        /// </summary>
        protected virtual void OnFinishedTransformation()
        {
            foreach (IPostTfHediffComp postTfHediffComp in comps.OfType<IPostTfHediffComp>())
            {
                postTfHediffComp.FinishedTransformation(pawn, this); 
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastStage, "lastStage", -1); 
        }


        public override void PostRemoved()
        {
            base.PostRemoved();

            if (Severity <= 0.01f) //don't compare to zero but close to it, sometime hediff is removed naturally before then? 
            {
                OnFinishedTransformation();
            }
        }
    }
}
