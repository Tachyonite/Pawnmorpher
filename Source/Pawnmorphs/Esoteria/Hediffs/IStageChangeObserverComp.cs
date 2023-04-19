using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Interface for hediff comps that do something on a stage change
	/// </summary>
	public interface IStageChangeObserverComp
	{
		/// <summary>
		/// Called when the stage changes on the parent hediff
		/// </summary>
		void OnStageChanged(HediffStage oldStage, HediffStage newStage);
	}
}