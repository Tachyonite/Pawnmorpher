using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Class for all hediff stages that full transformations.
	/// Any components defined in this class will override the equivalent component
	/// in the parent hediff
	/// </summary>
	/// <seealso cref="Verse.HediffStage" />
	/// <seealso cref="Pawnmorph.Hediffs.IDescriptiveStage" />
	/// <seealso cref="Pawnmorph.Hediffs.Hediff_MutagenicBase" />
	public class HediffStage_Transformation : HediffStage_MutagenicBase
	{
		/// <summary>
		/// Controls the chance of a full transformation
		/// </summary>
		[UsedImplicitly, NotNull] public TFChance tfChance;

		/// <summary>
		/// Controls what kind of animals transformations can result in
		/// </summary>
		[UsedImplicitly, NotNull] public TFTypes tfTypes;

		/// <summary>
		/// Controls the gender of the post-transformation pawn
		/// </summary>
		[UsedImplicitly, NotNull] public TFGenderSelector tfGenderSelector;

		/// <summary>
		/// Controls miscellaneous settings related to full transformations
		/// </summary>
		[UsedImplicitly, NotNull] public TFMiscSettings tfSettings = new TFMiscSettings();

		/// <summary>
		/// Callbacks called on the transformed pawn to perform additional behavior
		/// </summary>
		[UsedImplicitly, CanBeNull] public List<TFCallback> tfCallbacks;

		/// <summary>
		/// Returns a debug string displayed when inspecting hediffs in dev mode
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public string DebugString(Hediff_MutagenicBase hediff)
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.AppendLine("--" + tfChance.GetType().Name);
			string text = tfChance.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			stringBuilder.AppendLine("--" + tfTypes.GetType().Name);
			text = tfTypes.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			stringBuilder.AppendLine("--" + tfGenderSelector.GetType().Name);
			text = tfGenderSelector.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			stringBuilder.AppendLine("--" + tfSettings.GetType().Name);
			text = tfSettings.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			//TODO
			//stringBuilder.AppendLine("--" + tfCallbacks);
			//text = tfCallbacks.DebugString(hediff);
			//if (!text.NullOrEmpty())
			//stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			return stringBuilder.ToString();
		}

		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			List<string> tmpList = new List<string>();
			foreach (string configError in base.ConfigErrors(parentDef))
			{
				yield return configError;
			}
			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// ReSharper disable once HeuristicUnreachableCode
			if (tfChance == null) yield return $"{nameof(tfChance)} has not been set";

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// ReSharper disable once HeuristicUnreachableCode
			if (tfTypes == null) yield return $"{nameof(tfTypes)} has not been set";

			// ReSharper disable once ConditionIsAlwaysTrueOrFalse
			// ReSharper disable once HeuristicUnreachableCode
			if (tfGenderSelector == null) yield return $"{nameof(tfGenderSelector)} has not been set";


			var enumerable = GenerateSubErrors(tfChance, nameof(tfChance));
			enumerable = enumerable.Concat(GenerateSubErrors(tfTypes, nameof(tfTypes)))
								   .Concat(GenerateSubErrors(tfGenderSelector, nameof(tfGenderSelector)));

			foreach (string error in enumerable)
			{
				yield return error;
			}

			IEnumerable<string> GenerateSubErrors(IInitializableStage stage, string stageName)
			{
				if (stage == null) yield break;
				tmpList.Clear();
				foreach (string configError in stage.ConfigErrors(parentDef))
				{
					tmpList.Add(configError);
				}

				if (tmpList.Count > 0)
				{
					yield return $"in {stageName}:";
				}

				foreach (string s in tmpList)
				{
					yield return "\t" + s;
				}
			}

		}

	}
}