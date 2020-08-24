// DatabaseUtilities.cs modified by Iron Wolf for Pawnmorph on 09/02/2019 8:44 AM
// last updated 09/02/2019  8:44 AM

using System;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// static class for various chamber database utility functions 
    /// </summary>
    public static class DatabaseUtilities
    {
        private const string MUTATION_ADDED_MESSAGE = "MutationAddedToDatabase";
        private const string ANIMAL_ADDED_TO_DATABASE_MESSAGE = "AnimalAddedToDatabase"; 
        /// <summary>
        /// The minimum amount of storage space a mutation requires 
        /// </summary>
        public static int MIN_MUTATION_STORAGE_SPACE = 1;

        /// <summary>
        /// multiplier for converting 'value' into storage space for mutations 
        /// </summary>
        public static float STORAGE_PER_VALUE_MUTATION = 1;

        /// <summary>
        /// multiplier for converting 'value' into storage space for species 
        /// </summary>
        public static float STORAGE_PER_VALUE_SPECIES = 1;


        /// <summary>
        /// Determines whether this instance is the def of an animal that can be added to the chamber database
        /// </summary>
        /// <param name="inst">The inst.</param>
        /// <returns>
        ///   <c>true</c> if this instance can be added to the chamber database ; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">inst</exception>
        public static bool IsValidAnimal([NotNull] this ThingDef inst)
        {
            if (inst == null) throw new ArgumentNullException(nameof(inst));
            if (inst.race?.Animal != true) return false; //use != because inst.race?.Animal can be true,false or null
            return !IsChao(inst); 
        }
        /// <summary>
        /// Determines whether the specified definition for a chaomorph.
        /// </summary>
        /// <param name="def">The definition.</param>
        /// <returns>
        ///   <c>true</c> if the specified definition is a chaomorph; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsChao(ThingDef def)
        {
            return def.race.FleshType == DefDatabase<FleshTypeDef>.GetNamed("Chaomorph"); //all chaomorphs have the same flesh type of Chaomorph 
        }

        /// <summary>
        /// Gets the required storage.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">mutationDef</exception>
        public static int GetRequiredStorage([NotNull] this MutationDef mutationDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            return (int) Mathf.Max(MIN_MUTATION_STORAGE_SPACE, mutationDef.value * STORAGE_PER_VALUE_MUTATION); 
        }

        /// <summary>
        /// Gets the required storage.
        /// </summary>
        /// <param name="pawnkindDef">The pawnkind definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawnkindDef</exception>
        public static int GetRequiredStorage([NotNull] this PawnKindDef pawnkindDef)
        {
            if (pawnkindDef == null) throw new ArgumentNullException(nameof(pawnkindDef));
            return (int) Mathf.Max(MIN_MUTATION_STORAGE_SPACE, pawnkindDef.race.BaseMarketValue * STORAGE_PER_VALUE_SPECIES); 
        }

        /// <summary>
        /// Tries to add the specified mutation to the database, returning false on failure.
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="def">The definition.</param>
        /// <param name="displayMessageIfAdded">if set to <c>true</c> [display message if added].</param>
        /// <returns></returns>
        public static bool TryAddToDatabase([NotNull] this ChamberDatabase db, [NotNull] MutationDef def, bool displayMessageIfAdded=true)
        {
            if (!db.CanAddToDatabase(def)) return false;
            db.AddToDatabase(def);
            if (displayMessageIfAdded)
                Messages.Message(MUTATION_ADDED_MESSAGE.Translate(def.Named("Mutation")), MessageTypeDefOf.PositiveEvent); 
            return true; 
        }

        /// <summary>
        /// Tries to add the specified pawnkind to the database, returning false on failure
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="pawnKind">Kind of the pawn.</param>
        /// <param name="displayMessageIfAdded">if set to <c>true</c> [display message if added].</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">db
        /// or
        /// pawnKind</exception>
        public static bool TryAddToDatabase([NotNull] this ChamberDatabase db, [NotNull] PawnKindDef pawnKind, bool displayMessageIfAdded=true)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (pawnKind == null) throw new ArgumentNullException(nameof(pawnKind));
            if (!db.CanAddToDatabase(pawnKind)) return false;
            db.AddToDatabase(pawnKind);
            if (displayMessageIfAdded)
            {
                Messages.Message(ANIMAL_ADDED_TO_DATABASE_MESSAGE.Translate(pawnKind), MessageTypeDefOf.PositiveEvent);
            }
            return true; 
        }

        /// <summary>
        /// Tries to add the specified pawnkind to the database, returning false on failure and a translated reason why
        /// </summary>
        /// <param name="db">The database.</param>
        /// <param name="pawnKind">Kind of the pawn.</param>
        /// <param name="reason">The reason.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// db
        /// or
        /// pawnKind
        /// </exception>
        public static bool TryAddToDatabase([NotNull] this ChamberDatabase db, [NotNull] PawnKindDef pawnKind, out string reason)
        {
            if (db == null) throw new ArgumentNullException(nameof(db));
            if (pawnKind == null) throw new ArgumentNullException(nameof(pawnKind));
            if (!db.CanAddToDatabase(pawnKind, out reason))
            {
                return false; 
            }

            db.AddToDatabase(pawnKind);
            return true; 
        }

    }


}