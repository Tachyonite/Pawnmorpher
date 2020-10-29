using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
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

                var fHStatus = hitPawn.GetQuantizedSapienceLevel();
                if (fHStatus != null && fHStatus != SapienceLevel.PermanentlyFeral)
                {
                    Messages.Message("CannotTagFormerHuman".Translate(), MessageTypeDefOf.RejectInput);
                    return;
                }

                TryTagAnimal(database, pawn);

            }
        }

        private const int MIN_MUTATIONS_TAGGED = 1;
        private const int MAX_MUTATIONS_TAGGED = 5;
        private static void TryTagAnimal([NotNull] ChamberDatabase database, [NotNull] Pawn pawn)
        {
          
            if (database.TaggedAnimals.Contains(pawn.kindDef))
            {
                if (pawn.kindDef.GetAllMutationsFrom().Taggable().Any(m =>!m.IsTagged()))
                {
                    TryTagMutations(database, pawn);
                    return;
                }
                else
                {
                    Messages.Message("PMNothingTaggable".Translate(pawn.kindDef.Named("animal")), MessageTypeDefOf.NeutralEvent);
                    return;
                }
            }


            if (!database.TryAddToDatabase(pawn.kindDef, out string reason))
            {
                Messages.Message(reason, MessageTypeDefOf.RejectInput);
            }
            else
            {
                Messages.Message("PMAnimalAddedToDatabase".Translate(pawn.kindDef.Named("animal")),
                                 MessageTypeDefOf.TaskCompletion);
            }
        }

        [NotNull]
        private readonly static LinkedList<MutationDef> _scratchList = new LinkedList<MutationDef>(); 
        
        private static void TryTagMutations(ChamberDatabase database, Pawn pawn)
        {
            var mutations = pawn.kindDef.GetAllMutationsFrom().Taggable().Where(m => !m.IsTagged());
            var max = Rand.Range(MIN_MUTATIONS_TAGGED, MAX_MUTATIONS_TAGGED);

            if (database.FreeStorage > 0 && !database.CanTag)
            {
                Messages.Message(ChamberDatabase.NOT_ENOUGH_POWER.Translate(), MessageTypeDefOf.RejectInput);
                return;
            }

            _scratchList.Clear();
            foreach (MutationDef mutationDef in mutations)
            {
                var n = _scratchList.First;
                while (n != null) //sort the mutations in order of least required space to most 
                {
                    if (mutationDef.GetRequiredStorage() < n.Value.GetRequiredStorage())
                    {
                        break;
                    }

                    n = n.Next; 
                }

                if (n == null)
                {
                    _scratchList.AddLast(mutationDef); 
                }
                else
                {
                    _scratchList.AddBefore(n, mutationDef); 
                }
            }

            int c = 0;
            var node = _scratchList.First; 
            while (c < max && node != null)
            {
                var mut = node.Value; 
                if(database.FreeStorage < mut.GetRequiredStorage()) break;
                database.AddToDatabase(mut);
                node = node.Next;
                c++; 
            }

            if (c == 0)
            {
                Messages.Message("PMMutationNotEnoughSpace".Translate(pawn.kindDef.Named("animal")),MessageTypeDefOf.NeutralEvent);
            }
            else
            {
                var msgTxt = "PMMutationTagged".Translate(pawn.kindDef.Named("animal"), c.Named("num"));
                Messages.Message(msgTxt, MessageTypeDefOf.PositiveEvent); 
            }

        }
    }
}
