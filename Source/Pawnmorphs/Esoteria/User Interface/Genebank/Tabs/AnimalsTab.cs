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

        private static readonly string TAB_COLUMN_RACE = "PM_Genebank_AnimalsTab_Column_Race".Translate();

        private static readonly string TAB_COLUMN_TEMPERATURE = "PM_Genebank_AnimalsTab_Column_Temperature".Translate();
        private static readonly float TAB_COLUMN_TEMPERATURE_SIZE;

        private static readonly string TAB_COLUMN_LIFESPAN = "PM_Genebank_AnimalsTab_Column_Lifespan".Translate();
        private static readonly float TAB_COLUMN_LIFESPAN_SIZE;

        private static readonly string TAB_COLUMN_DIET = "PM_Genebank_AnimalsTab_Column_Diet".Translate();
        private static readonly float TAB_COLUMN_DIET_SIZE;

        private static readonly string TAB_COLUMN_VALUE = "PM_Genebank_AnimalsTab_Column_Value".Translate();
        private static readonly float TAB_COLUMN_VALUE_SIZE;

        private static readonly string TAB_COLUMN_MUTATIONS = "PM_Genebank_AnimalsTab_Column_Mutations".Translate();
        private static readonly float TAB_COLUMN_MUTATIONS_SIZE;

        private static readonly string DESCRIPTION_MUTATIONS = "PM_Genebank_AnimalsTab_Details_Mutations".Translate();


        static AnimalsTab()
        {
            Text.Font = GameFont.Small;
            TAB_COLUMN_TEMPERATURE_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_TEMPERATURE).x, 100f);
            TAB_COLUMN_LIFESPAN_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_LIFESPAN).x, 60f);
            TAB_COLUMN_DIET_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_DIET).x, 100f);
            TAB_COLUMN_VALUE_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_VALUE).x, 75f);
            TAB_COLUMN_MUTATIONS_SIZE = Mathf.Max(Text.CalcSize(TAB_COLUMN_MUTATIONS).x, 75f);
        }

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
            var colValue = table.AddColumn(TAB_COLUMN_VALUE, TAB_COLUMN_VALUE_SIZE);
            var colMutations = table.AddColumn(TAB_COLUMN_MUTATIONS, TAB_COLUMN_MUTATIONS_SIZE);
            //Nutrition requirements?

            GeneRowItem row;
            int totalStorage = _databank.TotalStorage;
            foreach (PawnKindDef animal in _databank.TaggedAnimals)
            {
                string searchText = animal.label;
                row = new GeneRowItem(animal, totalStorage, searchText);

                // Comfortable temperature
                float? minTemp = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.ComfyTemperatureMin)?.value;
                float? maxTemp = animal.race.statBases.SingleOrDefault(x => x.stat == StatDefOf.ComfyTemperatureMax)?.value;
                if (minTemp.HasValue && maxTemp.HasValue)
                    row[colTemperature] = $"{GenTemperature.CelsiusTo(minTemp.Value, Prefs.TemperatureMode)}-{GenText.ToStringTemperature(maxTemp.Value)}";

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
                

                // CompProperties_Milkable
                row.SearchString = searchText.ToLower();
                table.AddRow(row);
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
        }

        public override void Delete(Def def)
        {
            _databank.RemoveFromDatabase(def as PawnKindDef);
        }
    }
}
