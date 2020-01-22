// Worker_SapientAnimalBondedMaster.cs modified by Iron Wolf for Pawnmorph on 12/22/2019 8:47 PM
// last updated 12/22/2019  8:47 PM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// 
    /// </summary>
    public class Worker_SapientAnimalBondedMaster : ThoughtWorker
    {
        /// <summary>
        /// gets the current state .
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            SapienceLevel? sapienceLevel = p.GetQuantizedSapienceLevel();
            if (sapienceLevel == null || sapienceLevel == SapienceLevel.Feral || sapienceLevel == SapienceLevel.PermanentlyFeral)  return false;

            if (p.Faction != Faction.OfPlayer) return false;

            bool isBonded = false;
            bool masterToBondedPawn = false;
            foreach (DirectPawnRelation relationsDirectRelation in p.relations.DirectRelations)
            {
                if(relationsDirectRelation.def != PawnRelationDefOf.Bond) continue;
                if (relationsDirectRelation.otherPawn?.GetFormerHumanStatus() != null) continue; //ignore bonds to former humans 

                isBonded = true;//we can have only 1 bonded relationship 
                masterToBondedPawn = relationsDirectRelation.otherPawn == p.playerSettings?.RespectedMaster; 
                break;
            }

            if (isBonded && masterToBondedPawn)
            {
                var stage = Mathf.Min(def.stages.Count - 1, (int) sapienceLevel);
                return ThoughtState.ActiveAtStage(stage); 
            }

            return false; 

        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    public class Worker_SapientAnimalBondedNonMaster : ThoughtWorker
    {

        /// <summary>
        ///gets the current state .
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            SapienceLevel? sapienceLevel = p.GetQuantizedSapienceLevel();
            if (sapienceLevel == null || sapienceLevel == SapienceLevel.Feral || sapienceLevel == SapienceLevel.PermanentlyFeral) return false;
            if (p.Faction != Faction.OfPlayer) return false;

            bool isBonded = false;
            bool masterToBondedPawn = false;
            foreach (DirectPawnRelation relationsDirectRelation in p.relations.DirectRelations)
            {
                if (relationsDirectRelation.def != PawnRelationDefOf.Bond) continue;
                if(relationsDirectRelation.otherPawn?.GetFormerHumanStatus() != null) continue; //ignore bonds to former humans 

                isBonded = true;//we can have only 1 bonded relationship 
                masterToBondedPawn = relationsDirectRelation.otherPawn == p.playerSettings?.RespectedMaster;
                break;
            }

            if (isBonded && !masterToBondedPawn)
            {
                var stage = Mathf.Min(def.stages.Count - 1, (int)sapienceLevel);
                return ThoughtState.ActiveAtStage(stage);
            }

            return false;

        }
    }
}