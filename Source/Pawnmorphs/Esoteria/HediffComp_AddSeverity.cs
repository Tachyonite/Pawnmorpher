using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// hediff comp for adding severity to the parent hediff 
	/// </summary>
	/// <seealso cref="Verse.HediffComp" />
	public class HediffComp_AddSeverity : HediffComp
	{
		/// <summary>Gets the properties.</summary>
		/// <value>The properties.</value>
		public HediffCompProperties_AddSeverity Props
		{
			get
			{
				return (HediffCompProperties_AddSeverity)props;
			}
		}
		/// <summary>called after the parent is updated</summary>
		/// <param name="severityAdjustment">The severity adjustment.</param>
		public override void CompPostTick(ref float severityAdjustment)
		{
			AddSeverity();
		}
		/// <summary>Adds the severity.</summary>
		public void AddSeverity()
		{
			if (Rand.MTBEventOccurs(Props.mtbDays, 60000f, 60f) && !triggered && Pawn.health.hediffSet.HasHediff(Props.hediff))
			{
				HealthUtility.AdjustSeverity(Pawn, Props.hediff, Props.severity);
				triggered = true;
			}
		}

		private bool triggered = false;
	}
}
