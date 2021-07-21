// VeneratedMutationMemory.cs created by Iron Wolf for Pawnmorph on 07/21/2021 5:51 PM
// last updated 07/21/2021  5:51 PM

using Verse;

namespace Pawnmorph.Thoughts
{
    /// <summary>
    /// variant of mutation memory that formats thoughts for venerated mutations 
    /// </summary>
    /// <seealso cref="Pawnmorph.Thoughts.MutationMemory" />
    public class VeneratedMutationMemory : MutationMemory
    {
        /// <summary>
        /// The venerated animal label
        /// </summary>
        public string veneratedAnimalLabel = "";

        string FormatString(string str)
        {
            return str.Formatted(str.Named(ThoughtLabels.VENERATED_ANIMAL)) + CausedByBeliefInPrecept; 
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref veneratedAnimalLabel, nameof(veneratedAnimalLabel)); 

        }
    }
}