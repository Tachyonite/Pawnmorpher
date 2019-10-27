using Pawnmorph;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;

namespace EtherGun
{
    public class Projectile_TaggingBullet : Bullet
    {
        public ThingDef_TaggingBullet Def => def as ThingDef_TaggingBullet;

        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            var pgc = Find.World.GetComponent<PawnmorphGameComp>();
            if (hitThing != null && hitThing is Pawn pawn)
            {
                Pawn hitPawn = pawn;

                if (hitPawn.def.IsValidAnimal())
                {
                    if (pgc.taggedAnimals.Contains(hitPawn.kindDef))
                    {
                        Messages.Message("{0} already in genetic database".Formatted(hitPawn.kindDef.LabelCap), MessageTypeDefOf.RejectInput);
                        return;
                    }

                    pgc.tagPawn(hitPawn.kindDef);
                    Messages.Message("{0} added to database".Formatted(hitPawn.kindDef.LabelCap), MessageTypeDefOf.TaskCompletion);
                }
                else if (DatabaseUtilities.IsChao(hitPawn.def))
                {
                    Messages.Message("{0} is too genetically corrupted to be added".Formatted(hitPawn.kindDef.LabelCap), MessageTypeDefOf.RejectInput);
                }
            }
        }
    }
}
