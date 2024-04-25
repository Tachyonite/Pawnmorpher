// AddMorphCategoryTfHediff.cs modified by Iron Wolf for Pawnmorph on 02/13/2020 5:53 PM
// last updated 02/13/2020  5:53 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph.IngestionEffects
{
	/// <summary>
	/// ingestion outcome doer that adds a tf hediff picked from a given morph category 
	/// </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public class AddMorphCategoryTfHediff : IngestionOutcomeDoer
	{
		/// <summary>
		/// The full tf
		/// </summary>
		public bool fullTf;
		/// <summary>
		/// The morph category
		/// </summary>
		public MorphCategoryDef morphCategory;
		/// <summary>
		/// The initial severity
		/// </summary>
		public float severity = 1;

		/// <summary>
		/// Whether or not to allow restricted morphs.
		/// </summary>
		public bool allowRestricted = false;

		[Unsaved] private List<HediffDef> _allHediffs;

		[NotNull]
		List<HediffDef> AllHediffs
		{
			get
			{
				if (_allHediffs == null)
				{
					if (morphCategory == null)
					{
						Log.Error($"{nameof(AddMorphCategoryTfHediff)} does not have it's {nameof(morphCategory)} field set!");
						_allHediffs = new List<HediffDef>();
					}
					else
					{
						// If morph is not restricted, serum permits restricted or category itself is restricted.
						_allHediffs =
							morphCategory.AllMorphsInCategories
										 .Where(m => !m.Restricted || allowRestricted || morphCategory.restricted)
										 .Select(m => fullTf
																				? m.fullTransformation
																				: m.partialTransformation)
										 .ToList();
					}
				}

				return _allHediffs;
			}
		}

		/// <summary>
		/// Does the special hediff effect 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			if (pawn?.health == null)
				return;

			var hediff = AllHediffs.RandElement();

			if (hediff == null)
			{
				Log.Error($"No morphs were found in category {morphCategory.defName}. Allow restricted: {allowRestricted}");
				return;
			}

			var h = pawn.health.AddHediff(hediff);
			h.Severity = severity;
		}
	}
}