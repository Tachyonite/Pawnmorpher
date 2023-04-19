namespace Pawnmorph.Jobs
{
	/// <summary> Job driver to make humanoid pawns milk themselves using HediffComp_Production. </summary>
	public class Driver_MilkSelf : Driver_ProduceThing
	{
		/// <summary>
		/// Produce whatever resources this driver is producing.
		/// </summary>
		public override void Produce()
		{
			if (job.jobGiver is Giver_Producer giver)
			{
				HediffComp_Production comp = giver.ProductionComp;
				comp.Produce();
			}
		}
	}
}
