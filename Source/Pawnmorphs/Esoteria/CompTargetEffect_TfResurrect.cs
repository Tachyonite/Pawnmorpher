// CompTargetEffect_TfResurrect.cs modified by Iron Wolf for Pawnmorph on 11/02/2019 11:18 AM
// last updated 11/02/2019  11:18 AM

using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph
{
	/// <summary>
	///     resurrector effect that will turn the pawn into an animal
	/// </summary>
	/// <seealso cref="RimWorld.CompTargetEffect" />
	public class CompTargetEffect_TfResurrect : CompTargetEffect
	{
		/// <summary>
		///     Does the effect on.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="target">The target.</param>
		public override void DoEffectOn(Pawn user, Thing target)
		{
			if (!user.IsColonistPlayerControlled
			 || !user.CanReserveAndReach(target, PathEndMode.Touch, Danger.Deadly))
				return;
			var job = new Job(PMJobDefOf.PMResurrect, target, parent)
			{
				count = 1
			};
			user.jobs.TryTakeOrderedJob(job);
		}
	}
}