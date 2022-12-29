using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.UserInterface.PartPicker
{
	internal class MutationTemplate : IExposable
	{
		public const int GENEBANK_COST_PER_MUTATION = 1;

		List<MutationData> _mutationData;
		string _caption;


		public string Caption => _caption;
		public IEnumerable<IReadOnlyMutationData> MutationData => _mutationData;
		public int GenebankSize { get; private set; }


		public MutationTemplate()
		{
			_mutationData = new List<MutationData>();
			GenebankSize = 0;
		}

		public MutationTemplate(IEnumerable<MutationData> mutationData, string caption)
		{
			_caption = caption;
			_mutationData = mutationData.ToList();
			InvalidateMutationData();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref _mutationData, nameof(_mutationData));
			Scribe_Values.Look(ref _caption, nameof(_caption));


			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				InvalidateMutationData();
			}
		}

		private void InvalidateMutationData()
		{
			GenebankSize = GENEBANK_COST_PER_MUTATION * _mutationData.Count;

		}
	}
}
