// MutagenicDamageUtilities.cs created by Iron Wolf for Pawnmorph on 11/06/2019 7:47 AM
// last updated 11/06/2019  7:47 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.Damage
{
    /// <summary>
    ///     static class containing utilities related to mutagenic damage
    /// </summary>
    public class MutagenicDamageUtilities
    {
        /// <summary>the amount of hediff severity to add per point of damage </summary>
        public const float SEVERITY_PER_DAMAGE = 0.04f;

        /// <summary>the fraction by which the dInfo damage will be reduced by</summary>
        public const float REDUCE_VALUE = 1 / 3f;

        /// <summary>Applies the mutagenic damage.</summary>
        /// <param name="originalDamage">The original damage.</param>
        /// <param name="damageInfo">The damage information.</param>
        /// <param name="pawn">The pawn.</param>
        /// <param name="result">The result.</param>
        /// <param name="mutagenicDef">The definition of the mutagenic damage hediff to add.</param>
        /// <param name="severityPerDamage">The severity per damage.</param>
        /// <param name="mutagen">The mutagen.</param>
        /// <exception cref="ArgumentNullException">
        ///     pawn
        ///     or
        ///     result
        /// </exception>
        public static void ApplyMutagenicDamage(float originalDamage, DamageInfo damageInfo, [NotNull] Pawn pawn,
                                                [NotNull] DamageWorker.DamageResult result,
                                                HediffDef mutagenicDef=null, float severityPerDamage = SEVERITY_PER_DAMAGE,
                                                MutagenDef mutagen = null)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            if (result == null) throw new ArgumentNullException(nameof(result));
            mutagen = mutagen ?? MutagenDefOf.defaultMutagen;
            mutagenicDef = mutagenicDef ?? MorphTransformationDefOf.MutagenicBuildup_Weapon;

            if (!mutagen.CanInfect(pawn)) return;

            ApplyMutagen(originalDamage, damageInfo, pawn, mutagenicDef, severityPerDamage, result);
        }

        /// <summary>Reduces the damage by some amount.</summary>
        /// <param name="dInfo">The d information.</param>
        /// <param name="reduceAmount">The reduce amount.</param>
        /// <returns>a new damage info struct with the modified damage</returns>
        public static DamageInfo ReduceDamage(DamageInfo dInfo, float reduceAmount = REDUCE_VALUE)
        {
            float reducedDamage = dInfo.Amount * reduceAmount;

            dInfo = new DamageInfo(dInfo.Def, reducedDamage, dInfo.ArmorPenetrationInt, dInfo.Angle, dInfo.Instigator,
                                   dInfo.HitPart, dInfo.Weapon, dInfo.Category, dInfo.intendedTargetInt);
            return dInfo;
        }

        private static void ApplyMutagen(float damage, DamageInfo info, Pawn pawn, HediffDef mutagen, float severityPerDamage,
                                         DamageWorker.DamageResult result)
        {
            Hediff hediff = HediffMaker.MakeHediff(mutagen, pawn);
            float severityToAdd = damage * severityPerDamage;

            //apply  toxin resistance 
            severityToAdd *= pawn.GetStatValue(StatDefOf.ToxicSensitivity); //[0, 1?] 
            //0 is immune 
            //Add our own mutagenic sensitivity stat to? 
            hediff.Severity = severityToAdd;

            pawn.health.AddHediff(hediff, null, info, result);
        }
    }
}