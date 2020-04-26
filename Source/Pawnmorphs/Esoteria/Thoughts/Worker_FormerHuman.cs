// Worker_FormerHuman.cs modified by Iron Wolf for Pawnmorph on 12/15/2019 4:21 PM
// last updated 12/15/2019  4:21 PM

using Pawnmorph.DefExtensions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// thought worker to give former humans a constant 'i'm an animal now' thought 
    /// </summary>
    /// <seealso cref="RimWorld.ThoughtWorker" />
    public class Worker_FormerHuman : ThoughtWorker
    {
        /// <summary>
        /// gets the current thought state for the pawn 
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns></returns>
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            var sapientLevel = p.GetQuantizedSapienceLevel();

            if (sapientLevel == null && p.GetSapienceState()?.StateDef != SapienceStateDefOf.FormerHuman) return false;

            if (!def.IsValidFor(p)) return false;


            var stage = Mathf.Min(def.stages.Count - 1, (int) sapientLevel.Value);
            return ThoughtState.ActiveAtStage(stage); 
        }
    }
}