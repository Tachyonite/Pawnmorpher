namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Interface for all hediffs that can possibly cause mutations
	/// </summary>
	public interface IMutagenicHediff
	{
		/// <summary>
		/// Whether or not this hediff is currently blocking race checks
		/// </summary>
		/// <value><c>true</c> if blocks race check; otherwise, <c>false</c>.</value>
		bool BlocksRaceCheck { get; }

		/// <summary>
		/// Gets a value indicating whether there are any mutations in the current stage.
		/// </summary>
		/// <value>
		///   <c>true</c> if there are any mutations in the current stage; otherwise, <c>false</c>.
		/// </value>
		bool CurrentStageHasMutations { get; }

		/// <summary>
		/// Gets a value indicating whether there are any transformations in the current stage.
		/// </summary>
		/// <value>
		///   <c>true</c> if there are any transformations in the current stage; otherwise, <c>false</c>.
		/// </value>
		bool CurrentStageHasTransformation { get; }

		/// <summary>
		/// Marks the hediff for removal
		/// </summary>
		void MarkForRemoval();
	}
}
