using System.Collections.Generic;
using Verse;

namespace Pawnmorph
{
	/// <summary>Properties for the remove hediff comp </summary>
	/// <seealso cref="Verse.HediffCompProperties" />
	public class HediffCompProperties_Remove : HediffCompProperties
	{
		/// <summary>
		/// a list of hediffs to remove and make the pawn immune to 
		/// </summary>
		public List<HediffDef> makeImmuneTo;
		/// <summary>
		/// Initializes a new instance of the <see cref="HediffCompProperties_Remove"/> class.
		/// </summary>
		public HediffCompProperties_Remove()
		{
			compClass = typeof(HediffComp_Remove);
		}
	}
}
