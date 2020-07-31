// ChamberDatabase.cs created by Iron Wolf for Pawnmorph on 07/31/2020 6:06 PM
// last updated 07/31/2020  6:06 PM

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace Pawnmorph.Chambers
{
    /// <summary>
    ///     world component that acts as the central database for a given world instance
    /// </summary>
    /// <seealso cref="RimWorld.Planet.WorldComponent" />
    public class ChamberDatabase : WorldComponent
    {
        private int? _usedStorageCache;


        private int _totalStorage = 0;


        private List<MutationDef> _storedMutations = new List<MutationDef>();
        private List<PawnKindDef> _taggedSpecies = new List<PawnKindDef>();


        /// <summary>
        ///     Initializes a new instance of the <see cref="ChamberDatabase" /> class.
        /// </summary>
        /// <param name="world">The world.</param>
        public ChamberDatabase(World world) : base(world)
        {
        }

        /// <summary>
        ///     Gets the free storage.
        /// </summary>
        /// <value>
        ///     The free storage.
        /// </value>
        public int FreeStorage => TotalStorage - UsedStorage;

        /// <summary>
        ///     Gets or sets the total storage available in the system
        /// </summary>
        /// <value>
        ///     The total storage.
        /// </value>
        public int TotalStorage
        {
            get => _totalStorage;
            set => _totalStorage = Mathf.Max(0, value);
        }

        /// <summary>
        ///     Gets the amount of storage space currently in use.
        /// </summary>
        /// <value>
        ///     The used storage.
        /// </value>
        public int UsedStorage
        {
            get
            {
                if (_usedStorageCache == null)
                {
                    var v = 0;
                    foreach (MutationDef storedMutation in _storedMutations) v += storedMutation.GetRequiredStorage();

                    foreach (PawnKindDef taggedSpecy in _taggedSpecies) v += taggedSpecy.GetRequiredStorage();

                    _usedStorageCache = v;
                }

                return _usedStorageCache.Value;
            }
        }

        /// <summary>
        ///     Gets the stored mutations.
        /// </summary>
        /// <value>
        ///     The stored mutations.
        /// </value>
        [NotNull]
        public IReadOnlyList<MutationDef> StoredMutations => _storedMutations;

        /// <summary>
        ///     Gets the tagged animals.
        /// </summary>
        /// <value>
        ///     The tagged animals.
        /// </value>
        [NotNull]
        public IReadOnlyList<PawnKindDef> TaggedAnimals => _taggedSpecies;


        /// <summary>
        ///     Adds the mutation to the database
        /// </summary>
        /// Note: this does
        /// <b>not</b>
        /// check if there is enough space to add the mutation or if it is restricted, use
        /// <see cref="CanAddToDatabase(Pawnmorph.Hediffs.MutationDef)" />
        /// or
        /// <see cref="DatabaseUtilities.TryAddToDatabase(ChamberDatabase, Pawnmorph.Hediffs.MutationDef)" />
        /// to check
        /// <param name="mutationDef">The mutation definition.</param>
        /// <exception cref="ArgumentNullException">mutationDef</exception>
        public void AddToDatabase([NotNull] MutationDef mutationDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            if (_storedMutations.Contains(mutationDef)) return;
            _storedMutations.Add(mutationDef);

            if (_usedStorageCache != null) _usedStorageCache += mutationDef.GetRequiredStorage();
        }

        /// <summary>
        ///     Adds the pawnkind to the database directly.
        /// </summary>
        /// note: this function does
        /// <b>not</b>
        /// check if the database can store the given pawnKind, use
        /// <see cref="CanAddToDatabase(PawnKindDef)" />
        /// or
        /// <see cref="DatabaseUtilities.TryAddToDatabase(ChamberDatabase, PawnKindDef)" />
        /// to safely add to the database
        /// <param name="pawnKind">Kind of the pawn.</param>
        /// <exception cref="ArgumentNullException">pawnKind</exception>
        public void AddToDatabase([NotNull] PawnKindDef pawnKind)
        {
            if (pawnKind == null) throw new ArgumentNullException(nameof(pawnKind));
            if (_taggedSpecies.Contains(pawnKind)) return;
            if (!pawnKind.race.IsValidAnimal())
            {
                DebugLogUtils.Warning($"trying to enter invalid animal {pawnKind.defName} to the chamber database");
                return;
            }

            _taggedSpecies.Add(pawnKind);
            if (_usedStorageCache != null) _usedStorageCache += pawnKind.GetRequiredStorage();
        }

        /// <summary>
        ///     Determines whether this instance can add the specified mutation def to the database
        /// </summary>
        /// <param name="mutationDef">The mutation definition.</param>
        /// <returns>
        ///     <c>true</c> if this instance can add the specified mutation def to the database  otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">mutationDef</exception>
        public bool CanAddToDatabase([NotNull] MutationDef mutationDef)
        {
            if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
            return mutationDef.GetRequiredStorage() < FreeStorage && !mutationDef.IsRestricted;
        }

        /// <summary>
        ///     Determines whether this instance can add the specified PawnkindDef to the database
        /// </summary>
        /// <param name="kindDef">The kind definition.</param>
        /// <returns>
        ///     <c>true</c> if this instance can add the specified PawnkindDef to the database otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">kindDef</exception>
        public bool CanAddToDatabase([NotNull] PawnKindDef kindDef)
        {
            if (kindDef == null) throw new ArgumentNullException(nameof(kindDef));
            return kindDef.GetRequiredStorage() < FreeStorage && kindDef.race.IsValidAnimal();
        }

        /// <summary>
        ///     Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref _storedMutations, nameof(StoredMutations), LookMode.Def);
            Scribe_Collections.Look(ref _taggedSpecies, nameof(TaggedAnimals), LookMode.Def);
            Scribe_Values.Look(ref _totalStorage, nameof(TotalStorage));

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _storedMutations = _storedMutations ?? new List<MutationDef>();
                _taggedSpecies = _taggedSpecies ?? new List<PawnKindDef>();
            }
        }

        internal void ClearCache()
        {
            _usedStorageCache = null;
        }
    }
}