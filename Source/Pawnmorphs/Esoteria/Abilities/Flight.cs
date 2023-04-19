using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.Abilities
{
	internal class Flight : MutationAbility
	{
		private LocalTargetInfo _target;
		Skyfallers.FlightSkyFaller _skyfaller;
		private bool _isDrafted;
		private bool _selected;

		protected override MutationAbilityType Type => MutationAbilityType.Target;
		protected override TargetingParameters TargetParameters => new TargetingParameters()
		{
			canTargetLocations = true,
			validator = ((TargetInfo x) => DropCellFinder.IsGoodDropSpot(x.Cell, x.Map, allowFogged: true, canRoofPunch: false))
		};

		public Flight() : base()
		{
			_target = LocalTargetInfo.Invalid;
		}

		public Flight(MutationAbilityDef def) : base(def)
		{
			_target = LocalTargetInfo.Invalid;
		}

		protected override bool OnTryCast(LocalTargetInfo? target)
		{
			if (target == null || Pawn.Spawned == false)
				return false;

			if (Pawn.Position.Roofed(Pawn.Map))
			{
				MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "FailFly_Roofed".Translate());
				return false;
			}

			if (Pawn.Downed)
			{
				MoteMaker.ThrowText(Pawn.DrawPos, Pawn.Map, "Fail_Downed".Translate());
				return false;
			}

			_target = target.Value;
			return true;
		}

		protected override void OnInitialize()
		{
			if (_skyfaller != null)
			{
				_skyfaller.OnLanded += OnLanded;
			}
		}


		protected override void OnTick()
		{
			if (state == MutationAbilityState.Casting)
			{
				if (_target.IsValid == false)
				{
					state = MutationAbilityState.None;
					return;
				}

				_skyfaller = new Skyfallers.FlightSkyFaller(_target);
				_skyfaller.OnLanded += OnLanded;

				Map map = Pawn.Map;
				IntVec3 position = Pawn.Position;

				_isDrafted = Pawn.Drafted;
				_selected = Find.Selector.IsSelected(Pawn);

				Pawn.DeSpawn();
				_skyfaller.innerContainer.TryAddOrTransfer(Pawn);
				_skyfaller.def.graphicData = null;

				GenSpawn.Spawn(_skyfaller, position, map);
				state = MutationAbilityState.Active;
			}
		}

		private void OnLanded(Skyfallers.FlightSkyFaller skyfaller)
		{
			_skyfaller = null;

			if (_isDrafted)
			{
				Pawn.drafter.Drafted = true;

				if (_selected)
				{
					Selector selector = Find.Selector;
					if (selector.SelectedObjects.Count == 0)
					{
						selector.Select(Pawn);
					}
				}
			}

			StartCooldown();
		}

		protected override string OnIsDisabled()
		{
			float? lift = StatsUtility.GetStat(Pawn, PMStatDefOf.PM_Lift, 60);
			if (lift.HasValue && lift < 1.0f)
				return "FailFly_Lift".Translate();

			if (Pawn.Downed)
				return "Fail_Downed".Translate();

			return null;
		}

		protected override void OnExposeData()
		{
			Scribe_TargetInfo.Look(ref _target, nameof(_target));
			Scribe_References.Look(ref _skyfaller, nameof(_skyfaller));
			Scribe_Values.Look(ref _isDrafted, nameof(_isDrafted));
		}
	}
}
