using Verse;

namespace Pawnmorph.Hediffs
{
	internal class Hediff_Bullrush : HediffWithComps
	{
		private float _originalCooldown;

		public Hediff_Bullrush()
		{
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			Verb hornsVerb = pawn.health.hediffSet.GetHediffsVerbs().FirstOrDefault(x => x.tool.label == "gored");

			if (hornsVerb != null)
			{
				RimWorld.StatModifier offset = CurStage.statOffsets.FirstOrDefault(x => x.stat == RimWorld.StatDefOf.MeleeWeapon_CooldownMultiplier);
				if (offset != null)
				{
					_originalCooldown = hornsVerb.tool.cooldownTime;
					hornsVerb.tool.cooldownTime *= (1 + offset.value);
				}
			}


			base.PostAdd(dinfo);
		}

		public override void PostRemoved()
		{
			Verb hornsVerb = pawn.health.hediffSet.GetHediffsVerbs().FirstOrDefault(x => x.tool.label == "gored");
			if (hornsVerb != null)
			{
				RimWorld.StatModifier offset = CurStage.statOffsets.FirstOrDefault(x => x.stat == RimWorld.StatDefOf.MeleeWeapon_CooldownMultiplier);
				if (offset != null && _originalCooldown * (1 + offset.value) == hornsVerb.tool.cooldownTime)
				{
					hornsVerb.tool.cooldownTime = _originalCooldown;
				}
			}

			base.PostRemoved();
		}

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref _originalCooldown, nameof(_originalCooldown));
		}
	}
}
