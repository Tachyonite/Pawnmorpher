// IMorphTfListnerComp.cs created by Iron Wolf for Pawnmorph on 08/15/2021 8:19 AM
// last updated 08/15/2021  8:19 AM

using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// interface for hediff comps that listen to the parts that <see cref="TransformationBase"/> or <see cref="Hediff_MutagenicBase"/> visits while trying to add mutations 
	/// </summary>
	public interface ITfHediffObserverComp
	{
		/// <summary>
		/// called when the morph hediff is about to start visiting body parts.
		/// </summary>
		void Init();

		/// <summary>
		/// called when the hediff stage changes.
		/// </summary>
		void StageChanged();

		/// <summary>
		/// called when the morph tf observes the give body part record on the given pawn
		/// </summary>
		/// <param name="record">The record observed. if null a observing whole body hediffs</param>
		void Observe([CanBeNull] BodyPartRecord record);

		/// <summary>
		/// called after the given mutation is added to the pawn.
		/// </summary>
		/// <param name="newMutation">The new mutation.</param>
		void MutationAdded([NotNull] Hediff_AddedMutation newMutation);

	}
}