// RaceMutagenExtension.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:10 PM
// last updated 08/13/2019  4:10 PM

using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph
{
    /// <summary> Extension used to blacklist a race from one or more mutagen strains. </summary>
    public class RaceMutationSettingsExtension : DefModExtension
    {
        /// <summary>if to make this race immune to all mutations</summary>
        public bool immuneToAll;


        /// <summary>
        /// The mutation retrievers
        /// </summary>
        [CanBeNull]
        public List<IRaceMutationRetriever> mutationRetrievers;

        /// <summary>
        /// gets all configuration errors with this instance 
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string configError in base.ConfigErrors().MakeSafe())
            {
                yield return configError;
            }

            if (immuneToAll && (mutationRetrievers != null && mutationRetrievers.Count != 0))
            {
                yield return $"{nameof(immuneToAll)} is true but {nameof(mutationRetrievers)} is set!"; 
            }

            if (mutationRetrievers != null)
            {
                List<string> lst = new List<string>();
                StringBuilder builder = new StringBuilder(); 
                foreach (IRaceMutationRetriever retriever in mutationRetrievers)
                {
                    lst.Clear();
                    builder.Clear();
                    lst.AddRange(retriever.GetConfigErrors());
                    if (lst.Count != 0)
                    {
                        builder.AppendLine($"encountered errors in retriever: {retriever.GetType().Name}!");
                        foreach (string err in lst)
                        {
                            builder.AppendLine("\t" + err); 
                        }

                        yield return builder.ToString(); 
                    }
                }
            }
        }
    }
}