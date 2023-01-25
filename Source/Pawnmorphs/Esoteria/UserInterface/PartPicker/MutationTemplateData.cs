using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.PartPicker
{
	public class MutationTemplateData : IExposable
	{
		private MutationDef _mutationDef;
		private string _partLabelCap;
		private float _severity;
		private bool _halted;

		public MutationDef MutationDef => _mutationDef;
		public string PartLabelCap => _partLabelCap;
		public float Severity => _severity;
		public bool Halted => _halted;

		public MutationTemplateData(MutationDef mutation, string partLabelCap, float severity, bool halted)
		{
			_mutationDef = mutation;
			_partLabelCap = partLabelCap;
			_severity = severity;
			_halted = halted;
		}
		public void ExposeData()
		{

			Scribe_Defs.Look(ref _mutationDef, nameof(MutationDef));
			Scribe_Values.Look(ref _partLabelCap, nameof(PartLabelCap));
			Scribe_Values.Look(ref _severity, nameof(Severity));
			Scribe_Values.Look(ref _halted, nameof(Halted));
		}
	}
}
