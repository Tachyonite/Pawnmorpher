using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs.Utility
{
    /// <summary>
    /// A class that represents a (savable) cache of all possible mutations.
    /// Can be saved and loaded.
    /// 
    /// Don't add any logic outside of storage/retrieval to this class, since it's
    /// just a data structure.  Most logic should go in whatever generates the list
    /// of MutationEntries in the first place.
    /// </summary>
    public sealed class MutationCache : IExposable
    {
        private List<MutationEntry> mutationList;

        [Unsaved]
        private readonly MultiDict<BodyPartDef, MutationEntry> bodyPartMap
                = new MultiDict<BodyPartDef, MutationEntry>();

        [Unsaved]
        private readonly List<MutationEntry> wholeBodyParts = new List<MutationEntry>();

        /// <summary>
        /// Clears the stored cache of mutations
        /// </summary>
        public void ClearMutations()
        {
            mutationList = Enumerable.Empty<MutationEntry>().ToList();
            bodyPartMap.Clear();
            wholeBodyParts.Clear();
        }

        /// <summary>
        /// Replaces the stored cache mutations with the given one
        /// </summary>
        /// <param name="mutations">Mutations.</param>
        public void ReloadMutations(IEnumerable<MutationEntry> mutations)
        {
            mutationList = mutations.ToList();
            RegenerateCache();
        }

        /// <summary>
        /// Regenerates the body-part-to-mutation cache.
        /// </summary>
        private void RegenerateCache()
        {
            bodyPartMap.Clear();
            wholeBodyParts.Clear();
            foreach (var entry in mutationList)
                if (entry.mutation.parts != null)
                    foreach (BodyPartDef possiblePart in entry.mutation.parts)
                        bodyPartMap.Add(possiblePart, entry);
                else
                    wholeBodyParts.Add(entry);
        }

        /// <summary>
        /// Returns a checklist of all mutation entries that can apply to a given
        /// body part.
        /// </summary>
        /// <returns>The mutations for the body part.</returns>
        /// <param name="bodyPart">Body part.</param>
        public MutationList GetMutationsFor(BodyPartDef bodyPart)
        {
            if (bodyPart == null)
                return new MutationList(wholeBodyParts);
            return new MutationList(bodyPartMap[bodyPart]);
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref mutationList, nameof(mutationList), LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                RegenerateCache();
            }
        }
    }

    /// <summary>
    /// A class that represents a (savable) list of mutations.  Used to handle
    /// the state of iterating through a list of mutations.  Can be saved and
    /// loaded.
    /// 
    /// Don't add any logic to this class, since instances will be thrown away and
    /// recreated regularly. Instead, add the logic to the MutTypes entry that
    /// creates the list of mutations.
    /// </summary>
    public sealed class MutationList : Checklist<MutationEntry>
    {
        public MutationList() { }

        public MutationList(IEnumerable<MutationEntry> mutations) : base(mutations) { }

        public override LookMode LookMode => LookMode.Deep;
    }
}
