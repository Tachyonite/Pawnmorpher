// PawnColumnWorker_RecruitSapientAnimal.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:16 PM
// last updated 03/15/2020  3:16 PM

using RimWorld;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="RimWorld.PawnColumnWorker_Designator" />
	public class PawnColumnWorker_RecruitSapientAnimal : PawnColumnWorker_Designator
	{
		/// <summary>
		/// Gets the type of the designation.
		/// </summary>
		/// <value>
		/// The type of the designation.
		/// </value>
		protected override DesignationDef DesignationType => PMDesignationDefOf.RecruitSapientFormerHuman;

		/// <summary>
		/// Gets the tip.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		protected override string GetTip(Pawn pawn)
		{
			return "DesignatorTameDesc".Translate();
		}

		/// <summary>
		/// Determines whether the specified pawn has checkbox.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns>
		///   <c>true</c> if the specified pawn has checkbox; otherwise, <c>false</c>.
		/// </returns>
		protected override bool HasCheckbox(Pawn pawn)
		{
			if (pawn.IsSapientFormerHuman() && pawn.RaceProps.IsFlesh && (pawn.Faction == null || !pawn.Faction.def.humanlikeFaction))
			{
				return pawn.SpawnedOrAnyParentSpawned;
			}
			return false;
		}

		/// <summary>
		/// Notifies the designation added.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		protected override void Notify_DesignationAdded(Pawn pawn)
		{
			pawn.MapHeld.designationManager.TryRemoveDesignationOn(pawn, DesignationDefOf.Hunt);
			//TameUtility.ShowDesignationWarnings(pawn, showManhunterOnTameFailWarning: false);
		}
	}
}