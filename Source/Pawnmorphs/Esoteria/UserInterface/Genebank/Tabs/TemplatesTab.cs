using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using Pawnmorph.UserInterface.PartPicker;
using Pawnmorph.UserInterface.Preview;
using Pawnmorph.UserInterface.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.Genebank.Tabs
{
	internal class TemplatesTab : GenebankTab
	{
		MutationTemplate _selectedTemplate;
		ChamberDatabase _databank;
		HumanlikePreview _previewNorth;
		HumanlikePreview _previewEast;
		HumanlikePreview _previewSouth;
		StringBuilder _stringBuilder;
		Vector2 _detailsScrollPosition;



		private static readonly string BUTTON_IMPORT = "PM_Genebank_TemplateTab_ImportButton".Translate();
		private static readonly float BUTTON_IMPORT_SIZE;

		private static readonly string BUTTON_EXPORT = "PM_Genebank_TemplateTab_ExportButton".Translate();
		private static readonly float BUTTON_EXPORT_SIZE;

		static TemplatesTab()
		{
			BUTTON_IMPORT_SIZE = Mathf.Max(Text.CalcSize(BUTTON_IMPORT).x, 100f);
			BUTTON_EXPORT_SIZE = Mathf.Max(Text.CalcSize(BUTTON_EXPORT).x, 100f);
		}

		public override void Initialize(ChamberDatabase databank)
		{
			_databank = databank;
			_stringBuilder = new StringBuilder();

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
			var column = table.AddColumn("Template", 0.5f,
				(ref Rect box, GeneRowItem item) => Widgets.Label(box, item.Label),
				(collection, ascending, column) =>
				{
					if (ascending)
						collection.OrderBy(x => x.Label);
					else
						collection.OrderByDescending(x => x.Label);
				});
			column.IsFixedWidth = false;

			var mutationsColumn = table.AddColumn("Mutations", 100);

			AddColumnHook(table);

			foreach (MutationTemplate template in _databank.MutationTemplates)
			{
				string searchText = template.Caption;
				GeneRowItem row = new GeneRowItem(template, _databank.TotalStorage, searchText);


				row[mutationsColumn] = template.MutationData.Count().ToString();


				AddedRowHook(row, searchText);
				row.SearchString = searchText.ToLower();
				table.AddRow(row);
			}
		}


		public override void DrawDetails(Rect inRect)
		{
			DrawPreview(inRect);

			if (_selectedTemplate != null)
			{
				float curY = inRect.y;
				float width = inRect.width - PREVIEW_SIZE - SPACING;
				float x = inRect.xMax - width;

				Text.Font = GameFont.Medium;
				Widgets.LongLabel(x, inRect.width, _selectedTemplate.Caption, ref curY);

				curY += SPACING;


				_stringBuilder.Clear();
				foreach (MutationTemplateData mutation in _selectedTemplate.MutationData)
				{
					_stringBuilder.Append(mutation.MutationDef.LabelCap);
					_stringBuilder.Append(" (");
					_stringBuilder.Append(mutation.PartLabelCap);
					_stringBuilder.AppendLine(")");
				}

				Text.Font = GameFont.Small;
				Widgets.LabelScrollable(new Rect(x, curY, width, inRect.height - curY), _stringBuilder.ToString(), ref _detailsScrollPosition);
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

		private void UpdatePreviews()
		{
			_previewNorth.ClearMutations();
			_previewEast.ClearMutations();
			_previewSouth.ClearMutations();

			if (_selectedTemplate != null)
			{
				foreach (var item in _selectedTemplate.MutationData)
				{
					MutationDef mutation = item.MutationDef;
					_previewNorth.AddMutation(mutation);
					_previewEast.AddMutation(mutation);
					_previewSouth.AddMutation(mutation);
				}
			}

			_previewNorth.Refresh();
			_previewEast.Refresh();
			_previewSouth.Refresh();
		}


		public override void DrawFooter(Rect footer)
		{
			float currentX = footer.x;
			if (Widgets.ButtonText(new Rect(currentX, footer.y, BUTTON_IMPORT_SIZE, footer.height), BUTTON_IMPORT))
			{
				if (_selectedTemplate != null)
				{
					string serialized = _selectedTemplate.Serialize();
					Dialog_Textbox textbox = new Dialog_Textbox(serialized, true, new Vector2(300, 125));
					Find.WindowStack.Add(textbox);
				}
			}

			currentX += SPACING + BUTTON_IMPORT_SIZE;
			if (Widgets.ButtonText(new Rect(currentX, footer.y, BUTTON_EXPORT_SIZE, footer.height), BUTTON_EXPORT))
			{

				Dialog_Textbox textbox = new Dialog_Textbox(String.Empty, false, new Vector2(300, 125));
				textbox.ApplyAction += Import;
				Find.WindowStack.Add(textbox);
			}
		}

		private void Import(string serializedTemplate)
		{
			if (MutationTemplate.TryDeserialize(serializedTemplate, out var template))
			{
				// Imported
				if (_databank.CanAddToDatabase(template, out string reason))
				{
					_databank.AddToDatabase(template);
					Parent.SelectTab(new TemplatesTab()); // Reload tab.
					return;
				}

				Dialog_Popup messageBox = new Dialog_Popup(reason, new Vector2(300, 125));
				Find.WindowStack.Add(messageBox);
			}
			else
			{
				Dialog_Popup messageBox = new Dialog_Popup("Failed to parse text.", new Vector2(300, 125));
				Find.WindowStack.Add(messageBox);
			}
		}

		public override void SelectionChanged(IReadOnlyList<GeneRowItem> selectedRows)
		{
			_selectedTemplate = null;

			if (selectedRows.Count == 1)
			{
				_selectedTemplate = selectedRows[0].Def as MutationTemplate;
			}


			UpdatePreviews();
		}

		public override void Delete(object def)
		{
			_databank.RemoveFromDatabase(def as MutationTemplate);
		}

		public override void AddColumnHook(Table<GeneRowItem> table)
		{
		}

		public override void AddedRowHook(GeneRowItem row, string searchText)
		{
		}
	}
}
