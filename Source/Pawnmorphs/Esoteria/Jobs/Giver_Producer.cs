using Verse.AI;

namespace Pawnmorph.Jobs
{
	/// <summary>
	/// Abstract job giver that adds a reference to ProductionComp to parse component from Job assignment to driver.
	/// </summary>
	/// <seealso cref="Verse.AI.ThinkNode_JobGiver" />
	public abstract class Giver_Producer : ThinkNode_JobGiver
	{
		/// <summary>
		/// Reference to the production component that issued the production job.
		/// </summary>
		public HediffComp_Production ProductionComp;


	}
}
