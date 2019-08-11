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
        [Unsaved]
        private int _lastStage = -1; //ToDO can we save this?
        public override void PostTick()
        {
            base.PostTick();

            if (_lastStage != CurStageIndex && _lastStage != -1)
            {

                _lastStage = CurStageIndex;


                if (CurStage.hediffGivers == null) return; 

                foreach (HediffGiver_TF tfGiver in CurStage.hediffGivers.OfType<HediffGiver_TF>())
                {

                    if(tfGiver.TryTf(pawn, this)) break; //try each one, one by one. break at first one that succeeds  


                }


            }

            if (_lastStage == -1) _lastStage = CurStageIndex; //this means the above branch cannot be triggered on load 
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
