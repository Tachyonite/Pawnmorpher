using Pawnmorph.Chambers;
using Pawnmorph.DefExtensions;
using Verse;

namespace Pawnmorph.Genebank.Model
{
	internal class AnimalGenebankEntry : GenebankEntry<PawnKindDef>
	{
		/// <summary>
		///     translation label for the animal not taggable reason 
		/// </summary>
		public const string ANIMAL_TOO_CHAOTIC_REASON = "AnimalNotTaggable";
		private const string NOT_VALID_ANIMAL_REASON = "NotValidAnimal";

		public AnimalGenebankEntry()
			: base(null)
		{

		}


		public AnimalGenebankEntry(PawnKindDef value) : base(value)
		{
		}

		public override bool CanAddToDatabase(ChamberDatabase database, out string reason)
		{
			if (DatabaseUtilities.IsChao(_value.race))
			{
				reason = ANIMAL_TOO_CHAOTIC_REASON.Translate(_value);
				return false;
			}

			if (!_value.race.IsValidAnimal())
			{
				reason = NOT_VALID_ANIMAL_REASON.Translate(_value);
				return false;
			}

			reason = "";
			return true;
		}

		public override string GetCaption()
		{
			AnimalSelectorOverrides overrides = Value.GetModExtension<AnimalSelectorOverrides>();
			if (overrides != null && string.IsNullOrWhiteSpace(overrides.label) == false)
				return overrides.label;

			return _value.LabelCap;
		}

		public override int GetRequiredStorage()
		{
			return _value.GetRequiredStorage();
		}

		public override void ExposeData()
		{
			Scribe_Defs.Look(ref _value, "_value");
		}
	}
}
