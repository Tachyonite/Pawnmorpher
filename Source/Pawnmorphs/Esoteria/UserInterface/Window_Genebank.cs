﻿using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using Pawnmorph.UserInterface.Genebank;
using Pawnmorph.UserInterface.Genebank.Tabs;
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

namespace Pawnmorph.UserInterface
{
    internal class Window_Genebank : Window
    {
        private static Type _priorMode = typeof(MutationsTab);
        private static readonly string HEADER = "PM_Genebank_Header".Translate();
        private static readonly string CAPACITY_AVAILABLE = "PM_Genebank_AvailableHeader".Translate();
        private static readonly string CAPACITY_TOTAL = "PM_Genebank_TotalHeader".Translate();
        private static readonly float CAPACITY_WIDTH;


        private static readonly string TAB_MUTATIONS_HEADER = "PM_Genebank_MutationTab_Caption".Translate();
        private static readonly string TAB_ANIMALS_HEADER = "PM_Genebank_AnimalsTab_Caption".Translate();
		private static readonly string TAB_TEMPLATES_HEADER = "PM_Genebank_TemplateTab_Caption".Translate();

		private static readonly string COLUMN_SIZE = "PM_Column_Stats_Size".Translate();
        private static readonly float COLUMN_SIZE_SIZE;

        private static readonly string BUTTON_DELETE = "PM_Genebank_DeleteButton".Translate();
        private static readonly float BUTTON_DELETE_SIZE;


        private const float MAIN_COLUMN_WIDTH_FRACT = 0.60f;
        private const float SPACING = 10f;
        private const float HEADER_HEIGHT = 150;

        static Window_Genebank()
        {
            Text.Font = GameFont.Small;
            CAPACITY_WIDTH = Math.Max(Text.CalcSize(CAPACITY_AVAILABLE).x, Text.CalcSize(CAPACITY_TOTAL).x) * 2 + SPACING * 2;
            COLUMN_SIZE_SIZE = Mathf.Max(Text.CalcSize(COLUMN_SIZE).x, 100f);
            BUTTON_DELETE_SIZE = Mathf.Max(Text.CalcSize(BUTTON_DELETE).x, 100f);
        }

        private readonly List<TabRecord> _tabs;
        private readonly Table<GeneRowItem> _table;
        private float _mainWidth;
        private float _detailsWidth;
        private float _currentY;
        private ChamberDatabase _chamberDatabase;
        private GenebankTab _currentTab;

        public Window_Genebank()
        {
            _tabs = new List<TabRecord>();
            _table = new Table<GeneRowItem>((item, text) => item.SearchString.Contains(text));
            _table.SelectionChanged += Table_SelectionChanged;

            this.resizeable = true;
            this.draggable = true;
            this.doCloseX = true;
        }

        private void Table_SelectionChanged(object sender, IReadOnlyList<GeneRowItem> e)
        {
            _currentTab.SelectionChanged(e);
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            //base.windowRect = new Rect(40, 40, Screen.width - 80, Screen.height - 80);
            base.windowRect = new Rect(40, 40, Screen.width - 80, 860);
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
            
            Widgets.DrawLineHorizontal(footer.x, footer.y - SPACING, footer.width);
            if (Widgets.ButtonText(new Rect(footer.x, footer.y, BUTTON_DELETE_SIZE, footer.height), BUTTON_DELETE))
            {
                DeleteSelection();
            }

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
                _currentTab.Delete(item.Def);
                _table.DeleteRow(item);
            }
            _table.Refresh();
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

    }
}
