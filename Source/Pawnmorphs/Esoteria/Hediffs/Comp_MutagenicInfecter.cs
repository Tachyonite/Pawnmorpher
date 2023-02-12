// Comp_MutagenicInfecter.cs created by Iron Wolf for Pawnmorph on 02/07/2021 3:07 PM
// last updated 02/07/2021  3:07 PM

using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// </summary>
	/// <seealso cref="Verse.HediffComp_Infecter" />
	public class Comp_MutagenicInfecter : HediffComp
	{
		private bool _shouldRemove;
		/// <summary>
		/// Gets a value indicating whether the parent should be removed 
		/// </summary>
		/// <value>
		///   <c>true</c> if [comp should remove]; otherwise, <c>false</c>.
		/// </value>
		public override bool CompShouldRemove => _shouldRemove;


		/// <summary>
		/// Comps the post post add.
		/// </summary>
		/// <param name="dinfo">The dinfo.</param>
		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);

			if (!PMUtilities.MutagenicDiseasesEnabled) return;

			var hasBuildup = Pawn.health?.hediffSet?.hediffs?.Any(h => h is MutagenicBuildup) == true;
			if (!hasBuildup) return;

			_shouldRemove = true;
			var hDef = HediffMaker.MakeHediff(MorphTransformationDefOf.PM_MutagenicInfection, Pawn, parent.Part);
			Pawn.health.AddHediff(hDef);



		}


		/// <summary>
		/// save and load data
		/// </summary>
		public override void CompExposeData()
		{
			base.CompExposeData();

			Scribe_Values.Look(ref _shouldRemove, "shouldRemove");
		}
	}

	/// <summary>
	/// </summary>
	/// <seealso cref="Verse.HediffCompProperties_Infecter" />
	public class CompProps_MutagenicInfecter : HediffCompProperties
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="CompProps_MutagenicInfecter" /> class.
		/// </summary>
		public CompProps_MutagenicInfecter()
		{
			compClass = typeof(Comp_MutagenicInfecter);
		}
	}
}