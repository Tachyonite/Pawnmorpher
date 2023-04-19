using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

#pragma warning disable 1591

namespace Pawnmorph.ScenarioParts
{

	public class ScenPart_ForcedSapience : ScenPart_PawnModifier
	{
		// Token: 0x02000C83 RID: 3203
		private NeedDef need;

		private FloatRange levelRange;

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 3f + 31f);
			if (Widgets.ButtonText(scenPartRect.TopPartPixels(ScenPart.RowHeight), need.LabelCap))
			{
				FloatMenuUtility.MakeMenu(PossibleNeeds(), (NeedDef hd) => hd.LabelCap, delegate (NeedDef n)
				{
					ScenPart_ForcedSapience scenPart_ForcedSapience = this;
					return delegate
					{
						scenPart_ForcedSapience.need = n;
					};
				});
			}
			Widgets.FloatRange(new Rect(scenPartRect.x, scenPartRect.y + ScenPart.RowHeight, scenPartRect.width, 31f), listing.CurHeight.GetHashCode(), ref levelRange, 0f, 1f, "ConfigurableLevel");
			DoPawnModifierEditInterface(scenPartRect.BottomPartPixels(ScenPart.RowHeight * 2f));
		}

		private IEnumerable<NeedDef> PossibleNeeds()
		{
			return from x in DefDatabase<NeedDef>.AllDefsListForReading
				   where x.defName == "SapientAnimalControl"
				   select x;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref need, "need");
			Scribe_Values.Look(ref levelRange, "levelRange");
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_SetSapience".Translate(context.ToStringHuman(), chance.ToStringPercent(), need.label, levelRange.min.ToStringPercent(), levelRange.max.ToStringPercent()).CapitalizeFirst();
		}

		public override void Randomize()
		{
			base.Randomize();
			need = PossibleNeeds().RandomElement();
			levelRange.max = Rand.Range(0f, 1f);
			levelRange.min = levelRange.max * Rand.Range(0f, 0.95f);
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

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_ForcedSapience scenPart_SetSapience = other as ScenPart_ForcedSapience;
			if (scenPart_SetSapience != null && need == scenPart_SetSapience.need)
			{
				chance = GenMath.ChanceEitherHappens(chance, scenPart_SetSapience.chance);
				return true;
			}
			return false;
		}

		protected override void ModifyPawnPostGenerate(Pawn p, bool redressed)
		{
			if (p.needs != null)
			{
				Need need = p.needs.TryGetNeed(this.need);
				if (need != null)
				{
					need.CurLevelPercentage = levelRange.RandomInRange;
				}
			}
		}
		protected override void ModifyNewPawn(Pawn p)
		{
			if (p.needs != null)
			{
				Need need = p.needs.TryGetNeed(this.need);
				if (need != null)
				{
					need.CurLevelPercentage = levelRange.RandomInRange;
				}
			}
		}

		protected override void ModifyHideOffMapStartingPawnPostMapGenerate(Pawn p)
		{
			if (p.needs != null)
			{
				Need need = p.needs.TryGetNeed(this.need);
				if (need != null)
				{
					need.CurLevelPercentage = levelRange.RandomInRange;
				}
			}
		}
	}
}
