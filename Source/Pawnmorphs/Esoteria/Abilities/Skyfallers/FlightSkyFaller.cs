using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Abilities.Skyfallers
{
	internal class FlightSkyFaller : Skyfaller, IActiveDropPod
	{
		private LocalTargetInfo _target;

		ActiveDropPodInfo IActiveDropPod.Contents => new ActiveDropPodInfo();

		public event Action<FlightSkyFaller> OnLanded;
		public event Action<FlightSkyFaller> OnTakeOff;

		public FlightSkyFaller()
			: this(LocalTargetInfo.Invalid)
		{

		}

		public FlightSkyFaller(LocalTargetInfo target)
			: base()
		{
			def = SkyFallerDefOf.FlightLeaving;
			_target = target;
		}

		protected override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			Pawn pawn = innerContainer[0] as Pawn;

			if (def.skyfaller.xPositionCurve != null)
			{
				drawLoc.x += def.skyfaller.xPositionCurve.Evaluate(TimeInAnimation);
			}

			if (def.skyfaller.zPositionCurve != null)
			{
				drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(TimeInAnimation);
			}

			pawn.Rotation = drawLoc.x > DrawPos.x ? Rot4.West : Rot4.East;
			pawn.Drawer.renderer.RenderPawnAt(drawLoc, def.skyfaller.reversed ? pawn.Rotation.Opposite : pawn.Rotation);
			DrawDropSpotShadow();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_TargetInfo.Look(ref _target, nameof(_target));
		}


		protected override void LeaveMap()
		{
			OnTakeOff?.Invoke(this);
			Map map = Map;
			DeSpawn();

			def = SkyFallerDefOf.FlightIncoming;

			GenSpawn.Spawn(this, _target.Cell, map);
			this.ageTicks = 0;
		}

		protected override void Impact()
		{
			base.Impact();
			OnLanded?.Invoke(this);
		}
	}
}
