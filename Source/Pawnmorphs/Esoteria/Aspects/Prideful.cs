// SplitMind.cs created by Iron Wolf for Pawnmorph on 05/09/2020 1:18 PM
// last updated 05/09/2020  1:18 PM

using RimWorld;

namespace Pawnmorph.Aspects
{
	/// <summary>
	/// aspect for the giving merged pawns a 'split mind'
	/// </summary>
	/// <seealso cref="Pawnmorph.Aspect" />
	public class Prideful : Aspect
	{
		/// <inheritdoc />
		protected override void PostAdd()
		{
			TraitSet traitSet = Pawn.story?.traits;
			traitSet.GainTrait(new Trait(PMTraitDefOf.PM_PridefulTrait));

			base.PostAdd();
		}

		/// <inheritdoc />
		public override void PostRemove()
		{
			TraitSet traitSet = Pawn.story?.traits;
			traitSet.allTraits.RemoveAll(x => x.def == PMTraitDefOf.PM_PridefulTrait);

			base.PostRemove();
		}
	}
}