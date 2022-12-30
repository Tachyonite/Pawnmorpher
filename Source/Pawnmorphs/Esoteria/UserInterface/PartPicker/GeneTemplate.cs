using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		List<MutationData> _mutationData;
		string _caption;

		/// <summary>
		/// Gets the template caption.
		/// </summary>
		public string Caption => _caption;

		/// <summary>
		/// Gets the template mutations data.
		/// </summary>
		public IEnumerable<IReadOnlyMutationData> MutationData => _mutationData;

		/// <summary>
		/// Gets the size of the template when stored in the genebank.
		/// </summary>
		public int GenebankSize { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationTemplate"/> class.
		/// </summary>
		public MutationTemplate()
		{
			_mutationData = new List<MutationData>();
			GenebankSize = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MutationTemplate"/> class.
		/// </summary>
		/// <param name="mutationData">Collection of mutations to include in template.</param>
		/// <param name="caption">The caption of the template.</param>
		public MutationTemplate(IEnumerable<MutationData> mutationData, string caption)
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
	}
}
