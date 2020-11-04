// DatabaseUtilities.cs modified by Iron Wolf for Pawnmorph on 09/02/2019 8:44 AM
// last updated 09/02/2019  8:44 AM

using System;
using System.Collections.Generic;
using System.Linq;
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
    [StaticConstructorOnStartup]
    public static class DatabaseUtilities
    {

        [NotNull] private static readonly Dictionary<PawnKindDef, IReadOnlyList<MutationDef>> _taggableMutationsLookup =
            new Dictionary<PawnKindDef, IReadOnlyList<MutationDef>>(); 
        
        static DatabaseUtilities()
        {
            bool ConnectedToMorph(PawnKindDef pkDef)
            {
                return DefDatabase<MorphDef>.AllDefs.Any(m => m.AllAssociatedMutations.Any()
                                                           && (pkDef.race == m.race
                                                            || m.associatedAnimals?.Contains(pkDef.race) == true));
            }

            _pawnKindsWithMutations =
                DefDatabase<PawnKindDef>.AllDefs.Where(pk => pk.RaceProps.Animal && ConnectedToMorph(pk)).ToList();
        }

        [NotNull]
        private static readonly string[] Suffixes = new[]
        {
            "KB",
            "MB",
            "GB",
            "TB"
        };

        /// <summary>
        /// Gets the storage string.
        /// </summary>
        /// <param name="storageAmount">The storage amount.</param>
        /// <returns></returns>
        public static string GetStorageString(float storageAmount)
        {
            int idx = 0;
            while (storageAmount > 1000f && idx < Suffixes.Length)
            {
                idx++;
                storageAmount /= 1000f;
            }

            return storageAmount.ToString() + Suffixes[idx];
        }


        private const string MUTATION_ADDED_MESSAGE = "MutationAddedToDatabase";
        private const string ANIMAL_ADDED_TO_DATABASE_MESSAGE = "AnimalAddedToDatabase"; 
        /// <summary>
        /// The minimum amount of storage space a mutation requires 
        /// </summary>
        public static int MIN_MUTATION_STORAGE_SPACE = 1;

        /// <summary>
        /// multiplier for converting 'value' into storage space for mutations 
        /// </summary>
        public static float STORAGE_PER_VALUE_MUTATION = 0.1f;

        /// <summary>
        /// multiplier for converting 'value' into storage space for species 
        /// </summary>
        public static float STORAGE_PER_VALUE_SPECIES = 1;

        [NotNull]
        private static readonly 
            Dictionary<MorphDef, List<PawnKindDef>> _pkCache = new Dictionary<MorphDef, List<PawnKindDef>>();

        [NotNull]

        private static readonly List<PawnKindDef> _pawnKindsWithMutations;


        /// <summary>
        /// .returns an enumerable collection of all mutations that can be stored in the database 
        /// </summary>
        /// <param name="mutationDefs">The mutation defs.</param>
        /// <returns></returns>
        [Pure, NotNull]
        public static IEnumerable<MutationDef> Taggable([NotNull] this IEnumerable<MutationDef> mutationDefs)
        {
            return mutationDefs.Where(m => !m.IsRestricted); 
        }

        /// <summary>
        /// Gets all mutations that can be squired from the given animal.
        /// </summary>
        /// <param name="pkDef">The pk definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pkDef</exception>
        [NotNull]
        public static IReadOnlyList<MutationDef> GetAllMutationsFrom([NotNull] this PawnKindDef pkDef)
        {
            if (pkDef == null) throw new ArgumentNullException(nameof(pkDef));
            if (!pkDef.RaceProps.Animal) return Array.Empty<MutationDef>();


            if (_taggableMutationsLookup.TryGetValue(pkDef, out var lst))
            {
                return lst; 
            }

            List<MutationDef> tmpList = new List<MutationDef>();
            foreach (MorphDef morphDef in MorphDef.AllDefs)
            {
                if (morphDef.race == pkDef.race || morphDef.associatedAnimals?.Contains(pkDef.race) == true)
                {
                    foreach (MutationDef mDef in morphDef.AllAssociatedMutations)
                    {
                        if (!tmpList.Contains(mDef))
                            tmpList.Add(mDef); 
                    }
                }
            }

            _taggableMutationsLookup[pkDef] = tmpList;
            return tmpList; 

        }



        /// <summary>
        /// Gets all pawnkinds that mutations can be extracted from.
        /// </summary>
        /// <value>
        /// gets all .
        /// </value>
        [NotNull]
        public static IReadOnlyList<PawnKindDef> PawnkindsWithMutations => _pawnKindsWithMutations;


        /// <summary>
        /// Determines whether the specified morph is tagged.
        /// </summary>
        /// <param name="mDef">The m definition.</param>
        /// <returns>
        ///   <c>true</c> if the specified morph is tagged; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsTagged([NotNull] this MorphDef mDef)
        {
            var cdB = Find.World.GetComponent<ChamberDatabase>();
            //cache the list so we only have to do the lookup for pawnkinds once 
            if (!_pkCache.TryGetValue(mDef, out List<PawnKindDef> lst))
            {
                lst = DefDatabase<PawnKindDef>.AllDefsListForReading
                                              .Where(p => p.race == mDef.race || mDef.associatedAnimals?.Contains(p.race) == true)
                                              .Distinct()
                                              .ToList();
                _pkCache[mDef] = lst; 

            }

            foreach (PawnKindDef pawnKindDef in lst)
            {
                if (cdB.TaggedAnimals.Contains(pawnKindDef)) return true; 
            }

            return false; 
        }

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
            float spvM = mutationDef.value * STORAGE_PER_VALUE_MUTATION;
            return Mathf.Max(MIN_MUTATION_STORAGE_SPACE, Mathf.RoundToInt(spvM)); 
        }

        //slope of the linear fit curve for converting sqrt(value) to required storage space for pawnKinds 
        private const float PK_SPACE_M = 0.616f;
        //x intercept fo the linear fit curve for converting sqrt(value) to required storage for pawnKinds 
        private const float PK_SPACE_B = -2.49f; 

        /// <summary>
        /// Gets the required storage.
        /// </summary>
        /// <param name="pawnkindDef">The pawnkind definition.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawnkindDef</exception>
        public static int GetRequiredStorage([NotNull] this PawnKindDef pawnkindDef)
        {
            if (pawnkindDef == null) throw new ArgumentNullException(nameof(pawnkindDef));

            var l = Mathf.Sqrt(pawnkindDef.race.BaseMarketValue);
            var sP = PK_SPACE_M * l + PK_SPACE_B; 
            return (int) Mathf.Max(MIN_MUTATION_STORAGE_SPACE, Mathf.RoundToInt(sP)); 
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


        /// <summary>
        /// Determines whether this instance is tagged.
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <returns>
        ///   <c>true</c> if the specified mutation definition is tagged; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">mutationDef</exception>
        public static bool IsTagged([NotNull] this MutationDef mutationDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            var db = Find.World.GetComponent<ChamberDatabase>();
            return db?.StoredMutations.Contains(mutationDef) == true; 
        }
    }


}