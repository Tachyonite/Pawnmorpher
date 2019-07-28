using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Pawnmorph;

namespace EtherGun
{
    public class Projectile_EtherBullet : Bullet
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
        protected override void Impact(Thing hitThing)
        {

            base.Impact(hitThing);
            Pawn hitPawn;
            if (Def != null && hitThing != null && hitThing is Pawn)
            {
                hitPawn = (Pawn) hitThing; //already checked above 

                if (!Def.CanAddHediffToPawn(hitPawn)) return; //if the hediff can't be added to the hit pawn just abort 

                var rand = Rand.Value; // This is a random percentage between 0% and 100%
                if (rand <= Def.AddHediffChance) // If the percentage falls under the chance, success!
                {

                    //This checks to see if the character has a heal differential, or hediff on them already.
                    var etherOnPawn = hitPawn.health?.hediffSet?.GetFirstHediffOfDef(Def.HediffToAdd);
                    var randomSeverity = 1f;
                    if (etherOnPawn == null)
                    {
                        //These three lines create a new health differential or Hediff,
                        //put them on the character, and increase its severity by a random amount.
                        Hediff hediff = HediffMaker.MakeHediff(Def.HediffToAdd, hitPawn);
                        hediff.Severity = randomSeverity;
                        hitPawn.health.AddHediff(hediff);
                        IntermittentMagicSprayer.ThrowMagicPuffDown(hitPawn.Position.ToVector3(), base.Map);
                    }
                }
                else //failure!
                {
                }
            }
        }
        #endregion Overrides
    }

    public class Projectile_TaggingBullet : Bullet
    {
        #region Properties
        public ThingDef_TaggingBullet Def
        {
            get
            {
                //Case sensitive! If you use Def it will return Def, which is this getter. This will cause a never ending cycle and a stack overflow.
                return this.def as ThingDef_TaggingBullet;
            }
        }
        #endregion

        #region Overrides
        protected override void Impact(Thing hitThing)
        {

            base.Impact(hitThing);
            Pawn hitPawn;
            PawnmorphGameComp pgc = Find.World.GetComponent<PawnmorphGameComp>();
            if (hitThing != null && hitThing is Pawn)
            {

                hitPawn = hitThing as Pawn;
                if (hitPawn.RaceProps.intelligence != Intelligence.Humanlike && hitPawn.RaceProps.intelligence != Intelligence.ToolUser && !hitPawn.KindLabel.ToLower().StartsWith("chao"))
                {
                    if (pgc.taggedAnimals.Contains(hitPawn.kindDef))
                    {
                        Messages.Message("{0} already in genetic database".Formatted(hitPawn.kindDef.LabelCap), MessageTypeDefOf.RejectInput);
                        return;
                    }
                    pgc.tagPawn(hitPawn.kindDef);
                    Messages.Message("{0} added to database".Formatted(hitPawn.kindDef.LabelCap), MessageTypeDefOf.TaskCompletion);
                }
                if (hitPawn.KindLabel.ToLower().StartsWith("chao"))
                {
                    Messages.Message("{0} is too genetically corrupted to be added".Formatted(hitPawn.kindDef.LabelCap), MessageTypeDefOf.RejectInput);
                }
            }
        }
        #endregion Overrides
    }

}
