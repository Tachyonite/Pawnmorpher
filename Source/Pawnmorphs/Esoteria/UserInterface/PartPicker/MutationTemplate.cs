using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph.UserInterface.PartPicker
{
	/// <summary>
	/// A template of mutations for part picker.
	/// </summary>
	/// <seealso cref="Verse.IExposable" />
	public class MutationTemplate : IExposable
	{
		/// <summary>
		/// The genebank capacity cost per mutation in template.
		/// </summary>
		public const int GENEBANK_COST_PER_MUTATION = 1;

		List<MutationTemplateData> _mutationData;
		string _caption;

		/// <summary>
		/// Gets the template caption.
		/// </summary>
		public string Caption => _caption;

		/// <summary>
		/// Gets the template mutations data.
		/// </summary>
		public IEnumerable<MutationTemplateData> MutationData => _mutationData;

		/// <summary>
		/// Gets the size of the template when stored in the genebank.
		/// </summary>
		public int GenebankSize { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationTemplate"/> class.
		/// </summary>
		public MutationTemplate()
		{
			_mutationData = new List<MutationTemplateData>();
			GenebankSize = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationTemplate"/> class.
		/// </summary>
		/// <param name="mutationData">Collection of mutations to include in template.</param>
		/// <param name="caption">The caption of the template.</param>
		public MutationTemplate(IEnumerable<MutationTemplateData> mutationData, string caption)
		{
			_caption = caption;
			_mutationData = mutationData.ToList();
			InvalidateMutationData();
		}

		/// <summary>
		/// Save/Load data
		/// </summary>
		public void ExposeData()
		{
			Scribe_Collections.Look(ref _mutationData, nameof(_mutationData));
			Scribe_Values.Look(ref _caption, nameof(_caption));


			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				InvalidateMutationData();
			}
		}

		/// <summary>
		/// Invalidates caches related to mutation data.
		/// </summary>
		private void InvalidateMutationData()
		{
			GenebankSize = GENEBANK_COST_PER_MUTATION * _mutationData.Count;

		}

		/// <summary>
		/// Returns a string describing all the mutations in this template  
		/// </summary>
		/// <returns>A string describing the template</returns>
		public string Serialize()
		{
			// MutationDef, Part name, severity, halted.
			// Name:["","",0,0][][]
			StringBuilder builder = new StringBuilder();
			builder.Append(_caption);

			foreach (MutationTemplateData data in _mutationData)
			{
				builder.Append("[");
				builder.Append(data.MutationDef.defName);
				builder.Append(",");
				builder.Append(data.PartLabelCap);
				builder.Append(",");
				builder.Append(data.Severity);
				builder.Append(",");
				builder.Append(data.Halted);
				builder.Append("]");
			}

			return builder.ToString();
		}

		/// <summary>
		/// Attempts to deserializes the specified text.
		/// </summary>
		/// <param name="text">Text of a serialized mutation template.</param>
		/// <param name="template">The deserialized mutation template.</param>
		/// <returns></returns>
		public static bool TryDeserialize(string text, out MutationTemplate template)
		{
			template = null;
			text = text.Trim();
			if (String.IsNullOrWhiteSpace(text))
				return false;


			int startIndex = 0;
			int endIndex = text.IndexOf("[");

			if (endIndex == -1)
				return false;

			try
			{
				string caption = text.Substring(startIndex, endIndex);
				List<MutationTemplateData> mutationData = new List<MutationTemplateData>();
				do
				{
					startIndex = text.IndexOf("[", endIndex) + 1;
					endIndex = text.IndexOf("]", startIndex);
					string data = text.Substring(startIndex, endIndex - startIndex);
					string[] dataParts = data.Split(',');

					if (dataParts.Length != 4)
						return false;

					MutationDef mutation = DefDatabase<MutationDef>.GetNamedSilentFail(dataParts[0]);
					if (mutation == null)
						continue;

					string partLabelCap = dataParts[1];

					if (float.TryParse(dataParts[2], out float severity) == false)
						continue;


					if (bool.TryParse(dataParts[3], out bool halted) == false)
						continue;


					mutationData.Add(new MutationTemplateData(mutation, partLabelCap, severity, halted));
				}
				while (endIndex < text.Length - 2);

				template = new MutationTemplate(mutationData, caption);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
