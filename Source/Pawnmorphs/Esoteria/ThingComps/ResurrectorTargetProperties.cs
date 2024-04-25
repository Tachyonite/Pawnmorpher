// RessurrectorProperties.cs created by Iron Wolf for Pawnmorph on 10/17/2020 10:27 AM
// last updated 10/17/2020  10:27 AM

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Pawnmorph.Hybrids;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.ThingComps
{
	/// <summary>
	/// properties for <see cref="ResurrectorTargetComp"/>
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class ResurrectorTargetProperties : CompProperties
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ResurrectorTargetProperties"/> class.
		/// </summary>
		public ResurrectorTargetProperties()
		{
			compClass = typeof(ResurrectorTargetComp);
		}


		/// <summary>
		/// filter for specific animals 
		/// </summary>
		[CanBeNull]
		public Filter<ThingDef> animalFilter;

		/// <summary>
		/// The morph category filter
		/// </summary>
		[CanBeNull]
		public Filter<MorphCategoryDef> morphCategoryFilter;

		/// <summary>
		/// The forced sapience level
		/// </summary>
		public SapienceLevel? forcedSapienceLevel;

		/// <summary>
		/// if true, check if the tf target is a morph, and fully tf them into the animal 
		/// </summary>
		public bool checkForMorphFirst;

		/// <summary>
		/// The chaomorph settings
		/// </summary>
		public ChaomorphSetting chaomorphSetting = ChaomorphSetting.Allowed;

		/// <summary>
		/// chaomorph settings enum 
		/// </summary>
		public enum ChaomorphSetting
		{
			/// <summary>
			/// no chaomorphs will be chosen 
			/// </summary>
			None,
			/// <summary>
			/// chaomorphs will be allowed if they satisfy the other filters/parameters 
			/// </summary>
			Allowed,
			/// <summary>
			/// only chaomorphs will be allowed 
			/// </summary>
			Only
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString()
		{
			return $"{nameof(checkForMorphFirst)}: {checkForMorphFirst}\n{nameof(chaomorphSetting)}:{chaomorphSetting}"; //add in filter info 
		}
	}

	/// <summary>
	/// comp for giving valid animals for the resurrector serum 
	/// </summary>
	/// <seealso cref="Verse.ThingComp" />
	public class ResurrectorTargetComp : ThingComp
	{
		private ResurrectorTargetProperties Props => (ResurrectorTargetProperties)props;


		/// <summary>
		/// Gets the forced sapience level.
		/// </summary>
		/// <value>
		/// The forced sapience level.
		/// </value>
		public SapienceLevel? ForcedSapienceLevel => Props?.forcedSapienceLevel;

		[NotNull]
		private static readonly List<PawnKindDef> _scratchList = new List<PawnKindDef>();

		/// <summary>
		/// Gets the valid animal to turn the target into 
		/// </summary>
		/// <param name="target">The target.</param>
		/// <returns></returns>
		[NotNull]
		public PawnKindDef GetValidAnimalFor([NotNull] Pawn target)
		{
			if (Props.checkForMorphFirst)
			{
				var morph = target.def.GetMorphOfRace();
				if (morph != null)
				{
					try
					{
						return DefDatabase<PawnKindDef>.AllDefs.First(pk => pk.race == morph.race);
					}
					catch (Exception e)
					{
						Log.Error($"caught {e.GetType().Name} while getting animal for morph {morph.defName}!\n{e}");
						return PawnKindDef.Named("Wolf_Timber");
					}
				}
			}


			_scratchList.Clear();
			_scratchList.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(pk => pk.race != null && IsValidAnimal(pk.race)));

			if (_scratchList.Count == 0)
			{
				Log.Error($"unable to find a valid animal for {target.Label} using \n{Props}\nreturning fallback pawn kind");
				return PawnKindDef.Named("Wolf_Timber");
			}

			return _scratchList.RandomElement();
		}

		bool IsValidAnimal([NotNull] ThingDef race)
		{
			var p = Props;
			if (race.race?.Animal != true) return false;
			if (p?.animalFilter?.PassesFilter(race) == false) return false;

			Filter<MorphCategoryDef> morphCatFilter = p?.morphCategoryFilter;
			if (morphCatFilter != null)
			{
				var morph = race.TryGetBestMorphOfAnimal();
				if (!morphCatFilter.isBlackList && morph?.categories == null) return false; //if it's a white list this must be an animal associated with a morph 
				if (morphCatFilter.isBlackList && morph?.categories == null) return true;
				if (!morph.categories.Any(c => !morphCatFilter.PassesFilter(c))) return false;
			}

			var chaoSettings = p.chaomorphSetting;
			var isChaomorph = race.GetModExtension<ChaomorphExtension>() != null;

			switch (chaoSettings)
			{
				case ResurrectorTargetProperties.ChaomorphSetting.None:
					return !isChaomorph;
				case ResurrectorTargetProperties.ChaomorphSetting.Allowed:
					return true; //if we got to this point everything else passed 
				case ResurrectorTargetProperties.ChaomorphSetting.Only:
					return isChaomorph;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

	}
}