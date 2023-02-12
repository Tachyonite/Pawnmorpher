// MutantPlantExtension.cs modified by Iron Wolf for Pawnmorph on 12/14/2019 1:19 PM
// last updated 12/14/2019  1:19 PM

using System;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.DefExtensions
{
	/// <summary>
	/// mod extension for plants that come from mutating other plants 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	public class MutantPlantExtension : DefModExtension
	{
		/// <summary>
		/// if true, then the attached mutant plant will not be swapped for regular plants 
		/// </summary>
		public bool ignore;

		/// <summary>
		/// The priority
		/// </summary>
		/// this is used to determine what order the plants are checked,
		/// lower priority is checked first, then higher. mutagenic plants with the same priority are 'shuffled'  
		public int priority = 0;

		/// <summary>
		/// The source plant filter
		/// </summary>
		public Filter<ThingDef> sourcePlantFilter = new Filter<ThingDef>();

		/// <summary>
		/// if true, the source plant must be harvestable for the mutation to occur
		/// </summary>
		public bool mustBeHarvestable;

		/// <summary>
		/// if true, the source plant must be a tree 
		/// </summary>
		public bool mustBeTree;

		/// <summary>
		/// Determines whether this instance can mutate from the specified source plant  
		/// </summary>
		/// <param name="sourcePlant">The source plant.</param>
		/// <returns>
		///   <c>true</c> if this this instance can mutate from the specified source plant    otherwise, <c>false</c>.
		/// </returns>
		/// <exception cref="ArgumentNullException">sourcePlant</exception>
		public bool CanMutateFrom([NotNull] Plant sourcePlant)
		{
			if (sourcePlant == null) throw new ArgumentNullException(nameof(sourcePlant));

			if (!sourcePlantFilter.PassesFilter(sourcePlant.def)) return false;
			if (mustBeHarvestable && !sourcePlant.def.plant.Harvestable) return false;
			if (mustBeTree && !sourcePlant.def.plant.IsTree) return false;
			return true;
		}

	}
}