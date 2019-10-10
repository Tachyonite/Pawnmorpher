using Pawnmorph;
using RimWorld;
using Verse;

namespace EtherGun
{
    public class Projectile_EtherBullet : Bullet
    {
        public ThingDef_EtherBullet Def => def as ThingDef_EtherBullet;

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
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
                    var randomSeverity = 1f;
                    if (etherOnPawn == null)
                    {
                        //These three lines create a new health differential or Hediff,
                        //put them on the character, and increase its severity by a random amount.
                        Hediff hediff = HediffMaker.MakeHediff(Def.HediffToAdd, hitPawn);
                        hediff.Severity = randomSeverity;
                        hitPawn.health.AddHediff(hediff);
                        IntermittentMagicSprayer.ThrowMagicPuffDown(hitPawn.Position.ToVector3(), Map);
                    }
                }
            }
        }
    }
}