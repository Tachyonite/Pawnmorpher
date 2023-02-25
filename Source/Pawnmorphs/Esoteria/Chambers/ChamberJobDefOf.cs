// ChamberJobDefOf.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 10:24 AM
// last updated 08/26/2019  10:24 AM

using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph.Chambers
{
	/// <summary>
	/// def of for jobs related to mutagenic chambers 
	/// </summary>
	[DefOf]
	public class ChamberJobDefOf
	{
		static ChamberJobDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ChamberJobDefOf));
		}

		public static JobDef EnterMutagenChamber;
		public static JobDef CarryToMutagenChamber;
	}
}