using Pawnmorph;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace EtherGun
{
	/// <summary>
	/// bullet that are mutagenic in nature 
	/// </summary>
	/// <seealso cref="RimWorld.Bullet" />
	public class Projectile_EtherBullet : Bullet
	{

		ThingDef_EtherBullet Def => def as ThingDef_EtherBullet;

		/// <summary>
		/// called when this instance impacts the specified thing.
		/// </summary>
		/// <param name="hitThing">The hit thing.</param>
		/// <param name="blockedByShield"></param>
		protected override void Impact(Thing hitThing, bool blockedByShield = false)
		{
			base.Impact(hitThing, blockedByShield);
			Pawn hitPawn;
			if (Def != null && hitThing != null && hitThing is Pawn)
			{
				hitPawn = (Pawn)hitThing; // Already checked above.

				if (!Def.CanAddHediffToPawn(hitPawn)) return; //if the hediff can't be added to the hit pawn just abort 

				float rand = Rand.Value; // This is a random percentage between 0% and 100%
				if (rand <= Def.AddHediffChance) // If the percentage falls under the chance, success!
				{
					//This checks to see if the character has a heal differential, or hediff on them already.
					Hediff etherOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);

					if (etherOnPawn == null)
					{
						//These three lines create a new health differential or Hediff,
						//put them on the character, and increase its severity by a random amount.
						Hediff hediff = HediffMaker.MakeHediff(Def.HediffToAdd, hitPawn);
						hitPawn.health?.AddHediff(hediff);

						//this should be an interface 
						var syringeHediff = hediff as SyringeRifleTf;

						//hacky, want to figure out a better way to find the weapon that will allow turrets as well 
						Thing weapon = (launcher as Pawn)?.equipment?.Primary;
						weapon = weapon ?? (launcher as Building_TurretGun)?.gun;
						syringeHediff?.Initialize(weapon);

						IntermittentMagicSprayer.ThrowMagicPuffDown(hitPawn.Position.ToVector3(), Map);
					}
				}
			}
		}
	}
}