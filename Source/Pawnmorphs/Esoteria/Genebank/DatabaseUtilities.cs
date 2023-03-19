// DatabaseUtilities.cs modified by Iron Wolf for Pawnmorph on 09/02/2019 8:44 AM
// last updated 09/02/2019  8:44 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hediffs;
using UnityEngine;
using Verse;

namespace Pawnmorph.Chambers
{
	/// <summary>
	///     static class for various chamber database utility functions
	/// </summary>
	[StaticConstructorOnStartup]
	public static class DatabaseUtilities
	{
		private const string MUTATION_ADDED_MESSAGE = "MutationAddedToDatabase";
		private const string ANIMAL_ADDED_TO_DATABASE_MESSAGE = "AnimalAddedToDatabase";

		//slope of the linear fit curve for converting sqrt(value) to required storage space for pawnKinds 
		private const float PK_SPACE_M = 0.616f;

		//x intercept fo the linear fit curve for converting sqrt(value) to required storage for pawnKinds 
		private const float PK_SPACE_B = -2.49f;

		[NotNull]
		private static readonly Dictionary<PawnKindDef, IReadOnlyList<MutationDef>> _taggableMutationsLookup =
			new Dictionary<PawnKindDef, IReadOnlyList<MutationDef>>();

		[NotNull]
		private static readonly string[] Suffixes =
		{
			"KB",
			"MB",
			"GB",
			"TB"
		};

		/// <summary>
		///     The minimum amount of storage space a mutation requires
		/// </summary>
		public static int MIN_MUTATION_STORAGE_SPACE = 1;

		/// <summary>
		///     multiplier for converting 'value' into storage space for mutations
		/// </summary>
		public static float STORAGE_PER_VALUE_MUTATION = 0.1f;

		/// <summary>
		///     multiplier for converting 'value' into storage space for species
		/// </summary>
		public static float STORAGE_PER_VALUE_SPECIES = 1;

		[NotNull]
		private static readonly
			Dictionary<MorphDef, List<PawnKindDef>> _pkCache = new Dictionary<MorphDef, List<PawnKindDef>>();

		[NotNull] private static readonly List<PawnKindDef> _pawnKindsWithMutations;

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


		/// <summary>
		///     Gets all pawnkinds that mutations can be extracted from.
		/// </summary>
		/// <value>
		///     gets all .
		/// </value>
		[NotNull]
		public static IReadOnlyList<PawnKindDef> PawnkindsWithMutations => _pawnKindsWithMutations;

		private static ChamberDatabase DB => Find.World.GetComponent<ChamberDatabase>();

		/// <summary>
		///     Gets all mutations that can be squired from the given animal.
		/// </summary>
		/// <param name="pkDef">The pk definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pkDef</exception>
		[NotNull]
		public static IReadOnlyList<MutationDef> GetAllMutationsFrom([NotNull] this PawnKindDef pkDef)
		{
			if (pkDef == null) throw new ArgumentNullException(nameof(pkDef));
			if (!pkDef.RaceProps.Animal) return Array.Empty<MutationDef>();


			if (_taggableMutationsLookup.TryGetValue(pkDef, out IReadOnlyList<MutationDef> lst)) return lst;

			var tmpList = new List<MutationDef>();
			foreach (MorphDef morphDef in MorphDef.AllDefs)
				if (morphDef.race == pkDef.race || morphDef.associatedAnimals?.Contains(pkDef.race) == true)
					foreach (MutationDef mDef in morphDef.AllAssociatedMutations)
						if (!tmpList.Contains(mDef))
							tmpList.Add(mDef);

			_taggableMutationsLookup[pkDef] = tmpList;
			return tmpList;
		}

		/// <summary>
		///     Gets the required storage.
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

		/// <summary>
		///     Gets the required storage.
		/// </summary>
		/// <param name="pawnkindDef">The pawnkind definition.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">pawnkindDef</exception>
		public static int GetRequiredStorage([NotNull] this PawnKindDef pawnkindDef)
		{
			if (pawnkindDef == null) throw new ArgumentNullException(nameof(pawnkindDef));

			float l = Mathf.Sqrt(pawnkindDef.race.BaseMarketValue);
			float sP = PK_SPACE_M * l + PK_SPACE_B;
			return Mathf.Max(MIN_MUTATION_STORAGE_SPACE, Mathf.RoundToInt(sP));
		}

		/// <summary>
		///     Gets the storage string.
		/// </summary>
		/// <param name="storageAmount">The storage amount.</param>
		/// <returns></returns>
		public static string GetStorageString(float storageAmount)
		{
			var idx = 0;
			while (storageAmount > 1000f && idx < Suffixes.Length)
			{
				idx++;
				storageAmount /= 1000f;
			}

			// Round to 2 decimals
			storageAmount = Mathf.RoundToInt(storageAmount * 100) / 100;

			return storageAmount + Suffixes[idx];
		}

		/// <summary>
		///     Determines whether the specified definition for a chaomorph.
		/// </summary>
		/// <param name="def">The definition.</param>
		/// <returns>
		///     <c>true</c> if the specified definition is a chaomorph; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsChao(ThingDef def)
		{
			var chaoExt = def.GetModExtension<ChaomorphExtension>();
			return chaoExt != null;
		}


		/// <summary>
		///     Determines whether this instance is taggable.
		/// </summary>
		/// <param name="animalRace">The animal race.</param>
		/// <returns>
		///     <c>true</c> if the specified animal race is taggable; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTaggable([NotNull] this ThingDef animalRace)
		{
			if (animalRace == null) throw new ArgumentNullException(nameof(animalRace));
			var chao = animalRace.GetModExtension<ChaomorphExtension>();
			if (chao != null) return chao.taggable;
			return animalRace.IsValidAnimal();
		}

		/// <summary>
		///     Determines whether this instance is taggable.
		/// </summary>
		/// <param name="mutationDef">The mutation def.</param>
		/// <returns>
		///     <c>true</c> if the specified animal race is taggable; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsTaggable([NotNull] this MutationDef mutationDef)
		{
			if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
			return !mutationDef.IsRestricted;
		}


		/// <summary>
		///     Determines whether the specified morph is tagged.
		/// </summary>
		/// <param name="mDef">The m definition.</param>
		/// <returns>
		///     <c>true</c> if the specified morph is tagged; otherwise, <c>false</c>.
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
				if (cdB.TaggedAnimals.Contains(pawnKindDef))
					return true;

			return false;
		}

		/// <summary>
		///     Determines whether this instance is tagged.
		/// </summary>
		/// <param name="pkDef">The pk definition.</param>
		/// <returns>
		///     <c>true</c> if the specified pk definition is tagged; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">pkDef</exception>
		public static bool IsTagged([NotNull] this PawnKindDef pkDef)
		{
			if (pkDef == null) throw new ArgumentNullException(nameof(pkDef));
			return DB.TaggedAnimals.Contains(pkDef);
		}


		/// <summary>
		///     Determines whether this instance is tagged.
		/// </summary>
		/// <param name="mutationDef">The mutation definition.</param>
		/// <returns>
		///     <c>true</c> if the specified mutation definition is tagged; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">mutationDef</exception>
		public static bool IsTagged([NotNull] this MutationDef mutationDef)
		{
			if (mutationDef == null) throw new ArgumentNullException(nameof(mutationDef));
			var db = Find.World.GetComponent<ChamberDatabase>();
			return db?.StoredMutations.Contains(mutationDef) == true;
		}

		/// <summary>
		///     Determines whether this instance is the def of an animal that can be added to the chamber database
		/// </summary>
		/// <param name="inst">The inst.</param>
		/// <returns>
		///     <c>true</c> if this instance can be added to the chamber database ; otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">inst</exception>
		public static bool IsValidAnimal([NotNull] this ThingDef inst)
		{
			if (inst == null) throw new ArgumentNullException(nameof(inst));
			if (inst.IsValidFormerHuman() == false)
				return false;

			var chaoExt = inst.GetModExtension<ChaomorphExtension>();

			return chaoExt?.storable != false;
		}


		/// <summary>
		///     .returns an enumerable collection of all mutations that can be stored in the database
		/// </summary>
		/// <param name="mutationDefs">The mutation defs.</param>
		/// <returns></returns>
		[Pure]
		[NotNull]
		public static IEnumerable<MutationDef> Taggable([NotNull] this IEnumerable<MutationDef> mutationDefs)
		{
			return mutationDefs.Where(m => m.IsTaggable());
		}
	}
}