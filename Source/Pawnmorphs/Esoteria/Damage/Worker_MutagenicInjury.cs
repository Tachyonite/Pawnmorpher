// Worker_MutagenicInjury.cs modified by Iron Wolf for Pawnmorph on 11/03/2019 11:05 AM
// last updated 11/03/2019  11:05 AM

using System;
using System.Collections.Generic;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Damage
{

    /// <summary>
    /// damage worker that adds mutagenic buildup hediff in addition to regular injuries 
    /// </summary>
    /// <seealso cref="Verse.DamageWorker_AddInjury" />
    public class Worker_MutagenicInjury : DamageWorker_AddInjury
    {
        /// <summary>
        /// values below this should be considered 0
        /// </summary>
        protected const float EPSILON = 0.001f; 


        /// <summary>
        /// Adds some extra buildup. taking into account toxic resistance and immunities 
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="dInfo">The d information.</param>
        protected void AddExtraBuildup(Pawn pawn, DamageInfo dInfo)
        {
            if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return; 
            var extraSeverity = dInfo.Amount * 0.02f * dInfo.GetSeverityPerDamage();
            extraSeverity *= pawn.GetStatValue(StatDefOf.ToxicSensitivity);

            if (Mathf.Abs(extraSeverity) < EPSILON) return; 

            var hDef = dInfo.Def.GetModExtension<MutagenicDamageExtension>()?.mutagenicBuildup
                    ?? MorphTransformationDefOf.MutagenicBuildup_Weapon;
            HealthUtility.AdjustSeverity(pawn, hDef, extraSeverity);
            return;
        }


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

            dinfo = ReduceDamage(dinfo, pawn);  

            var res = base.Apply(dinfo, pawn);
            var severityPerDamage = dinfo.GetSeverityPerDamage(); 
            if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return res;
            MutagenicDamageUtilities.ApplyMutagenicDamage(originalDamage, dinfo, pawn, res, severityPerDamage:severityPerDamage);

            return res; 
        }

        /// <summary>Reduces the damage.</summary>
        /// <param name="dInfo">The d information.</param>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        protected DamageInfo ReduceDamage(DamageInfo dInfo, Pawn pawn)
        {
            float r;
            if (MutagenDefOf.defaultMutagen.CanInfect(pawn))
                r = dInfo.Def.GetModExtension<MutagenicDamageExtension>()?.reduceValue ?? REDUCE_VALUE;
            else
                r = 1; 
            return MutagenicDamageUtilities.ReduceDamage(dInfo, r);
        }

        /// <summary>
        /// Adds the mutation on.
        /// </summary>
        /// <param name="forceHitPart">The force hit part.</param>
        /// <param name="pawn">The pawn.</param>
        protected void AddMutationOn(BodyPartRecord forceHitPart, Pawn pawn)
        {
            if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return;  
            while (forceHitPart != null)
            {
                HediffGiver_Mutation giver = MutationUtilities.GetMutationsFor(forceHitPart.def).RandomElementWithFallback();
                if (giver == null)
                {
                    forceHitPart = forceHitPart.parent; //go upward until we hit a mutable part 
                    continue;
                }

                if (!giver.TryApply(pawn, forceHitPart))
                {
                    forceHitPart = forceHitPart.parent; //go upward until we hit a mutable part  
                    continue;
                }

                break; //if we apply a mutation break the loop 
            }
        }




        /// <summary>
        /// called when an explosion damages the given thing 
        /// </summary>
        /// <param name="explosion">The explosion.</param>
        /// <param name="t">The t.</param>
        /// <param name="damagedThings">The damaged things.</param>
        /// <param name="cell">The cell.</param>
        protected override void ExplosionDamageThing(Explosion explosion, Thing t, List<Thing> damagedThings, IntVec3 cell)
        {

            if (t.def.category == ThingCategory.Mote || t.def.category == ThingCategory.Ethereal)
            {
                return;
            }
            if (damagedThings.Contains(t))
            {
                return;
            }
            damagedThings.Add(t);
            //it puts out fires 
            if (this.def == DamageDefOf.Bomb && t.def == ThingDefOf.Fire && !t.Destroyed)
            {
                t.Destroy(DestroyMode.Vanish);
                return;
            }
            //only affects pawns 

            if (!(t is Pawn pawn)) return;

            if (!MutagenDefOf.defaultMutagen.CanInfect(pawn)) return; //only affects susceptible pawns 


            float num;
            if (t.Position == explosion.Position)
            {
                num = (float)Rand.RangeInclusive(0, 359);
            }
            else
            {
                num = (t.Position - explosion.Position).AngleFlat;
            }
            DamageDef damageDef = this.def;
            float amount = (float)explosion.GetDamageAmountAt(cell);
            float armorPenetrationAt = explosion.GetArmorPenetrationAt(cell);
            float angle = num;
            Thing instigator = explosion.instigator;
            ThingDef weapon = explosion.weapon;
            DamageInfo dinfo = new DamageInfo(damageDef, amount, armorPenetrationAt, angle, instigator, null, weapon, DamageInfo.SourceCategory.ThingOrUnknown, explosion.intendedTarget);
            var severityPerDamage = dinfo.GetSeverityPerDamage();
            MutagenicDamageUtilities.ApplyPureMutagenicDamage(dinfo, pawn,
                                                              severityPerDamage: severityPerDamage); 


            BattleLogEntry_ExplosionImpact battleLogEntry_ExplosionImpact = null;
            battleLogEntry_ExplosionImpact = new BattleLogEntry_ExplosionImpact(explosion.instigator, t, explosion.weapon, explosion.projectile, this.def);
            Find.BattleLog.Add(battleLogEntry_ExplosionImpact);

            pawn.stances?.StaggerFor(95);
            
        }
    }
}