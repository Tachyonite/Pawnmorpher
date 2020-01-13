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
    /// <summary>
    /// hediff representing 'morph transformations'
    /// </summary>
    /// <seealso cref="Verse.HediffWithComps" />
    public class Hediff_Morph : HediffWithComps
    {
        /// <summary>the stage to display a warning message about the pawn fully transforming.</summary>
        /// <value>The transformation warning stage.</value>
        protected virtual int TransformationWarningStage => 1;
        private const string TRANSFORMATION_WARNING_LETTER_ID = "TransformationStageWarning";

        private HediffComp_Single _comp;
        private MorphDef _morph;
        private List<HediffGiver_Mutation> _allGivers;
        private List<HediffGiver_Mutation> _givers;
        [Unsaved] private int _lastStage = -1; // ToDo can we save this?

        /// <summary>Gets the single comp.</summary>
        /// <value>The single comp.</value>
        [CanBeNull]
        public HediffComp_Single SingleComp
        {
            get { return _comp ?? (_comp = this.TryGetComp<HediffComp_Single>()); }
        }

        private List<HediffGiver_Mutation> AllGivers
        {
            get { return _allGivers ?? (_allGivers = def.GetAllHediffGivers().OfType<HediffGiver_Mutation>().ToList()); }
        }

        private MorphDef AssociatedMorph
        {
            get { return _morph ?? (_morph = MorphUtilities.GetAssociatedMorph(def).FirstOrDefault()); }
        }

        /// <summary>Gets the label base.</summary>
        /// <value>The label base.</value>
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

        /// <summary>Gets the mutation givers.</summary>
        /// <value>The mutation givers.</value>
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

        /// <summary>called after Tick is called on everything</summary>
        public override void PostTick()
        {
            base.PostTick();

            if (_lastStage != CurStageIndex)
            {
                _lastStage = CurStageIndex;
                EnterNextStage();
            }
        }

        /// <summary>Enters the next stage.</summary>
        protected virtual void EnterNextStage()
        {
            if (_lastStage == TransformationWarningStage && (pawn.IsColonist || pawn.IsPrisonerOfColony))
            {
                SendLetter();
            }
            
            TryGiveTransformations();
        }

        /// <summary>Tries to give transformations</summary>
        protected virtual void TryGiveTransformations()
        {
            if (CurStage == null) return;

            RandUtilities.PushState();

            foreach (var tfGiver in CurStage.GetAllTransformers())
            {
                if (tfGiver.TryTransform(pawn, this)) break; //try each one, one by one. break at first one that succeeds  
            }

            RandUtilities.PopState();
        }

        /// <summary>Tries the merge with.</summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public override bool TryMergeWith(Hediff other)
        {
            if (!base.TryMergeWith(other)) return false;

            ResetGivers();

            return true;
        }

        /// <summary>
        /// resets the hediff givers.
        /// </summary>
        protected void ResetGivers()
        {
            foreach (HediffGiver_Mutation hediffGiverMutation in MutationGivers) //make sure mutations can be re rolled 
            {
                hediffGiverMutation.ClearHediff(this);
            }
        }

        private void SendLetter()
        {
            var mutagen = this.GetMutagenDef();
            if (!mutagen.CanTransform(pawn)) return;

            var letterLabel = (TRANSFORMATION_WARNING_LETTER_ID + "Label").Translate(pawn);
            var letterContent = (TRANSFORMATION_WARNING_LETTER_ID + "Content").Translate(pawn);
            Find.LetterStack.ReceiveLetter(letterLabel, letterContent, LetterDefOf.NeutralEvent, new LookTargets(pawn));
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

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastStage, "lastStage", -1); 
        }

        /// <summary>called after this instance is removed</summary>
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
