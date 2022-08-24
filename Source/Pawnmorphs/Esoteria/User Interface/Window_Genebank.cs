﻿using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using Pawnmorph.User_Interface.Genebank;
using Pawnmorph.User_Interface.Genebank.Tabs;
using Pawnmorph.User_Interface.Preview;
using Pawnmorph.User_Interface.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface
{
    internal class Window_Genebank : Window
    {
        [DebugAction(category = "Pawnmorpher", name = "Open new genebank UI", actionType = DebugActionType.Action)]
        public static void OpenNewGenebank()
        {
            Find.WindowStack.Add(new Window_Genebank());
        }

        private static Type _priorMode = typeof(MutationsTab);
        private static readonly string HEADER = "PMGenebankHeader".Translate();
        private static readonly string CAPACITY_AVAILABLE = "PMGenebankAvailableHeader".Translate();
        private static readonly string CAPACITY_TOTAL = "PMGenebankTotalHeader".Translate();
        private static readonly float CAPACITY_WIDTH;

        private const float MAIN_COLUMN_WIDTH_FRACT = 0.60f;
        private const float SPACING = 10f;
        private const float HEADER_HEIGHT = 150;

        static Window_Genebank()
        {
            Text.Font = GameFont.Small;
            CAPACITY_WIDTH = Math.Max(Text.CalcSize(CAPACITY_AVAILABLE).x, Text.CalcSize(CAPACITY_TOTAL).x) * 2 + SPACING * 2;
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
            base.windowRect = new Rect(40, 40, Screen.width - 80, 830);
        }

        public override void PostOpen()
        {
            base.PostOpen();


            _tabs.Add(new TabRecord("Mutations", () => SelectTab(new MutationsTab()), () => _currentTab is MutationsTab));
            _tabs.Add(new TabRecord("Animals", () => SelectTab(new AnimalsTab()), () => _currentTab is AnimalsTab));



            _chamberDatabase = Find.World.GetComponent<ChamberDatabase>();

            if (_chamberDatabase == null)
                Log.Error("Unable to find chamber database world component!");

            SelectTab((GenebankTab)Activator.CreateInstance(_priorMode));
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (_chamberDatabase == null)
            {
                Widgets.Label(inRect, "NO DATABASE COMPONENT FOUND.");
                return;
            }



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
            if (Widgets.ButtonText(new Rect(footer.x, footer.y, 100f, footer.height), "Delete"))
            {
                DeleteSelection();
            }



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


        private void SelectTab(GenebankTab tab)
        {
            _currentTab = tab;
            _table.Clear();

            tab.Initialize(_chamberDatabase);
            tab.GenerateTable(_table);


            _table.AddColumn("Size", 100f, (ref Rect box, GeneRowItem item) =>
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