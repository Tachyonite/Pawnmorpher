// FormerHumanMemory.cs created by Iron Wolf for Pawnmorph on 12/12/2019 11:28 AM
// last updated 12/12/2019  11:28 AM

using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.Thoughts
{
	/// <summary>
	/// memory who's stage depends on the former human status of the pawn 
	/// </summary>
	/// <seealso cref="RimWorld.Thought_Memory" />
	public class FormerHumanMemory : Thought_Memory
	{
		private string _cachedLabel;

		private Pawn _cachedPawn;

		/// <summary>
		/// save/load data.
		/// </summary>
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref _cachedLabel, nameof(LabelCap));
			Scribe_References.Look(ref _cachedPawn, nameof(_cachedPawn));
		}

		/// <summary>
		/// Gets the label cap.
		/// </summary>
		/// <value>
		/// The label cap.
		/// </value>
		public override string LabelCap
		{
			get
			{
				if (string.IsNullOrEmpty(_cachedLabel) || _cachedPawn != pawn)
				{
					_cachedPawn = pawn; //memories can sometimes be re assigned? 
					_cachedLabel = GenerateLabel();
				}

				return _cachedLabel;
			}
		}

		/// <summary>
		/// Generates the label for this thought.
		/// </summary>
		/// <returns></returns>
		protected virtual string GenerateLabel()
		{
			if (string.IsNullOrEmpty(CurStage.label)) return "";
			return CurStage.label.Formatted(pawn.kindDef.label.Named("animal")).CapitalizeFirst();
		}

		/// <summary>Gets the index of the current stage.</summary>
		/// <value>The index of the current stage.</value>
		public override int CurStageIndex
		{
			get
			{
				// It seems some mods might call CurStageIndex in prefix before thought has been attached to a pawn.
				// For example, Vanilla Traits Expanded.
				// https://github.com/Vanilla-Expanded/VanillaTraitsExpanded/blob/b55456925c4a4ffbc7347444e98a622d3562508c/1.4/Source/VanillaTraitsExpanded/HarmonyPatches/Thought_Patches.cs#LL218C63-L218C92
				if (pawn == null)
					return 0;

				var fSapienceStatus = pawn.GetQuantizedSapienceLevel();
				if (fSapienceStatus == null)
				{
					return 0;
				}

				return Mathf.Min(def.stages.Count - 1, (int)fSapienceStatus.Value);
			}
		}
	}
}