// MutationData.cs created by Iron Wolf for Pawnmorph on //2020 
// last updated 09/20/2020  9:54 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph.User_Interface
{
    /// <summary>
    /// interface for a readonly variant of mutation data 
    /// </summary>
    public interface IReadOnlyMutationData : IExposable
    {
        /// <summary>
        /// Gets the mutation.
        /// </summary>
        /// <value>
        /// The mutation.
        /// </value>
        MutationDef Mutation { get; }

        /// <summary>
        /// Gets the part.
        /// </summary>
        /// <value>
        /// The part.
        /// </value>
        [CanBeNull]
        BodyPartRecord Part { get; }

        /// <summary>
        /// Gets the severity of the mutation
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        float Severity { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is halted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is halted; otherwise, <c>false</c>.
        /// </value>
        bool IsHalted { get; }

        /// <summary>
        /// Gets a value indicating whether the mutation is being removed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if removing; otherwise, <c>false</c>.
        /// </value>
        bool Removing { get; }
    }

    /// <summary>
    /// The mutation to be added to a pawn, along with some key data.
    /// </summary>
    public class MutationData : IReadOnlyMutationData
    {

        MutationDef IReadOnlyMutationData.Mutation => mutation;
        BodyPartRecord IReadOnlyMutationData.Part => part;
        float IReadOnlyMutationData.Severity => severity;

        bool IReadOnlyMutationData.IsHalted => isHalted;
        bool IReadOnlyMutationData.Removing => removing;

        /// <summary>
        /// The def of the mutation to add to the pawn.
        /// </summary>
        public MutationDef mutation;

        /// <summary>
        /// The body part record to add the mutation to.
        /// </summary>
        public BodyPartRecord part;

        /// <summary>
        /// The severity the mutation should be initialized with.
        /// </summary>
        public float severity;

        /// <summary>
        /// Wether the mutation should be able to progress, or should be locked at it's current stage.
        /// </summary>
        public bool isHalted;

        /// <summary>
        /// Whether or not this entry is designated to instead remove mutations from the body part.
        /// </summary>
        public bool removing;

        /// <summary>
        /// Constructor for MutationData used to gather all relevant information.
        /// </summary>
        /// <param name="mutation">The def of the mutation to add to the pawn.</param>
        /// <param name="part">The body part record to add the mutation to.</param>
        /// <param name="severity">The severity the mutation should be initialized with.</param>
        /// <param name="isHalted">Wether the mutation should be able to progress, or should be locked at it's current stage.</param>
        /// <param name="removing">Whether or not this entry is designated to instead remove mutations from the body part.</param>
        public MutationData(MutationDef mutation, BodyPartRecord part, float severity, bool isHalted, bool removing)
        {
            this.mutation = mutation;
            this.part = part;
            this.severity = severity;
            this.isHalted = isHalted;
            this.removing = removing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutationData"/> class.
        /// </summary>
        /// <param name="mData">The m data.</param>
        public MutationData([NotNull] IReadOnlyMutationData mData)
        {
            if (mData == null) throw new ArgumentNullException(nameof(mData));
            mutation = mData.Mutation;
            part = mData.Part;
            severity = mData.Severity;
            isHalted = mData.IsHalted;
            removing = mData.Removing; 
        }

        /// <summary>
        /// Exposes the data.
        /// </summary>
        public void ExposeData()
        {
            Scribe_Defs.Look(ref mutation, nameof(mutation));
            Scribe_BodyParts.Look(ref part, nameof(part));
            Scribe_Values.Look(ref severity, nameof(severity));
            Scribe_Values.Look(ref isHalted, nameof(isHalted));
            Scribe_Values.Look(ref removing, nameof(removing)); 
        }
    }
}