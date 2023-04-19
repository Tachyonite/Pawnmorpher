using Verse;

namespace Pawnmorph.Abilities
{
	internal class Bullrush : MutationAbility
	{
		protected override MutationAbilityType Type => MutationAbilityType.Action;

		public Bullrush()
		{

		}

		public Bullrush(MutationAbilityDef def) : base(def)
		{
		}


		protected override void OnExposeData()
		{

		}

		protected override void OnInitialize()
		{

		}

		protected override string OnIsDisabled()
		{
			return null;
		}

		protected override void OnTick()
		{
			if (state == MutationAbilityState.Casting)
			{
				Pawn.health.AddHediff(TfHediffDefOf.Bullrush);
				StartCooldown();
			}
		}

		protected override bool OnTryCast(LocalTargetInfo? target)
		{
			return true;
		}
	}
}
