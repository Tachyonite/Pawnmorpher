// MutagenDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:02 PM
// last updated 08/13/2019  4:02 PM

using System;
using System.Collections.Generic;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary> Def for a mutagen strain. <br />
    /// A mutagen is a collection of transformation related hediff's ingestionOutcomeDoers that all share a common IFF system.
    /// </summary>
    /// <seealso cref="Verse.Def" />
    public class MutagenDef : Def
    {
        public bool canInfectAnimals;
        public bool canInfectMechanoids;
        public Type mutagenType;
        public ThoughtDef revertedThoughtGood;
        public ThoughtDef revertedThoughtBad;
        [Unsaved] private Mutagen _mutagenCached;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var configError in base.ConfigErrors())
            {
                yield return configError; 
            }

            if (mutagenType == null)
                yield return "no mutagen type"; 
            else if (!typeof(Mutagen).IsAssignableFrom(mutagenType))
                yield return $"type {mutagenType.Name} is not a subtype of Mutagen"; 
        }

        /// <summary> Determines whether this instance can transform the specified pawn. </summary>
        /// <param name="pawn"> The pawn. </param>
        /// <returns> <c>true</c> if this instance can transform the specified pawn; otherwise, <c>false</c>. </returns>
        public bool CanTransform(Pawn pawn)
        {
            return MutagenCached.CanTransform(pawn); 
        }

        public Mutagen MutagenCached
        {
            get
            {
                Mutagen cached = _mutagenCached;
                if (cached != null)
                {
                    return cached;
                }

                _mutagenCached = (Mutagen) Activator.CreateInstance(mutagenType);
                _mutagenCached.def = this;
                return _mutagenCached;
            }
        }

        public bool CanInfect(Pawn pawn)
        {
            return MutagenCached.CanInfect(pawn); 
        }
    }
}