using System.Collections.Generic;
using System.Linq;
using Verse;

namespace EtherGun
{
	class Projectile_EtherExplosive : Projectile_Explosive
	{
		// Reuse of the ThingDef for EtherBullet because it needs the same information.
		public ThingDef_EtherBullet Def
		{
			get
			{
				return def as ThingDef_EtherBullet;
			}
		}

		// An override of the Explode method that allows us to insert our own custom code first.
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

			// No idea why, but it errors if we call the underride before the custom check.
			base.Explode();
		}
	}
}
