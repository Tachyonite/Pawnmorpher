// Giver_CheckRace.cs modified by Iron Wolf for Pawnmorph on 08/03/2019 6:03 PM
// last updated 08/03/2019  6:03 PM

using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// hediff component that checks the race of a pawn at the end of a Hediff_Morph 
	/// </summary>
	/// this is a component because it's set to go off just when a hediff_Morph ends naturally (after reeling) 
	public class Comp_CheckRace : HediffCompBase<CompProperties_CheckRace>
	{
		private bool _checked;

		/// <summary>
		/// called after the parent hediff is removed
		/// </summary>
		public override void CompPostPostRemoved()
		{
			base.CompPostPostRemoved();

			if (Pawn.Dead || _checked) return;

			Pawn.CheckRace(false);


		}

		private int _lastStage = -1;

		/// <summary>
		/// called every tick after the thing updates 
		/// </summary>
		/// <param name="severityAdjustment"></param>
		public override void CompPostTick(ref float severityAdjustment)
		{

			base.CompPostTick(ref severityAdjustment);
			if (parent.CurStageIndex != _lastStage)
			{
				_lastStage = parent.CurStageIndex;
				if (_lastStage == Props.triggerStage)
				{
					parent.pawn.CheckRace();
				}
			}
		}

		/// <summary>
		/// save or load data 
		/// </summary>
		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Values.Look(ref _lastStage, nameof(_lastStage), -1);
			Scribe_Values.Look(ref _checked, nameof(_checked), false);
		}
	}

	/// <summary>
	/// hediff comp that checks if the pawn should be turned into a hybrid at a certain stage 
	/// </summary>
	public class CompProperties_CheckRace : HediffCompProperties
	{
		/// <summary>
		/// the stage to check the pawns race at 
		/// </summary>
		public int triggerStage;

		/// <summary>
		/// 
		/// </summary>
		public CompProperties_CheckRace()
		{
			compClass = typeof(Comp_CheckRace);
		}
	}



}