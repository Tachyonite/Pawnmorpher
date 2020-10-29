// ChamberDatabase.cs created by Iron Wolf for Pawnmorph on 07/31/2020 6:06 PM
// last updated 07/31/2020  6:06 PM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Hediffs;
using Pawnmorph.ThingComps;
using Pawnmorph.Utilities;
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


        private PawnmorpherSettings Settings => LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();

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
        public int FreeStorage
        {
            get
            {

                if (Settings.chamberDatabaseIgnoreStorageLimit) return int.MaxValue; 
                return TotalStorage - UsedStorage;
            }
        }

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
        /// Removes the given mutation def from database.
        /// </summary>
        /// <param name="mDef">The m definition.</param>
        public void RemoveFromDatabase(MutationDef mDef)
        {
            if (!_storedMutations.Contains(mDef))
                return;
            _usedStorageCache = null;

            _storedMutations.Remove(mDef); 
        }



        /// <summary>
        ///     Adds the pawnkind to the database directly.
        /// </summary>
        /// note: this function does
        /// <b>not</b>
        /// check if the database can store the given pawnKind, use
        /// <see cref="CanAddToDatabase(PawnKindDef)" />
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
            if (mutationDef.GetRequiredStorage() <= FreeStorage) return false;
            else if (!CanTag)
            {
                return false; 
            }
            return !mutationDef.IsRestricted;
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
            return kindDef.GetRequiredStorage() > FreeStorage && kindDef.race.IsValidAnimal();
        }


        private const string NOT_ENOUGH_STORAGE_REASON = "NotEnoughStorageSpaceToTagPK";
        private const string ALREADY_TAGGED_REASON = "AlreadyTaggedAnimal";
        private const string ANIMAL_NOT_TAGGABLE = "AnimalNotTaggable";

        private bool _migrated;


        /// <summary>
        /// translation string for not enough free power
        /// </summary>
        public const string NOT_ENOUGH_POWER = "PMDatabaseWithoutPower";


        /// <summary>
        ///  Determines whether this instance can add the specified PawnkindDef to the database
        /// </summary>
        /// <param name="pawnKind">Kind of the pawn.</param>
        /// <param name="reason">if the pawnkind cannot be added to the database, The reason why</param>
        /// <returns>
        ///   <c>true</c>  if this instance can add the specified PawnkindDef to the database otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">pawnKind</exception>
        public bool CanAddToDatabase([NotNull] PawnKindDef pawnKind, out string reason)
        {
            if (pawnKind == null) throw new ArgumentNullException(nameof(pawnKind));

            if (pawnKind.GetRequiredStorage() > FreeStorage)
            {
                reason = NOT_ENOUGH_STORAGE_REASON.Translate(pawnKind); 
            }else if (!CanTag)
            {
                reason = NOT_ENOUGH_POWER.Translate();
            }
            else if (TaggedAnimals.Contains(pawnKind))
            {
                reason = ALREADY_TAGGED_REASON.Translate(pawnKind); 
            }else if (DatabaseUtilities.IsChao(pawnKind.race))
            {
                reason = ANIMAL_NOT_TAGGABLE.Translate(pawnKind);
            }
            else reason = "";

            return string.IsNullOrEmpty(reason); 
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
            Scribe_Values.Look(ref _migrated, "migrated"); 
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                _storedMutations = _storedMutations ?? new List<MutationDef>();
                _taggedSpecies = _taggedSpecies ?? new List<PawnKindDef>();
                if (!_migrated)
                {
                    _migrated = false;
                     //move any tagged animals from the previous system into the new one 
                    var oldWComp = Find.World.GetComponent<PawnmorphGameComp>();
                    if (oldWComp == null) return; 
#pragma warning disable 618
                    foreach (PawnKindDef taggedAnimal in oldWComp.taggedAnimals.MakeSafe())
#pragma warning restore 618
                    {
                        if(!_taggedSpecies.Contains(taggedAnimal)) _taggedSpecies.Add(taggedAnimal);
                    }



                }
            }
        }

        internal void ClearCache()
        {
            _usedStorageCache = null;
        }

        /// <summary>
        /// Removes the given pawnkind def from the database.
        /// </summary>
        /// <param name="pkDef">The pk definition.</param>
        public void RemoveFromDatabase(PawnKindDef pkDef)
        {
            if (!_taggedSpecies.Contains(pkDef)) return;

            _usedStorageCache = null;
            _taggedSpecies.Remove(pkDef); 
        }


        private int _inactiveAmount;


        /// <summary>
        /// Gets a value indicating whether this instance can tag.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can tag; otherwise, <c>false</c>.
        /// </value>
        public bool CanTag => FreeStorage - _inactiveAmount > 0; 

        /// <summary>
        /// Notifies that the given amount of storage capacity has lost power and is no longer available .
        /// </summary>
        /// <param name="storageAmount">The storage amount.</param>
        public void NotifyLostPower(int storageAmount)
        {
            _inactiveAmount += storageAmount;
        }

        /// <summary>
        /// Notifies the given amount of storage capacity has power restored 
        /// </summary>
        /// <param name="storageAmount">The storage amount.</param>
        public void NotifyPowerOn(int storageAmount)
        {
            _inactiveAmount = Mathf.Max(_inactiveAmount - storageAmount, 0); 
        }
    }
}