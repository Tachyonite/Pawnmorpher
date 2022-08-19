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
    internal class AnimalsTab : IGeneTab
    {
        public void GenerateTable(Table<GeneRowItem> table, ChamberDatabase databank)
        {
            var column = table.AddColumn("Race", 0.25f, (ref Rect box, GeneRowItem item) => Widgets.Label(box, item.Label));
            column.IsFixedWidth = false;

            var colTemperature = table.AddColumn("Temperature", 100f);
            var colLifespan = table.AddColumn("Tolerance", 60f);
            var colDiet = table.AddColumn("Diet", 100f);
            var colValue = table.AddColumn("Value", 75f);
            //Nutrition requirements?

            GeneRowItem item;
            int totalStorage = databank.TotalStorage;
            foreach (PawnKindDef animal in databank.TaggedAnimals)
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

        public void InitDetails(Preview.Preview[] previews)
        {
            previews[0] = new PawnKindDefPreview(200, 200, null);
            previews[0].Rotation = Rot4.North;


            previews[1] = new PawnKindDefPreview(200, 200, null);
            previews[1].Rotation = Rot4.East;
            previews[1].PreviewIndex = 2;


            previews[2] = new PawnKindDefPreview(200, 200, null);
            previews[2].Rotation = Rot4.South;
            previews[2].PreviewIndex = 3;
        }

        public void SelectedRow(IReadOnlyList<GeneRowItem> selectedRows, Preview.Preview[] previews)
        {
            if (selectedRows.Count == 1)
            {
                PawnKindDef thing = (PawnKindDef)selectedRows[0].Def;
                (previews[0] as PawnKindDefPreview).Thing = thing;
                (previews[1] as PawnKindDefPreview).Thing = thing;
                (previews[2] as PawnKindDefPreview).Thing = thing;
                return;
            }

            (previews[0] as PawnKindDefPreview).Thing = null;
            (previews[1] as PawnKindDefPreview).Thing = null;
            (previews[2] as PawnKindDefPreview).Thing = null;
        }
    }
}
