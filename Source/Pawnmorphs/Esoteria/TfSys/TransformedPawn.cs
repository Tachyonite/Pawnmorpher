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
        public IEnumerable<string> OriginalsDebugString
        {
            get { return OriginalPawns.Select(p => p?.Name.ToStringFull ?? "[Null]"); }
        }

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

        public static TransformedPawn Create(Pawn original, Pawn transformed)
        {
            return new
                TransformedPawnSingle //just using some factory methods for now, want to move this somewhere else eventually

                {
                    animal = transformed,
                    original = original
                };
        }

        public static TransformedPawn Create(IEnumerable<Pawn> originals, Pawn transformed)
        {
            return new MergedPawns
            {
                originals = originals.ToList(),
                meld = transformed
            };
        }

        public static TransformedPawn Create(Pawn original0, Pawn original1, Pawn meld)
        {
            return new MergedPawns
            {
                meld = meld,
                originals = new List<Pawn> {original0, original1}
            };
        }

        public static TransformedPawn Create(PawnMorphInstance inst)
        {
            return new TransformedPawnSingle()
            {
                original = inst.origin,
                animal = inst.replacement
            };
        }

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


        public string ToDebugString()
        {
            return
                $"originals [{string.Join(",", OriginalsDebugString.ToArray())}]\ntransformed [{string.Join(",", TransformedDebugString.ToArray())}]";
        }

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
        public Pawn original;
        public Pawn animal;


        public override IEnumerable<Pawn> OriginalPawns
        {
            get { yield return original; }
        }

        public override IEnumerable<Pawn> TransformedPawns
        {
            get { yield return animal; }
        }

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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref original, true, nameof(original));
            Scribe_References.Look(ref animal, nameof(animal), true); 

        }
    }


    public class MergedPawns : TransformedPawn
    {
        public List<Pawn> originals;
        public Pawn meld;
        public override IEnumerable<Pawn> OriginalPawns => originals ?? Enumerable.Empty<Pawn>();

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

        public override void ExposeData()
        {
            base.ExposeData();
           Scribe_Collections.Look(ref originals,  true, nameof(originals), LookMode.Deep);
           Scribe_References.Look(ref meld, nameof(meld), true); 
        }
    }
}