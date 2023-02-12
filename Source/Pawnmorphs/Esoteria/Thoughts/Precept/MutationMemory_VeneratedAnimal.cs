// Memory_VeneratedAnimal.cs created by Iron Wolf for Pawnmorph on 07/22/2021 7:22 AM
// last updated 07/22/2021  7:22 AM

using RimWorld;
using Verse;

namespace Pawnmorph.Thoughts.Precept
{
	/// <summary>
	/// memory that substitutes uses of ANIMALKIND for a given venerated animal 
	/// </summary>
	/// <seealso cref="Pawnmorph.Thoughts.MutationMemory" />
	public class MutationMemory_VeneratedAnimal : MutationMemory
	{
		/// <summary>
		/// determines if this instance groups with the other thought 
		/// </summary>
		/// <param name="other">The other.</param>
		/// <returns></returns>
		public override bool GroupsWith(Thought other)
		{
			if (other is MutationMemory_VeneratedAnimal mutMemory)
			{
				return mutMemory.veneratedAnimalLabel == veneratedAnimalLabel && base.GroupsWith(other);

			}
			return false;
		}

		private const string ANIMAL_TAG = "ANIMALKIND";

		/// <summary>
		/// The venerated animal label
		/// </summary>
		public string veneratedAnimalLabel;


		/// <summary>
		/// Gets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		public override string Description => CurStage.description.Formatted(veneratedAnimalLabel.Named(ANIMAL_TAG)) + CausedByBeliefInPrecept;

		/// <summary>
		/// Gets the label cap.
		/// </summary>
		/// <value>
		/// The label cap.
		/// </value>
		public override string LabelCap => CurStage.LabelCap.Formatted(veneratedAnimalLabel.Named(ANIMAL_TAG));


		/// <summary>
		/// Exposes the data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref veneratedAnimalLabel, nameof(veneratedAnimalLabel));
		}
	}
}