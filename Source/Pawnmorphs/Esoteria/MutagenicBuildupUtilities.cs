// MutagenicBuildupUtilties.cs created by Iron Wolf for Pawnmorph on 03/25/2020 7:20 PM
// last updated 03/25/2020  7:20 PM

using System;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Damage;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
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
        /// Determines whether this thing is a mutagenic weapon.
        /// </summary>
        /// <param name="thing">The thing.</param>
        /// <returns>
        ///   <c>true</c> if this thing is a mutagenic weapon; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">thing</exception>
        public static bool IsMutagenicWeapon([NotNull] this Thing thing)
        {
            if (thing == null) throw new ArgumentNullException(nameof(thing));
            return thing.def.IsMutagenicWeapon();
        }

        /// <summary>
        /// Determines whether this weapon is a mutagenic weapon.
        /// </summary>
        /// <param name="weaponDef">The weapon definition.</param>
        /// <returns>
        ///   <c>true</c> if this weapon is a mutagenic weapon; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">weaponDef</exception>
        public static bool IsMutagenicWeapon([NotNull] this ThingDef weaponDef)
        {
            if (weaponDef == null) throw new ArgumentNullException(nameof(weaponDef));
            if (!weaponDef.IsWeapon) return false;

            if (weaponDef.Verbs != null)
            {
                foreach (VerbProperties verbProps in weaponDef.Verbs)
                {
                    if (IsMutagenicVerb(verbProps)) return true; 
                }
            }

            if (weaponDef.tools != null)
            {
                foreach (Tool wTool in weaponDef.tools)
                {
                    foreach (VerbProperties wtVerb in wTool.VerbsProperties.MakeSafe())
                    {
                        if (IsMutagenicVerb(wtVerb)) return true; 
                    }
                }
            }

            var expComp = weaponDef.GetCompProperties<CompProperties_Explosive>();
            return expComp?.explosiveDamageType?.HasModExtension<MutagenicDamageExtension>() == true; 
        }


        static bool IsMutagenicVerb([NotNull] VerbProperties vProps)
        {
            if (vProps.meleeDamageDef?.HasModExtension<MutagenicDamageExtension>() == true) return true;
            if (vProps.defaultProjectile?.projectile?.damageDef?.HasModExtension<MutagenicDamageExtension>() == true) return true;
            return false; 
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