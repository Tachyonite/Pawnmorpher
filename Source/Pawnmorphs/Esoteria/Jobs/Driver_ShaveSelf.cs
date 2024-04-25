namespace Pawnmorph.Jobs
{
	class Driver_ShaveSelf : Driver_ProduceThing
	{
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
