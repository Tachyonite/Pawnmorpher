// Driver_UseGenome.cs created by Iron Wolf for Pawnmorph on 08/07/2020 3:06 PM
// last updated 08/07/2020  3:06 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Chambers;
using Pawnmorph.ThingComps;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Pawnmorph.Jobs
{
    /// <summary>
    /// job driver for using a genome 
    /// </summary>
    /// <seealso cref="Verse.AI.JobDriver" />
    public class Driver_UseGenome : JobDriver
    {
        const TargetIndex MUTAGEN_BENCH_INDEX = TargetIndex.A;
        const TargetIndex GENOME_INDEX = TargetIndex.B;
        private const TargetIndex HAUL_INDEX = TargetIndex.C;

        private const int DURATION = 100;
        private Building MutagenBench => (Building) job.GetTarget(MUTAGEN_BENCH_INDEX).Thing;

        private ThingWithComps Genome => (ThingWithComps) job.GetTarget(GENOME_INDEX).Thing;




        /// <summary>
        /// Tries the make pre toil reservations.
        /// </summary>
        /// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
        /// <returns></returns>
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(MutagenBench, job, errorOnFailed:errorOnFailed))
            {
                return pawn.Reserve(Genome, job, errorOnFailed: errorOnFailed);
            }

            return false; 
        }

        /// <summary>
        /// Makes the new toils.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            yield return Toils_General.DoAtomic(delegate { job.count = 1; });
            yield return Toils_Reserve.Reserve(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
                                   .FailOnDespawnedNullOrForbidden(TargetIndex.B)
                                   .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
            yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true).FailOnDestroyedNullOrForbidden(TargetIndex.B);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, null, false);
            yield return Toils_General.Wait(600)
                                      .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                                      .FailOnDestroyedNullOrForbidden(TargetIndex.A)
                                      .FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell)
                                      .WithProgressBarToilDelay(TargetIndex.A);
            var toil = new Toil
            {
                initAction = ApplyGenome,
                defaultCompleteMode = ToilCompleteMode.Instant
            };
            yield return toil;
        }

        private void ApplyGenome()
        {
            var wComp = Find.World.GetComponent<ChamberDatabase>();
            var mut = Genome.GetComp<MutationGenomeStorage>()?.Mutation;
            //just add to database, if not possible there is no recovery possible at this stage 
            //can add to database must be called by giver before hand

            //TODO message 

            if (mut == null)
            {
                var aGenome = Genome.GetComp<AnimalGenomeStorageComp>();
                if (aGenome == null) return; 

                wComp.AddToDatabase(aGenome.Animal);
                return;
            }
            
            var mutationToAdd = mut.AllMutations.Where(m => wComp.CanAddToDatabase(m)).RandomElementWithFallback();
            if (mutationToAdd == null)
            {
                Log.Error($"tried to use a genome with no addable mutations!");
                return;
            }

            wComp.AddToDatabase(mutationToAdd);
            Genome.Destroy();
        }
    }
}