// AlwaysMergedPawn.cs created by Iron Wolf for Pawnmorph on 05/08/2020 10:40 AM
// last updated 05/08/2020  10:40 AM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.FormerHumans;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// thing comp to make the attached pawn always a 'merged' pawn
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class AlwaysMergedPawn : ThingComp
	{
		private bool _triggered = false;


		/// <summary>
		/// called every tick 
		/// </summary>
		public override void CompTick()
		{
			base.CompTick();

			if (_triggered) return;
			_triggered = true;
			try
			{
				var pawn = (Pawn)parent;
				var sTracker = pawn?.GetSapienceTracker();
				if (sTracker == null)
				{
					DebugLogUtils.Warning($"unable to get sapience tracker on pawn {pawn?.LabelShort ?? "NULL"}");
					return;
				}

				MakeMergedPawn(sTracker);

			}
			catch (InvalidCastException e)
			{
				Log.Error($"unable to case {parent.GetType().Name} to {nameof(Pawn)}!\n{e}");
			}
		}

		/// <summary>
		/// Posts the spawn setup.
		/// </summary>
		/// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
		public override void PostSpawnSetup(bool respawningAfterLoad)
		{

			base.PostSpawnSetup(respawningAfterLoad);
			if (respawningAfterLoad) return;



		}




		private void MakeMergedPawn([NotNull] SapienceTracker sTracker)
		{
			if (sTracker.CurrentState != null) return;

			(Pawn p1, Pawn p2) = FormerHumanPawnGenerator.GenerateRandomUnmergedHumans(sTracker.Pawn);

			Pawn[] tmpArr = { p1, p2 };

			var tfPawn = new MergedPawns()
			{
				meld = sTracker.Pawn,
				originals = new List<Pawn> { p1, p2 }
			};

			var gComp = Find.World.GetComponent<PawnmorphGameComp>();
			gComp.AddTransformedPawn(tfPawn);

			//TODO figure out how relationships work for merged pawns 

			sTracker.EnterState(SapienceStateDefOf.MergedPawn, Rand.Range(0.2f, 1));

			FormerHumanUtilities.TryAssignBackstoryToTransformedPawn(sTracker.Pawn, tmpArr[0]);
			MergedPawnUtilities.TransferToMergedPawn(tmpArr, sTracker.Pawn);

			var mentalDef = sTracker.Pawn.MentalStateDef;
			if (mentalDef == MentalStateDefOf.Manhunter || mentalDef == MentalStateDefOf.ManhunterPermanent)
			{
				return;
			}

			if (sTracker.Pawn.IsRelatedToColonistPawn() && sTracker.Pawn.Faction != Faction.OfPlayer)
			{
				sTracker.Pawn.SetFaction(Faction.OfPlayer);
			}

		}

		/// <summary>
		/// called to save all data in this comp 
		/// </summary>
		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref _triggered, "triggered");
		}

		void CleanupPawn([NotNull] Pawn pawn)
		{
			pawn.equipment?.DestroyAllEquipment();
			pawn.apparel?.DestroyAll();
		}
	}
}