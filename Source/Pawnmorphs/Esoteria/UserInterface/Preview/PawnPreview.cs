using System;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.UserInterface.Preview
{
	internal class PawnPreview : ThingPreview
	{
		Pawn _pawn;

		public PawnKindDef PawnKindDef
		{
			get => _pawn?.kindDef;
			set
			{
				SetThing(value);
			}
		}


		public PawnPreview(int height, int width, PawnKindDef thing = null)
			: base(height, width)
		{
			SetThing(thing);
		}

		private void SetThing(PawnKindDef thing)
		{
			if (thing == null)
				return;


			Pawn obj = (Pawn)Activator.CreateInstance(thing.race.thingClass);
			obj.def = thing.race;
			obj.kindDef = thing;

			if (thing.fixedGender.HasValue)
				obj.gender = thing.fixedGender.Value;
			else
				obj.gender = Gender.Male;

			obj.health = new Pawn_HealthTracker(obj);
			obj.ageTracker = new Pawn_AgeTracker(obj);
			obj.ageTracker.AgeBiologicalTicks = (long)thing.RaceProps.lifeExpectancy * TimeMetrics.TICKS_PER_YEAR / 2;
			_scale = 1f / Math.Max(1, obj.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x / 2f);

			PawnComponentsUtility.CreateInitialComponents(obj);
			obj.InitializeComps();
			_pawn = obj;
			Thing = _pawn;
		}

		/// <summary>
		/// Sets the preview gender.
		/// </summary>
		/// <param name="gender">The gender.</param>
		public void SetGender(Gender gender)
		{
			_pawn.gender = gender;
		}

		protected override void OnRefresh()
		{
			if (_pawn == null)
				return;

			_pawn.Drawer.renderer.SetAllGraphicsDirty();
		}
	}
}
