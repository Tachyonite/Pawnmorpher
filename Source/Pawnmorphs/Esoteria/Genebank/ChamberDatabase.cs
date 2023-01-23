// ChamberDatabase.cs created by Iron Wolf for Pawnmorph on 07/31/2020 6:06 PM
// last updated 07/31/2020  6:06 PM

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.DebugUtils;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using Pawnmorph.UserInterface.PartPicker;
using Pawnmorph.Utilities;
using Pawnmorph.Utilities.Collections;
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
        
        private const string NOT_ENOUGH_STORAGE_REASON = "NotEnoughStorageSpaceToTagPK";
        private const string ALREADY_TAGGED_REASON = "AlreadyTaggedAnimal";
        private const string ALREADY_TAGGED_MULTI_REASON = "PMAlreadyTaggedMulti";

        /// <summary>
        ///     translation string for not enough free power
        /// </summary>
        public const string NOT_ENOUGH_POWER = "PMDatabaseWithoutPower";
        private const string NOT_TAGGABLE = "PMMutationNotTaggable";
        private const string RESTRICTED_MUTATION = "PMMutationRestricted";

        private int? _usedStorageCache;
        private int _totalStorage = 0;
        private int _inactiveAmount;

		Dictionary<Type, ExposedListContainer<IGenebankEntry>> _genebankDatabase = new Dictionary<Type, ExposedListContainer<IGenebankEntry>>();

		/// <summary>
		///     Initializes a new instance of the <see cref="ChamberDatabase" /> class.
		/// </summary>
		/// <param name="world">The world.</param>
		public ChamberDatabase(World world) : base(world)
        {
        }



        /// <summary>
        ///     Gets the stored mutations.
        /// </summary>
        /// <value>
        ///     The stored mutations.
        /// </value>
        [NotNull]
        public IReadOnlyList<MutationDef> StoredMutations => GetEntryValues<MutationDef>();

		/// <summary>
		///     Gets the tagged animals.
		/// </summary>
		/// <value>
		///     The tagged animals.
		/// </value>
		[NotNull]
		public IReadOnlyList<PawnKindDef> TaggedAnimals => GetEntryValues<PawnKindDef>();

        public IReadOnlyList<MutationTemplate> MutationTemplates => GetEntryValues<MutationTemplate>();

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
                return TotalStorage - UsedStorage - _inactiveAmount;
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

                    foreach (IList<IGenebankEntry> genebankEntries in _genebankDatabase.Values)
                    {
                        v += genebankEntries.Sum(x => x.GetRequiredStorage());
                    }

					_usedStorageCache = v;
                }

                return _usedStorageCache.Value;
            }
        }




        public IReadOnlyList<T> GetEntryValues<T>()
        {
            Type entryType = typeof(GenebankEntry<T>);

			if (_genebankDatabase.ContainsKey(entryType))
            {
			    return _genebankDatabase[entryType].Select(x => (x as GenebankEntry<T>).Value).ToList();
            }
            return new List<T>();
		}

		public IReadOnlyList<GenebankEntry<T>> GetEntryItems<T>()
		{
			Type entryType = typeof(GenebankEntry<T>);

			if (_genebankDatabase.ContainsKey(entryType))
			{
				return _genebankDatabase[entryType].Cast<GenebankEntry<T>>().ToList();
			}
            return new List<GenebankEntry<T>>();
		}

		/// <summary>
		///     Gets a value indicating whether this instance can tag.
		/// </summary>
		/// <value>
		///     <c>true</c> if this instance can tag; otherwise, <c>false</c>.
		/// </value>
		public bool CanTag => FreeStorage > 0;


        private PawnmorpherSettings Settings => LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();

		/// <summary>
		/// Adds the template to the database
		/// </summary>
		/// <param name="entry">The entry to add.</param>
		/// <param name="failMode">The fail mode.</param>
		/// <exception cref="ArgumentNullException">mutationDef</exception>
		/// Note: this does
		/// <b>not</b>
		/// check if there is enough space to add the mutation or if it is restricted, use
		/// <see cref="CanAddToDatabase(Pawnmorph.Hediffs.MutationDef)" />
		/// to check
		public bool AddToDatabase<T>([NotNull] GenebankEntry<T> entry, LogFailMode failMode = LogFailMode.Silent)
		{
			if (entry == null)
				throw new ArgumentNullException(nameof(entry));

            if (CanAddToDatabase(entry, out string reason) == false)
            {
                failMode.LogFail(reason);
                return false;
            }

            Type entryType = typeof(GenebankEntry<T>);
            if (_genebankDatabase.ContainsKey(entryType) == false)
                _genebankDatabase.Add(entryType, new ExposedListContainer<IGenebankEntry>());

            _genebankDatabase[entryType].Add(entry);

			if (_usedStorageCache != null)
				_usedStorageCache += entry.GetRequiredStorage();

            return true;
		}

		public bool TryAddToDatabase<T>([NotNull] GenebankEntry<T> entry, out string reason)
        {
            if (CanAddToDatabase(entry, out reason) == false)
                return false;

            return AddToDatabase(entry);
        }


		public bool CanAddToDatabase<T>([NotNull] GenebankEntry<T> entry)
		{
			return CanAddToDatabase(entry, out _);
		}



		/// <summary>
		///     Determines whether this instance with the specified mutation definition can be added to the database
		/// </summary>
		/// <param name="entry">The genebank entry to check.</param>
		/// <param name="reason">The reason.</param>
		/// <returns>
		///     <c>true</c> if this instance with the specified mutation definition  [can add to database]  otherwise, <c>false</c>
		///     .
		/// </returns>
		public bool CanAddToDatabase<T>([NotNull] GenebankEntry<T> entry, out string reason)
		{
			if (entry == null) 
                throw new ArgumentNullException(nameof(entry));

            Type entryType = typeof(GenebankEntry<T>);
			if (_genebankDatabase.ContainsKey(entryType) && _genebankDatabase[entryType].Contains(entry))
			{
				reason = ALREADY_TAGGED_REASON.Translate(entry.GetCaption());
				return false;
			}

            int requiredStorage = entry.GetRequiredStorage();
			if (FreeStorage < requiredStorage)
			{
				reason = NOT_ENOUGH_STORAGE_REASON.Translate(entry.GetCaption(), DatabaseUtilities.GetStorageString(requiredStorage), DatabaseUtilities.GetStorageString(FreeStorage));
				return false;
			}

			if (!CanTag)
			{
				reason = NOT_ENOUGH_POWER.Translate();
				return false;
			}

            if (entry.CanAddToDatabase(this, out reason) == false)
            {
                return false;
            }

			reason = "";
			return true;
		}



		/// <summary>
		///     Determines whether any of the specified mutation definitions can be
		///     added to the database, and outputs an error if not.
		/// </summary>
		/// <param name="mutationDefs">The mutation definitions.</param>
		/// <param name="reason">The reason the mutation cannot be ad.</param>
		/// <returns>
		///     <c>true</c> if at least one mutation definition can be added to database, otherwise <c>false</c>.
		/// </returns>
		public bool CanAddAnyToDatabase([NotNull] IEnumerable<MutationDef> mutationDefs, out string reason)
		{
			if (mutationDefs == null) throw new ArgumentNullException(nameof(mutationDefs));

            MutationGenebankEntry entry = new MutationGenebankEntry(mutationDefs.First());
            _genebankDatabase.TryGetValue(typeof(GenebankEntry<MutationDef>), out ExposedListContainer<IGenebankEntry> mutationEntries);
            int? minRequiredCapacity = null;
            MutationDef smallestMutation = null;
			int neededCapacity = 0;
            if (mutationEntries != null)
            {
                bool hasValid = false;
                foreach (MutationDef mutationDef in mutationDefs)
                {
                    entry.Value = mutationDef;
                    if (mutationEntries.Contains(entry) == false)
                    {
                        hasValid = true;
                        neededCapacity = entry.GetRequiredStorage();
                        if (minRequiredCapacity == null || minRequiredCapacity > neededCapacity)
                        {
                            minRequiredCapacity = neededCapacity;
                            smallestMutation = mutationDef;
						}
                    }
				}

                if (hasValid == false)
                {
				    // All already tagged.
				    reason = ALREADY_TAGGED_MULTI_REASON.Translate();
				    return true;
                }
            }

			if (FreeStorage < minRequiredCapacity)
			{
				reason = NOT_ENOUGH_STORAGE_REASON.Translate(smallestMutation, DatabaseUtilities.GetStorageString(smallestMutation.GetRequiredStorage()), DatabaseUtilities.GetStorageString(FreeStorage));
				return false;
			}

			if (!CanTag)
			{
				reason = NOT_ENOUGH_POWER.Translate();
				return false;
			}

			reason = "";
			return true;
		}

        /// <summary>
        ///     Exposes the data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Collections.Look(ref _genebankDatabase, nameof(_genebankDatabase), LookMode.Value, LookMode.Deep);


			if (Scribe.mode == LoadSaveMode.LoadingVars && _genebankDatabase == null)
            {
                _genebankDatabase = new Dictionary<Type, ExposedListContainer<IGenebankEntry>>();

				List<MutationDef> mutationDefs = null;
                Scribe_Collections.Look(ref mutationDefs, "StoredMutations", LookMode.Def);
                if (mutationDefs != null)
                    _genebankDatabase.Add(typeof(GenebankEntry<MutationDef>), new ExposedListContainer<IGenebankEntry>(mutationDefs.Select(x => new MutationGenebankEntry(x))));

				List<PawnKindDef> taggedAnimalDefs = null;
				Scribe_Collections.Look(ref taggedAnimalDefs, "TaggedAnimals", LookMode.Def);
				if (taggedAnimalDefs != null)
					_genebankDatabase.Add(typeof(GenebankEntry<PawnKindDef>), new ExposedListContainer<IGenebankEntry>(taggedAnimalDefs.Select(x => new AnimalGenebankEntry(x))));
				
                List<MutationTemplate> templates = null;
				Scribe_Collections.Look(ref templates, "_storedTemplates");
				if (templates != null)
					_genebankDatabase.Add(typeof(GenebankEntry<MutationTemplate>), new ExposedListContainer<IGenebankEntry>(templates.Select(x => new TemplateGenebankEntry(x))));
			}

			Scribe_Values.Look(ref _totalStorage, nameof(TotalStorage));
        }

        /// <summary>
        ///     Finalizes the initialize.
        /// </summary>
        public override void FinalizeInit()
        {
            base.FinalizeInit();
            _usedStorageCache = null;
        }

        /// <summary>
        ///     Notifies that the given amount of storage capacity has lost power and is no longer available .
        /// </summary>
        /// <param name="storageAmount">The storage amount.</param>
        public void NotifyLostPower(int storageAmount)
        {
            _inactiveAmount += storageAmount;
        }

        /// <summary>
        ///     Notifies the given amount of storage capacity has power restored
        /// </summary>
        /// <param name="storageAmount">The storage amount.</param>
        public void NotifyPowerOn(int storageAmount)
        {
            _inactiveAmount = Mathf.Max(_inactiveAmount - storageAmount, 0);
        }

		/// <summary>
		///     Removes the given mutation def from database.
		/// </summary>
		/// <param name="entry">The entry to remove.</param>
		public void RemoveFromDatabase<T>(GenebankEntry<T> entry)
        {
            if (_genebankDatabase.TryGetValue(typeof(GenebankEntry<T>), out ExposedListContainer<IGenebankEntry> genebankEntries))
            {
				if (genebankEntries.Contains(entry))
                {
                    genebankEntries.Remove(entry);
                    ClearCache();
                }
			}
        }


		internal void ClearCache()
        {
            _usedStorageCache = null;
        }
    }
}