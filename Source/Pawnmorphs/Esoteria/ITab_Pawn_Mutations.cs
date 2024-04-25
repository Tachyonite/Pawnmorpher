using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using static UnityEngine.ParticleSystem;

namespace Pawnmorph
{
	[HotSwappable]
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
			if ((pawn.GetAspectTracker()?.AspectCount ?? 0) > 0)
				return true;

			// Former humans currently don't have mutation tracker.
			if (pawn.IsFormerHuman() == false && (pawn.GetMutationTracker()?.AllMutations.Count ?? 0) > 0)
				return true;

			return false;
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
			if (SelPawn.GetMutationTracker() != null)
				DrawMutTabHeader(ref col1, mainView.width);


			// Set up scrolling area.
			Rect outRect = new Rect(col1.x, col1.y, mainView.width, mainView.height - col1.y - 10f);
			Rect viewRect = new Rect(col1.x, col1.y, mainView.width - 16f, mainScrollViewHeight - col1.y);
			Widgets.BeginScrollView(outRect, ref mainScrollPosition, viewRect, true);

			// Set up referance variables for the other two column's current xy position.
			Vector2 col2 = new Vector2(viewRect.width / 3, col1.y);
			Vector2 col3 = new Vector2(viewRect.width / 3 * 2, col2.y);
			float colWidth = viewRect.width / 3 - 10f;
			
			// Draw the headers for all three columns (labels are provided by the xml).
			DrawColumnHeader(ref col1, colWidth, "MorphsITabHeader".Translate());
			DrawColumnHeader(ref col2, colWidth, "TraitsITabHeader".Translate());
			DrawColumnHeader(ref col3, colWidth, "ProductionITabHeader".Translate());

			// Draw the content of the columns.
			if (MorphUtilities.HybridsAreEnabledFor(PawnToShowMutationsFor.def))
				DrawMorphInfluenceList(ref col1, colWidth);
			DrawMorphTraitsList(ref col2, colWidth);
			DrawMorphProductionList(ref col3, colWidth);

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
			if (mutationTracker == null) return;

			// Create a list of the current morph influences upon the pawn.
			var influences = mutationTracker.ToList();

			// Determine the remaining human influence.
			float maxRaceInfluence = MorphUtilities.GetMaxInfluenceOfRace(PawnToShowMutationsFor.def);

			float humInf = maxRaceInfluence;
			int influencesCount = influences.Count;
			for (int i = influences.Count - 1; i >= 0; i--)
				humInf -= influences[i].Value;

			// If the remaining human influence is greater than 0.0001, print its influence first.
			// (0.0001 is used to compensate for floating point number's occasional lack of precision.)
			bool renderHuman = false;
			if (influencesCount > 0)
			{
				// List the morph influences upon the pawn in descending order.
				bool first = true;
				foreach (var influence in influences.OrderByDescending(x => x.Value))
				{
					var nVal = influence.Value / maxRaceInfluence;
					if (renderHuman == false && humInf / maxRaceInfluence > nVal)
					{
						GUI.color = Color.green;
						DrawInfluenceRow(ref curPos, width, "Human", humInf / maxRaceInfluence);
						renderHuman = true;
					}

					// Set the greatest influence's color to cyan
					if (first)
					{
						GUI.color = Color.cyan;
						first = false;
					}

					string label = null;
					if (influence.Key is MorphDef morph)
						label = morph.ExplicitHybridRace?.LabelCap;

					if (label == null)
						label = influence.Key.LabelCap;

					DrawInfluenceRow(ref curPos, width, label, nVal);
				}

			}

			//float maxInfluence = influences.MaxBy(x => x.Value).Value;

			if (renderHuman == false && humInf > EPSILON)
			{
				GUI.color = Color.green;
				DrawInfluenceRow(ref curPos, width, "Human", humInf / maxRaceInfluence);
			}

		}

		private void DrawInfluenceRow(ref Vector2 curPos, float width, string label, float value)
		{
			string text = $"{label} ({value.ToStringPercent()})";
			float rectHeight = Text.CalcHeight(text, width);
			Widgets.Label(new Rect(curPos.x, curPos.y, width, rectHeight), text);
			curPos.y += rectHeight;
			GUI.color = Color.white;
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
				if (prodcomp.CanProduce == false)
					continue;


				// Figure out what stage the hedif is in.
				HediffComp_Staged stage = prodcomp.CurStage;

				string stageString = "";
				if (prodcomp.Stage > 0)
					stageString = " " + new string('+', prodcomp.Stage) + "";

				// Draw the main text (the mutation's label, current stage and a percentage to completion).
				float startPosition = curPos.y;

				string text;
				string mutLabel = prodMutation.def.LabelCap;
				if (prodcomp.IsDry) //display something special if it's dry 
				{
					GUI.color = Color.red;
					text = $"{mutLabel} (dry)";
				}
				else
				{
					float curPercent = prodcomp.HatchingTicker / (stage.daysToProduce * 60000);
					text = $"{mutLabel}{stageString} ({curPercent.ToStringPercent()}) ";
				}
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
				string subtext2 = $"Total produced: {prodcomp.totalProduced}";
				float subRectHeight2 = Text.CalcHeight(subtext, width);
				Widgets.Label(new Rect(curPos.x, curPos.y, width, subRectHeight2), subtext2);
				curPos.y += subRectHeight2;

				GUI.color = Color.white;
				Text.Font = GameFont.Small;

				if (prodMutation is IDescriptiveHediff descriptive)
				{
					Rect fullRect = new Rect(curPos.x, startPosition, width, curPos.y - startPosition);
					if (Mouse.IsOver(fullRect))
					{
						Widgets.DrawHighlight(fullRect);
					}

					TipSignal tip = new TipSignal(() => descriptive.Description, (int)curPos.y * 37);
					TooltipHandler.TipRegion(fullRect, tip);
				}
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

			var cachedLogDisplay = GetMutationLogs(PawnToShowMutationsFor);
			foreach (KeyValuePair<MutationLogEntry, ITab_Pawn_Log_Utility.LogLineDisplayable> line in cachedLogDisplay)
			{
				StringBuilder stringBuilder = new StringBuilder();
				line.Value.AppendTo(stringBuilder);
				stringBuilder.Length--;
				DrawMutLogEntry(ref curPos, viewRect.width, stringBuilder.ToString(), line.Key);
			}

			// Set the scroll view height
			if (Event.current.type == EventType.Layout)
			{
				logScrollViewHeight = curPos.y;
			}
			Widgets.EndScrollView();
		}

		List<KeyValuePair<MutationLogEntry, ITab_Pawn_Log_Utility.LogLineDisplayable>> logCache = new List<KeyValuePair<MutationLogEntry, ITab_Pawn_Log_Utility.LogLineDisplayable>>();
		List<KeyValuePair<MutationLogEntry, ITab_Pawn_Log_Utility.LogLineDisplayable>> GetMutationLogs(Pawn pawn)
		{
			logCache.Clear();
			MutationTracker tracker = pawn.GetMutationTracker();
			if (tracker != null)
			{
				logCache.AddRange(tracker.MutationLog.Select(x => new KeyValuePair<MutationLogEntry, ITab_Pawn_Log_Utility.LogLineDisplayable>(x, new ITab_Pawn_Log_Utility.LogLineDisplayableLog(x, pawn))));
				logCache.Reverse();
			}
			return logCache;
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

		private void DrawMutLogEntry(ref Vector2 curPos, float width, string text, MutationLogEntry entry)
		{
			// Set up the drawing rect
			Rect entryRect = new Rect(curPos.x, curPos.y, width, Text.CalcHeight(text, width));

			// Draw a highlight every other line.
			if (highlightCurrentLogEntry)
				Widgets.DrawRectFast(entryRect, new Color(1f, 1f, 1f, highlightAlpha));
			highlightCurrentLogEntry = !highlightCurrentLogEntry;

			// Draw the entry's text.
			Widgets.Label(entryRect, text);



			TipSignal tip = new TipSignal(() =>
			{
				string tooltip;
				int ticksAgo = entry.Age;
				
				if (ticksAgo > TimeMetrics.TICKS_PER_DAY)
					tooltip = $"PmDaysAgo".Translate(ticksAgo / TimeMetrics.TICKS_PER_DAY);
				if (ticksAgo > TimeMetrics.TICKS_PER_HOUR)
					tooltip = $"PmHoursAgo".Translate(ticksAgo / TimeMetrics.TICKS_PER_HOUR);
				else
					tooltip = $"PmLessThanHour".Translate();

				return tooltip;

			}, (int)curPos.y * 37);
			TooltipHandler.TipRegion(entryRect, tip);


			if (entry.Causes.Location.HasValue)
			{
				if (Mouse.IsOver(entryRect))
				{
					Widgets.DrawHighlight(entryRect);
					TargetHighlighter.Highlight(entry.Causes.Location.Value);
				}

				if (Widgets.ButtonInvisible(entryRect))
				{
					CameraJumper.TryJump(entry.Causes.Location.Value);
				}
			}

			// Update the location for the next entry.
			curPos.y += entryRect.height;
		}
	}
}
