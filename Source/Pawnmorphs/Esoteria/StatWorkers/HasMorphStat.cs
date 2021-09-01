// HasMorphStat.cs created by Iron Wolf for Pawnmorph on 09/01/2021 5:21 PM
// last updated 09/01/2021  5:21 PM

using RimWorld;
using Verse;

namespace Pawnmorph.StatWorkers
{
    /// <summary>
    /// stat worker for the utility stat HasMorph 
    /// </summary>
    /// <seealso cref="RimWorld.StatWorker" />
    public class HasMorphStat : StatWorker
    {
        /// <summary>
        /// Determines whether this stat is disabled for the given thing.
        /// </summary>
        /// <param name="thing">The thing.</param>
        /// <returns>
        ///   <c>true</c> if this stat is disabled for the given thing; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsDisabledFor(Thing thing)
        {
            if (thing?.def?.race?.Animal != true) return true;
            if (thing.def.TryGetBestMorphOfAnimal() == null) return true; 
            return false; 
        }

        private const string NO_MORPH_LABEL = "PmMorphInfo_NoMorph";
        private const string MORPH_LABEL = "PmMorphInfo_MorphDisplay";
        private const string MORPH_TAG = "MORPH";

        /// <summary>
        /// Gets the stat draw entry label.
        /// </summary>
        /// <param name="stat">The stat.</param>
        /// <param name="value">The value.</param>
        /// <param name="numberSense">The number sense.</param>
        /// <param name="optionalReq">The optional req.</param>
        /// <param name="finalized">if set to <c>true</c> [finalized].</param>
        /// <returns></returns>
        public override string GetStatDrawEntryLabel(StatDef stat, float value, ToStringNumberSense numberSense, StatRequest optionalReq,
                                                     bool finalized = true)
        {
            var def = optionalReq.Def as ThingDef;
            def = def ?? optionalReq.Pawn?.def; 
            var morph = def?.TryGetBestMorphOfAnimal();
            if (morph == null)
            {
                return NO_MORPH_LABEL.Translate(); 
            }

            return MORPH_LABEL.Translate(morph.Named(MORPH_TAG));
        }

      
    }
}