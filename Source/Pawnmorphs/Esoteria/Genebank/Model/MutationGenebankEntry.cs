using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Verse;

namespace Pawnmorph.Genebank.Model
{
	internal class MutationGenebankEntry : GenebankEntry<MutationDef>
	{
		public MutationGenebankEntry()
			: base(null)
		{

		}

		public MutationGenebankEntry(MutationDef value) : base(value)
		{
		}

		public override bool CanAddToDatabase(ChamberDatabase database, out string reason)
		{
			reason = "";
			return true;
		}

		public override void ExposeData()
		{
			Scribe_Defs.Look(ref _value, "_value");
		}

		public override string GetCaption()
		{
			return _value.LabelCap;
		}

		public override int GetRequiredStorage()
		{
			return _value.GetRequiredStorage();
		}
	}
}
