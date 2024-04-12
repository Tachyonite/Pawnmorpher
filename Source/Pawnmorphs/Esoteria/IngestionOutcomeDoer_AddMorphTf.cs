// IngestionOutcomeDooer_AddClassMorphTf.cs modified by Iron Wolf for Pawnmorph on 01/13/2020 6:35 PM
// last updated 01/13/2020  6:35 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// ingestion outcome doer for adding morph tf based on a specific morph, or class of morphs 
	/// </summary>
	/// <seealso cref="RimWorld.IngestionOutcomeDoer" />
	public class IngestionOutcomeDoer_AddMorphTf : IngestionOutcomeDoer
	{
		/// <summary>
		/// The animal class or specific morph to pick the morph tf from 
		/// </summary>
		public AnimalClassBase animalClass;

		/// <summary>
		/// if this instance can pick restricted morphs
		/// </summary>
		public bool allowRestrictedMorphs;

		/// <summary>
		/// The severity to add the hediff at 
		/// </summary>
		public float severity = 1;

		/// <summary>
		/// The tf types to pick from 
		/// </summary>
		/// this is a flag so you can pick both Full and partial if you want
		public MorphTransformationTypes tfTypes = MorphTransformationTypes.Full;

		/// <summary>
		/// The category filter
		/// </summary>
		public Filter<MorphCategoryDef> categoryFilter;

		[Unsaved] private List<HediffDef> _tfDefs;

		[NotNull]
		IEnumerable<HediffDef> TfDefs
		{
			get
			{
				if (_tfDefs == null)
				{
					_tfDefs = new List<HediffDef>();

					foreach (MorphDef morphDef in animalClass.GetAllMorphsInClass().Where(ValidMorph))
					{
						if (morphDef.fullTransformation != null && (tfTypes & MorphTransformationTypes.Full) != 0)
						{
							if (!_tfDefs.Contains(morphDef.fullTransformation))
								_tfDefs.Add(morphDef.fullTransformation);
						}

						if (morphDef.partialTransformation != null && (tfTypes & MorphTransformationTypes.Partial) != 0)
						{
							if (!_tfDefs.Contains(morphDef.partialTransformation))
							{
								_tfDefs.Add(morphDef.partialTransformation);
							}
						}
					}

				}

				return _tfDefs;
			}
		}

		private bool ValidMorph(MorphDef m)
		{
			if (!allowRestrictedMorphs && m.Restricted) return false;
			if (categoryFilter != null)
			{
				foreach (MorphCategoryDef category in m.Categories)
				{
					if (!categoryFilter.PassesFilter(category)) return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Does the ingestion outcome special.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="ingested">The ingested.</param>
		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int count)
		{
			var tf = TfDefs.RandomElementWithFallback();
			if (tf != null)
			{
				var hediff = HediffMaker.MakeHediff(tf, pawn);
				pawn.health.AddHediff(hediff);
				hediff.Severity = severity;
			}
		}
	}
}