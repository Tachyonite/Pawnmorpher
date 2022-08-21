using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.User_Interface.Preview;
using Pawnmorph.User_Interface.TableBox;
using RimWorld;
using RimWorld.BaseGen;
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
        string _animalDescription;
        StringBuilder _stringBuilder;
        Vector2 _descriptionScrollPosition;

        public override void Initialize(ChamberDatabase databank)
        {
            _databank = databank;
            _stringBuilder = new StringBuilder();
            int size = (int)PREVIEW_SIZE;

            _previewNorth = new PawnKindDefPreview(size, size, null)
            {
                Rotation = Rot4.North
            };


            _previewEast = new PawnKindDefPreview(size, size, null)
            {
                Rotation = Rot4.East,
                PreviewIndex = 2
            };


            _previewSouth = new PawnKindDefPreview(size, size, null)
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
            var colLifespan = table.AddColumn("Lifespan", 60f, (x, ascending, column) =>
            {
                if (ascending)
                    x.OrderBy(y => int.Parse(y.RowData[column]));
                else
                    x.OrderByDescending(y => int.Parse(y.RowData[column]));
            });
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
                {
                    string dietString = diet.ToString();
                    item.RowData[colDiet] = dietString;
                    searchText += " " + dietString;
                }


                float? marketValue = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.MarketValue)?.value;
                if (marketValue.HasValue)
                    item.RowData[colValue] = $"${marketValue.Value}";

                // CompProperties_Milkable
                item.SearchString = searchText;
                table.AddRow(item);
            }
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
                selectedRace = selectedRows[0].Def as PawnKindDef;

            _previewNorth.Thing = selectedRace;
            _previewEast.Thing = selectedRace;
            _previewSouth.Thing = selectedRace;

            _previewNorth.Refresh();
            _previewEast.Refresh();
            _previewSouth.Refresh();
        }

        private void UpdateDetails(IReadOnlyList<GeneRowItem> selectedRows)
        {
            if (selectedRows.Count == 1)
            {
                PawnKindDef selectedRace = selectedRows[0].Def as PawnKindDef;


                _stringBuilder.Clear();
                foreach (StatDrawEntry item in selectedRace.RaceProps.SpecialDisplayStats(selectedRace.race, StatRequest.ForEmpty()))
                {
                    if (item.ShouldDisplay)
                        _stringBuilder.AppendLine(item.LabelCap + ": " + item.ValueString);
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
            _previewNorth.Draw(previewBox.ContractedBy(3));

            previewBox.y = previewBox.yMax + SPACING;

            Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
            _previewEast.Draw(previewBox.ContractedBy(3));

            previewBox.y = previewBox.yMax + SPACING;

            Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
            _previewSouth.Draw(previewBox.ContractedBy(3));
        }

        public override void Delete(Def def)
        {
            _databank.RemoveFromDatabase(def as PawnKindDef);
        }
    }
}
