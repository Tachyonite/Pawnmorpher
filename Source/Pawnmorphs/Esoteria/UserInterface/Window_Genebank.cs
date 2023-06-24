using System;
using System.Collections.Generic;
using Pawnmorph.Chambers;
using Pawnmorph.UserInterface.Genebank;
using Pawnmorph.UserInterface.Genebank.Tabs;
using Pawnmorph.UserInterface.TableBox;
using RimWorld;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface
{
	internal class Window_Genebank : Window
	{
		static Type _priorMode = typeof(MutationsTab);
		private readonly string HEADER = "PM_Genebank_Header".Translate();
		private readonly string CAPACITY_AVAILABLE = "PM_Genebank_AvailableHeader".Translate();
		private readonly string CAPACITY_TOTAL = "PM_Genebank_TotalHeader".Translate();
		private readonly float CAPACITY_WIDTH;


		private readonly string TAB_MUTATIONS_HEADER = "PM_Genebank_MutationTab_Caption".Translate();
		private readonly string TAB_ANIMALS_HEADER = "PM_Genebank_AnimalsTab_Caption".Translate();
		private readonly string TAB_TEMPLATES_HEADER = "PM_Genebank_TemplateTab_Caption".Translate();

		private readonly string COLUMN_SIZE = "PM_Column_Stats_Size".Translate();
		private readonly float COLUMN_SIZE_SIZE;

		private readonly string BUTTON_DELETE = "PM_Genebank_DeleteButton".Translate();
		private readonly float BUTTON_DELETE_SIZE;

		private readonly string BUTTON_FONT = "PM_Genebank_FontButton".Translate();
		private readonly float BUTTON_FONT_SIZE;
		private readonly string BUTTON_FONT_TINY = "PM_Genebank_FontButtonTiny".Translate();
		private readonly string BUTTON_FONT_SMALL = "PM_Genebank_FontButtonSmall".Translate();
		private readonly string BUTTON_FONT_MEDIUM = "PM_Genebank_FontButtonMedium".Translate();

		private const float MAIN_COLUMN_WIDTH_FRACT = 0.60f;
		private const float SPACING = 10f;
		private const float HEADER_HEIGHT = 150;

		public Window_Genebank()
		{
			Text.Font = GameFont.Small;
			CAPACITY_WIDTH = Math.Max(Text.CalcSize(CAPACITY_AVAILABLE).x, Text.CalcSize(CAPACITY_TOTAL).x) * 2 + SPACING * 2;
			COLUMN_SIZE_SIZE = Mathf.Max(Text.CalcSize(COLUMN_SIZE).x, 100f);
			BUTTON_DELETE_SIZE = Mathf.Max(Text.CalcSize(BUTTON_DELETE).x, 100f);
			BUTTON_FONT_SIZE = Mathf.Max(Text.CalcSize(BUTTON_FONT).x, 80f);


			_tabs = new List<TabRecord>();
			_table = new Table<GeneRowItem>((item, text) => item.SearchString.Contains(text));
			_table.SelectionChanged += Table_SelectionChanged;
			_table.MultiSelect = true;

			this.resizeable = true;
			this.draggable = true;
			this.doCloseX = true;
		}

		private readonly List<TabRecord> _tabs;
		private readonly Table<GeneRowItem> _table;
		private float _mainWidth;
		private float _detailsWidth;
		private float _currentY;
		private ChamberDatabase _chamberDatabase;
		private GenebankTab _currentTab;


		private void Table_SelectionChanged(object sender, IReadOnlyList<GeneRowItem> e)
		{
			_currentTab.SelectionChanged(e);
		}

		protected override void SetInitialSizeAndPosition()
		{
			base.SetInitialSizeAndPosition();

			Vector2 location = PawnmorpherMod.Settings.GenebankWindowLocation ?? new Vector2(40, 40);
			Vector2 size = PawnmorpherMod.Settings.GenebankWindowSize ?? new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.8f);
			_table.LineFont = PawnmorpherMod.Settings.GenebankWindowFont ?? GameFont.Tiny;

			

			base.windowRect = new Rect(location, size);
		}

		public override void PostOpen()
		{
			base.PostOpen();


			_chamberDatabase = Find.World.GetComponent<ChamberDatabase>();
			if (_chamberDatabase == null)
			{
				Log.Error("Unable to find chamber database world component!");
				this.Close();
			}



			_tabs.Add(new TabRecord(TAB_MUTATIONS_HEADER, () => SelectTab(new MutationsTab()), () => _currentTab is MutationsTab));
			_tabs.Add(new TabRecord(TAB_ANIMALS_HEADER, () => SelectTab(new AnimalsTab()), () => _currentTab is AnimalsTab));
			_tabs.Add(new TabRecord(TAB_TEMPLATES_HEADER, () => SelectTab(new TemplatesTab()), () => _currentTab is TemplatesTab));

			SelectTab((GenebankTab)Activator.CreateInstance(_priorMode));
		}

		public override void DoWindowContents(Rect inRect)
		{
			if (_chamberDatabase == null)
				return;

			// Apply edge spacing
			inRect = inRect.ContractedBy(SPACING);

			_mainWidth = inRect.width * MAIN_COLUMN_WIDTH_FRACT;
			_detailsWidth = inRect.width - _mainWidth - SPACING;


			Rect headerBox = new Rect(inRect.x, inRect.y, _mainWidth, HEADER_HEIGHT);
			DrawHeader(headerBox);


			Rect mainBox = new Rect(inRect.x, HEADER_HEIGHT + SPACING, _mainWidth, inRect.height - HEADER_HEIGHT + SPACING);
			mainBox.y += TabDrawer.TabHeight;
			mainBox.height -= TabDrawer.TabHeight;

			Widgets.DrawBoxSolidWithOutline(mainBox, Color.black, Color.gray);


			Rect footer = new Rect(mainBox.x, mainBox.yMax - 40, mainBox.width, 40);
			mainBox.height -= footer.height;
			footer = footer.ContractedBy(SPACING);

			TabDrawer.DrawTabs(mainBox, _tabs);
			_table.Draw(mainBox.ContractedBy(SPACING));

			Text.Font = GameFont.Small;

			Widgets.DrawLineHorizontal(footer.x, footer.y - SPACING, footer.width);
			if (Widgets.ButtonText(new Rect(footer.x, footer.y, BUTTON_DELETE_SIZE, footer.height), BUTTON_DELETE))
			{
				DeleteSelection();
			}

			if (Widgets.ButtonText(new Rect(footer.xMax - BUTTON_FONT_SIZE, footer.y, BUTTON_FONT_SIZE, footer.height), BUTTON_FONT))
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>(3);
				options.Add(new FloatMenuOption(BUTTON_FONT_TINY, () => _table.LineFont = GameFont.Tiny));
				options.Add(new FloatMenuOption(BUTTON_FONT_SMALL, () => _table.LineFont = GameFont.Small));
				options.Add(new FloatMenuOption(BUTTON_FONT_MEDIUM, () => _table.LineFont = GameFont.Medium));
				Find.WindowStack.Add(new FloatMenu(options));
			}

			footer.width -= BUTTON_FONT_SIZE;

			if (_currentTab != null)
				_currentTab.DrawFooter(new Rect(footer.x + BUTTON_DELETE_SIZE + SPACING, footer.y, footer.width - BUTTON_DELETE_SIZE - SPACING, footer.height));


			Rect detailsBox = new Rect(inRect.xMax - _detailsWidth, inRect.y, _detailsWidth, inRect.height + SPACING);
			Widgets.DrawBoxSolidWithOutline(detailsBox, Color.black, Color.gray);

			if (_currentTab != null)
				_currentTab.DrawDetails(detailsBox.ContractedBy(SPACING));
		}

		private void DeleteSelection()
		{
			foreach (GeneRowItem item in _table.SelectedRows)
			{
				_currentTab.Delete(item.RowObject);
				_table.DeleteRow(item);
			}
			_table.Refresh();
		}

		public override void PreClose()
		{
			PawnmorpherMod.Settings.GenebankWindowLocation = base.windowRect.position;
			PawnmorpherMod.Settings.GenebankWindowSize = base.windowRect.size;
			PawnmorpherMod.Settings.GenebankWindowFont = _table.LineFont;

			PawnmorpherMod.Settings.Mod.WriteSettings();
			base.PreClose();
		}

		public override void PostClose()
		{
			// Remember selected tab for next time interface is opened.
			_priorMode = _currentTab.GetType();

			base.PostClose();
		}

		private void DrawHeader(Rect inRect)
		{
			Text.Font = GameFont.Medium;

			// Title
			Widgets.Label(inRect, HEADER);

			_currentY = inRect.y + Text.LineHeight * 2;
			Rect capacity = new Rect(inRect.x, _currentY, CAPACITY_WIDTH, inRect.height - _currentY);
			Rect capSection = new Rect(capacity);
			capSection.width = (capSection.width - SPACING) / 2;

			Text.Font = GameFont.Small;
			Widgets.Label(capSection, CAPACITY_AVAILABLE);

			capSection.x = capSection.width + SPACING;
			Widgets.Label(capSection, CAPACITY_TOTAL);

			capSection.y += Text.LineHeight;

			float freeCapacity = _chamberDatabase.FreeStorage;
			float totalCapacity = _chamberDatabase.TotalStorage;

			capSection.x = inRect.x;
			Widgets.Label(capSection, DatabaseUtilities.GetStorageString(freeCapacity));

			capSection.x = capSection.width + SPACING;
			Widgets.Label(capSection, DatabaseUtilities.GetStorageString(totalCapacity));

			capacity.y = capSection.y + Text.LineHeight;

			// Capacity meter.
			float usedCapacity = _chamberDatabase.UsedStorage;
			Rect capacityBarRect = new Rect(capacity.x, capSection.y + Text.LineHeight, capacity.width, Text.LineHeight);
			Widgets.DrawBoxSolid(capacityBarRect, Color.black);
			capacityBarRect = capacityBarRect.ContractedBy(3);
			capacityBarRect.width = Mathf.Max(0f, capacityBarRect.width * (usedCapacity / totalCapacity));
			Widgets.DrawBoxSolid(capacityBarRect, Color.Lerp(Color.green, Color.red, usedCapacity / totalCapacity));
		}


		public void SelectTab(GenebankTab tab)
		{
			_currentTab = tab;
			_table.Clear();
			tab.Parent = this;
			tab.Initialize(_chamberDatabase);
			tab.GenerateTable(_table);


			_table.AddColumn(COLUMN_SIZE, COLUMN_SIZE_SIZE, (ref Rect box, GeneRowItem item) =>
			{
				box.width = box.width / 2 - 5;
				Widgets.Label(box, item.StorageSpaceUsed);

				box.x = box.xMax + 5;
				Widgets.Label(box, item.StorageSpaceUsedPercentage);
			}, (collection, ascending, column) =>
			{
				if (ascending)
					collection.OrderBy(x => x.Size);
				else
					collection.OrderByDescending(x => x.Size);
			});


			_table.Refresh();
		}

		internal void ResetToDefaults()
		{
			PawnmorpherMod.Settings.GenebankWindowLocation = null;
			PawnmorpherMod.Settings.GenebankWindowSize = null;
			PawnmorpherMod.Settings.GenebankWindowFont = null;
		}
	}
}
