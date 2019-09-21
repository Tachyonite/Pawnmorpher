using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;

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
            return pawn.IsColonist || pawn.IsPrisonerOfColony;
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 20f, size.x, size.y);
            Rect rect2 = rect.ContractedBy(10f);
            Rect position = new Rect(rect2.x, rect2.y, rect2.width, rect2.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 0f, position.width, position.height);
            Rect viewRect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            Vector2 col1 = new Vector2(1f, 1f);
            DrawMutTabHeader(ref col1, viewRect.width, PawnToShowMutationsFor);

            Vector2 col2 = new Vector2(rect2.width / 3, col1.y);
            Vector2 col3 = new Vector2(rect2.width * 2 / 3, col2.y);
            DrawColumnHeader(ref col1, rect2.width / 3, "MorphsITabHeader".Translate(), PawnToShowMutationsFor);
            DrawColumnHeader(ref col2, rect2.width / 3, "TraitsITabHeader".Translate(), PawnToShowMutationsFor);
            DrawColumnHeader(ref col3, rect2.width / 3, "ProductionITabHeader".Translate(), PawnToShowMutationsFor);

            for (int i = 0; i < 20; i++)
            {
                Widgets.Label(new Rect(col2.x, col2.y, rect2.width / 3, 24f), (i + 1).ToString());
                col2.y += 24f;
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
            Rect rect = new Rect(0f, curPos.y, width, 30f);
            Text.Anchor = TextAnchor.UpperLeft;
            string nameAndRace = pawn.Name.ToStringFull + " - " + pawn.def.label.CapitalizeFirst();
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, nameAndRace);
            Text.Font = GameFont.Small;
            curPos.y += 30f;
            Widgets.DrawLineHorizontal(0f, curPos.y, width);
            curPos.y += 2f;
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
