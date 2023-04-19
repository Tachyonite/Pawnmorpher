// Driver_RecruitSapientFormerHuman.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:45 PM
// last updated 03/15/2020  3:45 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// job driver for recruiting a sapient former human
	/// </summary>
	/// <seealso cref="Pawnmorph.Jobs.Driver_InteractFormerHuman" />
	public class Driver_RecruitSapientFormerHuman : Driver_InteractFormerHuman
	{
		/// <summary>
		/// Gets a value indicating whether this instance can interact now.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance can interact now; otherwise, <c>false</c>.
		/// </value>
		protected override bool CanInteractNow => !TameUtility.TriedToTameTooRecently(Animal);

		/// <summary>
		/// Gets the Final indirect toil
		/// </summary>
		/// <returns></returns>
		protected override Toil FinalInteractToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.actor;
				Pawn pawn = (Pawn)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing;
				if (pawn.Spawned && pawn.Awake())
				{
					actor.interactions.TryInteractWith(pawn, InteractionDefOf.RecruitAttempt);
				}
			};
			toil.socialMode = RandomSocialMode.Off;
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 350;
			toil.activeSkill = () => SkillDefOf.Social;
			return toil;
		}

		/// <summary>
		/// Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil item in base.MakeNewToils().MakeSafe()) yield return item;
			this.FailOn(() => Map.designationManager.DesignationOn(Animal, PMDesignationDefOf.RecruitSapientFormerHuman) == null && !OnLastToil);
		}

		/// <summary>
		/// gets the Feeds toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> FeedToils()
		{
			return Enumerable.Empty<Toil>();
		}


		/// <summary>
		///     gets the 'talk to animal' toil
		/// </summary>
		/// <param name="tameeInd">The tamee ind.</param>
		/// <returns></returns>
		protected override Toil TalkToAnimal(TargetIndex tameeInd)
		{
			var toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.GetActor();
				var recipient = (Pawn)(Thing)actor.CurJob.GetTarget(tameeInd);
				actor.interactions.TryInteractWith(recipient, InteractionDefOf.BuildRapport);
			};
			toil.FailOn(() => !CanInteractNow);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 270;
			return toil;
		}
	}
}