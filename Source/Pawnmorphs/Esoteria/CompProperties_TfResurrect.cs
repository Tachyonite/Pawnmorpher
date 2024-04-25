// CompProperties_TfResurrect.cs modified by Iron Wolf for Pawnmorph on //2019 
// last updated 11/02/2019  12:22 PM

using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	///     properties for the Tf Resurrect effect
	/// </summary>
	/// <seealso cref="Verse.CompProperties" />
	public class CompProperties_TfResurrect : CompProperties
	{
		/// <summary>
		///     if true, the resurrector will always make the resultant animal permanently feral
		/// </summary>
		public bool makePermanentlyFeral;

		/// <summary>
		///     an optional filter to restrict the kinds of animals the resurrector will turn dead pawns into
		/// </summary>
		public Filter<PawnKindDef> animalFilter;

		/// <summary>
		///     The gender tf options for the resurrector
		/// </summary>
		public TFGender genderTf;

		/// <summary>
		///     The tale definition to add to the tale database when the pawn is resurrected
		/// </summary>
		public TaleDef taleDef;

		[Unsaved] private ThingDef _parentDef;

		[Unsaved] private List<PawnKindDef> _animals;

		/// <summary>
		///     Initializes a new instance of the <see cref="CompProperties_TfResurrect" /> class.
		/// </summary>
		public CompProperties_TfResurrect()
		{
			compClass = typeof(CompTargetEffect_TfResurrect);
		}

		/// <summary>
		///     list of animals that the pawn can be transformed into
		/// </summary>
		public List<PawnKindDef> Animals
		{
			get
			{
				if (_animals == null)
				{
					_animals = DefDatabase<PawnKindDef>.AllDefs.Where(d => d.race.race.IsFlesh && d.race.race.Animal)
													   .Where(d => animalFilter?.PassesFilter(d) ?? true)
													   .ToList();
					if (_animals.Count == 0)
						Log.Error($"{_parentDef.defName}'s {nameof(CompProperties_TfResurrect)} could not find any animals to transform a pawn into, filter is too strict");
				}

				return _animals;
			}
		}

		/// <summary>
		///     gets all configuration errors with this instance
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef)) yield return configError;

			_parentDef = parentDef;
		}
	}
}