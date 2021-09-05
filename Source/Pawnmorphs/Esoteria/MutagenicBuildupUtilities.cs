// MutagenicBuildupUtilties.cs created by Iron Wolf for Pawnmorph on 03/25/2020 7:20 PM
// last updated 03/25/2020  7:20 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph
{

    /// <summary>
    /// static class for various mutagenic buildup related utilities 
    /// </summary>
    public static class MutagenicBuildupUtilities
    {
        /// <summary>
        /// Gets the net mutagenic buildup multiplier for this pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="mutagenDef">The mutagen definition.</param>
        /// <returns></returns>
        public static float GetMutagenicBuildupMultiplier([NotNull] this Pawn pawn, MutagenDef mutagenDef = null)
        {
            mutagenDef = mutagenDef ?? MutagenDefOf.defaultMutagen;
            if (!mutagenDef.CanInfect(pawn)) return 0; 
            return (pawn.GetStatValue(StatDefOf.ToxicSensitivity)*pawn.GetStatValue(PMStatDefOf.MutagenSensitivity)); 
        }


        /// <summary>
        /// Adjusts the mutagenic buildup for the given pawn using the given source
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="adjustValue">The adjust value.</param>
        /// <param name="overrideMutagen">The override mutagen.</param>
        /// <returns></returns>
        public static float AdjustMutagenicBuildup([CanBeNull] Def source, [NotNull] Pawn pawn, float adjustValue, MutagenDef overrideMutagen=null)
        {
            var settings = source?.GetModExtension<MutagenicBuildupSourceSettings>();
            float max = settings?.maxBuildup ?? 1;
            var hediffDef = settings?.mutagenicBuildupDef ?? MorphTransformationDefOf.MutagenicBuildup;
            var mutagen = overrideMutagen ?? settings?.mutagenDef ?? MutagenDefOf.defaultMutagen;
            if (!mutagen.CanInfect(pawn)) return 0; 
            adjustValue *= pawn.GetMutagenicBuildupMultiplier(mutagen);  

            var fHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (fHediff == null)
            {
                fHediff = HediffMaker.MakeHediff(hediffDef, pawn);
                fHediff.Severity = 0; 
                pawn.health.AddHediff(fHediff);
                if (fHediff is Hediff_MutagenicBase mBase)
                {
                    mBase.Causes.Add(MutationCauses.MUTAGEN_PREFIX, mutagen); 
                }
            }

            if (fHediff.Severity > max) return 0;
            float sev = fHediff.Severity;
            float finalSev = Mathf.Min(fHediff.Severity + adjustValue, max);
            fHediff.Severity = finalSev;

            float aSev = Mathf.Max(sev, 0.01f); //prevent division by zero 
            if (adjustValue / aSev > 0.5f && fHediff is Hediff_MutagenicBase mutagenicBase && !mutagenicBase.Causes.HasDefCause(mutagen))
            {
                mutagenicBase.Causes.Add(MutationCauses.MUTAGEN_PREFIX, mutagen); 
            }
            return finalSev - sev; 
        }
    }
}