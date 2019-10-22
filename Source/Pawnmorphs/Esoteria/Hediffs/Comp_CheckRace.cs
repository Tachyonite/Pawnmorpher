// Giver_CheckRace.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 6:03 PM
// last updated 08/03/2019  6:03 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff component that checks the race of a pawn at the end of a Hediff_Morph 
    /// </summary>
    /// this is a component because it's set to go off just when a hediff_Morph ends naturally (after reeling) 
    public class Comp_CheckRace : HediffCompBase<CompProperties_CheckRace>
    {
        private bool _checked; 
        private readonly List<VTuple<MorphDef, float>> _scratchList = new List<VTuple<MorphDef, float>>();
        public void CheckRace(Pawn pawn)
        {
            _checked = true; 
            if (pawn.ShouldBeConsideredHuman()) return;

            _scratchList.Clear();
            var linq = pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>().GetInfluences();
            _scratchList.AddRange(linq);

            if (_scratchList.Count == 0) return;

            MorphDef morph = null;
            float max = float.NegativeInfinity;
            foreach (var tuple in _scratchList)
                if (max < tuple.second)
                {
                    morph = tuple.first;
                    max = tuple.second;
                }

            if (morph == null)
            { //null means there is no clear dominant morph even thought the pawn isn't "human" anymore
                return; //TODO chimera race? 
            }


            if(morph.hybridRaceDef != pawn.def) //make sure the morph they're begin shifted to is different then they're current race 
                RaceShiftUtilities.ChangePawnToMorph(pawn, morph); 
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn.Dead || _checked) return;

            CheckRace(Pawn); 


        }

        private int _lastStage = -1; 
        public override void CompPostTick(ref float severityAdjustment)
        {
            
            base.CompPostTick(ref severityAdjustment);
            if (parent.CurStageIndex != _lastStage)
            {
                _lastStage = parent.CurStageIndex;
                if (_lastStage == Props.triggerStage)
                {
                    CheckRace(parent.pawn);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref _lastStage, nameof(_lastStage), -1);
            Scribe_Values.Look(ref _checked, nameof(_checked), false); 
        }
    }

    public class CompProperties_CheckRace : HediffCompProperties
    {
        public int triggerStage; 
        public CompProperties_CheckRace()
        {
            compClass = typeof(Comp_CheckRace); 
        }
    }

    

}