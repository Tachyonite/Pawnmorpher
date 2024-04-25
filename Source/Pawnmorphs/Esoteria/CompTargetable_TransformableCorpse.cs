// CompTargetable_TransformableCorpse.cs modified by Iron Wolf for Pawnmorph on 11/02/2019 11:49 AM
// last updated 11/02/2019  11:49 AM

using System.Collections.Generic;
using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph
{

	public class CompTargetable_TransformableCorpse : CompTargetable
	{
		protected override TargetingParameters GetTargetingParameters()
		{
			return new TargetingParameters
			{
				canTargetPawns = false,
				canTargetBuildings = false,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,

				validator = (Validator)
			};
		}

		private bool Validator(TargetInfo x)
		{
			if (!(x.Thing is Corpse c))
			{
				return false;
			}

			return base.ValidateTarget(x.Thing) && MutagenDefOf.defaultMutagen.CanTransform(c.InnerPawn);
		}

		public override IEnumerable<Thing> GetTargets(Thing targetChosenByPlayer = null)
		{
			yield return targetChosenByPlayer;
		}

		protected override bool PlayerChoosesTarget => true;
	}

	public class CompProperties_TransformableCorpse : CompProperties_Targetable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CompProperties_TransformableCorpse"/> class.
		/// </summary>
		public CompProperties_TransformableCorpse()
		{
			compClass = typeof(CompTargetable_TransformableCorpse);
		}
	}
}