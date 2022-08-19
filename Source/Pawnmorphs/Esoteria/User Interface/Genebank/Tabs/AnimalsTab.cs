using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.User_Interface.Preview;
using Pawnmorph.User_Interface.TableBox;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface.Genebank.Tabs
{
    internal class AnimalsTab : GenebankTab
    {
        PawnKindDefPreview _previewNorth;
        PawnKindDefPreview _previewEast;
        PawnKindDefPreview _previewSouth;
        ChamberDatabase _databank;

        public override void Initialize(ChamberDatabase databank)
        {
            _databank = databank;

            _previewNorth = new PawnKindDefPreview(200, 200, null)
            {
                Rotation = Rot4.North
            };


            _previewEast = new PawnKindDefPreview(200, 200, null)
            {
                Rotation = Rot4.East,
                PreviewIndex = 2
            };


            _previewSouth = new PawnKindDefPreview(200, 200, null)
            {
                Rotation = Rot4.South,
                PreviewIndex = 3
            };
        }


        public override void GenerateTable(Table<GeneRowItem> table)
        {
            var column = table.AddColumn("Race", 0.25f, (ref Rect box, GeneRowItem item) => Widgets.Label(box, item.Label));
            column.IsFixedWidth = false;

            var colTemperature = table.AddColumn("Temperature", 100f);
            var colLifespan = table.AddColumn("Tolerance", 60f);
            var colDiet = table.AddColumn("Diet", 100f);
            var colValue = table.AddColumn("Value", 75f);
            //Nutrition requirements?

            GeneRowItem item;
            int totalStorage = _databank.TotalStorage;
            foreach (PawnKindDef animal in _databank.TaggedAnimals)
            {
                string searchText = animal.label;
                item = new GeneRowItem(animal, totalStorage, searchText);

                float? minTemp = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.ComfyTemperatureMin)?.value;
                float? maxTemp = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.ComfyTemperatureMax)?.value;
                if (minTemp.HasValue && maxTemp.HasValue)
                    item.RowData[colTemperature] = $"{minTemp.Value}-{maxTemp.Value}c";

                item.RowData[colLifespan] = Mathf.FloorToInt(animal.RaceProps.lifeExpectancy).ToString();

                DietCategory diet = animal.RaceProps.ResolvedDietCategory;
                if (diet != DietCategory.NeverEats)
                    item.RowData[colDiet] = diet.ToString();

                float? marketValue = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.MarketValue)?.value;
                if (marketValue.HasValue)
                    item.RowData[colValue] = $"${marketValue.Value}";

                // CompProperties_Milkable

                table.AddRow(item);
            }
        }

        public override void SelectionChanged(IReadOnlyList<GeneRowItem> selectedRows)
        {
            if (selectedRows.Count == 1)
            {
                UpdatePreviews(selectedRows[0].Def as PawnKindDef);
                return;
            }
            UpdatePreviews(null);
        }

        private void UpdatePreviews(PawnKindDef thing)
        {
            _previewNorth.Thing = thing;
            _previewEast.Thing = thing;
            _previewSouth.Thing = thing;

            _previewNorth.Refresh();
            _previewEast.Refresh();
            _previewSouth.Refresh();
        }


        public override void DrawDetails(Rect inRect)
        {
            Rect previewBox = new Rect(inRect.x, inRect.y, 200, 200);

            Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
            _previewNorth.Draw(previewBox);

            previewBox.y = previewBox.yMax + SPACING;

            Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
            _previewEast.Draw(previewBox);

            previewBox.y = previewBox.yMax + SPACING;

            Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
            _previewSouth.Draw(previewBox);
        }

        public override void Delete(Def def)
        {
            _databank.RemoveFromDatabase(def as PawnKindDef);
        }
    }
}
