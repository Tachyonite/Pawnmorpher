// MergedPawn.cs created by Iron Wolf for Pawnmorph on 05/08/2020 10:33 AM
// last updated 05/08/2020  10:33 AM

using Verse;

namespace Pawnmorph.SapienceStates
{
	/// <summary>
	/// sapience stat for a merged pawn 
	/// </summary>
	/// <seealso cref="Pawnmorph.SapienceStates.FormerHuman" />
	public class MergedPawn : FormerHuman
	{
		/// <summary>
		///     Initializes this instance.
		/// </summary>
		/// this is always called before enter and after loading a pawn
		protected override void Init()
		{
			base.Init();

			Pawn_HealthTracker health = Pawn?.health;
			HediffSet hDiffs = health?.hediffSet;
			if (health == null || hDiffs == null) return;
			//fix for old saves 
			HediffDef hDef = SapienceStateDefOf.FormerHuman.forcedHediff;
			Hediff hDiff = hDiffs.GetFirstHediffOfDef(hDef);
			if (hDiff != null) health.RemoveHediff(hDiff);
		}
	}
}