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
                return this.def as ThingDef_EtherBullet;
            }
        }
        #endregion

        #region Overrides
        protected override void Explode()
        {
            List<Thing> thingList = GenRadial.RadialDistinctThingsAround(Position, Map, def.projectile.explosionRadius, true).ToList();
            List<Pawn> pawnsAffected = new List<Pawn>();
            HediffDef hediff = Def.HediffToAdd;
            float chance = Def.AddHediffChance;

            for (int i = 0; i < thingList.Count; i++)
            {
                Pawn pawn = thingList[i] as Pawn;
                if (pawn != null && !pawnsAffected.Contains(pawn))
                {
                    pawnsAffected.Add(pawn);
                }
            }

            TransformPawn.ApplyHediff(pawnsAffected, Map, hediff, chance);
            base.Explode();
        }
        #endregion Overrides
    }
}
