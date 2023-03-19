using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pawnmorph.Hediffs.Composable;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// Class for handling all hediffs that cause mutations and transformation
	/// Any components defined in this class will override the equivalent component
	/// in the parent hediff
	/// </summary>
	/// <seealso cref="Pawnmorph.IDescriptiveHediff" />
	/// <seealso cref="Verse.Hediff" />
	public class HediffStage_Mutation : HediffStage_MutagenicBase
	{
		//cna spreadOrder, mutationRate and/or mutationType ever be null?

		/// <summary>
		/// Controls the order that mutations spread over the body
		/// </summary>
		[UsedImplicitly] public MutSpreadOrder spreadOrder;

		/// <summary>
		/// Controls how fast mutations are added
		/// </summary>
		[UsedImplicitly] public MutRate mutationRate;

		/// <summary>
		/// Controls what kinds of mutations can be added
		/// </summary>
		[UsedImplicitly] public MutTypes mutationTypes;

		/// <summary>
		/// Returns a debug string displayed when inspecting hediffs in dev mode
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public string DebugString(Hediff_MutagenicBase hediff)
		{
			StringBuilder stringBuilder = new StringBuilder();

			stringBuilder.AppendLine("--" + spreadOrder.GetType().Name);
			string text = spreadOrder.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			stringBuilder.AppendLine("--" + mutationRate.GetType().Name);
			text = mutationRate.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

			stringBuilder.AppendLine("--" + mutationTypes.GetType().Name);
			text = mutationTypes.DebugString(hediff);
			if (!text.NullOrEmpty())
				stringBuilder.AppendLine(text.TrimEndNewlines().Indented("  "));

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

			if (mutationRate == null && mutationTypes == null && spreadOrder == null)
				yield return $"none of {nameof(mutationRate)}, {nameof(mutationTypes)}, or {spreadOrder} are set";


			if (mutationRate == null)
			{
				yield return $"{nameof(mutationRate)} is not defined";
			}

			var enumerable = GenerateSubErrors(mutationRate, nameof(mutationRate))
							.Concat(GenerateSubErrors(mutationTypes, nameof(mutationTypes)))
							.Concat(GenerateSubErrors(spreadOrder, nameof(spreadOrder)));
			foreach (string s in enumerable)
			{
				yield return s;
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