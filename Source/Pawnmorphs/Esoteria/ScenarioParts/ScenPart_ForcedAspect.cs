using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

#pragma warning disable 1591

namespace Pawnmorph.ScenarioParts
{
	// Token: 0x02000C83 RID: 3203
	public class ScenPart_ForcedAspect : ScenPart_PawnModifier
	{

		/// <summary>
		/// Do the ui elements for this scenario part.
		/// </summary>
		/// <param name="listing"></param>
		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, RowHeight * 3f + 31f);
			if (Widgets.ButtonText(scenPartRect.TopPartPixels(RowHeight), aspectDef.LabelCap))
			{
				FloatMenuUtility.MakeMenu(PossibleAspects(), (AspectDef ad) => ad.LabelCap, (AspectDef ad) => delegate ()
				{
					aspectDef = ad;
					if (stageIndex >= aspectDef.stages.Count())
					{
						stageIndex = aspectDef.stages.Count() - 1;
					}
					if (stageIndex < 0)
					{
						stageIndex = 0;
					}
				});
			}
			if (Widgets.ButtonText(new Rect(scenPartRect.x, scenPartRect.y + RowHeight, scenPartRect.width, 31f), aspectDef.stages[stageIndex].LabelCap ?? aspectDef.LabelCap))
			{
				FloatMenuUtility.MakeMenu(aspectDef.stages, (AspectStage stage) => stage.LabelCap ?? aspectDef.LabelCap, (AspectStage stage) => delegate ()
				{
					stageIndex = aspectDef.stages.IndexOf(stage);
				});
			}
			DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(RowHeight * 2f));
		}

		private IEnumerable<AspectDef> PossibleAspects()
		{
			return from x in DefDatabase<AspectDef>.AllDefsListForReading
				   where x.scenarioCanAdd
				   select x;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref aspectDef, "aspect");
			Scribe_Values.Look(ref stageIndex, "stageIndex");
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_PawnsHaveAspect".Translate(context.ToStringHuman(), chance.ToStringPercent(), aspectDef.label).CapitalizeFirst();
		}

		public override void Randomize()
		{
			base.Randomize();
			aspectDef = PossibleAspects().RandomElement();
			stageIndex = Rand.Range(0, aspectDef.stages.Count() - 1);
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_ForcedAspect scenPart_ForcedAspect = other as ScenPart_ForcedAspect;
			if (scenPart_ForcedAspect != null && aspectDef == scenPart_ForcedAspect.aspectDef)
			{
				chance = GenMath.ChanceEitherHappens(chance, scenPart_ForcedAspect.chance);
				return true;
			}
			return false;
		}

		public override bool AllowPlayerStartingPawn(Pawn pawn, bool tryingToRedress, PawnGenerationRequest req)
		{
			if (!base.AllowPlayerStartingPawn(pawn, tryingToRedress, req))
			{
				return false;
			}
			if (hideOffMap)
			{
				if (!req.AllowDead)
				{
					return false;
				}
				if (!req.AllowDowned)
				{
					return false;
				}
			}
			return true;
		}

		protected override void ModifyNewPawn(Pawn p)
		{
			AddAspect(p);
		}

		protected override void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
		{
			AddAspect(p);
		}

		private void AddAspect(Pawn p)
		{
			AspectTracker aspectTracker = p.GetAspectTracker();
			if (aspectTracker == null) return;

			var aspect = aspectTracker.GetAspect(aspectDef);
			if (aspect == null)
			{
				aspectTracker.Add(aspectDef, stageIndex);
			}
		}

		private AspectDef aspectDef;

		private int stageIndex;
	}
}
