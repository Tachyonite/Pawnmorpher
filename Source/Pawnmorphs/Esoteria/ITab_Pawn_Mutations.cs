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
        // Constants
        private Vector2 mainSize = new Vector2(500f, 450f);
        private Vector2 logButSize = new Vector2(100f, 30f);
        private const float LOG_WIDTH = 300f;
        private const float EPSILON = 0.0001f; // All numbers smaller than this should be considered 0.
        private const float highlightAlpha = 0.03f;

        // Variables
        private float mainScrollViewHeight;
        private float logScrollViewHeight;
        private Vector2 mainScrollPosition = Vector2.zero;
        private Vector2 logScrollPosition = Vector2.zero;
        private bool toggleLog = false;
        private bool highlightCurrentLogEntry = false;

        public ITab_Pawn_Mutations()
        {
            size = new Vector2(mainSize.x, mainSize.y);
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
            return (pawn.IsColonist || pawn.IsPrisonerOfColony) && ((pawn.GetMutationTracker()?.AllMutations.Count() ?? 0) > 0);
        }


        protected override void FillTab()
        {
            // Set up the rects.
            Rect rect = new Rect(0f, 10f, mainSize.x, mainSize.y);
            Rect mainView = rect.ContractedBy(10f);
            Rect logView = new Rect(rect.x + rect.width, rect.y, LOG_WIDTH, rect.height).ContractedBy(10f);

            // No idea what this does, but it sounds important.
            GUI.BeginGroup(mainView);

            // Set the defualts, in case they weren't properly reset elsewhere.
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;

            // Draw the header.
            Vector2 col1 = new Vector2(0f, 0f);
            DrawMutTabHeader(ref col1, mainView.width);

            // Set up scrolling area.
            Rect outRect = new Rect(col1.x, col1.y, mainView.width, mainView.height - col1.y - 10f);
            Rect viewRect = new Rect(col1.x, col1.y, mainView.width - 16f, mainScrollViewHeight - col1.y);
            Widgets.BeginScrollView(outRect, ref mainScrollPosition, viewRect, true);

            // Set up referance variables for the other two column's current xy position.
            Vector2 col2 = new Vector2(viewRect.width / 3, col1.y);
            Vector2 col3 = new Vector2(viewRect.width / 3 * 2, col2.y);

            // Draw the headers for all three columns (labels are provided by the xml).
            DrawColumnHeader(ref col1, viewRect.width / 3, "MorphsITabHeader".Translate());
            DrawColumnHeader(ref col2, viewRect.width / 3, "TraitsITabHeader".Translate());
            DrawColumnHeader(ref col3, viewRect.width / 3, "ProductionITabHeader".Translate());

            // Draw the content of the columns.
            DrawMorphInfluenceList(ref col1, viewRect.width / 3);
            DrawMorphTraitsList(ref col2, viewRect.width / 3);
            DrawMorphProductionList(ref col3, viewRect.width / 3);

            // Set the scroll view height
            if (Event.current.type == EventType.Layout)
            {
                mainScrollViewHeight = Math.Max(col1.y, Math.Max(col2.y, col3.y));
            }
            Widgets.EndScrollView();

            // Ya, this thing is important for some reason, but IDK why.
            GUI.EndGroup();

            // Boolean to dynamically resize the iTab and draw the log's content if necessary.
            if (toggleLog)
            {
                size.x = mainSize.x + LOG_WIDTH;
                DrawMutationLog(logView);
            }
            else
            {
                size.x = mainSize.x;
            }

            // Make sure everything was reset properly.
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }

        private void DrawMutTabHeader(ref Vector2 curPos, float width)
        {
            // Print the pawn's nickname and race as a header.
            Text.Font = GameFont.Medium;
            string text = PawnToShowMutationsFor.Name.ToStringShort + " - " + PawnToShowMutationsFor.def.label.CapitalizeFirst();
            float rectHeight = Text.CalcHeight(text, width - logButSize.x);
            Widgets.Label(new Rect(curPos.x, curPos.y, width - logButSize.x, rectHeight), text);
            Text.Font = GameFont.Small;

            // Create the log button.
            Rect rect2 = new Rect(width - logButSize.x, curPos.y, logButSize.x, rectHeight);
            if (Widgets.ButtonText(rect2, "MutationLogButtonText".Translate(), doMouseoverSound: true))
            {
                toggleLog = !toggleLog;
            }
            curPos.y += rectHeight;

            // Draw a seperating line for seperating purposes, visually seperating the header from the body seperatingly.
            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(0f, curPos.y, width);
            GUI.color = Color.white;
            curPos.y += 10f;
        }

        private void DrawColumnHeader(ref Vector2 curPos, float width, string text)
        {
            Text.Font = GameFont.Medium;
            float rectHeight = Text.CalcHeight(text, width);
            Rect rect = new Rect(curPos.x, curPos.y, width, rectHeight);
            Widgets.Label(rect, text);
            curPos.y += rectHeight;
            Text.Font = GameFont.Small;
        }

        private void DrawMorphInfluenceList(ref Vector2 curPos, float width)
        {
            // Set up the mutation tracker.
            MutationTracker mutationTracker = PawnToShowMutationsFor.GetMutationTracker();
            Assert(mutationTracker != null, "mutationTracker != null"); //mutationTracker should never be null

            // Create a list of the current morph influences upon the pawn.
            IEnumerable<VTuple<MorphDef, float>> influences = mutationTracker.NormalizedInfluences.ToList();

            // Determine the remaining human influence.
            float humInf = 1f;
            foreach (VTuple<MorphDef, float> influence in influences)
                humInf -= influence.second;

            // If the remaining human influence is greater than 0.0001, print its influence first.
            // (0.0001 is used to compensate for floating point number's occasional lack of precision.)
            if (humInf > EPSILON)
            {
                GUI.color = Color.green;
                string text = $"Human ({(humInf).ToStringPercent()})";
                float rectHeight = Text.CalcHeight(text, width);
                Widgets.Label(new Rect(curPos.x, curPos.y, width, rectHeight), ($"Human ({(humInf).ToStringPercent()})"));
                curPos.y += rectHeight;
                GUI.color = Color.white;
            }
            
            // List the morph influences upon the pawn in descending order.
            foreach (VTuple<MorphDef, float> influence in influences.OrderByDescending(x => x.second))
            {
                // Set the greatest influence's color to cyan
                if (Math.Abs(influence.second - influences.MaxBy(x => x.second).second) < EPSILON)
                    GUI.color = Color.cyan;

                string text = $"{influence.first.race.LabelCap} ({influence.second.ToStringPercent()})";
                float rectHeight = Text.CalcHeight(text, width);
                Widgets.Label(new Rect(curPos.x, curPos.y, width, rectHeight), text);
                curPos.y += rectHeight;
                GUI.color = Color.white;
            }
        }

        private void DrawMorphTraitsList(ref Vector2 curPos, float width)
        {
            var aspectTracker = PawnToShowMutationsFor.GetAspectTracker();

            if (aspectTracker != null)
            {
                foreach (Aspect aspect in aspectTracker.Aspects)
                {
                    var label = aspect.Label.CapitalizeFirst();
                    Rect rect = new Rect(curPos.x, curPos.y, width, Text.CalcHeight(label, width));

                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }

                    GUI.color = aspect.def.labelColor;
                    Widgets.Label(rect, label);
                    curPos.y += rect.height;
                    GUI.color = Color.white;

                    TipSignal tip = new TipSignal(() => aspect.TipString(PawnToShowMutationsFor), (int)curPos.y * 37);
                    TooltipHandler.TipRegion(rect, tip);
                }
            }
        }

        private void DrawMorphProductionList(ref Vector2 curPos, float width)
        {
            foreach (Hediff prodMutation in PawnToShowMutationsFor.GetProductionMutations().OrderBy(x => x.def.label))
            {
                HediffComp_Production prodcomp = prodMutation.TryGetComp<HediffComp_Production>();

                // Figure out what stage the hedif is in.
                string stageString = "";
                if (prodMutation.CurStageIndex > 0)
                    stageString = " (" + new string('+', prodMutation.CurStageIndex) + ")";
                HediffComp_Staged stage = prodcomp.Props.stages.ElementAt(prodMutation.CurStageIndex);

                // Draw the main text (the mutation's label, current stage and a percentage to completion).
                string mutLabel = prodMutation.def.LabelCap;
                float curPercent = prodcomp.HatchingTicker / (stage.daysToProduce * 60000);
                string text = $"{mutLabel}{stageString} ({curPercent.ToStringPercent()}) ";
                float rectHeight = Text.CalcHeight(text, width);
                Widgets.Label(new Rect(curPos.x, curPos.y, width, rectHeight), text);
                curPos.y += rectHeight;

                GUI.color = Color.grey;
                Text.Font = GameFont.Tiny;

                // Draw the subtext (# hours left).
                float hoursLeft = stage.daysToProduce * 24 - prodcomp.HatchingTicker / 2500;
                string subtext = $"{hoursLeft.ToStringDecimalIfSmall()} hours left";
                float subRectHeight = Text.CalcHeight(subtext, width);
                Widgets.Label(new Rect(curPos.x, curPos.y, width, subRectHeight), subtext);
                curPos.y += subRectHeight;

                GUI.color = Color.white;
                Text.Font = GameFont.Small;
            }
        }

        private void DrawMutationLog(Rect rect)
        {
            // Set up a referance var to tell the drawers where to put stuff.
            Vector2 curPos = new Vector2(rect.x, rect.y);

            // Reset this flag to prevent some weirdness.
            highlightCurrentLogEntry = false;

            // Draw the header.
            DrawMutLogHeader(ref curPos, rect.width);

            // Set up scrolling view.
            Rect outRect = new Rect(curPos.x, curPos.y, rect.width, rect.height - curPos.y + 10f);
            Rect viewRect = new Rect(curPos.x, curPos.y, rect.width - 16f, logScrollViewHeight - curPos.y);
            Widgets.BeginScrollView(outRect, ref logScrollPosition, viewRect, true);

            // A bit of test code to show something long enough to scroll in the log.
            DrawMutLogEntry(ref curPos, viewRect.width, "A really long line so that yap can see if the code can handle multiple lines.");
            for (int i = 0; i < 20; i++)
            {
                DrawMutLogEntry(ref curPos, viewRect.width, (i + 1).ToString());
            }

            // Set the scroll view height
            if (Event.current.type == EventType.Layout)
            {
                logScrollViewHeight = curPos.y;
            }
            Widgets.EndScrollView();
        }

        private void DrawMutLogHeader(ref Vector2 curPos, float width)
        {
            // Draw header text (taken from xml).
            Text.Font = GameFont.Medium;
            string text = "MutationLogHeader".Translate();
            float rectHeight = Text.CalcHeight(text, width);
            Widgets.Label(new Rect(curPos.x, curPos.y, width, rectHeight), text);
            curPos.y += rectHeight;
            Text.Font = GameFont.Small;

            // Draw seperating line.
            GUI.color = Color.gray;
            Widgets.DrawLineHorizontal(curPos.x, curPos.y, width);
            curPos.y += 10f;
            GUI.color = Color.white;
        }

        private void DrawMutLogEntry(ref Vector2 curPos, float width, string text)
        {
            // Set up the drawing rect
            Rect entryRect = new Rect(curPos.x, curPos.y, width, Text.CalcHeight(text, width));

            // Draw a highlight every other line.
            if (highlightCurrentLogEntry)
                Widgets.DrawRectFast(entryRect, new Color(1f, 1f, 1f, highlightAlpha));
            highlightCurrentLogEntry = !highlightCurrentLogEntry;

            // Draw the entry's text.
            Widgets.Label(entryRect, text);

            // Update the location for the next entry.
            curPos.y += entryRect.height;
        }
    }
}
