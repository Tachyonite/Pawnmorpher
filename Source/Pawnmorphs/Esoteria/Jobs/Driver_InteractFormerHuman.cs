// Driver_InteractFormerHuman.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:48 PM
// last updated 03/15/2020  3:48 PM

using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	///     abstract base class for all interactions with former humans
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public abstract class Driver_InteractFormerHuman : JobDriver
	{
		/// <summary>
		/// The animal index
		/// </summary>
		protected const TargetIndex AnimalInd = TargetIndex.A;

		private const TargetIndex FoodHandInd = TargetIndex.B;

		private const int FeedDuration = 270;

		private const int TalkDuration = 270;

		private const float NutritionPercentagePerFeed = 0.15f;

		private const float MaxMinNutritionPerFeed = 0.3f;

		/// <summary>
		/// The feed count
		/// </summary>
		public const int FeedCount = 2;

		/// <summary>
		/// The maximum food preferability
		/// </summary>
		public const FoodPreferability MaxFoodPreferability = FoodPreferability.RawTasty;

		private float feedNutritionLeft;

		/// <summary>
		/// Gets the animal.
		/// </summary>
		/// <value>
		/// The animal.
		/// </value>
		protected Pawn Animal => (Pawn)job.targetA.Thing;

		/// <summary>
		/// Gets a value indicating whether this instance can interact now.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance can interact now; otherwise, <c>false</c>.
		/// </value>
		protected virtual bool CanInteractNow => true;

		/// <summary>
		/// Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref feedNutritionLeft, "feedNutritionLeft");
		}

		/// <summary>
		///     gets the amount of nutrition needed per feed.
		/// </summary>
		/// <param name="animal">The animal.</param>
		/// <returns></returns>
		public static float RequiredNutritionPerFeed(Pawn animal)
		{
			return Mathf.Min(animal.needs.food.MaxLevel * NutritionPercentagePerFeed, MaxMinNutritionPerFeed);
		}

		/// <summary>
		/// Tries the make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(Animal, job, 1, -1, null, errorOnFailed);
		}

		/// <summary>
		/// Finals the interact toil.
		/// </summary>
		/// <returns></returns>
		protected abstract Toil FinalInteractToil();

		/// <summary>
		/// Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOnDowned(TargetIndex.A);
			this.FailOnNotCasualInterruptible(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return TalkToAnimal(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return TalkToAnimal(TargetIndex.A);
			foreach (Toil item in FeedToils()) yield return item;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return TalkToAnimal(TargetIndex.A);
			foreach (Toil item2 in FeedToils()) yield return item2;
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOn(() => !CanInteractNow);
			yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);
			yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
			yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
			yield return FinalInteractToil();
		}

		/// <summary>
		///     gets the start feed animal toil
		/// </summary>
		/// <param name="tameeInd">The tamee ind.</param>
		/// <returns></returns>
		protected virtual Toil StartFeedAnimal(TargetIndex tameeInd)
		{
			var toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.GetActor();
				var pawn = (Pawn)(Thing)actor.CurJob.GetTarget(tameeInd);
				PawnUtility.ForceWait(pawn, 270, actor);
				Thing thing =
					FoodUtility.BestFoodInInventory(actor, pawn, FoodPreferability.NeverForNutrition, FoodPreferability.RawTasty);
				if (thing == null)
				{
					actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					actor.mindState.lastInventoryRawFoodUseTick = Find.TickManager.TicksGame;
					int num = FoodUtility.StackCountForNutrition(feedNutritionLeft, thing.GetStatValue(StatDefOf.Nutrition));
					int stackCount = thing.stackCount;
					Thing thing2 = actor.inventory.innerContainer.Take(thing, Mathf.Min(num, stackCount));
					actor.carryTracker.TryStartCarry(thing2);
					actor.CurJob.SetTarget(TargetIndex.B, thing2);
					float num2 = thing2.stackCount * thing2.GetStatValue(StatDefOf.Nutrition);
					ticksLeftThisToil = Mathf.CeilToInt(FeedDuration * (num2 / RequiredNutritionPerFeed(pawn)));
					if (num <= stackCount)
					{
						feedNutritionLeft = 0f;
					}
					else
					{
						feedNutritionLeft -= num2;
						if (feedNutritionLeft < 0.001f) feedNutritionLeft = 0f;
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			return toil;
		}

		/// <summary>
		///     gets the 'talk to animal' toil
		/// </summary>
		/// <param name="tameeInd">The tamee ind.</param>
		/// <returns></returns>
		protected virtual Toil TalkToAnimal(TargetIndex tameeInd)
		{
			var toil = new Toil();
			toil.initAction = delegate
			{
				Pawn actor = toil.GetActor();
				var recipient = (Pawn)(Thing)actor.CurJob.GetTarget(tameeInd);
				actor.interactions.TryInteractWith(recipient, PMInteractionDefOf.FormerHumanAnimalChat);
			};
			toil.FailOn(() => !CanInteractNow);
			toil.defaultCompleteMode = ToilCompleteMode.Delay;
			toil.defaultDuration = 270;
			return toil;
		}

		/// <summary>
		/// gets the Feeds toils.
		/// </summary>
		/// <returns></returns>
		protected virtual IEnumerable<Toil> FeedToils()
		{
			var toil = new Toil();
			toil.initAction = delegate { feedNutritionLeft = RequiredNutritionPerFeed(Animal); };
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return toil;
			Toil gotoAnimal = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return gotoAnimal;
			yield return StartFeedAnimal(TargetIndex.A);
			yield return Toils_Ingest.FinalizeIngest(Animal, TargetIndex.B);
			yield return Toils_General.PutCarriedThingInInventory();
			yield return Toils_General.ClearTarget(TargetIndex.B);
			yield return Toils_Jump.JumpIf(gotoAnimal, () => feedNutritionLeft > 0f);
		}
	}
}