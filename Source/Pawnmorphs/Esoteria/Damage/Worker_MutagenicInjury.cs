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
            dinfo = new DamageInfo(dinfo.Def,  dinfo.Amount * REDUCE_VALUE , dinfo.ArmorPenetrationInt, dinfo.Angle, dinfo.Instigator,
                                   dinfo.HitPart, dinfo.Weapon, dinfo.Category, dinfo.intendedTargetInt);

            var res = base.Apply(dinfo, pawn);

            if (res.deflected && !res.deflectedByMetalArmor)
            {
                //even if no damage is done still apply some buildup if the armor isn't metal 
                float oDamage = originalDamage * REDUCE_VALUE ;
                Log.Message($"Applying {oDamage * SEVERITY_PER_DAMAGE} worth of mutagenic buildup");
                ApplyMutagenicBuildup(dinfo, pawn, res, oDamage); 
            }else if (!res.deflected)
            {
                //make metal armor better at preventing mutagenic buildup 
                var oDamage = res.diminishedByMetalArmor ? res.totalDamageDealt : originalDamage;
                ApplyMutagenicBuildup(dinfo, pawn, res, oDamage);

            }

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