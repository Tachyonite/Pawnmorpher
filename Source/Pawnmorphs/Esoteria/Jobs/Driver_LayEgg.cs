namespace Pawnmorph.Jobs
{
	/// <summary> Job driver to make humanoid pawns lay eggs using HediffComp_Production. </summary>
	public class Driver_LayEgg : Driver_ProduceThing
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