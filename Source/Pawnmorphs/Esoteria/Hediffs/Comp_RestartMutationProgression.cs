// Comp_RestartMutationProgression.cs created by Iron Wolf for Pawnmorph on 05/04/2020 9:02 PM
// last updated 05/04/2020  9:02 PM

using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff comp that restarts 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	public class Comp_RestartMutationProgression : HediffComp
	{
		/// <summary>
		/// Comps the post post add.
		/// </summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);

			var mutations = (Pawn?.health?.hediffSet?.hediffs).MakeSafe().OfType<Hediff_AddedMutation>();

			foreach (Hediff_AddedMutation mutation in mutations)
			{
				mutation.ResumeAdaption();
			}

		}
	}
}