using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using Pawnmorph.UserInterface.Preview;
using Pawnmorph.UserInterface.TableBox;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.Genebank.Tabs
{
	internal class AnimalsTab : GenebankTab
	{

		private readonly string TAB_COLUMN_RACE = "PM_Genebank_AnimalsTab_Column_Race".Translate();

		private readonly string TAB_COLUMN_TEMPERATURE = "PM_Genebank_AnimalsTab_Column_Temperature".Translate();
		private readonly float TAB_COLUMN_TEMPERATURE_SIZE;

		private readonly string TAB_COLUMN_LIFESPAN = "PM_Genebank_AnimalsTab_Column_Lifespan".Translate();
		private readonly float TAB_COLUMN_LIFESPAN_SIZE;

		private readonly string TAB_COLUMN_DIET = "PM_Genebank_AnimalsTab_Column_Diet".Translate();
		private readonly float TAB_COLUMN_DIET_SIZE;

		private readonly string TAB_COLUMN_VALUE = "PM_Genebank_AnimalsTab_Column_Value".Translate();
		private readonly float TAB_COLUMN_VALUE_SIZE;

		private readonly string TAB_COLUMN_MUTATIONS = "PM_Genebank_AnimalsTab_Column_Mutations".Translate();
		private readonly float TAB_COLUMN_MUTATIONS_SIZE;

		private readonly string DESCRIPTION_MUTATIONS = "PM_Genebank_AnimalsTab_Details_Mutations".Translate();


		public AnimalsTab()
		{
			Text.Font = GameFont.Small;
			TAB_COLUMN_TEMPERATURE_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_TEMPERATURE).x, 100f);
			TAB_COLUMN_LIFESPAN_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_LIFESPAN).x, 60f);
			TAB_COLUMN_DIET_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_DIET).x, 100f);
			TAB_COLUMN_VALUE_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_VALUE).x, 75f);
			TAB_COLUMN_MUTATIONS_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_MUTATIONS).x, 75f);
		}

		PawnPreview _previewNorth;
		PawnPreview _previewEast;
		PawnPreview _previewSouth;
		ChamberDatabase _databank;
		string _animalDescription;
		StringBuilder _stringBuilder;
		Vector2 _descriptionScrollPosition;

		public override void Initialize(ChamberDatabase databank)
		{
			_databank = databank;
			_stringBuilder = new StringBuilder();
			int size = (int)PREVIEW_SIZE;

			_previewNorth = new PawnPreview(size, size, null)
			{
				Rotation = Rot4.North
			};


			_previewEast = new PawnPreview(size, size, null)
			{
				Rotation = Rot4.East,
				PreviewIndex = 2
			};


			_previewSouth = new PawnPreview(size, size, null)
			{
				Rotation = Rot4.South,
				PreviewIndex = 3
			};
		}


		public override void GenerateTable(Table<GeneRowItem> table)
		{
			var column = table.AddColumn(TAB_COLUMN_RACE, 0.25f, (ref Rect box, GeneRowItem item) => Widgets.Label(box, item.Label));
			column.IsFixedWidth = false;

			var colTemperature = table.AddColumn(TAB_COLUMN_TEMPERATURE, TAB_COLUMN_TEMPERATURE_SIZE);
			var colLifespan = table.AddColumn(TAB_COLUMN_LIFESPAN, TAB_COLUMN_LIFESPAN_SIZE, (x, ascending, column) =>
			{
				if (ascending)
					x.OrderBy(y => int.Parse(y[column]));
				else
					x.OrderByDescending(y => int.Parse(y[column]));
			});
			var colDiet = table.AddColumn(TAB_COLUMN_DIET, TAB_COLUMN_DIET_SIZE);
			var colValue = table.AddColumn(TAB_COLUMN_VALUE, TAB_COLUMN_VALUE_SIZE, (x, ascending, column) =>
			{
				if (ascending)
					x.OrderBy(y => int.Parse(y[column].Replace("$", "")));
				else
					x.OrderByDescending(y => int.Parse(y[column].Replace("$", "")));
			});
			var colMutations = table.AddColumn(TAB_COLUMN_MUTATIONS, TAB_COLUMN_MUTATIONS_SIZE);
			//Nutrition requirements?

			// Call column hook
			AddColumnHook(table);

			GeneRowItem row;
			int totalStorage = _databank.TotalStorage;
			foreach (GenebankEntry<PawnKindDef> animalEntry in _databank.GetEntryItems<PawnKindDef>())
			{
				PawnKindDef animal = animalEntry.Value;
				string searchText = animal.label;
				row = new GeneRowItem(animalEntry, totalStorage, searchText);

				// Comfortable temperature
				float? minTemp = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.ComfyTemperatureMin)?.value;
				float? maxTemp = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.ComfyTemperatureMax)?.value;
				if (minTemp.HasValue && maxTemp.HasValue)
					row[colTemperature] = $"{GenTemperature.CelsiusTo(minTemp.Value, Prefs.TemperatureMode)}~{GenText.ToStringTemperature(maxTemp.Value, "")}";

				// life expectancy
				row[colLifespan] = Mathf.FloorToInt(animal.RaceProps.lifeExpectancy).ToString();

				// Diet
				DietCategory diet = animal.RaceProps.ResolvedDietCategory;
				if (diet != DietCategory.NeverEats)
				{
					string dietString = diet.ToString();
					row[colDiet] = dietString;
					searchText += " " + dietString;
				}

				// Market value
				float? marketValue = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.MarketValue)?.value;
				if (marketValue.HasValue)
					row[colValue] = $"${marketValue.Value}";

				// Get mutations
				IReadOnlyList<MutationDef> animalMutations = animal.GetAllMutationsFrom();
				int totalMutations = animalMutations.Count();
				if (totalMutations > 0)
				{
					int taggedMutations = animalMutations.Intersect(_databank.StoredMutations).Count();
					row[colMutations] = $"{taggedMutations}/{totalMutations}";
				}

				// Call any hooks
				AddedRowHook(row, searchText);

				// CompProperties_Milkable
				row.SearchString = searchText.ToLower();
				table.AddRow(row);
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
			UpdatePreviews(selectedRows);
			UpdateDetails(selectedRows);
		}

		private void UpdatePreviews(IReadOnlyList<GeneRowItem> selectedRows)
		{
			PawnKindDef selectedRace = null;
			if (selectedRows.Count == 1)
				selectedRace = (selectedRows[0].RowObject as GenebankEntry<PawnKindDef>).Value;

			_previewNorth.PawnKindDef = selectedRace;
			_previewEast.PawnKindDef = selectedRace;
			_previewSouth.PawnKindDef = selectedRace;

			_previewNorth.Refresh();
			_previewEast.Refresh();
			_previewSouth.Refresh();
		}

		private void UpdateDetails(IReadOnlyList<GeneRowItem> selectedRows)
		{
			if (selectedRows.Count == 1)
			{
				PawnKindDef selectedRace = (selectedRows[0].RowObject as GenebankEntry<PawnKindDef>).Value;


				_stringBuilder.Clear();
				foreach (StatDrawEntry item in selectedRace.RaceProps.SpecialDisplayStats(selectedRace.race, StatRequest.ForEmpty()))
				{
					if (item.ShouldDisplay())
						_stringBuilder.AppendLine(item.LabelCap + ": " + item.ValueString);
				}


				IReadOnlyList<MutationDef> animalMutations = selectedRace.GetAllMutationsFrom();
				int totalMutations = animalMutations.Count();
				if (totalMutations > 0)
				{
					_stringBuilder.AppendLine();
					_stringBuilder.AppendLine(DESCRIPTION_MUTATIONS);
					IEnumerable<MutationDef> taggedMutations = animalMutations.Intersect(_databank.StoredMutations);
					foreach (MutationDef mutation in taggedMutations)
					{
						_stringBuilder.AppendLine(mutation.LabelCap);
					}

				}
				_animalDescription = _stringBuilder.ToString();
			}
		}


		public override void DrawDetails(Rect inRect)
		{
			Rect previewBox = new Rect(inRect.x, inRect.y, PREVIEW_SIZE, PREVIEW_SIZE);
			DrawPreviews(previewBox);

			Text.Font = GameFont.Small;
			Rect descriptionBox = new Rect(inRect.x + previewBox.width + SPACING, inRect.y, inRect.width - previewBox.width - SPACING, previewBox.height * 3 + SPACING * 2);
			Widgets.LabelScrollable(descriptionBox, _animalDescription, ref _descriptionScrollPosition);
		}

		private void DrawPreviews(Rect previewBox)
		{
			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_previewEast.Draw(previewBox.ContractedBy(3));

			previewBox.y = previewBox.yMax + SPACING;

			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_previewSouth.Draw(previewBox.ContractedBy(3));

			previewBox.y = previewBox.yMax + SPACING;

			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_previewNorth.Draw(previewBox.ContractedBy(3));

			if (_previewEast.PawnKindDef != null)
			{
				if (_previewEast.PawnKindDef.fixedGender == null && _previewEast.PawnKindDef.RaceProps.hasGenders)
				{
					previewBox.y = previewBox.yMax + SPACING;
					previewBox.width = 75f;
					previewBox.height = 20f;


					if (Widgets.ButtonText(previewBox, MALE))
						SetPreviewGender(Gender.Male);

					previewBox.x = previewBox.x + PREVIEW_SIZE - previewBox.width;
					if (Widgets.ButtonText(previewBox, FEMALE))
						SetPreviewGender(Gender.Female);
				}
			}
		}

		private void SetPreviewGender(Gender gender)
		{
			_previewEast.SetGender(gender);
			_previewNorth.SetGender(gender);
			_previewSouth.SetGender(gender);

			_previewEast.Refresh();
			_previewNorth.Refresh();
			_previewSouth.Refresh();
		}

		public override void Delete(IGenebankEntry def)
		{
			_databank.RemoveFromDatabase(def as GenebankEntry<PawnKindDef>);
		}
	}
}
