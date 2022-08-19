using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
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

namespace Pawnmorph.User_Interface.Genebank.Tabs
{
    internal class MutationsTab : IGeneTab
    {
        public void GenerateTable(Table<GeneRowItem> table, ChamberDatabase databank)
        {
            
            var column = table.AddColumn("Mutation", 0.5f,
                (ref Rect box, GeneRowItem item) => Widgets.Label(box, item.Label),
                (collection, ascending) =>
                {
                    if (ascending)
                        collection.OrderBy(x => x.Label);
                    else
                        collection.OrderByDescending(x => x.Label);
                });
            column.IsFixedWidth = false;

            TableColumn colParagon = table.AddColumn("Paragon", 60f);
            TableColumn colAbilities = table.AddColumn("Abilities", 100f);
            TableColumn colStats = table.AddColumn("Stats", 0.5f);
            colStats.IsFixedWidth = false;

            GeneRowItem item;
            int totalCapacity = databank.TotalStorage;
            foreach (MutationDef mutation in databank.StoredMutations)
            {
                string searchText = mutation.label;
                item = new GeneRowItem(mutation, totalCapacity, searchText);

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
                table.AddRow(item);
            }

        }

        public void InitDetails(Preview.Preview[] previews)
        {
            previews[0] = new PawnPreview(200, 200, ThingDefOf.Human as AlienRace.ThingDef_AlienRace);
            previews[0].Rotation = Rot4.North;


            previews[1] = new PawnPreview(200, 200, ThingDefOf.Human as AlienRace.ThingDef_AlienRace);
            previews[1].Rotation = Rot4.East;
            previews[1].PreviewIndex = 2;


            previews[2] = new PawnPreview(200, 200, ThingDefOf.Human as AlienRace.ThingDef_AlienRace);
            previews[2].Rotation = Rot4.South;
            previews[2].PreviewIndex = 3;
        }

        public void SelectedRow(IReadOnlyList<GeneRowItem> selectedRows, Preview.Preview[] previews)
        {
            (previews[0] as PawnPreview).ClearMutations();
            (previews[1] as PawnPreview).ClearMutations();
            (previews[2] as PawnPreview).ClearMutations();

            foreach (var item in selectedRows)
            {
                MutationDef mutation = item.Def as MutationDef;
                (previews[0] as PawnPreview).AddMutation(mutation);
                (previews[1] as PawnPreview).AddMutation(mutation);
                (previews[2] as PawnPreview).AddMutation(mutation);
            }
        }
    }
}
