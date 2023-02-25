using Pawnmorph.Chambers;
using Pawnmorph.UserInterface.PartPicker;
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
