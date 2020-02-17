// Giver_MutationClass.cs modified by Iron Wolf for Pawnmorph on 01/12/2020 1:47 PM
// last updated 01/12/2020  1:47 PM

using System;
using System.Linq;
using JetBrains.Annotations;
//using Multiplayer.API;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs
{

    /// <summary>
    /// hediff giver for giving mutation in a class 
    /// </summary>
    /// <seealso cref="Verse.HediffGiver" />
    public class Giver_MutationClass : HediffGiver
    {
        /// <summary>
        /// The animal classification to pull mutations from 
        /// </summary>
        [NotNull, UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AnimalClassDef animalClass;



        /// <summary>
        ///     The MTB days
        /// </summary>
        public float mtbDays = 0.4f;

        /// <summary>
        ///     The MTB unit
        /// </summary>
        public float mtbUnits = 60000f;

        /// <summary>
        ///     occurs every so often for all hediffs that have this giver
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="cause"></param>
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (!animalClass.GetAllMutationIn().Any()) return;

            //if (MP.IsInMultiplayer) Rand.PushState(RandUtilities.MPSafeSeed);

            if (Rand.MTBEventOccurs(mtbDays, mtbUnits, 60) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
            {
                MutagenDef mutagen = (cause as Hediff_Morph)?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
                TryApply(pawn, cause, mutagen);
            }

            //if (MP.IsInMultiplayer) Rand.PopState();
        }


        /// <summary>
        ///     Tries to apply this hediff giver
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="cause">The cause.</param>
        /// <param name="mutagen">The mutagen.</param>
        public void TryApply(Pawn pawn, Hediff cause, [NotNull] MutagenDef mutagen)
        {
            if (mutagen == null) throw new ArgumentNullException(nameof(mutagen));
            var mut = animalClass.GetAllMutationIn().RandomElement(); //grab a random mutation 
            if (MutationUtilities.AddMutation(pawn, mut))
            {
                IntermittentMagicSprayer.ThrowMagicPuffDown(pawn.Position.ToVector3(), pawn.MapHeld);
                if (cause.def.HasComp(typeof(HediffComp_Single))) pawn.health.RemoveHediff(cause);
                mutagen.TryApplyAspects(pawn);
                if (mut.mutationTale != null) TaleRecorder.RecordTale(mut.mutationTale, pawn);
            }
        }

    }
}