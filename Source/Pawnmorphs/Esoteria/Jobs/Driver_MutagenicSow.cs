// Driver_MutagenicSow.cs created by Iron Wolf for Pawnmorph on 03/10/2022 11:07 AM
// last updated 03/10/2022  11:07 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	///     job driver for sowing mutagenic plants
	/// </summary>
	/// <seealso cref="Verse.AI.JobDriver" />
	public class Driver_MutagenicSow : JobDriver
	{
		private float sowWorkDone;

		private Plant Plant => (Plant)job.GetTarget(TargetIndex.A).Thing;

		/// <summary>
		///     Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref sowWorkDone, "sowWorkDone");
		}

		/// <summary>
		///     Tries the make pre toil reservations.
		/// </summary>
		/// <param name="errorOnFailed">if set to <c>true</c> [error on failed].</param>
		/// <returns></returns>
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		/// <summary>
		///     Makes the new toils.
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<Toil> MakeNewToils()
		{


			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch)
								   .FailOn(() => PlantUtility.AdjacentSowBlocker(job.plantDefToSow, TargetA.Cell, Map) != null)
								   .FailOn(() => !job.plantDefToSow.CanNowPlantAt(TargetLocA, Map));
			var sowToil = new Toil();
			sowToil.initAction = delegate
			{
				TargetThingA = GenSpawn.Spawn(job.plantDefToSow, TargetLocA, Map);
				pawn.Reserve(TargetThingA, sowToil.actor.CurJob);
				var obj = (Plant)TargetThingA;
				obj.Growth = 0f;
				obj.sown = true;
			};
			sowToil.tickAction = delegate
			{
				Pawn actor = sowToil.actor;
				if (actor != null)
					WorkPlant(actor);
			};
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => sowWorkDone / Plant.def.plant.sowWork, true);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (TargetThingA != null)
				{
					var plant = (Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (sowWorkDone < plant.def.plant.sowWork && !TargetThingA.Destroyed) TargetThingA.Destroy();
				}
			});
			sowToil.activeSkill = () => SkillDefOf.Plants;
			yield return sowToil;
		}

		private void WorkPlant([NotNull] Pawn actor)
		{
			if (actor.skills != null) actor.skills.Learn(SkillDefOf.Plants, 0.085f);
			float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed);
			Plant plant2 = Plant;
			if (plant2.LifeStage != 0) Log.Error(string.Concat(this, " getting sowing work while not in Sowing life stage."));
			sowWorkDone += statValue;
			if (sowWorkDone >= plant2.def.plant.sowWork)
			{
				SowPlant(actor, plant2);
			}
		}

		private void SowPlant(Pawn actor, Plant plantSowed)
		{
			plantSowed.Growth = 0.0001f;
			Map.mapDrawer.MapMeshDirty(plantSowed.Position, MapMeshFlagDefOf.Things);
			actor.records.Increment(RecordDefOf.PlantsSown);
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedPlant,
																   actor.Named(HistoryEventArgsNames.Doer)));
			if (plantSowed.def.plant.humanFoodPlant)
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedHumanFoodPlant,
																	   actor.Named(HistoryEventArgsNames.Doer)));

			if (plantSowed.IsMutantPlant())
			{
				var hEV = new HistoryEvent(PMHistoryEventDefOf.SowMutagenicPlants, actor.Named(HistoryEventArgsNames.Doer),
										   plantSowed.def.Named(HistoryEventArgsNames.Subject));
				Find.HistoryEventsManager.RecordEvent(hEV);
			}

			ReadyForNextToil();
		}
	}
}