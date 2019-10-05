// IngestionOutcomeDooer_Productive.cs modified by Iron Wolf for Pawnmorph on 10/05/2019 1:04 PM
// last updated 10/05/2019  1:04 PM

using RimWorld;
using Verse;

namespace Pawnmorph
{
    public class IngestionOutcomeDooer_AddAspect : IngestionOutcomeDoer
    {
        public AspectDef aspectDef;
        public bool increaseStage; //if true will increase the stage of the aspect by 1 every time the thing is consumed 
        
        

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            var aspectT = pawn.GetAspectTracker();
            if (aspectT == null) return;
            var aspect = aspectT.GetAspect(aspectDef);
            if (aspect == null)
            {
                aspectT.Add(aspectDef); 
            }else if (increaseStage)
            {
                aspect.StageIndex += 1; 
            }

        }
    }
}