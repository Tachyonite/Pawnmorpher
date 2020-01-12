// MutationEntry.cs modified by Iron Wolf for Pawnmorph on //2020 
// last updated 01/05/2020  1:58 PM

using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// simple POD that stores information about a mutation entry 
    /// </summary>
    public class MutationEntry
    {
        /// <summary>
        /// The mutation
        /// </summary>
        public MutationDef mutation;

        /// <summary>
        /// The chance to add this mutation 
        /// </summary>
        public float addChance = 0.76f;

        /// <summary>
        /// if true, a mutation chain will not progress further until this mutation is added 
        /// </summary>
        public bool blocks;

    }
}