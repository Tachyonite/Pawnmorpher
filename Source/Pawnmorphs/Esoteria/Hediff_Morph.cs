using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Multiplayer.API;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class Hediff_Morph : HediffWithComps
    {
        private const int TRANSFORMATION_STAGE_INDEX = 1;
        private const string TRANSFORMATION_WARNING_LETTER_ID = "TransformationStageWarning"; 

        [Unsaved]
        private int _lastStage = -1; //ToDO can we save this?

        public override void PostTick()
        {
            base.PostTick();

            if (_lastStage != CurStageIndex)
            {
                if (MP.IsInMultiplayer) Rand.PushState(RandUtilities.MPSafeSeed);
                _lastStage = CurStageIndex;

                if (_lastStage == TRANSFORMATION_STAGE_INDEX) SendLetter();


                if (CurStage.hediffGivers == null) return;

                foreach (HediffGiver_TF tfGiver in CurStage.hediffGivers.OfType<HediffGiver_TF>())
                    if (tfGiver.TryTf(pawn, this))
                        break; //try each one, one by one. break at first one that succeeds  

                if (MP.IsInMultiplayer) Rand.PopState();
            }
        }

        private void SendLetter()
        {
            Messages.Message((TRANSFORMATION_WARNING_LETTER_ID).Translate(pawn),def:MessageTypeDefOf.NegativeHealthEvent);

        }

        /// <summary>
        /// this is called when the transformation hediff is removed naturally (after reaching a severity of 0) 
        /// </summary>
        /// this is only called when the hediff is removed after reaching a severity of zero, not when the pawn it's self is removed 
        protected virtual void OnFinishedTransformation()
        {
            //Log.Message($"{pawn.Name.ToStringFull} has finished transforming!");
            foreach (IPostTfHediffComp postTfHediffComp in comps.OfType<IPostTfHediffComp>())
            {
                postTfHediffComp.FinishedTransformation(pawn, this); 
            }

        }

        public override void Tick()
        {
            if (MP.IsInMultiplayer)
            {
                Rand.PushState(RandUtilities.MPSafeSeed);
            }
            base.Tick();

            if (MP.IsInMultiplayer)
            {
                Rand.PopState();
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
