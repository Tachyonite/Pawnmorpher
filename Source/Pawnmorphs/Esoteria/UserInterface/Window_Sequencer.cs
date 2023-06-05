using Pawnmorph.Chambers;
using Pawnmorph.UserInterface.Genebank.Tabs;
using Pawnmorph.UserInterface.Genebank;
using Pawnmorph.UserInterface.TableBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Pawnmorph.Genebank.Model;
using Pawnmorph.Hediffs;
using Pawnmorph.UserInterface.Preview;
using RimWorld;
using AlienRace;
using Pawnmorph.Utilities;
using Verse.Sound;
using Pawnmorph.ThingComps;

namespace Pawnmorph.UserInterface
{
	internal class Window_Sequencer : Window
	{
		private readonly string COLUMN_RACE = "SequenceTableColumnRace".Translate();
		private const float COLUMN_RACE_WIDTH_FRACT = 0.60f;
		private readonly string COLUMN_MORPH = "SequenceTableColumnMorph".Translate();
		private readonly float COLUMN_MORPH_SIZE;
		private readonly string COLUMN_MUTATIONS = "SequenceTableColumnMutations".Translate();
		private readonly float COLUMN_MUTATIONS_SIZE;

		private readonly string BUTTON_SEQUENCE = "SequenceButtonStart".Translate();

		private readonly string ROTATE_CW_LOC_STRING = "RotCW".Translate();
		private readonly string ROTATE_CCW_LOC_STRING = "RotCCW".Translate();
		private readonly string FEMALE = "Female".Translate().CapitalizeFirst();
		private readonly string MALE = "Male".Translate().CapitalizeFirst();

		private const float SPACING = 10f;
		private const int PREVIEW_SIZE = 200;
		private const float PROGRESS_COLUMN_WIDTH = 200f;

		private readonly Table<TableRow<PawnKindDef>> _table;
		private ChamberDatabase _chamberDatabase;

		private PawnPreview _animalPreview;
		private HumanlikePreview _morphPreview;
		private PawnKindDef _selectedAnimal;
		private string _sequencedMutationsLabel;
		private string _sequencedMutationsTooltip;
		private string _sequenceTargetAnimalLabel;
		private MutationSequencerComp _sequencer;
		private Texture2D _progressFillTexture;

		public Window_Sequencer(MutationSequencerComp sequencer)
		{
			_sequencer = sequencer;
			Text.Font = GameFont.Small;
			COLUMN_MUTATIONS_SIZE = Mathf.Max(Text.CalcSize(COLUMN_MUTATIONS).x, 100f);
			COLUMN_MORPH_SIZE = Mathf.Max(Text.CalcSize(COLUMN_MORPH).x, 100f);

			_table = new Table<TableRow<PawnKindDef>>((item, text) => item.SearchString.Contains(text));
			_table.SelectionChanged += Table_SelectionChanged;
			_table.MultiSelect = false;
			_progressFillTexture = SolidColorMaterials.NewSolidColorTexture(0.423f, 0.6705f, 0.188f, 1);

			_animalPreview = new PawnPreview(PREVIEW_SIZE, PREVIEW_SIZE)
			{
				PreviewIndex = 2,
			};

			_morphPreview = new HumanlikePreview(PREVIEW_SIZE, PREVIEW_SIZE, ThingDefOf.Human as ThingDef_AlienRace)
			{
			};

			this.resizeable = true;
			this.draggable = true;
			this.doCloseX = true;
		}

		private void Table_SelectionChanged(object sender, IReadOnlyList<TableRow<PawnKindDef>> e)
		{
			if (e.Count == 0)
			{
				ClearSelection();
				return;
			}

			Select(e[0].RowObject);
		}

		private void ClearSelection()
		{
			_selectedAnimal = null;
			_animalPreview.PawnKindDef = null;
			_animalPreview.Refresh();
			_morphPreview.ClearMutations();
			_morphPreview.Refresh();
		}

		private void Select(PawnKindDef animalKind)
		{
			_selectedAnimal = animalKind;
			_animalPreview.PawnKindDef = animalKind;

			_morphPreview.ClearMutations();
			IReadOnlyList<MutationDef> mutations = animalKind.GetAllMutationsFrom();
			for (int i = mutations.Count - 1; i >= 0; i--)
			{
				MutationDef mutation = mutations[i];
				_morphPreview.AddMutation(mutation);
			}

			_animalPreview.Refresh();
			_morphPreview.Refresh();
		}

		protected override void SetInitialSizeAndPosition()
		{
			base.SetInitialSizeAndPosition();

			Vector2 location = new Vector2(40, 40);
			Vector2 size = new Vector2(UI.screenWidth * 0.9f, UI.screenHeight * 0.8f);
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

			// Add columns
			var colRace = _table.AddColumn(COLUMN_RACE, COLUMN_RACE_WIDTH_FRACT);
			colRace.IsFixedWidth = false;
			var colMorph = _table.AddColumn(COLUMN_MORPH, COLUMN_MORPH_SIZE);
			var colMutations = _table.AddColumn(COLUMN_MUTATIONS, COLUMN_MUTATIONS_SIZE, (x, ascending, column) =>
			{
				if (ascending)
					x.OrderBy(y => (int)y.Tag);
				else
					x.OrderByDescending(y => (int)y.Tag);
			});

			// Add rows
			IReadOnlyList<GenebankEntry<PawnKindDef>> taggedAnimals = _chamberDatabase.GetEntryItems<PawnKindDef>();
			IReadOnlyList<MutationDef> sequencedMutations = _chamberDatabase.StoredMutations;
			for (int i = taggedAnimals.Count - 1; i >= 0; i--)
			{
				GenebankEntry<PawnKindDef> entry = taggedAnimals[i];
				PawnKindDef animal = entry.Value;

				// Skip if entry has no mutations.
				IReadOnlyList<MutationDef> allMutations = animal.GetAllMutationsFrom();
				int totalMutations = allMutations.Count;
				if (totalMutations == 0)
					continue;

				TableRow<PawnKindDef> row = new TableRow<PawnKindDef>(animal);

				// animal data
				string searchText = animal.label;
				row[colRace] = animal.LabelCap;

				// morph data
				MorphDef morph = MorphUtilities.TryGetBestMorphOfAnimal(animal.race);
				Log.Message($"{animal.LabelCap} - {morph?.LabelCap}");
				if (morph != null)
				{
					searchText += " " + morph.label;
					row[colMorph] = morph.LabelCap;
				}

				// Mutations data
				int taggedMutations = allMutations.Intersect(sequencedMutations).Count();
				row[colMutations] = $"{taggedMutations}/{totalMutations}";
				row.Tag = totalMutations - taggedMutations;

				row.SearchString = searchText;
				_table.AddRow(row);
			}


			_table.LineFont = PawnmorpherMod.Settings.GenebankWindowFont ?? GameFont.Tiny;
			_table.Refresh();



			if (_sequencer.TargetAnimal != null)
			{
				Select(_sequencer.TargetAnimal);
				UpdateControlPanel();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			inRect.SplitVerticallyWithMargin(out Rect mainRect, out Rect detailsRect, out float _, SPACING, rightWidth: PREVIEW_SIZE + PROGRESS_COLUMN_WIDTH + SPACING);

			// Draw table
			Widgets.DrawBoxSolidWithOutline(mainRect, Color.black, Color.gray);
			_table.Draw(mainRect.ContractedBy(SPACING));

			detailsRect.SplitVerticallyWithMargin(out Rect controlColumnRect, out Rect progressRect, out float _, SPACING, rightWidth: PROGRESS_COLUMN_WIDTH);

			// Draw previews
			Rect previewBox = new Rect(controlColumnRect.x, controlColumnRect.y, PREVIEW_SIZE, PREVIEW_SIZE);
			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_animalPreview.Draw(previewBox.ContractedBy(3));

			previewBox.y = previewBox.yMax + SPACING;
			Widgets.DrawBoxSolidWithOutline(previewBox, Color.black, Color.gray);
			_morphPreview.Draw(previewBox.ContractedBy(3));

			// Draw control panel
			float curY = previewBox.yMax + SPACING;

			Text.Anchor = TextAnchor.MiddleCenter;
			Text.Font = GameFont.Small;
			if (_sequencer.TargetAnimal != null)
				Widgets.Label(previewBox.x, ref curY, previewBox.width, _sequenceTargetAnimalLabel);

			if (_sequencedMutationsLabel != null)
			{
				float prevY = curY;
				Widgets.Label(previewBox.x, ref curY, previewBox.width, _sequencedMutationsLabel);
				if (_sequencedMutationsTooltip != null)
					TooltipHandler.TipRegion(new Rect(previewBox.x, prevY, previewBox.width, curY - prevY), _sequencedMutationsTooltip);
			}
			Text.Anchor = TextAnchor.UpperLeft;

			Rect buttonRect = new Rect(previewBox.x, curY, previewBox.width, 30);
			DrawPreviewButtons(buttonRect);

			buttonRect.y = buttonRect.yMax + SPACING;
			buttonRect.height = 30;
			if (Widgets.ButtonText(buttonRect, BUTTON_SEQUENCE))
				StartSequence();

			// Draw progress tube
			DrawProgressBar(progressRect);
		}

		private void DrawProgressBar(Rect rect)
		{
			Rect result = rect;
			result.height *= _sequencer.Progress;
			result.y = rect.yMax - result.height;
			GUI.DrawTexture(result, _progressFillTexture);
			GUI.DrawTexture(rect, PMTextures.SquencerStrand);
		}

		private void DrawPreviewButtons(Rect inRect)
		{
			Rect buttonRect = new Rect(inRect);
			buttonRect.width = 30;

			// Rotate right
			if (Widgets.ButtonImageFitted(buttonRect, ButtonTexturesPM.rotCW, Color.white, GenUI.MouseoverColor))
				RotatePreviews(RotationDirection.Clockwise);
			TooltipHandler.TipRegion(buttonRect, ROTATE_CW_LOC_STRING);


			buttonRect.Set(buttonRect.xMax + SPACING, buttonRect.y, 55, buttonRect.height);
			if (Widgets.ButtonText(buttonRect, MALE))
				SetPreviewGender(Gender.Male);

			buttonRect.x = buttonRect.xMax + SPACING;
			if (Widgets.ButtonText(buttonRect, FEMALE))
				SetPreviewGender(Gender.Female);


			buttonRect.Set(inRect.xMax - 30, buttonRect.y, 30, buttonRect.height);
			if (Widgets.ButtonImageFitted(buttonRect, ButtonTexturesPM.rotCCW, Color.white, GenUI.MouseoverColor))
				RotatePreviews(RotationDirection.Counterclockwise);
			TooltipHandler.TipRegion(buttonRect, ROTATE_CCW_LOC_STRING);
		}

		/// <summary>
		/// Set sequence target and update UI.
		/// </summary>
		private void StartSequence()
		{
			_sequencer.TargetAnimal = _selectedAnimal;
			UpdateControlPanel();
		}

		/// <summary>
		/// Refresh control panel elements.
		/// </summary>
		private void UpdateControlPanel()
		{
			if (_sequencer.TargetAnimal == null) 
			{
				_sequenceTargetAnimalLabel = null;
				_sequencedMutationsLabel = null;
				_sequencedMutationsTooltip = null;
				return;
			}

			_sequenceTargetAnimalLabel = "SequencingProgress".Translate(_sequencer.TargetAnimal.label.Named("animal")) + ".";

			IReadOnlyList<MutationDef> allMutations = _sequencer.TargetAnimal.GetAllMutationsFrom();
			IReadOnlyList<MutationDef> sequencedMutations = _chamberDatabase.StoredMutations;
			IEnumerable<MutationDef> taggedMutations = allMutations.Intersect(sequencedMutations);

			int taggedCount = taggedMutations.Count();
			int allCount = allMutations.Count;

			if (taggedCount == allCount)
			{
				_sequencedMutationsLabel = $"SequencingComplete".Translate(_sequencer.TargetAnimal.LabelCap.Named("animal"));
			}
			else
				_sequencedMutationsLabel = $"{taggedCount}/{allCount} mutations sequenced";
			_sequencedMutationsTooltip = String.Join(Environment.NewLine, taggedMutations.Select(x => x.LabelCap));
		}



		private void SetPreviewGender(Gender gender)
		{
			if (_animalPreview.PawnKindDef != null)
			{
				if (_animalPreview.PawnKindDef.fixedGender == null && _animalPreview.PawnKindDef.RaceProps.hasGenders)
				{
					_animalPreview.SetGender(gender);
					_animalPreview.Refresh();
				}
			}
			_morphPreview.SetGender(gender);
			_morphPreview.Refresh();
		}

		private void RotatePreviews(RotationDirection direction)
		{
			SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
			Rot4 rotation = _animalPreview.Rotation;
			rotation.Rotate(direction);
			_animalPreview.Rotation = rotation;

			rotation = _morphPreview.Rotation;
			rotation.Rotate(direction);
			_morphPreview.Rotation = rotation;

			_animalPreview.Refresh();
			_morphPreview.Refresh();
		}
	}
}
