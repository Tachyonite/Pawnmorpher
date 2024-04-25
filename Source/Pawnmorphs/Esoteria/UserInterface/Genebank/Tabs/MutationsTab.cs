using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Abilities;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using Pawnmorph.UserInterface.Preview;
using Pawnmorph.UserInterface.TableBox;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.Genebank.Tabs
{
	internal class MutationsTab : GenebankTab
	{
		private const float ABILITY_SIZE = 100;
		private const float STAGE_BUTTON_SIZE = 30;

		private readonly string TAB_COLUMN_MUTATION = "PM_Genebank_MutationTab_Column_Mutation".Translate();

		private readonly string TAB_COLUMN_BODYPART = "PM_Genebank_MutationTab_Column_BodyPart".Translate();
		private readonly float TAB_COLUMN_BODYPART_SIZE;

		private readonly string TAB_COLUMN_ANIMAL = "PM_Genebank_MutationTab_Column_Animal".Translate();
		private readonly float TAB_COLUMN_ANIMAL_SIZE;

		private readonly string TAB_COLUMN_PARAGON = "PM_Genebank_MutationTab_Column_Paragon".Translate();
		private readonly float TAB_COLUMN_PARAGON_SIZE;

		private readonly string TAB_COLUMN_ABILITIES = "PM_Genebank_MutationTab_Column_Abilities".Translate();
		private readonly float TAB_COLUMN_ABILITIES_SIZE;

		private readonly string TAB_COLUMN_STATS = "PM_Genebank_MutationTab_Column_Stats".Translate();

		private readonly string DESCRIPTION_STAGES = "PM_Genebank_MutationTab_Details_Stages".Translate();
		private readonly float DESCRIPTION_STAGES_SIZE;

		private readonly string DESCRIPTION_ATTACKS = "PM_Genebank_MutationTab_Details_Attacks".Translate();
		private readonly string DESCRIPTION_ABILITIES = "PM_Genebank_MutationTab_Details_Abilities".Translate();
		private readonly float DESCRIPTION_ABILITIES_SIZE;
		private readonly string DESCRIPTION_OTHERS = "PM_Genebank_MutationTab_Details_OtherInfluences".Translate();
		private readonly string DESCRIPTION_COOLDOWN = "PM_Genebank_MutationTab_Details_AbilityCooldown".Translate();
		private readonly string DESCRIPTION_HOURS = "LetterHour".Translate();
		private readonly string DESCRIPTION_DPS = "PM_Genebank_MutationTab_Details_DamagePerSecond".Translate();


		HumanlikePreview _previewNorth;
		HumanlikePreview _previewEast;
		HumanlikePreview _previewSouth;
		ChamberDatabase _databank;
		List<MutationStage> _stages;
		Vector2 _abilitiesScrollPosition;
		Vector2 _stageScrollPosition;
		int _currentStage;
		string _stageDescription;
		StringBuilder _stringBuilder;
		Vector2 _stageDescriptionScrollbarPosition;
		MutationDef _selectedDef;
		Dictionary<string, string> _dpsCache;
		private IReadOnlyList<GeneRowItem> _selectedRows;

        public MutationsTab()
        {
			Text.Font = GameFont.Small;
			TAB_COLUMN_PARAGON_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_PARAGON).x, 60f);
			TAB_COLUMN_ABILITIES_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_ABILITIES).x, 100f);

			TAB_COLUMN_BODYPART_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_BODYPART).x, 100f);
			TAB_COLUMN_ANIMAL_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_ANIMAL).x, 100f);

			Text.Font = GameFont.Medium;
			DESCRIPTION_STAGES_SIZE = Text.CalcSize(DESCRIPTION_STAGES).x;
			DESCRIPTION_ABILITIES_SIZE = Text.CalcSize(DESCRIPTION_ABILITIES).x;
		}

        public override void Initialize(ChamberDatabase databank)
		{
			_databank = databank;
			_stages = new List<MutationStage>();
			_stringBuilder = new StringBuilder();
			_dpsCache = new Dictionary<string, string>();
			int size = (int)(PREVIEW_SIZE - GenUI.GapTiny);

			_previewNorth = new HumanlikePreview(size, size, ThingDefOf.Human as AlienRace.ThingDef_AlienRace)
			{
				Rotation = Rot4.North
			};


			_previewEast = new HumanlikePreview(size, size, ThingDefOf.Human as AlienRace.ThingDef_AlienRace)
			{
				Rotation = Rot4.East,
				PreviewIndex = 2
			};


			_previewSouth = new HumanlikePreview(size, size, ThingDefOf.Human as AlienRace.ThingDef_AlienRace)
			{
				Rotation = Rot4.South,
				PreviewIndex = 3
			};
		}


		public override void GenerateTable(Table<GeneRowItem> table)
		{

			var column = table.AddColumn(TAB_COLUMN_MUTATION, 0.5f,
				(ref Rect box, GeneRowItem item) => Widgets.Label(box, item.Label),
				(collection, ascending, column) =>
				{
					if (ascending)
						collection.OrderBy(x => x.Label);
					else
						collection.OrderByDescending(x => x.Label);
				});
			column.IsFixedWidth = false;

			TableColumn colBodyPart = table.AddColumn(TAB_COLUMN_BODYPART, TAB_COLUMN_BODYPART_SIZE);
			TableColumn colAnimal = table.AddColumn(TAB_COLUMN_ANIMAL, TAB_COLUMN_ANIMAL_SIZE);

			TableColumn colParagon = table.AddColumn(TAB_COLUMN_PARAGON, TAB_COLUMN_PARAGON_SIZE);
			TableColumn colAbilities = table.AddColumn(TAB_COLUMN_ABILITIES, TAB_COLUMN_ABILITIES_SIZE);
			TableColumn colStats = table.AddColumn(TAB_COLUMN_STATS, 0.5f);
			colStats.IsFixedWidth = false;

			AddColumnHook(table);

			GeneRowItem item;
			int totalCapacity = _databank.TotalStorage;
			foreach (GenebankEntry<MutationDef> mutationEntry in _databank.GetEntryItems<MutationDef>())
			{
				MutationDef mutation = mutationEntry.Value;
				string searchText = mutation.label;
				item = new GeneRowItem(mutationEntry, totalCapacity, searchText);

				string parts;
				if (mutation.RemoveComp?.layer == MutationLayer.Skin)
				{
					// Skin mutations cover all surface body parts.
					parts = "Skin";
					searchText += " " + parts;
				}
				else if (mutation.parts.Count < 3)
				{
					// If at most 2 body parts, then list them.
					parts = String.Join(", ", mutation.parts.Select(x => x.LabelCap).Distinct());
					searchText += " " + parts;
				}
				else
				{
					// If more than 2 body parts then show as multiple instead but still make them all searchable.
					parts = "Multiple";
					searchText += " " + String.Join(" ", mutation.parts.Select(x => x.label).Distinct());
				}
				item[colBodyPart] = parts;


				string animal = "Multiple";
				List<ThingDef> animals = mutation.AssociatedAnimals.ToList();
				if (animals.Count == 1)
					animal = animals[0].LabelCap;

				item[colAnimal] = animal;
				searchText += " " + String.Join(" ", animals.Select(x => x.label));


				if (mutation.stages != null)
				{
					var stages = mutation.stages.OfType<MutationStage>().ToList();
					if (stages.Count > 0)
					{
						var lastStage = stages[stages.Count - 1];


						// If any stage has abilities, it will be the last one. Paragon or not.
						if (lastStage.abilities != null)
						{
							string abilities = String.Join(", ", lastStage.abilities.Select(x => x.label));
							item[colAbilities] = abilities;
							searchText += " " + abilities;
						}

						if (lastStage.key == "paragon")
						{
							item[colParagon] = TAB_COLUMN_PARAGON.ToLower();
							lastStage = stages[Math.Max(0, stages.Count - 2)];
							searchText += " " + item[colParagon];
						}

						List<StatDef> stats = new List<StatDef>();

						if (lastStage.statOffsets != null)
							stats.AddRange(lastStage.statOffsets.Select(x => x.stat));

						string statsImpact = String.Join(", ", stats.Where(x => x != null).Select(x => x.LabelCap).Distinct());
						searchText += " " + statsImpact;

						item[colStats] = statsImpact;
					}
				}

				AddedRowHook(item, searchText);

				item.SearchString = searchText.ToLower();
				table.AddRow(item);
			}

		}


		public override void AddedRowHook(GeneRowItem row, string searchText)
		{
			// Hook method.
		}

		public override void AddColumnHook(Table<GeneRowItem> table)
		{
			// Hook method.
		}

		public override void SelectionChanged(IReadOnlyList<GeneRowItem> selectedRows)
		{
			_selectedRows = selectedRows;
			UpdateStages(selectedRows);
			UpdatePreviews(selectedRows);
		}

		/// <summary>
		/// Updates all data based on the provided stage index.
		/// </summary>
		/// <param name="stageIndex">The stage index.</param>
		private void SelectStage(int stageIndex)
		{
			_currentStage = stageIndex;
			_stringBuilder.Clear();

			MutationStage stage = _stages[_currentStage];

			_stringBuilder.AppendLine(_previewSouth.AdjustText(stage.description ?? _selectedDef.description));
			if (_stringBuilder.Length > 0)
				_stringBuilder.AppendLine();

			foreach (StatDrawEntry item in Verse.HediffStatsUtility.SpecialDisplayStats(stage, null))
			{
				if (item.ShouldDisplay())
					_stringBuilder.AppendLine(item.LabelCap + ": " + item.ValueString);
			}

			if (_stringBuilder.Length > 0)
				_stringBuilder.AppendLine();

			// Add attack descriptions.
			HediffCompProperties_VerbGiver verbComp = _selectedDef.CompProps<HediffCompProperties_VerbGiver>();
			if (stage.verbOverrides != null && verbComp != null)
			{
				_dpsCache.Clear();
				foreach (var verb in stage.verbOverrides)
				{
					Tool verbTool = verbComp.tools.SingleOrDefault(x => x.label == verb.label);
					if (verbTool == null)
						continue;

					if ((verb.power ?? 0) + (verb.cooldownTime ?? 0) + (verb.chanceFactor ?? 0) == 0)
						continue;

					float power = verb.power ?? verbTool.power;
					float cooldown = verb.cooldownTime ?? verbTool.cooldownTime;
					float chance = verb.chanceFactor ?? verbTool.chanceFactor;
					_dpsCache[verbTool.label] = $"{verbTool.LabelCap}: {power / cooldown * chance:0.##}{DESCRIPTION_DPS}";
				}

				if (_dpsCache.Count > 0)
				{
					_stringBuilder.AppendLine(DESCRIPTION_ATTACKS);

					foreach (var item in _dpsCache)
						_stringBuilder.AppendLine(item.Value);
				}
			}

			_stageDescription = _stringBuilder.ToString();
		}

		private void UpdatePreviews(IReadOnlyList<GeneRowItem> selectedRows)
		{
			_previewNorth.ClearMutations();
			_previewEast.ClearMutations();
			_previewSouth.ClearMutations();

			foreach (var item in selectedRows)
			{
				MutationDef mutation = (item.RowObject as GenebankEntry<MutationDef>).Value;
				_previewNorth.AddMutation(mutation);
				_previewEast.AddMutation(mutation);
				_previewSouth.AddMutation(mutation);

				if (_stages.Count > 0)
				{
					MutationStage stage = _stages[_currentStage];
					_previewEast.SetSeverity(mutation, stage.minSeverity);
					_previewNorth.SetSeverity(mutation, stage.minSeverity);
					_previewSouth.SetSeverity(mutation, stage.minSeverity);
				}
			}

			_previewNorth.Refresh();
			_previewEast.Refresh();
			_previewSouth.Refresh();
		}

		private void UpdateStages(IReadOnlyList<GeneRowItem> selectedRows)
		{
			_stages.Clear();
			_currentStage = 0;

			if (selectedRows.Count == 1)
			{
				_selectedDef = (selectedRows[0].RowObject as GenebankEntry<MutationDef>).Value;
				if (_selectedDef.stages != null)
					_stages.AddRange(_selectedDef.stages.OfType<MutationStage>());
				else
					_stages.Add(new MutationStage());



				foreach (var stage in _stages)
				{
					if (stage.abilities != null)
					{
						foreach (var ability in stage.abilities)
						{
							ability.CacheTexture();
						}
					}
				}

				if (_stages[_stages.Count - 1].minSeverity > 1)
					SelectStage(_stages.Count - 2);
				else
					SelectStage(_stages.Count - 1);
			}
		}

		public override void DrawDetails(Rect inRect)
		{
			DrawPreview(inRect);

			if (_selectedDef != null)
			{
				float width = inRect.width - PREVIEW_SIZE - SPACING;

				Text.Font = GameFont.Medium;
				Rect stageSelectionRect = new Rect(0, inRect.y, width / 3 * 2, STAGE_BUTTON_SIZE + SPACING + Text.LineHeight);
				stageSelectionRect.x = inRect.xMax - stageSelectionRect.width;
				DrawStageSelection(ref stageSelectionRect);

				Rect titleBox = new Rect(inRect.x + PREVIEW_SIZE + SPACING, inRect.y, width - stageSelectionRect.width, 100);
				float titleHeight = DrawTitle(titleBox);

				float descriptionHeight = Mathf.Max(stageSelectionRect.yMax, titleHeight);

				Text.Font = GameFont.Small;
				Rect descriptionRect = new Rect(inRect.x + PREVIEW_SIZE + SPACING, descriptionHeight + SPACING, 0, 0);
				descriptionRect.xMax = inRect.xMax;
				descriptionRect.yMax = inRect.yMax - ABILITY_SIZE - SPACING;
				DrawDescriptionBox(descriptionRect);


				Text.Font = GameFont.Medium;
				Rect abilitiesRect = new Rect(inRect.x, inRect.yMax - ABILITY_SIZE, inRect.width, ABILITY_SIZE);
				DrawAbilities(abilitiesRect);
			}
		}

		private float DrawTitle(Rect inRect)
		{
			if (_selectedDef == null)
				return inRect.y;

			float curY = inRect.y;
			Text.Font = GameFont.Medium;
			Widgets.LongLabel(inRect.x, inRect.width, _selectedDef.LabelCap, ref curY);

			_stringBuilder.Clear();
			Text.Font = GameFont.Small;
			IReadOnlyList<ThingDef> _influences = _selectedDef.AssociatedAnimals;
			int iterations = Math.Min(_influences.Count, 9);
			for (int i = 0; i < iterations; i++)
			{
				_stringBuilder.AppendLine("/ " + _influences[i].LabelCap);
			}
			Widgets.LongLabel(inRect.x, inRect.width, _stringBuilder.ToString(), ref curY);
			curY -= Text.LineHeight;
			int others = _influences.Count - iterations;
			if (others > 0)
			{
				_stringBuilder.Clear();
				for (int i = iterations; i < _influences.Count; i++)
				{
					_stringBuilder.AppendLine("/ " + _influences[i].LabelCap);
				}
				Widgets.Label(inRect.x, ref curY, inRect.width, $"/ {others} {DESCRIPTION_OTHERS}.", _stringBuilder.ToString());
			}

			return curY;
		}

		private void DrawDescriptionBox(Rect descriptionRect)
		{
			if (_stages.Count > 0)
			{
				Widgets.LabelScrollable(descriptionRect, _stageDescription, ref _stageDescriptionScrollbarPosition);
			}
		}

		private void DrawStageSelection(ref Rect inRect)
		{
			if (_stages.Count > 0)
			{
				Rect stageButtonViewRect = new Rect(0, 0, (STAGE_BUTTON_SIZE + SPACING) * _stages.Count, STAGE_BUTTON_SIZE);
				Rect scrollBox = new Rect(inRect);
				scrollBox.width -= DESCRIPTION_STAGES_SIZE;
				if (stageButtonViewRect.width < scrollBox.width)
				{
					scrollBox.width = stageButtonViewRect.width;
				}
				scrollBox.height = stageButtonViewRect.height + GenUI.ScrollBarWidth;
				scrollBox.x = inRect.xMax - scrollBox.width + SPACING;

				Widgets.Label(new Rect(scrollBox.x - DESCRIPTION_STAGES_SIZE, inRect.y, DESCRIPTION_STAGES_SIZE, STAGE_BUTTON_SIZE), DESCRIPTION_STAGES);

				Rect stageButtonRect = new Rect(0, 0, STAGE_BUTTON_SIZE, STAGE_BUTTON_SIZE);
				Widgets.BeginScrollView(scrollBox, ref _stageScrollPosition, stageButtonViewRect);
				for (int i = 0; i < _stages.Count; i++)
				{
					Widgets.DrawBoxSolidWithOutline(stageButtonRect, Color.black, Color.grey);
					if (Widgets.ButtonInvisible(stageButtonRect))
					{
						SelectStage(i);
						UpdatePreviews(_selectedRows);
					}

					Rect labelRect = new Rect(stageButtonRect);
					labelRect.x += 10;
					Widgets.Label(labelRect, (i + 1).ToString());

					if (i == _currentStage)
						Widgets.DrawHighlightSelected(stageButtonRect);

					Widgets.DrawHighlightIfMouseover(stageButtonRect);
					stageButtonRect.x = stageButtonRect.xMax + SPACING;
				}

				Widgets.EndScrollView();

				Widgets.Label(new Rect(scrollBox.x, scrollBox.yMax, scrollBox.width, Text.LineHeight), _stages[_currentStage].label.CapitalizeFirst());

				inRect.width = scrollBox.width + DESCRIPTION_STAGES_SIZE;
			}

		}

		public void DrawPreview(Rect inRect)
		{
			Rect previewBox = new Rect(inRect.x, inRect.y, PREVIEW_SIZE, PREVIEW_SIZE);

			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_previewEast.Draw(previewBox);

			previewBox.y = previewBox.yMax + SPACING;

			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_previewSouth.Draw(previewBox);

			previewBox.y = previewBox.yMax + SPACING;

			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_previewNorth.Draw(previewBox);

			previewBox.y = previewBox.yMax + SPACING;
			previewBox.width = 75f;
			previewBox.height = 20f;
			if (Widgets.ButtonText(previewBox, MALE))
				SetPreviewGender(Gender.Male);

			previewBox.x = inRect.x + PREVIEW_SIZE - previewBox.width;
			if (Widgets.ButtonText(previewBox, FEMALE))
				SetPreviewGender(Gender.Female);

		}

		private void SetPreviewGender(Gender gender)
		{
			_previewNorth.SetGender(gender, null);
			_previewEast.SetGender(gender, null);
			_previewSouth.SetGender(gender, null);
			_previewNorth.Refresh();
			_previewEast.Refresh();
			_previewSouth.Refresh();
		}

		public void DrawAbilities(Rect inRect)
		{
			Widgets.Label(new Rect(inRect.x, inRect.y - Text.LineHeight - SPACING, DESCRIPTION_ABILITIES_SIZE, Text.LineHeight), DESCRIPTION_ABILITIES);
			if (_stages.Count > 0)
			{
				var abilities = _stages[_currentStage].abilities;
				if (abilities == null || abilities.Count == 0)
					return;

				Rect abilitiesViewRect = new Rect(0, 0, (ABILITY_SIZE + SPACING) * abilities.Count, ABILITY_SIZE);
				Rect abilityRect = new Rect(0, 0, ABILITY_SIZE, ABILITY_SIZE);

				Widgets.BeginScrollView(inRect, ref _abilitiesScrollPosition, abilitiesViewRect);

				for (int i = 0; i < abilities.Count; i++)
				{
					MutationAbilityDef ability = abilities[i];
					Widgets.DrawBoxSolidWithOutline(abilityRect, Color.black, Color.gray);
					Widgets.DrawTextureFitted(abilityRect.ContractedBy(GenUI.GapTiny), ability.IconTexture, 1f);
					TooltipHandler.TipRegion(abilityRect, () => $"{ability.label}\n{DESCRIPTION_COOLDOWN} {ability.cooldown / TimeMetrics.TICKS_PER_HOUR}{DESCRIPTION_HOURS}", (int)abilityRect.x + 117858);
					abilityRect.x = abilityRect.xMax + SPACING;
				}

				Widgets.EndScrollView();
			}
		}

		public override void Delete(IGenebankEntry def)
		{
			_databank.RemoveFromDatabase(def as GenebankEntry<MutationDef>);
		}
	}
}
