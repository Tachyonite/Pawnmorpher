using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// hediff comp for adding a hediff when over specific terrain 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	public class HediffComp_TerrainBasedMorph : HediffComp
	{
		/// <summary>Gets the properties.</summary>
		/// <value>The properties.</value>
		public HediffCompProperties_TerrainBasedMorph Props
		{
			get
			{
				return (HediffCompProperties_TerrainBasedMorph)this.props;
			}
		}

		/// <summary>called after the parent is updated</summary>
		/// <param name="severityAdjustment">The severity adjustment.</param>
		public override void CompPostTick(ref float severityAdjustment)
		{
			if (parent.pawn.Position.GetTerrain(parent.pawn.Map) == Props.terrain)
			{
				Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, parent.pawn, null);
				hediff.Severity = 1f;
				parent.pawn.health.AddHediff(hediff, null, null, null);
			}
		}
	}
}
