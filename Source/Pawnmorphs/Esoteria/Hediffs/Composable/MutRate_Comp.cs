// MutRate_Comp.cs created by Iron Wolf for Pawnmorph on 09/05/2021 8:28 PM
// last updated 09/05/2021  8:28 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// mute rate that defers it's logic to a <see cref="HediffComp_Composable"/> class 
    /// </summary>
    /// <seealso cref="Pawnmorph.Hediffs.Composable.MutRate" />
    public class MutRate_Comp : MutRate
    {

        [CanBeNull]
        MutRate GetCompRate([NotNull] Hediff hDiff)
        {
            var comp = hDiff.TryGetComp<HediffComp_Composable>();
            if (comp == null)
                Log.ErrorOnce($"{hDiff.def} has {nameof(MutRate_Comp)} but no {nameof(HediffComp_Composable)}!",
                              hDiff.def.shortHash);
            MutRate rate = comp?.Rate;
            if (rate == null)
                Log.ErrorOnce($"{hDiff.def} has {nameof(MutRate_Comp)} but {nameof(HediffComp_Composable)} is missing {nameof(HediffCompProps_Composable.mutRate)}!",
                              hDiff.def.shortHash);
            return rate;
        }

        /// <summary>
        /// How many mutations to queue up for the next second.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public override int GetMutationsPerSecond(Hediff_MutagenicBase hediff)
        {
            return GetCompRate(hediff)?.GetMutationsPerSecond(hediff) ?? 0; 
        }

        /// <summary>
        /// How many mutations to queue up for a given severity change.  Note that severity
        /// changes can be negative, and negative mutations are allowed.
        /// (negative mutations can cancel queued mutations but won't remove existing ones)
        /// 
        /// Called any time severity changes in Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        /// <param name="sevChange">How much severity changed by.</param>
        public override int GetMutationsPerSeverity(Hediff_MutagenicBase hediff, float sevChange)
        {
            return GetCompRate(hediff)?.GetMutationsPerSeverity(hediff, sevChange) ?? 0; 
        }

        /// <summary>
        /// gets all configuration errors in this stage .
        /// </summary>
        /// <param name="parentDef">The parent definition.</param>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            var props = parentDef.CompProps<HediffCompProps_Composable>();
            if (props == null) yield return $"no {nameof(HediffCompProps_Composable)}!";
        }
    }
}