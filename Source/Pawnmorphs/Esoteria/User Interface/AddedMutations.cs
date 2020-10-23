// AddedMutations.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 09/20/2020  9:51 AM

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using Verse;

namespace Pawnmorph.User_Interface
{
    /// <summary>
    /// interface for a readonly reference to added mutations 
    /// </summary>
    public interface IReadOnlyAddedMutations : IReadOnlyList<IReadOnlyMutationData>, IExposable
    {

        /// <summary>
        /// Gets the parts.
        /// </summary>
        /// <value>
        /// The parts.
        /// </value>
        List<BodyPartRecord> Parts { get; }

        /// <summary>
        /// gets mutations by part and layer
        /// </summary>
        /// <param name="part">The part.</param>
        /// <param name="layer">The layer.</param>
        /// <returns></returns>
        IReadOnlyMutationData MutationsByPartAndLayer(BodyPartRecord part, MutationLayer layer);
    }

    /// <summary>
    /// A list of the mutations to add to a pawn, along with key data and accessors.
    /// </summary>
    public class AddedMutations : IEnumerable<MutationData>, IReadOnlyAddedMutations
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="AddedMutations"/> class.
        /// </summary>
        public AddedMutations() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddedMutations"/> class.
        /// </summary>
        /// <param name="other">The other.</param>
        public AddedMutations([NotNull] IReadOnlyAddedMutations other)
        {
            foreach (IReadOnlyMutationData mData in other)
            {
                Add(mData);
            }
        }

        IReadOnlyMutationData IReadOnlyAddedMutations.MutationsByPartAndLayer(BodyPartRecord part, MutationLayer layer)
        {
            return MutationsByPartAndLayer(part, layer); 
        }



        /// <summary>
        /// The list of mutations to be added to the pawn, as well as some key data associated with them.
        /// </summary>
        public List<MutationData> mutationData = new List<MutationData>();

        /// <summary>
        /// Returns a list of all body part records currently slated to be modified.
        /// </summary>
        public List<BodyPartRecord> Parts
        {
            get
            {
                return mutationData.Select(m => m.part).ToList();
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        IEnumerator<IReadOnlyMutationData> IEnumerable<IReadOnlyMutationData>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<MutationData> GetEnumerator()
        {
            return mutationData.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)mutationData).GetEnumerator();
        }

        /// <summary>
        /// Adds a new entry to the list of mutations to give the pawn.
        /// </summary>
        /// <param name="mutation">The def of the mutation to add to the pawn.</param>
        /// <param name="part">The body part record to apply the mutation to.</param>
        /// <param name="severity">What severity the added mutation should be intialized with.</param>
        /// <param name="isHalted">Whether or not the addded mutation will be able to progress.</param>
        /// <param name="removing">Whether or not this entry is intended to remove the mutation.</param>
        public void AddData(MutationDef mutation, BodyPartRecord part, float severity, bool isHalted, bool removing)
        {
            mutationData.Add(new MutationData(mutation, part, severity, isHalted, removing));
        }

        /// <summary>
        /// Adds the specified m data.
        /// </summary>
        /// <param name="mData">The m data.</param>
        public void Add([NotNull] IReadOnlyMutationData mData)
        {
            mutationData.Add(new MutationData(mData));
        }

        /// <summary>
        /// Removes the first entry in the mutation data list whose part and layer matches the one provided.
        /// </summary>
        /// <param name="part">The body part record to filter out of the mutation data.</param>
        /// <param name="layer">The mutation layer to filter out of the mutation data.</param>
        public void RemoveByPartAndLayer(BodyPartRecord part, MutationLayer layer)
        {
            mutationData.Remove(mutationData.Where(m => m.part == part && m.mutation.RemoveComp.layer == layer).FirstOrDefault());
            //mutationData = mutationData.Where(m => m.part != part && m.mutation.RemoveComp.layer != layer).ToList();
        }

        /// <summary>
        /// Finds and returns the first entry whose part and layer matches the provided part and layer.
        /// </summary>
        /// <param name="part">The part to match.</param>
        /// <param name="layer">The mutation layer to match.</param>
        /// <returns>The first entry whose part and layer matches the provied part and layer.</returns>
        public MutationData MutationsByPartAndLayer(BodyPartRecord part, MutationLayer layer)
        {
            return mutationData.Where(m => m.part == part).FirstOrDefault();
        }


        /// <summary>
        /// Exposes the data.
        /// </summary>
        public void ExposeData()
        {
            Scribe_Collections.Look(ref mutationData, nameof(mutationData), LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                mutationData = mutationData ?? new List<MutationData>(); 
        }

        /// <summary>Gets the number of elements in the collection.</summary>
        /// <returns>The number of elements in the collection. </returns>
        public int Count => mutationData?.Count ?? 0;

        /// <summary>Gets the element at the specified index in the read-only list.</summary>
        /// <param name="index">The zero-based index of the element to get. </param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        public IReadOnlyMutationData this[int index] {
            get
            {
                if (mutationData == null) throw new InvalidOperationException();
                return mutationData[index]; 
            }

        }
    }
}