using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// A class for hediff with description tooltips.  Used as a base for all
	/// Pawnmorpher hediffs, but also usable by itself if you just want to add
	/// custom description tooltips/label overrides to a hediff.
	/// </summary>
	public class Hediff_Descriptive : HediffWithComps, IDescriptiveHediff
	{
		[NotNull] private StringBuilder _descriptionBuilder = new StringBuilder();
		[NotNull] private readonly Dictionary<int, string> _descCache = new Dictionary<int, string>();

		/// <summary>
		/// Controls the description tooltip rendered by Pawnmorpher.
		/// </summary>
		/// <value>
		/// The tooltip description.
		/// </value>
		public override string Description
		{
			get
			{
				_descriptionBuilder.Clear();

				string description;
				if (!_descCache.TryGetValue(CurStageIndex, out description))
				{
					if (CurStage is IDescriptiveStage dStage && !dStage.DescriptionOverride.NullOrEmpty())
						description = dStage.DescriptionOverride;
					else
						description = def.description;

					if (String.IsNullOrEmpty(description))
						_descriptionBuilder.AppendLine("PawnmorphTooltipNoDescription".Translate());
					else
						_descriptionBuilder.AppendLine(description.AdjustedFor(pawn));

					_descCache[CurStageIndex] = _descriptionBuilder.ToString();
				}
				else
					_descriptionBuilder.AppendLine(description);

				// Append component description
				HediffComp_Production productionComp = this.TryGetComp<HediffComp_Production>();
				if (productionComp != null && productionComp.CurStage != null)
				{
					_descriptionBuilder.AppendLine(productionComp.GetDescription());
				}

				return _descriptionBuilder.ToString();
			}
		}

		/// <summary>
		/// Controls the base portion of the label (the part not in parentheses)
		/// </summary>
		/// <value>The base label.</value>
		public override string LabelBase
		{
			get
			{
				if (CurStage is IDescriptiveStage dStage && !dStage.LabelOverride.NullOrEmpty())
					return dStage.LabelOverride;

				return base.LabelBase;
			}
		}
	}
}
