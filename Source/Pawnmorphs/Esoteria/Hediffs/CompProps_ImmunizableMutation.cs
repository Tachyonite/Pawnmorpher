// CompProps_TFImmunizable.cs modified by Iron Wolf for Pawnmorph on 01/23/2020 6:07 PM
// last updated 01/23/2020  6:07 PM

using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.Hediffs
{
	public class CompProps_ImmunizableMutation : HediffCompProperties_Immunizable
	{
		public CompProps_ImmunizableMutation()
		{
			compClass = typeof(Comp_ImmunizableMutation);
		}
	}

	public class Comp_ImmunizableMutation : HediffComp_Immunizable
	{
		/// <summary>
		/// gets the severity change per day 
		/// </summary>
		/// <returns></returns>
		public override float SeverityChangePerDay()
		{
			var mult = Pawn.GetStatValue(PMStatDefOf.MutagenSensitivity);
			var baseVal = base.SeverityChangePerDay();
			if (baseVal > 0) baseVal *= mult; //only have it boost the severity gain when the mutations are getting worse 
			return baseVal;

		}
	}
}