using Pawnmorph.Abilities;
using Pawnmorph.Chambers;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
using Pawnmorph.User_Interface.Preview;
using Pawnmorph.User_Interface.TableBox;
using Pawnmorph.Utilities;
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
    internal class MutationsTab : GenebankTab
    {
        private const float ABILITY_SIZE = 100;

        PawnPreview _previewNorth;
        PawnPreview _previewEast;
        PawnPreview _previewSouth;
        ChamberDatabase _databank;
        List<MutationStage> _stages;
        int _currentStage;
        

        public override void Initialize(ChamberDatabase databank)
        {
            _databank = databank;
            _stages = new List<MutationStage>();

            _previewNorth = new PawnPreview(195, 195, ThingDefOf.Human as AlienRace.ThingDef_AlienRace)
            {
                Rotation = Rot4.North
            };


            _previewEast = new PawnPreview(195, 195, ThingDefOf.Human as AlienRace.ThingDef_AlienRace)
            {
                Rotation = Rot4.East,
                PreviewIndex = 2
            };


            _previewSouth = new PawnPreview(195, 195, ThingDefOf.Human as AlienRace.ThingDef_AlienRace)
            {
                Rotation = Rot4.South,
                PreviewIndex = 3
            };
        }


        public override void GenerateTable(Table<GeneRowItem> table)
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
            int totalCapacity = _databank.TotalStorage;
            foreach (MutationDef mutation in _databank.StoredMutations)
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

        public override void SelectionChanged(IReadOnlyList<GeneRowItem> selectedRows)
        {
            UpdatePreviews(selectedRows);
            UpdateStages(selectedRows);



        }

        private void UpdatePreviews(IReadOnlyList<GeneRowItem> selectedRows)
        {
            _previewNorth.ClearMutations();
            _previewEast.ClearMutations();
            _previewSouth.ClearMutations();

            //if (selectedRows.Count == 1)
            //{
                foreach (var item in selectedRows)
                {
                    MutationDef mutation = item.Def as MutationDef;
                    _previewNorth.AddMutation(mutation);
                    _previewEast.AddMutation(mutation);
                    _previewSouth.AddMutation(mutation);
                }
            //}

            _previewNorth.Refresh();
            _previewEast.Refresh();
            _previewSouth.Refresh();
        }

        private void UpdateStages(IReadOnlyList<GeneRowItem> selectedRows)
        {
            _stages.Clear();
            _currentStage = 0;

            if (selectedRows.Count == 1)
            {
                _stages.AddRange((selectedRows[0].Def as MutationDef).stages.OfType<MutationStage>());
                foreach (var stage in _stages)
                {
                    if (stage.abilities == null)
                        continue;

                    foreach (var ability in stage.abilities)
                    {
                        ability.CacheTexture();
                    }
                }
            }
        }

        public override void DrawDetails(Rect inRect)
        {
            DrawPreview(inRect);

            Text.Font = GameFont.Medium;
            float height = ABILITY_SIZE + Text.LineHeight + SPACING;
            Rect abilitiesRect = new Rect(inRect.x, inRect.yMax - height, inRect.width, height);
            Widgets.Label(new Rect(abilitiesRect.x, abilitiesRect.y - Text.LineHeight - SPACING, 100, Text.LineHeight), "Abilities:");
            DrawAbilities(abilitiesRect);
        }

        public void DrawPreview(Rect inRect)
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

        Vector2 _abilitiesScrollPosition;
        public void DrawAbilities(Rect inRect)
        {
            if (_stages.Count > 0)
            {
                var abilities = _stages[_currentStage].abilities;
                if (abilities == null || abilities.Count == 0)
                    return;

                Rect abilitiesViewRect = new Rect(0, 0, (ABILITY_SIZE + SPACING) * abilities.Count, ABILITY_SIZE);
                Rect abilityRect = new Rect(0, 0, ABILITY_SIZE, ABILITY_SIZE);

                Widgets.BeginScrollView(inRect, ref _abilitiesScrollPosition, abilitiesViewRect);

                for (int i = 0; i < abilities.Count; i++)
                {
                    MutationAbilityDef ability = abilities[i];
                    Widgets.DrawBoxSolidWithOutline(abilityRect, Color.black, Color.gray);
                    Widgets.DrawTextureFitted(abilityRect.ContractedBy(GenUI.GapTiny), ability.IconTexture, 1f);
                    TooltipHandler.TipRegion(abilityRect, () => $"Cooldown: {ability.cooldown / TimeMetrics.TICKS_PER_HOUR}h", (int)abilityRect.x + 117858);
                    Widgets.Label(new Rect(abilityRect.x, abilityRect.yMax + SPACING, ABILITY_SIZE, Text.LineHeight), ability.label);

                    abilityRect.x = abilityRect.xMax + SPACING;
                }

                Widgets.EndScrollView();
            }
        }

        public override void Delete(Def def)
        {
            _databank.RemoveFromDatabase(def as MutationDef);
        }
    }
}
