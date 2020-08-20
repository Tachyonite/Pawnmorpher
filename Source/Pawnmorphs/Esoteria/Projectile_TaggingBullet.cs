using Pawnmorph;
using Pawnmorph.Chambers;
using RimWorld;
using Verse;

namespace EtherGun
{
    /// <summary>
    /// bullet for the tagging rifle, adds creatures to the chamber database on hit 
    /// </summary>
    /// <seealso cref="RimWorld.Bullet" />
    public class Projectile_TaggingBullet : Bullet
    {
        ThingDef_TaggingBullet Def => def as ThingDef_TaggingBullet;
        /// <summary>
        /// called when this instance impacts the given thing 
        /// </summary>
        /// <param name="hitThing">The hit thing.</param>
        protected override void Impact(Thing hitThing)
        {
            base.Impact(hitThing);
            var pgc = Find.World.GetComponent<PawnmorphGameComp>();
            var database = Find.World.GetComponent<ChamberDatabase>();
            if (hitThing != null && hitThing is Pawn pawn)
            {
                Pawn hitPawn = pawn;

                if (!database.TryAddToDatabase(pawn.kindDef, out string reason))
                {
                    Messages.Message(reason, MessageTypeDefOf.RejectInput);
                }
                else
                {
                    Messages.Message("AnimalAddedToDatabase".Formatted(hitPawn.kindDef.LabelCap),
                                     MessageTypeDefOf.TaskCompletion); 
                }

            }
        }
    }
}
