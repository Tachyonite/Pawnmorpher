// Worker_HasMutations.cs created by Iron Wolf for Pawnmorph on 09/18/2019 2:14 PM
// last updated 09/18/2019  2:14 PM

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    public class Worker_HasMutations : ThoughtWorker
    {
        private static List<Worker_HasMutations> specificHandlers = new List<Worker_HasMutations>();

        private List<Thought> _scratchList = new List<Thought>(); 


        public Worker_HasMutations()
        {
            specificHandlers.Add(this); 
        }


        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            
            var mutTracker = p.GetMutationTracker();
            if (mutTracker == null) return false;

            var morphTDef = def as Def_MorphThought;
            bool isDefault = morphTDef != null; 





            if (!mutTracker.AllMutations.Any())
            {
                
                return false;
            }
            
            //check morph if available 
           
            if (!isDefault)
            {
                isDefault = false; 
                if (mutTracker.HighestInfluence != morphTDef.morph)
                {
                    return false; 
                }


                if (mutTracker[morphTDef.morph] <= 0) return false; 
            }
            else
            {
                var thoughtHandler = p.needs.mood.thoughts; 
                
                _scratchList.Clear();
                thoughtHandler.situational.AppendMoodThoughts(_scratchList);
                if (_scratchList.Any(t => t.def != def && t.def.workerClass == GetType() && t.def is Def_MorphThought))
                    return false;  //if any thoughts are of this class nad have a specific morph associated with them do no activate the default thought 




            }
          





            var animalInfluence = mutTracker.TotalNormalizedInfluence;

            


            //now get the stage number 



            var num = Mathf.FloorToInt(animalInfluence * def.stages.Count);
            num = Mathf.Clamp(num, 0, def.stages.Count - 1); 
            
            return ThoughtState.ActiveAtStage(num); 
            
        }
    }
}