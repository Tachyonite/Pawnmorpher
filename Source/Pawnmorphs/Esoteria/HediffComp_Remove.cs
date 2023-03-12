using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// hediff comp that removes other hediffs 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	public class HediffComp_Remove : HediffComp
	{
		/// <summary>Gets the properties.</summary>
		/// <value>The properties.</value>
		public HediffCompProperties_Remove Props
		{
			get
			{
				return (HediffCompProperties_Remove)this.props;
			}
		}

		/// <summary>called every tick after it's parent is updated .</summary>
		/// <param name="severityAdjustment">The severity adjustment.</param>
		public override void CompPostTick(ref float severityAdjustment)
		{
			var hS = new List<Hediff>(Pawn.health.hediffSet.hediffs);

			foreach (Hediff hD in hS)
				if (Props.makeImmuneTo.Contains(hD.def))
					Pawn.health.RemoveHediff(hD);
		}
	}
}
