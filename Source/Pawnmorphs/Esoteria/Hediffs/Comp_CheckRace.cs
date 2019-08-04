// Giver_CheckRace.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 6:03 PM
// last updated 08/03/2019  6:03 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Hybrids;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// hediff component that checks the race of a pawn at the end of a Hediff_Morph 
    /// </summary>
    /// this is a component because it's set to go off just when a hediff_Morph ends naturally (after reeling) 
    public class Comp_CheckRace : HediffComp, IPostTfHediffComp
    {
        private readonly List<MorphUtilities.Tuple> _scratchList = new List<MorphUtilities.Tuple>();
        
        public void CheckRace(Pawn pawn, Hediff_Morph morphTf)
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

        /// <summary>
        /// called when the morph hediff ends naturally (after reaching 0 or below severity) 
        /// </summary>
        public void FinishedTransformation(Pawn pawn, Hediff_Morph hediff)
        {
            Log.Message($"checking if {pawn.Name.ToStringFull} is still human!");
            CheckRace(pawn, hediff); 
        }
    }

    public class CompProperties_CheckRace : HediffCompProperties
    {
        public CompProperties_CheckRace()
        {
            compClass = typeof(Comp_CheckRace); 
        }
    }

    

}