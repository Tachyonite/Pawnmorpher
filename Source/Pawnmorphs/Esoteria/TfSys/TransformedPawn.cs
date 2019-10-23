// TransformedPawn.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 7:26 PM
// last updated 08/13/2019  8:16 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.TfSys
{
    /// <summary>
    ///     base class for storing a "transformed pawn" in such a way that the original can be retried
    /// </summary>
    public abstract class TransformedPawn : IExposable
    {
        /// <summary>The mutagen definition</summary>
        public MutagenDef mutagenDef; 


        /// <summary>
        ///     Gets the original pawns.
        /// </summary>
        /// <value>
        ///     The original pawns.
        /// </value>
        [NotNull]
        public abstract IEnumerable<Pawn> OriginalPawns { get; }

        /// <summary>
        ///     Gets the transformed pawns.
        /// </summary>
        /// <value>
        ///     The transformed pawns.
        /// </value>
        [NotNull]
        public abstract IEnumerable<Pawn> TransformedPawns { get; }


        /// <summary>
        ///     Returns true if this instance is valid.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                foreach (var originalPawn in OriginalPawns)
                    if (originalPawn.DestroyedOrNull())
                        return false;

                foreach (var transformedPawn in TransformedPawns)
                    if (transformedPawn.DestroyedOrNull())
                        return false;

                return true;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can be reverted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can be reverted; otherwise, <c>false</c>.
        /// </value>
        public abstract bool CanRevert { get; }


        //some debug stuff         
        /// <summary>Gets the originals debug string.</summary>
        /// <value>The originals debug string.</value>
        public IEnumerable<string> OriginalsDebugString
        {
            get { return OriginalPawns.Select(p => p?.Name.ToStringFull ?? "[Null]"); }
        }

        /// <summary>Gets the transformed debug string.</summary>
        /// <value>The transformed debug string.</value>
        public IEnumerable<string> TransformedDebugString
        {
            get { return TransformedPawns.Select(p => p?.Name.ToStringFull ?? "[Null]"); }
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return
                $"[{string.Join(",", OriginalsDebugString.ToArray())}] => [{string.Join(",", TransformedDebugString.ToArray())}]";
        }

        /// <summary>Creates a new transformed pawn instance out of the given original pawn and transformed pawn.</summary>
        /// for backwards compatibility with old saves, should not be used in new code
        /// <param name="original">The original.</param>
        /// <param name="transformed">The transformed.</param>
        /// <returns></returns>
        public static TransformedPawn Create(Pawn original, Pawn transformed)
        {
            return new
                TransformedPawnSingle //just using some factory methods for now, want to move this somewhere else eventually

                {
                    animal = transformed,
                    original = original
                };
        }

        /// <summary>Creates a new TransformedPawn instance out of the given original pawns and the specified transformed pawn</summary>
        /// for backwards compatibility with old saves, should not be used with new code
        /// <param name="originals">The originals.</param>
        /// <param name="transformed">The transformed.</param>
        /// <returns></returns>
        public static TransformedPawn Create(IEnumerable<Pawn> originals, Pawn transformed)
        {
            return new MergedPawns
            {
                originals = originals.ToList(),
                meld = transformed
            };
        }
        /// <summary>Create a new TransformedPawn instance from the given original pawns and the transformed pawn</summary>
        ///for backwards compatibility with old saves, should not be used with new code
        /// <param name="original0">The original0.</param>
        /// <param name="original1">The original1.</param>
        /// <param name="meld">The meld.</param>
        /// <returns></returns>
        public static TransformedPawn Create(Pawn original0, Pawn original1, Pawn meld)
        {
            return new MergedPawns
            {
                meld = meld,
                originals = new List<Pawn> {original0, original1}
            };
        }

        /// <summary>Creates the specified inst.</summary>
        /// for backwards compatibility with old saves, should not be used with new code
        /// <param name="inst">The inst.</param>
        /// <returns></returns>
        public static TransformedPawn Create(PawnMorphInstance inst)
        {
            return new TransformedPawnSingle()
            {
                original = inst.origin,
                animal = inst.replacement
            };
        }

        /// <summary>Creates the specified inst.</summary>
        /// for backwards compatibility with old saves, should not be used with new code
        /// <param name="inst">The inst.</param>
        /// <returns></returns>
        public static TransformedPawn Create(PawnMorphInstanceMerged inst)
        {
            return Create(inst.origin, inst.origin2, inst.replacement); 
        }

        /// <summary>
        ///     Gets the status of the given pawn with regards to this instance
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>if the pawn is the original pawn, transformed pawn, or null if neither</returns>
        public abstract TransformedStatus? GetStatus(Pawn pawn);

        /// <summary>generates a debug string</summary>
        /// <returns></returns>
        public string ToDebugString()
        {
            return
                $"originals [{string.Join(",", OriginalsDebugString.ToArray())}]\ntransformed [{string.Join(",", TransformedDebugString.ToArray())}]";
        }

        /// <summary>Exposes the data.</summary>
        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref mutagenDef, nameof(mutagenDef)); 
        }
    }

    /// <summary>
    ///     transformed pawn instance for a single original-animal pair
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.TransformedPawn" />
    public class TransformedPawnSingle : TransformedPawn
    {
        /// <summary>The original pawn</summary>
        public Pawn original;
        /// <summary>The transformed pawn</summary>
        public Pawn animal;


        /// <summary>Gets the original pawns.</summary>
        /// <value>The original pawns.</value>
        public override IEnumerable<Pawn> OriginalPawns
        {
            get { yield return original; }
        }

        /// <summary>Gets the transformed pawns.</summary>
        /// <value>The transformed pawns.</value>
        public override IEnumerable<Pawn> TransformedPawns
        {
            get { yield return animal; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can be reverted.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can be reverted; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRevert => animal.health.hediffSet.HasHediff(TfHediffDefOf.TransformedHuman);

        /// <summary>
        ///     Gets the status of the given pawn with regards to this instance
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>if the pawn is the original pawn, transformed pawn, or null if neither</returns>
        public override TransformedStatus? GetStatus(Pawn pawn)
        {
            if (pawn == original) return TransformedStatus.Original;
            if (pawn == animal) return TransformedStatus.Transformed;
            return null;
        }

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref original, true, nameof(original));
            Scribe_References.Look(ref animal, nameof(animal), true); 

        }
    }


    /// <summary>
    /// TransformedPawn instance for merged pawns
    /// </summary>
    /// <seealso cref="Pawnmorph.TfSys.TransformedPawn" />
    public class MergedPawns : TransformedPawn
    {
        /// <summary>The original pawns</summary>
        public List<Pawn> originals;
        /// <summary>The resultant meld</summary>
        public Pawn meld;
        /// <summary>Gets the original pawns.</summary>
        /// <value>The original pawns.</value>
        public override IEnumerable<Pawn> OriginalPawns => originals ?? Enumerable.Empty<Pawn>();

        /// <summary>Gets the transformed pawns.</summary>
        /// <value>The transformed pawns.</value>
        public override IEnumerable<Pawn> TransformedPawns
        {
            get { yield return meld; }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance can be reverted.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can be reverted; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRevert => meld.health.hediffSet.HasHediff(TfHediffDefOf.TransformedHuman);

        /// <summary>
        ///     Gets the status of the given pawn with regards to this instance
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>if the pawn is the original pawn, transformed pawn, or null if neither</returns>
        public override TransformedStatus? GetStatus(Pawn pawn)
        {
            if (originals?.Contains(pawn) ?? false) return TransformedStatus.Original;
            if (pawn == meld) return TransformedStatus.Transformed;
            return null;
        }

        /// <summary>Exposes the data.</summary>
        public override void ExposeData()
        {
            base.ExposeData();
           Scribe_Collections.Look(ref originals,  true, nameof(originals), LookMode.Deep);
           Scribe_References.Look(ref meld, nameof(meld), true); 
        }
    }
}