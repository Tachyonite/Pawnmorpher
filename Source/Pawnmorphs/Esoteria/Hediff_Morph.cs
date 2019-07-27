using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    class Hediff_Morph : HediffWithComps
    {
        private int _lastStage = -1;
        public override void PostTick()
        {
            base.PostTick();

            if (_lastStage != CurStageIndex)
            {

                _lastStage = CurStageIndex;


                if (CurStage.hediffGivers == null) return; 

                foreach (HediffGiver_TF tfGiver in CurStage.hediffGivers.OfType<HediffGiver_TF>())
                {

                    if(tfGiver.TryTf(pawn, this)) break; //try each one, one by one. break at first one that succeeds  


                }


            }
        }
    }
}
