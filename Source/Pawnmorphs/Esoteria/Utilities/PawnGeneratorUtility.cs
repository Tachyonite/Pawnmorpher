using RimWorld;
using Verse;

namespace Pawnmorph.Utilities
{
	internal static class PawnGeneratorUtility
	{
		public static Pawn GenerateAnimal(PawnKindDef kind, Faction faction = null)
		{
			;
			Pawn pawn = PawnGenerator.GeneratePawn(kind, faction);


			float minimumAnimalAge = TransformerUtility.ConvertAge(ThingDefOf.Human.race, kind.RaceProps, 17);
			float ageOffset = minimumAnimalAge - pawn.ageTracker.AgeBiologicalYearsFloat;
			if (ageOffset > 0)
			{
				long offsetTicks = (long)(ageOffset * 3600000L);
				pawn.ageTracker.AgeBiologicalTicks += offsetTicks;
				pawn.ageTracker.AgeChronologicalTicks += offsetTicks;
			}

			return pawn;
		}
	}
}
