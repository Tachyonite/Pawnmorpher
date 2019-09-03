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
        private readonly List<MorphUtilities.Tuple> _scratchList = new List<MorphUtilities.Tuple>();
        public void CheckRace(Pawn pawn)
        {
            if (pawn.ShouldBeConsideredHuman()) return;

            _scratchList.Clear();
            IEnumerable<MorphUtilities.Tuple> linq = pawn.health.hediffSet.hediffs.OfType<Hediff_AddedMutation>().GetInfluences();
            _scratchList.AddRange(linq);

            if (_scratchList.Count == 0) return;

            MorphDef morph = null;
            float max = float.NegativeInfinity;
            foreach (MorphUtilities.Tuple tuple in _scratchList)
                if (max < tuple.influence)
                {
                    morph = tuple.morph;
                    max = tuple.influence;
                }

            if (morph == null)
            { //null means there is no clear dominant morph even thought the pawn isn't "human" anymore
                return; //TODO chimera race? 
            }

            RaceShiftUtilities.ChangePawnToMorph(pawn, morph); 
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