// MutagenDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:02 PM
// last updated 08/13/2019  4:02 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using Pawnmorph.Utilities;
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
        /// <summary>if this instance can infect animals</summary>
        public bool canInfectAnimals;
        /// <summary>
        /// if this instance can infect mechanoids
        /// </summary>
        public bool canInfectMechanoids;
        /// <summary>The mutagen type</summary>
        public Type mutagenType;
        /// <summary>
        /// the positive thought to add when a pawn is reverted
        /// </summary>
        public ThoughtDef revertedThoughtGood;
        /// <summary>
        /// The negative thought to add when a pawn is reverted 
        /// </summary>
        public ThoughtDef revertedThoughtBad;
        [Unsaved] private Mutagen _mutagenCached;
        /// <summary>
        /// The damage properties
        /// </summary>
        [NotNull]
        public MutagenDamageProperties damageProperties = new MutagenDamageProperties();


        /// <summary>
        /// a list of hediffs that make a pawn immune to the effects of this mutagen source 
        /// </summary>
        [NotNull]
        public List<HediffDef> immunizingHediffs = new List<HediffDef>();

        /// <summary>
        /// The aspect givers
        /// </summary>
        [NotNull]
        public List<AspectGiver> aspectGivers = new List<AspectGiver>(); 

        /// <summary>Get all Configuration Errors with this instance</summary>
        /// <returns></returns>
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (var configError in base.ConfigErrors())
            {
                yield return configError; 
            }

            foreach (AspectGiver aspectGiver in aspectGivers.MakeSafe()) //check the aspect givers for errors 
            {
                if (aspectGiver == null) yield return "null aspect giver";
                else
                {
                    foreach (string configError in aspectGiver.ConfigErrors())
                    {
                        yield return configError; 
                    }
                }
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

        /// <summary>
        /// Determines whether this instance can transform the specified race definition.
        /// </summary>
        /// <param name="raceDef">The race definition.</param>
        /// <returns>
        ///   <c>true</c> if this instance can transform the specified race definition; otherwise, <c>false</c>.
        /// </returns>
        public bool CanTransform(ThingDef raceDef)
        {
            return MutagenCached.CanTransform(raceDef);
        }

        /// <summary>Gets the cached mutagen </summary>
        /// <value>The cached mutagen </value>
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
        /// <summary>
        /// Determines whether this instance can infect the specified pawn.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns>
        ///   <c>true</c> if this instance can infect the specified pawn; otherwise, <c>false</c>.
        /// </returns>
        public bool CanInfect(Pawn pawn)
        {
            return MutagenCached.CanInfect(pawn); 
        }
    }

    /// <summary>
    /// class that stores information about mutagenic damage 
    /// </summary>
    public class MutagenDamageProperties
    {
        /// <summary>
        /// the minimum amount of damage to do to apparel 
        /// </summary>
        public int apparelDamageOffset;
        /// <summary>
        /// The apparel damage multiplier
        /// </summary>
        public float apparelDamageMultiplier = 1;
        /// <summary>
        /// how much biproduct to spawn per point of damage 
        /// </summary>
        public float spawnedBiproductMult;

        /// <summary>
        /// The biproduct to spawn when apparel takes damage 
        /// </summary>
        [CanBeNull] public ThingDef biproduct; 
    }

}