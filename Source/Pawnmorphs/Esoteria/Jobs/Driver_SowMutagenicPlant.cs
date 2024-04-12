// Driver_SowMutagenicPlant.cs created by Iron Wolf for Pawnmorph on 03/09/2022 6:49 AM
// last updated 03/09/2022  6:49 AM

using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	///     job driver for sowing mutagenic plants
	/// </summary>
	/// <seealso cref="RimWorld.JobDriver_PlantSow" />
	public class Driver_SowMutagenicPlant : JobDriver
	{
		private float _sowWorkDone;
		private Plant Plant => (Plant)job.GetTarget(TargetIndex.A).Thing;

		/// <summary>
		///     Exposes the data for saving/loading.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _sowWorkDone, "sowWorkDone");
		}

		/// <summary>
		///     Tries to make the pre toil reservations.
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
			sowToil.tickAction = delegate { IncrementSowWork(sowToil.actor); };
			sowToil.defaultCompleteMode = ToilCompleteMode.Never;
			sowToil.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			sowToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			sowToil.WithEffect(EffecterDefOf.Sow, TargetIndex.A);
			sowToil.WithProgressBar(TargetIndex.A, () => _sowWorkDone / Plant.def.plant.sowWork, true);
			sowToil.PlaySustainerOrSound(() => SoundDefOf.Interact_Sow);
			sowToil.AddFinishAction(delegate
			{
				if (TargetThingA != null)
				{
					var plant = (Plant)sowToil.actor.CurJob.GetTarget(TargetIndex.A).Thing;
					if (_sowWorkDone < plant.def.plant.sowWork && !TargetThingA.Destroyed) TargetThingA.Destroy();
				}
			});
			sowToil.activeSkill = () => SkillDefOf.Plants;
			yield return sowToil;
		}

		private void IncrementSowWork(Pawn actor)
		{
			if (actor.skills != null) actor.skills.Learn(SkillDefOf.Plants, 0.085f);
			float statValue = actor.GetStatValue(StatDefOf.PlantWorkSpeed);
			Plant plant2 = Plant;
			if (plant2.LifeStage != 0) Log.Error(string.Concat(this, " getting sowing work while not in Sowing life stage."));
			_sowWorkDone += statValue;
			if (_sowWorkDone >= plant2.def.plant.sowWork) SowPlant(plant2, actor);
		}

		private void SowPlant(Plant lPlant, Pawn actor)
		{
			lPlant.Growth = 0.0001f;
			Map.mapDrawer.MapMeshDirty(lPlant.Position, MapMeshFlagDefOf.Things);
			actor.records.Increment(RecordDefOf.PlantsSown);
			Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedPlant,
																   actor.Named(HistoryEventArgsNames.Doer)));
			if (lPlant.def.plant.humanFoodPlant)
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(HistoryEventDefOf.SowedHumanFoodPlant,
																	   actor.Named(HistoryEventArgsNames.Doer)));

			if (lPlant.def.IsMutantPlant())
			{
				var newEv = new HistoryEvent(PMHistoryEventDefOf.SowMutagenicPlants, actor.Named(HistoryEventArgsNames.Doer),
											 lPlant.def.Named(HistoryEventArgsNames.Subject));
				Find.HistoryEventsManager.RecordEvent(newEv);
			}

			ReadyForNextToil();
		}
	}
}