using Pawnmorph.Chambers;
using Pawnmorph.UserInterface.PartPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.Genebank.Model
{
	internal class TemplateGenebankEntry : GenebankEntry<MutationTemplate>
	{
		public TemplateGenebankEntry() 
			: base(null)
		{
		}

		public TemplateGenebankEntry(MutationTemplate value) : base(value)
		{
		}

		public override bool CanAddToDatabase(ChamberDatabase database, out string reason)
		{
			reason = "";
			return true;
		}

		public override void ExposeData()
		{
			Scribe_Deep.Look(ref _value, "_value");
		}

		public override string GetCaption()
		{
			return _value.Caption;
		}

		public override int GetRequiredStorage()
		{
			return _value.GenebankSize;
		}
	}
}
