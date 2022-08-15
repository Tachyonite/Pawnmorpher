using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
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

        private static Mode _priorMode = Mode.Mutations;

        private const float MAIN_COLUMN_WIDTH_FRACT = 0.60f;
        private const float SPACING = 10f;
        private const float HEADER_HEIGHT = 150;

        private enum Mode
        {
            Mutations,
            Animal,
            //Races,
            //Templates,
        }

        private class RowItem : ITableRow
        {
            public readonly Def Def;
            public readonly string Label;
            public readonly string StorageSpaceUsed;
            public readonly string StorageSpaceUsedPercentage;
            public readonly int Size;
            public string SearchString;

            public Dictionary<TableColumn, string> RowData { get; }

            private RowItem(Def def)
            {
                RowData = new Dictionary<TableColumn, string>();
                Label = def.LabelCap;
                Def = def;
                StorageSpaceUsedPercentage = "0%";
            }

            public RowItem(MutationDef def, int totalCapacity, string searchString)
                : this(def)
            {
                SearchString = searchString.ToLower();
                Size = def.GetRequiredStorage();
                StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
                if (totalCapacity > 0)
                    StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
            }

            public RowItem(PawnKindDef def, int totalCapacity, string searchString)
                : this(def)
            {
                SearchString = searchString.ToLower();
                Size = def.GetRequiredStorage();
                StorageSpaceUsed = DatabaseUtilities.GetStorageString(Size);
                if (totalCapacity > 0)
                    StorageSpaceUsedPercentage = ((float)Size / totalCapacity).ToStringPercent();
            }
        }

        private static string HEADER = "PMGenebankHeader".Translate();
        private static string CAPACITY_AVAILABLE = "PMGenebankAvailableHeader".Translate();
        private static string CAPACITY_TOTAL = "PMGenebankTotalHeader".Translate();

        private static float CAPACITY_WIDTH;

        static Window_Genebank()
        {
            Text.Font = GameFont.Small;
            CAPACITY_WIDTH = Math.Max(Text.CalcSize(CAPACITY_AVAILABLE).x, Text.CalcSize(CAPACITY_TOTAL).x) * 2 + SPACING*2;
        }


        private List<TabRecord> _tabs;
        private Mode _currentTab;
        private float _mainWidth;
        private float _detailsWidth;
        private float _currentY;
        private ChamberDatabase _chamberDatabase;
        private TableBox.Table<RowItem> _table;
        private int _totalCapacity;

        public Window_Genebank()
        {
            _tabs = new List<TabRecord>();
            _table = new TableBox.Table<RowItem>((item, text) => item.SearchString.Contains(text));

            this.resizeable = true;
            this.doCloseX = true;
        }

        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            base.windowRect = new Rect(40, 40, Screen.width - 80, Screen.height - 80);
        }

        public override void PostOpen()
        {
            base.PostOpen();

            IEnumerable<Mode> modes = Enum.GetValues(typeof(Mode)).Cast<Mode>();

            foreach (Mode mode in modes)
            {
                _tabs.Add(new TabRecord(mode.ToString(), () => SelectTab(mode), () => _currentTab == mode));
            }
            _chamberDatabase = Find.World.GetComponent<ChamberDatabase>();
            _totalCapacity = _chamberDatabase.TotalStorage;

            if (_chamberDatabase == null)
                Log.Error("Unable to find chamber database world component!");

            SelectTab(_priorMode);
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
                foreach (RowItem item in _table.SelectedRows)
                {
                    Delete(item.Def);
                    _table.DeleteRow(item);
                }

                _table.Refresh();
            }



            Rect detailsBox = new Rect(inRect.xMax - _detailsWidth, inRect.y, _detailsWidth, inRect.height + SPACING);
            Widgets.DrawBoxSolidWithOutline(detailsBox, Color.black, Color.gray);
        }

        public override void PostClose()
        {
            // Remember selected tab for next time interface is opened.
            _priorMode = _currentTab;

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

            capSection.x = inRect.x;
            Widgets.Label(capSection, DatabaseUtilities.GetStorageString(freeCapacity));

            capSection.x = capSection.width + SPACING;
            Widgets.Label(capSection, DatabaseUtilities.GetStorageString(_totalCapacity));

            capacity.y = capSection.y + Text.LineHeight;

            // Capacity meter.
            Rect capacityBarRect = new Rect(capacity.x, capSection.y + Text.LineHeight, capacity.width, Text.LineHeight);
            Widgets.DrawBoxSolid(capacityBarRect, Color.black);
            capacityBarRect = capacityBarRect.ContractedBy(3);
            capacityBarRect.width = Mathf.Max(0f, capacityBarRect.width / _totalCapacity * freeCapacity);
            Widgets.DrawBoxSolid(capacityBarRect, Color.green);
        }


        private void SelectTab(Mode mode)
        {
            _currentTab = mode;
            _table.Clear();

            switch (mode)
            {
                case Mode.Animal:
                    GenerateAnimalTabContent();
                    break;

                case Mode.Mutations:
                    GenerateMutationsTabContent();
                    break;
            }


            _table.AddColumn("Size", 100f, (ref Rect box, RowItem item) =>
            {
                box.width = box.width / 2 - 5;
                Widgets.Label(box, item.StorageSpaceUsed);

                box.x = box.xMax + 5;
                Widgets.Label(box, item.StorageSpaceUsedPercentage);
            }, (collection, ascending) =>
            {
                if (ascending)
                    collection.OrderBy(x => x.Size);
                else
                    collection.OrderByDescending(x => x.Size);
            });


            _table.Refresh();
        }


        private void GenerateAnimalTabContent()
        {
            _currentTab = Mode.Animal;

            var column = _table.AddColumn("Race", 0.25f, (ref Rect box, RowItem item) => Widgets.Label(box, item.Label));
            column.IsFixedWidth = false;

            RowItem item;
            foreach (PawnKindDef animal in _chamberDatabase.TaggedAnimals)
            {
                string searchText = animal.label;
                item = new RowItem(animal, _totalCapacity, searchText);

                _table.AddRow(item);
            }
        }

        private void GenerateMutationsTabContent()
        {
            _currentTab = Mode.Mutations;

            var column = _table.AddColumn("Mutation", 0.5f, 
                (ref Rect box, RowItem item) => Widgets.Label(box, item.Label), 
                (collection, ascending) =>
                {
                    if (ascending)
                        collection.OrderBy(x => x.Label);
                    else
                        collection.OrderByDescending(x => x.Label);
                });
            column.IsFixedWidth = false;

            TableColumn colParagon = _table.AddColumn("Paragon", 60f);
            TableColumn colAbilities = _table.AddColumn("Abilities", 100f);
            TableColumn colStats = _table.AddColumn("Stats", 0.5f);
            colStats.IsFixedWidth = false;

            RowItem item;
            foreach (MutationDef mutation in _chamberDatabase.StoredMutations)
            {
                string searchText = mutation.label;
                item = new RowItem(mutation, _totalCapacity, searchText);

                if (mutation.stages == null)
                    continue;

                var stages = mutation.stages.OfType<MutationStage>().ToList();
                if (stages.Count > 0)
                {
                    var lastStage = stages[stages.Count - 1];


                    // If any stage has abilities, it will be the last one. Paragon or not.
                    if (lastStage.abilities != null)
                    {
                        string abilities = String.Join(", ", lastStage.abilities.Select(x => x.label));
                        item.RowData[colAbilities] = abilities;
                        searchText += " " + abilities;
                    }

                    if (lastStage.key == "paragon")
                    {
                        item.RowData[colParagon] = "Paragon";
                        lastStage = stages[stages.Count - 2];
                        searchText += " " + "paragon";
                    }

                    List<StatDef> stats = new List<StatDef>();
                    if (lastStage.statFactors != null)
                        stats.AddRange(lastStage.statFactors.Where(x => x.value > 1).Select(x => x.stat));

                    if (lastStage.statOffsets != null)
                        stats.AddRange(lastStage.statOffsets.Where(x => x.value > 0).Select(x => x.stat));

                    string statsImpact = String.Join(", ", stats.Where(x => x != null).Select(x => x.LabelCap).Distinct());
                    searchText += " " + statsImpact;

                    item.RowData[colStats] = statsImpact;
                }

                item.SearchString = searchText.ToLower();
                _table.AddRow(item);
            }
        }

        private void GenerateRacesTabContent()
        {

        }

        private void GenerateTemplatesTabContent()
        {

        }

        private void GenerateDetailsWindow()
        {

        }

        private void Delete(Def def)
        {
            //explicit type casts here are hacky but best way to reuse the gui code 
            if (def is PawnKindDef pkDef)
                _chamberDatabase.RemoveFromDatabase(pkDef);
            else if (def is MutationDef mDef)
                _chamberDatabase.RemoveFromDatabase(mDef);
            else
                throw new NotImplementedException(nameof(Delete) + " is not implemented for " + def.GetType().Name + "!");
        }
    }
}
