using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Utilities;
using Pawnmorph.Utilities.Collections;
using Verse;

namespace Pawnmorph.Hediffs.Utility
{
    /// <summary>
    /// Utility class to handle the interactions between body parts and mutation lists.
    /// Handles all the logic of keeping everything in sync, because it's finnicky and
    /// error-prone to do it manually.
    /// </summary>
    public sealed class BodyMutationManager : IExposable
    {
        private List<MutationEntry> mutations;

        private BodyPartChecklist spreadList;
        private IExposableChecklist<MutationEntry> bodyPartMutationList;

        [Unsaved]
        private readonly MultiDict<BodyPartDef, MutationEntry> bodyPartMutationCache
                = new MultiDict<BodyPartDef, MutationEntry>();

        [Unsaved]
        private readonly List<MutationEntry> wholeBodyMutationCache = new List<MutationEntry>();


        /// <summary>
        /// Gets the current body part.
        /// </summary>
        /// <value>The body part.</value>
        public BodyPartRecord BodyPart => spreadList?.Entry;

        /// <summary>
        /// Gets the current mutation.
        /// </summary>
        /// <value>The mutation.</value>
        public MutationEntry Mutation => bodyPartMutationList?.Entry;

        /// <summary>
        /// Whether or not the current body part has any remaining mutations
        /// </summary>
        /// <returns><c>true</c>, if there is a next body part, <c>false</c> if the list reset.</returns>
        public bool HasMutations()
        {
            return bodyPartMutationList.HasEntry;
        }

        /// <summary>
        /// Iterates to the next body part in the list.  If there are no more body parts,
        /// return false (and resets the body part list)
        /// </summary>
        /// <returns><c>true</c>, if there is a next body part, <c>false</c> if the list reset.</returns>
        public bool NextBodyPart()
        {
            bool hasNext = spreadList?.NextEntryOrReset() ?? false;
            ResetBodyPartMutationList();
            return hasNext;
        }

        /// <summary>
        /// Iterates to the next mutation in the list.  If there are no more mutations, return false
        /// </summary>
        /// <returns><c>true</c>, if there is a next body part, <c>false</c> if the list reset.</returns>
        public bool NextMutation()
        {
            return bodyPartMutationList?.NextEntry() ?? false;
        }

        /// <summary>
        /// Resets the list of mutations.  Call this when the list of possible mutations change.
        /// (usually because of a stage change, or because the dynamic mutation comp changes)
        /// </summary>
        /// <param name="mutations">The new mutations to use.</param>
        public void ResetMutationList(IEnumerable<MutationEntry> mutations)
        {
            this.mutations = mutations.ToList();
            RegenerateBodyPartMutationCache();
        }

        /// <summary>
        /// Resets the spread list. Call this when the spread order changes.
        /// (due to a stage change, or because something that the spread order relies
        /// on has changed)
        /// </summary>
        /// <param name="spreadOrder">The new spread order to use.</param>
        public void ResetSpreadList(IEnumerable<BodyPartRecord> spreadOrder)
        {
            // Append null so that we handle full-body mutations as part of the list
            spreadList = new BodyPartChecklist(spreadOrder.Append(null));
            ResetBodyPartMutationList();
        }

        /// <summary>
        /// Resets the body part mutation list, usually because the body part changed.
        /// Also called if the mutation list changes.
        /// </summary>
        private void ResetBodyPartMutationList()
        {
            IEnumerable<MutationEntry> mutationList;
            if (BodyPart == null)
                mutationList = wholeBodyMutationCache;
            else
                mutationList = bodyPartMutationCache[BodyPart.def];

            bodyPartMutationList = new IExposableChecklist<MutationEntry>(mutationList);
        }

        /// <summary>
        /// Regenerates the body-part-to-mutation cache.
        /// </summary>
        private void RegenerateBodyPartMutationCache()
        {
            bodyPartMutationCache.Clear();
            wholeBodyMutationCache.Clear();

            foreach (var entry in mutations.MakeSafe())
                if (entry.mutation.parts != null)
                    foreach (BodyPartDef possiblePart in entry.mutation.parts)
                        bodyPartMutationCache.Add(possiblePart, entry);
                else
                    wholeBodyMutationCache.Add(entry);
        }


        /// <summary>
        /// Exposes data to be saved/loaded from XML upon saving the game
        /// </summary>
        public void ExposeData()
        {
            Scribe_Collections.Look(ref mutations, nameof(mutations), LookMode.Deep);
            Scribe_Deep.Look(ref spreadList, nameof(spreadList));
            Scribe_Deep.Look(ref bodyPartMutationList, nameof(bodyPartMutationList));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                RegenerateBodyPartMutationCache();
            }
        }

        /// <summary>
        /// Generates a debug string indicating the status of the mutation manager
        /// </summary>
        /// <returns>The debug string.</returns>
        public string DebugString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("  BodyMutationManager");
            builder.AppendLine($"    Next Body Part: {spreadList?.Entry?.def?.defName} ({spreadList.Index + 1}/{spreadList.Count})");
            builder.AppendLine($"    Next Mutation: {bodyPartMutationList?.Entry?.mutation?.defName} ({bodyPartMutationList.Index + 1}/{bodyPartMutationList.Count})");
            return builder.ToString();
        }
    }
}
