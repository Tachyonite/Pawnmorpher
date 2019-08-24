using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Hediffs;
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

                _lastStage = CurStageIndex;

                if (_lastStage == TRANSFORMATION_STAGE_INDEX)
                {
                    SendLetter(); 



                }



                if (CurStage.hediffGivers == null) return; 

                foreach (HediffGiver_TF tfGiver in CurStage.hediffGivers.OfType<HediffGiver_TF>())
                {

                    if(tfGiver.TryTf(pawn, this)) break; //try each one, one by one. break at first one that succeeds  


                }


            }

          
        }

        private void SendLetter()
        {
            var letterDef = LetterDefOf.NeutralEvent; //not sure what to use here 

            var letterLabel = (TRANSFORMATION_WARNING_LETTER_ID + "Label").Translate(pawn);
            var letterContent = (TRANSFORMATION_WARNING_LETTER_ID + "Content").Translate(pawn);
            Find.LetterStack.ReceiveLetter(letterLabel, letterContent, letterDef, pawn); 

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
