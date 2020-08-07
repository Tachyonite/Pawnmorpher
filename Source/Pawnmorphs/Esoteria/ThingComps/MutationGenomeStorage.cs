// MutationGenomeStorage.cs created by Iron Wolf for Pawnmorph on 08/07/2020 1:43 PM
// last updated 08/07/2020  1:43 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.ThingComps
{

    /// <summary>
    /// comp that acts like a techprint comp for mutation genomes 
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class MutationGenomeStorage : ThingComp
    {
        private MutationGenomeStorageProps Props => (MutationGenomeStorageProps) props;

        /// <summary>
        /// Gets the mutation this provides the genome info for.
        /// </summary>
        /// <value>
        /// The mutation.
        /// </value>
        [NotNull]
        public MutationDef Mutation => Props.mutation; 
    }

    /// <summary>
    /// comp properties for <see cref="MutationGenomeStorage"/>
    /// </summary>
    /// <seealso cref="Verse.CompProperties" />
    public class MutationGenomeStorageProps : CompProperties
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MutationGenomeStorageProps"/> class.
        /// </summary>
        public MutationGenomeStorageProps()
        {
            compClass = typeof(MutationGenomeStorage); 
        }
        /// <summary>
        /// The mutation this provides the genomes info about 
        /// </summary>
        public MutationDef mutation;

        /// <summary>
        /// gets all configuration errors 
        /// </summary>
        /// <param name="parentDef">The parent definition.</param>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef).MakeSafe())
            {
                yield return configError;
            }

            if (mutation == null)
                yield return "no mutation set"; 
        }
    } 

}