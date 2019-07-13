using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Pawnmorph;

namespace EtherGun
{
    class Projectile_EtherExplosive : Projectile_Explosive
    {
        #region Properties
        public ThingDef_EtherBullet Def
        {
            get
            {
                //Case sensitive! If you use Def it will return Def, which is this getter. This will cause a never ending cycle and a stack overflow.
                return this.def as ThingDef_EtherBullet;
            }
        }
        #endregion

        #region Overrides
        protected override void Explode()
        {
            this.Transform();
            Map map = base.Map;
            this.Destroy(DestroyMode.Vanish);
            if (this.def.projectile.explosionEffect != null)
            {
                Effecter effecter = this.def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(base.Position, map, false), new TargetInfo(base.Position, map, false));
                effecter.Cleanup();
            }
            IntVec3 position = base.Position;
            Map map2 = map;
            float explosionRadius = this.def.projectile.explosionRadius;
            DamageDef damageDef = this.def.projectile.damageDef;
            Thing launcher = this.launcher;
            int damageAmount = base.DamageAmount;
            float armorPenetration = base.ArmorPenetration;
            SoundDef soundExplode = this.def.projectile.soundExplode;
            ThingDef equipmentDef = this.equipmentDef;
            ThingDef def = this.def;
            Thing thing = this.intendedTarget.Thing;
            ThingDef postExplosionSpawnThingDef = this.def.projectile.postExplosionSpawnThingDef;
            float postExplosionSpawnChance = this.def.projectile.postExplosionSpawnChance;
            int postExplosionSpawnThingCount = this.def.projectile.postExplosionSpawnThingCount;
            ThingDef preExplosionSpawnThingDef = this.def.projectile.preExplosionSpawnThingDef;
            GenExplosion.DoExplosion(position, map2, explosionRadius, damageDef, launcher, damageAmount, armorPenetration, soundExplode, equipmentDef, def, thing, postExplosionSpawnThingDef, postExplosionSpawnChance, postExplosionSpawnThingCount, this.def.projectile.applyDamageToExplosionCellsNeighbors, preExplosionSpawnThingDef, this.def.projectile.preExplosionSpawnChance, this.def.projectile.preExplosionSpawnThingCount, this.def.projectile.explosionChanceToStartFire, this.def.projectile.explosionDamageFalloff);
        }
        #endregion Overrides

        protected void Transform()
        {
            List<Pawn> pawnsAffected = new List<Pawn>();

            List<Thing> thingList = GenRadial.RadialDistinctThingsAround(base.Position, base.Map, this.def.projectile.explosionRadius, true).ToList();
            for (int i = 0; i < thingList.Count; i++)
            {
                Pawn pawn = thingList[i] as Pawn;
                if (pawn != null && !pawnsAffected.Contains(pawn))
                {
                    pawnsAffected.Add(pawn);
                }
            }
            for (int i = 0; i < pawnsAffected.Count(); i++)
            {
                var rand = Rand.Value;
                if (rand <= Def.AddHediffChance)
                {
                    var etherOnPawn = pawnsAffected[i].health?.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);
                    var randomSeverity = 1f;
                    if (etherOnPawn != null)
                    {
                        etherOnPawn.Severity += randomSeverity;
                    }
                    else
                    {
                        Hediff hediff = HediffMaker.MakeHediff(Def.HediffToAdd, pawnsAffected[i]);
                        hediff.Severity = randomSeverity;
                        pawnsAffected[i].health.AddHediff(hediff);
                        IntermittentMagicSprayer.ThrowMagicPuffDown(pawnsAffected[i].Position.ToVector3(), base.Map);
                    }
                }
            }

            //    List<IntVec3> areaAffected = new List<IntVec3>();
            //    List<Pawn> pawnsInMap = this.Map.mapPawns.AllPawnsSpawned;

            //    // This creates a list of quordinates affected.
            //    for (int i = 0; i < GenRadial.NumCellsInRadius(this.def.projectile.explosionRadius); i++)
            //    {
            //        IntVec3 intVec = base.Position + GenRadial.RadialPattern[i];
            //        if (intVec.InBounds(base.Map))
            //        {
            //            if (GenSight.LineOfSight(base.Position, intVec, base.Map, true, null, 0, 0))
            //            {
            //                areaAffected.Add(intVec);
            //            }
            //        }
            //    }

            //    // This cross-refences the coords found with pawn cords.
            //    for (int i = 0; i < areaAffected.Count; i++)
            //    {
            //        for (int j = 0; j < pawnsInMap.Count; j++)
            //        {
            //            if (areaAffected[i] == pawnsInMap[j].Position)
            //            {
            //                pawnsAffected.Add(pawnsInMap[j]);
            //            }
            //        }
            //    }

            //    // For the pawns in the area, apply the EtherBullet's effects
            //    for (int i = 0; i < pawnsAffected.Count; i++)
            //    {
            //        var rand = Rand.Value;
            //        if (rand <= Def.AddHediffChance)
            //        {
            //            var etherOnPawn = pawnsAffected[i].health?.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);
            //            var randomSeverity = 1f;
            //            if (etherOnPawn != null)
            //            {
            //                etherOnPawn.Severity += randomSeverity;
            //            }
            //            else
            //            {
            //                Hediff hediff = HediffMaker.MakeHediff(Def.HediffToAdd, pawnsAffected[i]);
            //                hediff.Severity = randomSeverity;
            //                pawnsAffected[i].health.AddHediff(hediff);
            //                IntermittentMagicSprayer.ThrowMagicPuffDown(pawnsAffected[i].Position.ToVector3(), base.Map);
            //            }
            //        }
            //    }
        }
    }
}
