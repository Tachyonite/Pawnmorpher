using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Pawnmorph.Utilities;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph
{
    class ITab_Pawn_Mutations : ITab
    {
        private float scrollViewHeight;
        private Vector2 scrollPosition = Vector2.zero;

        public ITab_Pawn_Mutations()
        {
            size = new Vector2(600f, 450f);
            labelKey = "TabMutations";
            tutorTag = "Mutations";
        }

        public override bool IsVisible
        {
            get
            {
                Pawn pawnToShowMutationsFor = PawnToShowMutationsFor;
                return ShouldShowTab(pawnToShowMutationsFor);
            }
        }

        private Pawn PawnToShowMutationsFor
        {
            get
            {
                if (SelPawn != null)
                {
                    return SelPawn;
                }
                Corpse corpse = SelThing as Corpse;
                if (corpse != null)
                {
                    return corpse.InnerPawn;
                }
                throw new InvalidOperationException("Mutation tab on non-pawn non-corpse " + SelThing);
            }
        }

        protected bool ShouldShowTab(Pawn pawn)
        {
            return (pawn.IsColonist || pawn.IsPrisonerOfColony) && ((pawn.GetMutationTracker()?.AllMutations.Count() ?? 0)> 0);
        }

        private const float EPSILON = 0.0001f; //all numbers smaller then this should be considered '0'

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 10f, size.x, size.y);
            Rect rect2 = rect.ContractedBy(10f);
            Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height - 10f);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            Vector2 col1 = new Vector2(1f, 1f);
            DrawMutTabHeader(ref col1, viewRect.width, PawnToShowMutationsFor);

            Vector2 col2 = new Vector2(rect2.width / 3, col1.y);
            Vector2 col3 = new Vector2(rect2.width * 2 / 3, col2.y);
            DrawColumnHeader(ref col1, rect2.width / 3, "MorphsITabHeader".Translate(), PawnToShowMutationsFor);
            DrawColumnHeader(ref col2, rect2.width / 3, "TraitsITabHeader".Translate(), PawnToShowMutationsFor);
            DrawColumnHeader(ref col3, rect2.width / 3, "ProductionITabHeader".Translate(), PawnToShowMutationsFor);

            MutationTracker mutationTracker = PawnToShowMutationsFor.GetMutationTracker();

            Assert(mutationTracker != null, "mutationTracker != null"); //mutationTracker should never be null

            IEnumerable<VTuple<MorphDef, float>> influences = mutationTracker.NormalizedInfluences.ToList();

            float nInfluenceSum = 0;
            foreach (VTuple<MorphDef, float> influence in influences)
            {
                nInfluenceSum += influence.second; //the normalized influences will not always add to 1 if there is any "human" influence left 
            }


            float humInf = 1 - nInfluenceSum; //1 - the normalized morphInfluence is the remaining human influence 


            if (humInf > EPSILON) {
                GUI.color = Color.green;
                Widgets.Label(new Rect(col1.x, col1.y, rect2.width / 3, 24f), ($"Human ({(humInf).ToStringPercent()})"));
                col1.y += 24f;
                GUI.color = Color.white;
            }


            foreach (VTuple<MorphDef, float> influence in influences.OrderByDescending(x => x.second))
            {
                if (Math.Abs(influence.second - influences.MaxBy(x => x.second).second) < EPSILON) GUI.color = Color.cyan;
                Widgets.Label(new Rect(col1.x, col1.y, rect2.width / 3, 24f),
                              $"{influence.first.race.LabelCap} ({influence.second.ToStringPercent()})");
                col1.y += 24f;
                GUI.color = Color.white;
            }
            
            

            for (int i = 0; i < 20; i++)
            {
                Widgets.Label(new Rect(col2.x, col2.y, rect2.width / 3, 24f), (i + 1).ToString());
                col2.y += 24f;
            }

            foreach (Hediff prodMutation in PawnToShowMutationsFor.GetProductionMutations())
            {
                HediffComp_Production prodcomp = prodMutation.TryGetComp<HediffComp_Production>();
                string stageString = "";
                if (prodMutation.CurStageIndex > 0)
                {
                    stageString = " (" + new string('+', prodMutation.CurStageIndex) + ")";
                }

                HediffComp_Staged stage = prodcomp.Props.stages.ElementAt(prodMutation.CurStageIndex);

                Widgets.Label(new Rect(col3.x, col3.y, rect2.width / 3, 24f), String.Format("{0} ({1})", prodMutation.def.LabelCap + stageString, (prodcomp.HatchingTicker / (stage.daysToProduce * 60000)).ToStringPercent()));
                col3.y += 18f;
                GUI.color = Color.grey;
                Text.Font = GameFont.Tiny;                                                   
                Widgets.Label(new Rect(col3.x, col3.y, rect2.width / 3, 24f), String.Format("{0} hours left", ((stage.daysToProduce * 24) - ((stage.daysToProduce * 24) * (prodcomp.HatchingTicker / (stage.daysToProduce * 60000)))).ToStringDecimalIfSmall()));
                col3.y += 24f;
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
            }

            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = Math.Max(col1.y, Math.Max(col2.y, col3.y)) + 30f;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawMutTabHeader(ref Vector2 curPos, float width, Pawn pawn)
        {
            Rect rect = new Rect(0f, curPos.y, width, 50f);
            Text.Anchor = TextAnchor.UpperLeft;
            string nameAndRace = pawn.Name.ToStringShort + " - " + pawn.def.label.CapitalizeFirst();
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, nameAndRace);
            Text.Font = GameFont.Small;
            curPos.y += 30f;
            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(0f, curPos.y, width);
            GUI.color = Color.white;
            curPos.y += 10f;
        }

        private void DrawColumnHeader(ref Vector2 curPos, float width, string text, Pawn pawn)
        {
            Rect rect = new Rect(curPos.x, curPos.y, width - 10f, 30f);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, text);
            Text.Font = GameFont.Small;
            curPos.y += 30f;
        }
    }
}
