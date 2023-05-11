using System;
using System.Linq;
using JetBrains.Annotations;
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
		public AnimalClassBase animalClass;



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

			if (Rand.MTBEventOccurs(mtbDays, mtbUnits, 60) && pawn.RaceProps.intelligence == Intelligence.Humanlike)
			{
				MutagenDef mutagen = cause?.def?.GetMutagenDef() ?? MutagenDefOf.defaultMutagen;
				TryApply(pawn, cause, mutagen);
			}
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