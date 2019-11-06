// Worker_MutagenicInjury.cs modified by Iron Wolf for Pawnmorph on 11/03/2019 11:05 AM
// last updated 11/03/2019  11:05 AM

using System;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph.Damage
{

    /// <summary>
    /// damage worker that adds mutagenic buildup hediff in addition to regular injuries 
    /// </summary>
    /// <seealso cref="Verse.DamageWorker_AddInjury" />
    public class Worker_MutagenicInjury : DamageWorker_AddInjury
    {

        private const float SEVERITY_PER_DAMAGE = 0.04f;
        private const float REDUCE_VALUE = 1 / 3f;

        /// <summary>
        /// Applies the specified dinfo.
        /// </summary>
        /// <param name="dinfo">The dinfo.</param>
        /// <param name="thing">The thing.</param>
        /// <returns></returns>
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            if (thing is Pawn pawn)
            {
                return ApplyToPawn(dinfo, pawn); 
            }
            else
            {
                return base.Apply(dinfo, thing); 
            }
        }

        private DamageResult ApplyToPawn(DamageInfo dinfo, Pawn pawn)
        {
            //reduce the amount to make it less likely to kill the pawn 
            float originalDamage = dinfo.Amount;


            var r = MutagenDefOf.defaultMutagen.CanInfect(pawn) ? REDUCE_VALUE : 1;

            dinfo = MutagenicDamageUtilities.ReduceDamage(dinfo, r); 

            var res = base.Apply(dinfo, pawn);

            if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return res;
            MutagenicDamageUtilities.ApplyMutagenicDamage(originalDamage, dinfo, pawn, res);

            return res; 
        }

        private static void ApplyMutagenicBuildup(DamageInfo dinfo, Pawn pawn, DamageResult res, float oDamage)
        {
            var hediff = HediffMaker.MakeHediff(MorphTransformationDefOf.MutagenicBuildup_Weapon, pawn);
            hediff.Severity = oDamage * SEVERITY_PER_DAMAGE;
            pawn.health.AddHediff(hediff, null, dinfo, res);
        }
    }
}