// MutationEntry.cs modified by Iron Wolf for Pawnmorph on //2020 
// last updated 01/05/2020  1:58 PM

using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// simple POD that stores information about a mutation entry 
	/// </summary>
	public class MutationEntry : IExposable
	{
		/// <summary>
		/// The default add chance for a MutationEntry if one isn't supplied
		/// TODO - should this just be 1f?
		/// </summary>
		public const float DEFAULT_ADD_CHANCE = 0.76f;

		/// <summary>
		/// Convienience method that builds a MutationEntry directly from a def
		/// using the default values.
		/// </summary>
		/// <returns>The mutation entry.</returns>
		/// <param name="mutation">Mutation.</param>
		/// <param name="chanceMult">A multiplier on the add chance</param>
		public static MutationEntry FromMutation(MutationDef mutation, float chanceMult = 1f)
		{
			return new MutationEntry()
			{
				mutation = mutation,
				addChance = mutation.defaultAddChance * chanceMult,
				blocks = mutation.defaultBlocks
			};
		}

		/// <summary>
		/// The mutation
		/// </summary>
		public MutationDef mutation;

		/// <summary>
		/// The chance to add this mutation 
		/// </summary>
		public float addChance = DEFAULT_ADD_CHANCE;

		/// <summary>
		/// if true, a mutation chain will not progress further until this mutation is added 
		/// </summary>
		public bool blocks;

		/// <summary>
		/// Exposes the data to be Saved/Loaded from XML.
		/// </summary>
		public void ExposeData()
		{
			Scribe_Defs.Look(ref mutation, nameof(mutation));
			Scribe_Values.Look(ref addChance, nameof(addChance), DEFAULT_ADD_CHANCE);
			Scribe_Values.Look(ref blocks, nameof(blocks));
		}
	}
}