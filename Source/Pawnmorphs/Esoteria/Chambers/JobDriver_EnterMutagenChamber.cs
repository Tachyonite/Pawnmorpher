// JobDriver_EnterMutagenChamber.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 08/25/2019  7:06 PM

using System;
using System.Collections.Generic;
using Pawnmorph.Chambers;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
    /// <summary>
    /// job driver for making a pawn enter a mutagenic chamber 
    /// </summary>
    /// <seealso cref="Verse.AI.JobDriver" />
    public class JobDriver_EnterMutagenChamber : JobDriver
    {
        /// <summary>Tries the make pre toil reservations.</summary>
        /// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
        /// <returns></returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Pawn pawn = base.pawn;
            LocalTargetInfo targetA = base.job.targetA;
            Job job = base.job;
            bool errorOnFailed2 = errorOnFailed;
            return pawn.Reserve(targetA, job, 1, -1, null, errorOnFailed2);
        }

        /// <summary>Makes the new toils.</summary>
        /// <returns></returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            Toil prepare = Toils_General.Wait(500);
            prepare.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            prepare.WithProgressBarToilDelay(TargetIndex.A);
            yield return prepare;
            Toil enter = new Toil();
            enter.initAction = delegate
            {
                Pawn actor = enter.actor;
                var pod = (MutaChamber)actor.CurJob.targetA.Thing;
                Action action = delegate
                {
                    

                    actor.DeSpawn();
                    pod.TryAcceptThing(actor);
                };
                if (!pod.def.building.isPlayerEjectable)
                {
                    int freeColonistsSpawnedOrInPlayerEjectablePodsCount = Map.mapPawns.FreeColonistsSpawnedOrInPlayerEjectablePodsCount;
                    if (freeColonistsSpawnedOrInPlayerEjectablePodsCount <= 1)
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("CasketWarning".Translate(actor.Named("PAWN")).AdjustedFor(actor), action));
                    }
                    else
                    {
                        action();
                    }
                }
                else
                {
                    action();
                }
            };
            enter.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return enter;
        }
    }
}