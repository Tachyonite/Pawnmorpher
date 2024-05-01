using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlienRace;
using JetBrains.Annotations;
using Pawnmorph.Chambers;
using Pawnmorph.Genebank.Model;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using Pawnmorph.UserInterface.PartPicker;
using Pawnmorph.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.UserInterface
{
	/// <summary>
	/// part picker dialogue windo
	/// </summary>
	/// <seealso cref="Verse.Window" />
	[HotSwappable]
	public partial class Dialog_PartPicker : Window
	{

		/// <summary>
		/// handler for the window closed event 
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="addedMutations">The added mutations.</param>
		public delegate void WindowClosedHandle([NotNull] Dialog_PartPicker sender, [NotNull] IReadOnlyAddedMutations addedMutations);

		/// <summary>
		/// Occurs when the window is closed.
		/// </summary>
		public event WindowClosedHandle WindowClosed;

		// Constants
		private const string WINDOW_TITLE_LOC_STRING = "PartPickerMenuTitle";
		private const string NO_MUTATIONS_LOC_STRING = "NoMutationsOnPart";
		private const string EDIT_PARAMS_LOC_STRING = "EditParams";
		private const string IS_PAUSED_LOC_STRING = "IsPaused";
		private const string TOGGLE_CLOTHES_LOC_STRING = "ToggleClothes";
		private const string ROTATE_CW_LOC_STRING = "RotCW";
		private const string ROTATE_CCW_LOC_STRING = "RotCCW";
		private const string CROWN_LABEL_LOC_STRING = "CrownLabel";
		private const string BODY_LABEL_LOC_STRING = "BodyLabel";
		private const string DO_SYMMETRY_LOC_STRING = "DoSymmetry";
		private const string SKIN_SYNC_LOC_STRING = "SkinSync";
		private const string SUMMARY_TITLE_LOC_STRING = "SummaryTitle";
		private const string ADDED_MUTATION_LOC_STRING = "SummaryAddedMutation";
		private const string HALTED_MUTATION_LOC_STRING = "SummaryHaltedMutation";
		private const string REMOVED_MUTATION_LOC_STRING = "SummaryRemovedMutation";
		private const string PART_DESCRIPTION_TITLE_LOC_STRING = "PartDescTitle";
		private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
		private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
		private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
		private const float SPACER_SIZE = 17f;
		private const float BUTTON_HORIZONTAL_PADDING = 6f;
		private const float MENU_SECTION_CONSTRICTION_SIZE = 4f;
		private const float SLIDER_HEIGHT = 13f;
		private static int PREVIEW_POSITION_X = -10; // Render pawn away from the map to avoid capturing parts of the map.
		private static Vector2Int PREVIEW_SIZE = new Vector2Int(200, 280);
		private static Vector2 TOGGLE_CLOTHES_BUTTON_SIZE = new Vector2(30, 30);
		private static Vector2 ROTATE_CW_BUTTON_SIZE = new Vector2(30, 30);
		private static Vector2 ROTATE_CCW_BUTTON_SIZE = new Vector2(30, 30);
		private static Vector2 IS_PAUSED_CHECKBOX_SIZE = new Vector2(17, 17);
		private static Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
		private static Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);

		// Scrolling variables
		private Vector2 partListScrollPos;
		private Vector2 partListScrollSize;
		private Vector2 summaryScrollPos;
		private Vector2 summaryScrollSize;
		private Vector2 descScrollPos;
		private Vector2 descScrollSize;

		// Description builders
		private StringBuilder summaryBuilder = new StringBuilder();
		private StringBuilder partDescBuilder = new StringBuilder();

		// Flags
		private bool debugMode = false;
		private bool confirmed = false;

		// Toggles
		private bool toggleClothesEnabled = true;
		private bool doSymmetry = true;
		private bool skinSync = true;
		private Tuple<BodyPartRecord, MutationLayer> detailPart = new Tuple<BodyPartRecord, MutationLayer>(new BodyPartRecord(), MutationLayer.Core);
		private Rot4 previewRot = Rot4.South;

		// Preview related variables        
		/// <summary>
		/// The recache preview
		/// </summary>
		public bool recachePreview = false;
		private GameObject gameObject;
		private Camera camera;
		private RenderTexture previewImage;

		// Caching variables
		private Pawn pawn;
		private List<Hediff_AddedMutation> pawnCurrentMutations;
		private static List<HediffInitialState> cachedInitialHediffs;

		[NotNull]
		private static readonly Dictionary<BodyPartDef, List<MutationDef>> cachedMutationDefsByPartDef =
			new Dictionary<BodyPartDef, List<MutationDef>>();
		[NotNull]
		private static readonly Dictionary<BodyPartDef, List<MutationLayer>> cachedMutationLayersByPartDef = new Dictionary<BodyPartDef, List<MutationLayer>>();
		[NotNull]
		private static readonly List<BodyPartRecord> cachedMutableParts = new List<BodyPartRecord>();
		[NotNull]
		private static readonly List<BodyPartRecord> cachedMutableCoreParts = new List<BodyPartRecord>();
		[NotNull]
		private static readonly List<BodyPartRecord> cachedMutableSkinParts = new List<BodyPartRecord>();
		private static BodyTypeDef initialBodyType;
		private static HeadTypeDef initialCrownType;
		private static string editButtonText = EDIT_PARAMS_LOC_STRING.Translate();
		private static float editButtonWidth = Text.CalcSize(editButtonText).x + 2 * BUTTON_HORIZONTAL_PADDING;

		private float maxPawnSeverity = 1;

		private ValueTuple<Color, Color> initialPawnColors;

		private AlienPartGenerator.AlienComp alienComp;

		// Return data
		private AddedMutations addedMutations = new AddedMutations();
		private Dictionary<Def, string> _labelCache = new Dictionary<Def, string>();
		private ChamberDatabase _database = Find.World.GetComponent<ChamberDatabase>();

		/// <summary>
		/// Gets the initial size.
		/// </summary>
		/// <value>
		/// The initial size.
		/// </value>
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(750f, 840f);
			}
		}

		static void SetMutationPartDefsCache()
		{
			cachedMutationLayersByPartDef.Clear();
			foreach (MutationDef allMutation in MutationDef.AllMutations)
			{
				var layer = allMutation.Layer;
				if (layer == null) continue;
				if (allMutation.parts == null || allMutation.parts.Count == 0) continue;
				foreach (BodyPartDef part in allMutation.parts)
				{
					if (!cachedMutationLayersByPartDef.TryGetValue(part, out var lst))
					{
						lst = new List<MutationLayer>();
						cachedMutationLayersByPartDef[part] = lst;
					}
					if (lst.Contains(layer.Value)) continue;
					lst.Add(layer.Value);
				}
			}
		}

		static Dialog_PartPicker()
		{
			SetMutationPartDefsCache();



		}

		void SetCaches()
		{
			cachedMutableParts.Clear();
			cachedMutableCoreParts.Clear();
			cachedMutableSkinParts.Clear();
			cachedMutationDefsByPartDef.Clear();
			foreach (BodyPartRecord record in pawn.RaceProps.body.AllParts)
			{


				foreach (MutationDef mutation in MutationDef.AllMutations)
				{
					if (mutation.parts == null) continue;
					var layer = mutation.Layer;
					foreach (BodyPartDef mutationPart in mutation.parts)
					{
						if (mutationPart == record.def)
						{
							if (!cachedMutableParts.Contains(record))
								cachedMutableParts.Add(record);

							switch (layer)
							{
								case MutationLayer.Core:
									if (!cachedMutableCoreParts.Contains(record)) cachedMutableCoreParts.Add(record);
									break;
								case MutationLayer.Skin:
									if (!cachedMutableSkinParts.Contains(record)) cachedMutableSkinParts.Add(record);
									break;
								case null:
									break;
								default:
									throw new ArgumentOutOfRangeException();
							}

							break;


						}
					}
				}


			}


			foreach (MutationDef mutation in MutationDef.AllMutations)
			{
				if (mutation.parts == null || mutation.parts.Count == 0) continue;

				foreach (BodyPartDef mutationPart in mutation.parts)
				{

					if (!cachedMutationDefsByPartDef.TryGetValue(mutationPart, out var lst))
					{
						lst = new List<MutationDef>();
						cachedMutationDefsByPartDef[mutationPart] = lst;
					}
					if (!lst.Contains(mutation))
						lst.Add(mutation);
				}
			}


		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Dialog_PartPicker"/> class.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <param name="debugMode">if set to <c>true</c> [debug mode].</param>
		public Dialog_PartPicker(Pawn pawn, bool debugMode = false)
		{
			// Copying passed in data for use elsewhere in the class.
			this.pawn = pawn;
			this.debugMode = debugMode;

			// Settting flags for this window. 
			forcePause = true;

			alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();
			var initialSkin = alienComp.GetChannel("skin");
			initialPawnColors = new ValueTuple<Color, Color>(initialSkin.first, initialSkin.second);

			SetCaches();

			// Initial caching of the mutations currently affecting the pawn and their initial hediff list.
			maxPawnSeverity = 1 + StatsUtility.GetStat(pawn, PMStatDefOf.MutationAdaptability, 300) ?? 0;
			cachedInitialHediffs = pawn.health.hediffSet.hediffs.Select(h => new HediffInitialState(h, h.Severity, (h as Hediff_AddedMutation)?.ProgressionHalted ?? false)).ToList();
			initialBodyType = pawn.story.bodyType;
			initialCrownType = pawn.story.headType;
			RecachePawnMutations();
		}

		/// <summary>
		/// Closes the specified do close sound.
		/// </summary>
		/// <param name="doCloseSound">if set to <c>true</c> [do close sound].</param>
		public override void Close(bool doCloseSound = false)
		{
			if (!confirmed)
			{
				Reset();
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
			}

			if (pawn != null)
			{
				var comp = pawn.GetComp<GraphicsUpdaterComp>();
				if (comp != null)
				{
					// If pawn has visible graphics, then refresh.
					comp.RefreshGraphics();
					PortraitsCache.SetDirty(pawn);
				}
				pawn.health?.capacities?.Notify_CapacityLevelsDirty();
			}

			base.Close(doCloseSound);
			WindowClosed?.Invoke(this, addedMutations);
		}

		/// <summary>
		/// Called when [accept key pressed].
		/// </summary>
		public override void OnAcceptKeyPressed()
		{
			confirmed = true;
			base.OnAcceptKeyPressed();
		}

		/// <summary>
		/// Does the window contents.
		/// </summary>
		/// <param name="inRect">The in rect.</param>
		public override void DoWindowContents(Rect inRect)
		{
			// Draw the window title.
			Text.Font = GameFont.Medium;
			string titleLabel = $"{WINDOW_TITLE_LOC_STRING.Translate()} - {pawn.Name.ToStringShort} ({pawn.def.LabelCap})";
			float titleHeight = Text.CalcHeight(titleLabel, inRect.width);
			Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, titleHeight);
			Widgets.Label(titleRect, titleLabel);
			Text.Font = GameFont.Small;

			// Determine draw areas for parts list and descriptions.
			float columnWidth = (inRect.width - PREVIEW_SIZE.x) / 2 - SPACER_SIZE;
			float columnHeight = inRect.height - titleHeight - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - 2 * SPACER_SIZE;

			// Draw parts list as the left column. This displays all mutable parts and the mutations currently applied to them.
			Rect partListOutRect = new Rect(inRect.x, titleHeight + SPACER_SIZE, columnWidth, columnHeight);
			Rect partListViewRect = new Rect(partListOutRect.x, partListOutRect.y, partListScrollSize.x, partListScrollSize.y);
			DrawPartsList(partListOutRect, partListViewRect);

			// Draw the preview area, mode toggle buttons, body/crown type selection and aspect selection in the middle column (Might need to outsource these to a helper function).
			// First the preview...
			float curY = titleHeight + SPACER_SIZE;
			Rect previewRect = new Rect(columnWidth + SPACER_SIZE, curY, PREVIEW_SIZE.x, PREVIEW_SIZE.y);
			if (recachePreview || previewImage == null)
			{
				SetPawnPreview();
			}
			GUI.DrawTexture(previewRect, previewImage);
			curY += previewRect.height;

			// Then the preview buttons...
			float rotCWHorPos = previewRect.x + previewRect.width / 2 - TOGGLE_CLOTHES_BUTTON_SIZE.x / 2 - ROTATE_CW_BUTTON_SIZE.x - SPACER_SIZE;
			float toggleClothingHorPos = previewRect.x + previewRect.width / 2 - TOGGLE_CLOTHES_BUTTON_SIZE.x / 2;
			float rotCCWHorPos = previewRect.x + previewRect.width / 2 + TOGGLE_CLOTHES_BUTTON_SIZE.x / 2 + SPACER_SIZE;
			Rect rotCWRect = new Rect(rotCWHorPos, curY, ROTATE_CW_BUTTON_SIZE.x, ROTATE_CW_BUTTON_SIZE.y);
			Rect toggleClothesRect = new Rect(toggleClothingHorPos, curY, TOGGLE_CLOTHES_BUTTON_SIZE.x, TOGGLE_CLOTHES_BUTTON_SIZE.y);
			Rect rotCCWRect = new Rect(rotCCWHorPos, curY, ROTATE_CCW_BUTTON_SIZE.x, ROTATE_CCW_BUTTON_SIZE.y);
			curY += SPACER_SIZE;
			if (Widgets.ButtonImageFitted(rotCWRect, ButtonTexturesPM.rotCW, Color.white, GenUI.MouseoverColor))
			{
				previewRot.Rotate(RotationDirection.Clockwise);
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				recachePreview = true;
			}
			TooltipHandler.TipRegionByKey(rotCWRect, ROTATE_CW_LOC_STRING);
			if (Widgets.ButtonImageFitted(toggleClothesRect, ButtonTexturesPM.toggleClothes, Color.white, GenUI.MouseoverColor))
			{
				toggleClothesEnabled = !toggleClothesEnabled;
				(toggleClothesEnabled ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
				recachePreview = true;
			}
			TooltipHandler.TipRegionByKey(toggleClothesRect, TOGGLE_CLOTHES_LOC_STRING);
			if (Widgets.ButtonImageFitted(rotCCWRect, ButtonTexturesPM.rotCCW, Color.white, GenUI.MouseoverColor))
			{
				previewRot.Rotate(RotationDirection.Counterclockwise);
				SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
				recachePreview = true;
			}
			TooltipHandler.TipRegionByKey(rotCCWRect, ROTATE_CCW_LOC_STRING);
			curY += Math.Max(TOGGLE_CLOTHES_BUTTON_SIZE.y, Math.Max(ROTATE_CW_BUTTON_SIZE.y, ROTATE_CCW_BUTTON_SIZE.y));

			// Then the crown and body type selectors...
			string currentHeadLabel = GetHeadtypeLabel(pawn.story.headType);
			Rect crownLabelRect = new Rect(previewRect.x, curY, previewRect.width / 3, Text.CalcHeight(CROWN_LABEL_LOC_STRING.Translate(), previewRect.width / 3));
			Rect crownButtonRect = new Rect(previewRect.x + previewRect.width / 3, curY, previewRect.width * 2 / 3, Text.CalcHeight(currentHeadLabel, previewRect.width * 2 / 3));
			Widgets.Label(crownLabelRect, CROWN_LABEL_LOC_STRING.Translate());
			if (Widgets.ButtonText(crownButtonRect, currentHeadLabel))
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();
				ThingDef_AlienRace pawnDef = pawn.def as ThingDef_AlienRace;
				foreach (HeadTypeDef headType in pawnDef.alienRace.generalSettings.alienPartGenerator.HeadTypes.Where(x => x.gender == Gender.None || x.gender == pawn.gender))
				{
					void changeHeadType()
					{
						pawn.story.headType = headType;
						recachePreview = true;
					}
					options.Add(new FloatMenuOption(GetHeadtypeLabel(headType), changeHeadType));
				}
				Find.WindowStack.Add(new FloatMenu(options));
			}
			curY += Math.Max(crownLabelRect.height, crownButtonRect.height);

			Rect bodyLabelRect = new Rect(previewRect.x, curY, previewRect.width / 3, Text.CalcHeight(BODY_LABEL_LOC_STRING.Translate(), previewRect.width / 3));
			Rect bodyButtonRect = new Rect(previewRect.x + previewRect.width / 3, curY, previewRect.width * 2 / 3, Text.CalcHeight(pawn.story.bodyType.defName, previewRect.width * 2 / 3));
			Widgets.Label(bodyLabelRect, BODY_LABEL_LOC_STRING.Translate());
			if (Widgets.ButtonText(bodyButtonRect, pawn.story.bodyType.defName))
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();

				// Get list of body types from alien race if possible otherwise list all.
				IEnumerable<BodyTypeDef> bodyTypes;
				if (pawn.def is ThingDef_AlienRace alien)
					bodyTypes = alien.GetPartGenerator().bodyTypes;
				else
					bodyTypes = DefDatabase<BodyTypeDef>.AllDefs;

				foreach (BodyTypeDef bodyType in bodyTypes)
				{
					void changeBodyType()
					{
						pawn.story.bodyType = bodyType;
						recachePreview = true;
					}
					options.Add(new FloatMenuOption(bodyType.defName, changeBodyType));
				}
				Find.WindowStack.Add(new FloatMenu(options));
			}
			curY += Math.Max(bodyLabelRect.height, bodyButtonRect.height);

			// Then the parts list toggles...
			string skinSyncText = SKIN_SYNC_LOC_STRING.Translate();
			Rect skinSyncRect = new Rect(columnWidth + SPACER_SIZE, curY, PREVIEW_SIZE.x, Text.CalcHeight(skinSyncText, PREVIEW_SIZE.x));
			Widgets.CheckboxLabeled(skinSyncRect, skinSyncText, ref skinSync);
			curY += skinSyncRect.height;

			string symmetryToggleText = DO_SYMMETRY_LOC_STRING.Translate();
			Rect symmetryToggleRect = new Rect(columnWidth + SPACER_SIZE, curY, PREVIEW_SIZE.x, Text.CalcHeight(symmetryToggleText, PREVIEW_SIZE.x));
			Widgets.CheckboxLabeled(symmetryToggleRect, symmetryToggleText, ref doSymmetry);
			curY += symmetryToggleRect.height;


			Rect buttonLabelRect = new Rect(columnWidth + SPACER_SIZE, curY, previewRect.width / 3, Text.LineHeight);
			Rect buttonRect = new Rect(buttonLabelRect.xMax, curY, previewRect.width - buttonLabelRect.width, Text.LineHeight);
			// Apply template
			Widgets.Label(buttonLabelRect, "Template:");
			if (_database.MutationTemplates.Count > 0)
			{
				if (Widgets.ButtonText(buttonRect, "Select"))
				{
					List<FloatMenuOption> options = new List<FloatMenuOption>();
					foreach (MutationTemplate template in _database.MutationTemplates)
						options.Add(new FloatMenuOption(template.Caption, () => ApplyTemplate(template)));

					Find.WindowStack.Add(new FloatMenu(options));
				}
			}
			else
				Widgets.Label(buttonRect, "No templates.");

			curY += buttonRect.height;


			buttonRect.y = curY;
			// Save template
			if (_database.FreeStorage > (pawnCurrentMutations.Count * MutationTemplate.GENEBANK_COST_PER_MUTATION))
			{
				if (Widgets.ButtonText(buttonRect, "Save"))
				{
					Dialog_Textbox textbox = new Dialog_Textbox(String.Empty, false, new Vector2(300, 125));
					textbox.ApplyAction = (value) => SaveTemplate(value);
					Find.WindowStack.Add(textbox);
				}
			}
			else
				Widgets.Label(buttonRect, "Not enough capacity.");
			curY += buttonRect.height;

			// Then finally the Aspect selection list.
			// Remember this needs scrolling, Brennen.







			// Draw the right column, consisting of the modification summary (top box) and the currently hovered over mutation description (bottom box).
			DrawDescriptionBoxes(new Rect(inRect.width - columnWidth, titleHeight + SPACER_SIZE, columnWidth, columnHeight));

			// Draw the apply, reset and cancel buttons.
			float buttonVertPos = inRect.height - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y));
			float applyHorPos = inRect.width / 2 - APPLY_BUTTON_SIZE.x - RESET_BUTTON_SIZE.x / 2 - SPACER_SIZE;
			float resetHorPos = inRect.width / 2 - RESET_BUTTON_SIZE.x / 2;
			float cancelHorPos = inRect.width / 2 + RESET_BUTTON_SIZE.x / 2 + SPACER_SIZE;
			if (Widgets.ButtonText(new Rect(applyHorPos, buttonVertPos, APPLY_BUTTON_SIZE.x, APPLY_BUTTON_SIZE.y), APPLY_BUTTON_LOC_STRING.Translate()))
			{
				OnAcceptKeyPressed();
			}
			if (Widgets.ButtonText(new Rect(resetHorPos, buttonVertPos, RESET_BUTTON_SIZE.x, RESET_BUTTON_SIZE.y), RESET_BUTTON_LOC_STRING.Translate()))
			{
				SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera(null);
				Reset();
			}
			if (Widgets.ButtonText(new Rect(cancelHorPos, buttonVertPos, CANCEL_BUTTON_SIZE.x, CANCEL_BUTTON_SIZE.y), CANCEL_BUTTON_LOC_STRING.Translate()))
			{
				OnCancelKeyPressed();
			}
		}

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
		{
			pawn.health.hediffSet.hediffs = new List<Hediff>(cachedInitialHediffs.Select(m => m.hediff));
			addedMutations = new AddedMutations();
			foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
			{
				hediff.Severity = cachedInitialHediffs.FirstOrDefault(m => m.hediff == hediff).severity;
				var comp = (hediff as Hediff_AddedMutation)?.TryGetComp<Comp_MutationSeverityAdjust>();
				if (comp != null) comp.Halted = cachedInitialHediffs.FirstOrDefault(m => m.hediff == hediff).isHalted;
			}
			RecachePawnMutations();
			pawn.story.bodyType = initialBodyType;
			pawn.story.headType = initialCrownType;
			recachePreview = true;
		}


		private string GetHeadtypeLabel(HeadTypeDef headtype)
		{
			if (_labelCache.TryGetValue(headtype, out string value) == false)
			{
				value = headtype.LabelCap;
				if (String.IsNullOrWhiteSpace(value))
					value = headtype.defName;

				value = value.Replace("_", " ");
				value = value.Replace(pawn.gender.ToString(), "").Trim();
				value = value.Replace("Average", "");
				_labelCache[headtype] = value;
			}

			return value;
		}

		private void RecachePawnMutations()
		{
			pawnCurrentMutations = pawn.health.hediffSet.hediffs.Where(m => m.def.GetType() == typeof(MutationDef)).Cast<Hediff_AddedMutation>().ToList();
		}

		private void DrawPartsList(Rect outRect, Rect viewRect)
		{
			float curY = outRect.y;
			Widgets.BeginScrollView(outRect, ref partListScrollPos, viewRect);
			if (skinSync)
			{
				DrawPartEntry(ref curY, viewRect, cachedMutableSkinParts, MutationLayer.Skin.ToString(), true);
			}
			if (doSymmetry)
			{
				foreach (BodyPartDef partDef in cachedMutableParts.Select(m => m.def).Distinct())
				{
					DrawPartEntry(ref curY, viewRect, (skinSync ? cachedMutableCoreParts : cachedMutableParts).Where(m => m.def == partDef).ToList(), partDef.LabelCap);
				}
			}
			else
			{
				foreach (BodyPartRecord partRecord in cachedMutableParts)
				{
					DrawPartEntry(ref curY, viewRect, (skinSync ? cachedMutableCoreParts : cachedMutableParts).Where(m => m == partRecord).ToList(), partRecord.LabelCap);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				partListScrollSize.x = outRect.width - 16f;
				partListScrollSize.y = curY - outRect.y;
			}
			Widgets.EndScrollView();
		}

		private void DrawPartEntry(ref float curY, Rect partListViewRect, List<BodyPartRecord> parts, string label, bool skinEntry = false)
		{
			if (parts.Count == 0)
				return;

			List<Hediff_AddedMutation> mutations;
			string buttonLabel;
			Widgets.ListSeparator(ref curY, partListViewRect.width, label);
			if (!skinSync || cachedMutationLayersByPartDef[parts.FirstOrDefault().def].Count > 2)
			{
				foreach (MutationLayer layer in cachedMutationLayersByPartDef[parts.FirstOrDefault().def])
				{
					mutations = pawnCurrentMutations.Where(m => parts.Contains(m.Part) && m.Def.Layer == layer).ToList();
					buttonLabel = $"{layer.ToString().Translate()}: {(mutations.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutations.Select(m => $"{m.Def.LabelCap} ({String.Join(", ", m.Def.ClassInfluences.Select(x => x.label.CapitalizeFirst()))})").Distinct()))}";
					DrawPartButtons(ref curY, partListViewRect, mutations, parts, layer, buttonLabel);
				}
			}
			else
			{
				MutationLayer layer;
				if (skinEntry)
				{
					mutations = pawnCurrentMutations.Where(m => parts.Contains(m.Part) && m.Def.Layer == MutationLayer.Skin).ToList();
					layer = MutationLayer.Skin;
				}
				else
				{
					mutations = pawnCurrentMutations.Where(m => parts.Contains(m.Part) && m.Def.Layer == MutationLayer.Core).ToList();
					layer = MutationLayer.Core;
				}

				buttonLabel = $"{(mutations.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutations.Select(m => $"{m.Def.LabelCap}").Distinct()))}";
				DrawPartButtons(ref curY, partListViewRect, mutations, parts, layer, buttonLabel);
			}
		}


		private void DrawPartButtons(ref float curY, Rect partListViewRect, List<Hediff_AddedMutation> mutations, List<BodyPartRecord> parts, MutationLayer layer, string label)
		{
			bool hasDetails = false;
			if (mutations.Count > 0 && mutations.Any(x => x.SeverityAdjust != null))
				hasDetails = true;

			// Draw the main mutation selection button. It should take up the whole width if there are no details, otherwise it will leave a space for the edit button.
			float partButtonWidth = partListViewRect.width - (hasDetails == false ? 0 : editButtonWidth);
			Rect partButtonRect = new Rect(partListViewRect.x, curY, partButtonWidth, Text.CalcHeight(label, partButtonWidth - BUTTON_HORIZONTAL_PADDING));
			if (Widgets.ButtonText(partButtonRect, label))
			{
				List<FloatMenuOption> options = new List<FloatMenuOption>();
				void removeMutations()
				{
					foreach (Hediff_AddedMutation mutation in mutations)
					{
						addedMutations.RemoveByPartAndLayer(mutation.Part, layer);
						if (cachedInitialHediffs.Select(m => m.hediff).Contains(mutation))
						{
							addedMutations.AddData(mutation.Def, mutation.Part, mutation.Severity, mutation.ProgressionHalted, true);
						}
						pawn.health.RemoveHediff(mutation);
					}
					recachePreview = true;
					RecachePawnMutations();
				}
				options.Add(new FloatMenuOption(NO_MUTATIONS_LOC_STRING.Translate(), removeMutations));


				List<MutationDef> mutationDefs = cachedMutationDefsByPartDef[parts.FirstOrDefault().def];
				foreach (MutationDef mutationDef in mutationDefs.Where(m => m.Layer == layer && (DebugSettings.godMode || m.IsTagged())))
				{
					options.Add(new FloatMenuOption($"{mutationDef.LabelCap} ({String.Join(", ", mutationDef.ClassInfluences.Select(x => x.label.CapitalizeFirst()))})", () => AddMutation(mutations, parts, layer, mutationDef)));
				}
				Find.WindowStack.Add(new FloatMenu(options));
			}
			curY += partButtonRect.height;

			// If there are actually mutations, draw the edit button.
			if (hasDetails)
			{
				Rect editButtonRect = new Rect(partButtonWidth, partButtonRect.y, editButtonWidth, partButtonRect.height);
				if (Widgets.ButtonText(editButtonRect, editButtonText))
				{
					detailPart = (detailPart.Item1 == parts.FirstOrDefault() && detailPart.Item2 == layer) ? new Tuple<BodyPartRecord, MutationLayer>(new BodyPartRecord(), 0) : new Tuple<BodyPartRecord, MutationLayer>(parts.FirstOrDefault(), layer);
				}
			}

			// If the currently selected part and layer match up with the part to give details for, draw the edit area below the buttons.
			if (hasDetails && detailPart.Item1 == parts.FirstOrDefault() && detailPart.Item2 == layer)
			{
				foreach (MutationDef mutationDef in mutations.Select(m => m.Def).Distinct())
				{
					List<Hediff_AddedMutation> mutationsOfDef = mutations.Where(m => m.Def == mutationDef).ToList();

					// Draw the LabelCap of the current Def if there is more than one type of mutation in the current list.
					if (mutations.Select(m => m.Def).Distinct().Count() > 1)
					{
						Widgets.ListSeparator(ref curY, partListViewRect.width, mutationDef.LabelCap);
					}

					if (mutationDef.HasComp(typeof(Comp_MutationSeverityAdjust)))
						DrawSeverityBar(ref curY, ref partListViewRect, layer, mutationDef, mutationsOfDef);

					// If the mutation has the ability to be paused, show the toggle for it.
					// This is a CheckboxMulti to handle edge cases, but likely could be replaced with a simple Checkbox.
					if (mutationDef.CompProps<CompProperties_MutationSeverityAdjust>() != null)
					{
						float pauseLabelWidth = partListViewRect.width - IS_PAUSED_CHECKBOX_SIZE.x;
						Rect pauseLabelRect = new Rect(partListViewRect.x, curY, pauseLabelWidth, Text.CalcHeight(IS_PAUSED_LOC_STRING.Translate(), partListViewRect.width));
						Rect checkBoxRect = new Rect(partListViewRect.x + pauseLabelWidth, curY, IS_PAUSED_CHECKBOX_SIZE.x, IS_PAUSED_CHECKBOX_SIZE.y);
						MultiCheckboxState initialState = !mutationsOfDef.Select(n => n.ProgressionHalted).Contains(true) ? MultiCheckboxState.Off : !mutationsOfDef.Select(n => n.ProgressionHalted).Contains(false) ? MultiCheckboxState.On : MultiCheckboxState.Partial;
						Widgets.Label(pauseLabelRect, IS_PAUSED_LOC_STRING.Translate());
						MultiCheckboxState newState = Widgets.CheckboxMulti(checkBoxRect, initialState);
						if (initialState != newState)
						{
							initialState = newState;
							mutationsOfDef.FirstOrDefault().SeverityAdjust.Halted = !mutationsOfDef.FirstOrDefault().SeverityAdjust.Halted;
							foreach (Hediff_AddedMutation mutationOfDef in mutationsOfDef)
							{
								MutationData relevantEntry = addedMutations.MutationsByPartAndLayer(mutationOfDef.Part, layer);
								if (cachedInitialHediffs.Select(m => m.hediff).Contains(mutationOfDef))
								{
									bool initialHediffIsHalted = cachedInitialHediffs.Where(m => m.hediff == mutationOfDef).FirstOrDefault().isHalted;
									if (newState == MultiCheckboxState.On == initialHediffIsHalted)
										addedMutations.RemoveByPartAndLayer(mutationOfDef.Part, layer);
								}
								if (relevantEntry != null)
								{
									relevantEntry.isHalted = newState == MultiCheckboxState.On;
								}
								else
								{
									addedMutations.AddData(mutationOfDef.Def, mutationOfDef.Part, mutationOfDef.Severity, newState == MultiCheckboxState.On, false);
								}
							}
						}
						curY += Math.Max(pauseLabelRect.height, checkBoxRect.height);
					}
				}
			}

			// Create a zone for updating the lower description box (The one that shows details based on the currently hovered over mutation).
			Rect descriptionUpdateRect = new Rect(partListViewRect.x, partButtonRect.y, partListViewRect.width, curY - partButtonRect.y);
			if (Mouse.IsOver(descriptionUpdateRect))
			{
				foreach (MutationDef mutation in mutations.Select(m => m.def).Distinct())
				{
					Hediff_AddedMutation firstMutationOfDef = mutations.Where(m => m.def == mutation).FirstOrDefault();
					partDescBuilder.AppendLine(firstMutationOfDef.LabelCap);
					partDescBuilder.AppendLine(firstMutationOfDef.Description);
					partDescBuilder.AppendLine(firstMutationOfDef.TipStringExtra);
					partDescBuilder.AppendLine();
				}
			}
		}

		private void AddMutation(IEnumerable<Hediff_AddedMutation> mutations, IEnumerable<BodyPartRecord> parts, MutationLayer layer, MutationDef mutationDef, float? severity = null, bool halted = false)
		{
			float severityValue = Mathf.Min(severity ?? mutationDef.maxSeverity, maxPawnSeverity);

			foreach (Hediff_AddedMutation mutation in mutations)
			{
				pawn.health.RemoveHediff(mutation);
			}


			if (mutationDef.blockSites != null)
			{
				// Foreach body part blocked by added mutation
				foreach (var blockedPartDef in mutationDef.blockSites)
				{
					// Find all equivalent body part records
					foreach (BodyPartRecord partRecord in (skinSync ? cachedMutableCoreParts : cachedMutableParts).Where(m => m.def == blockedPartDef))
					{
						// Remove all mutations on those body parts of same layer
						foreach (Hediff_AddedMutation mutation in pawnCurrentMutations.Where(m => m.Part == partRecord && m.Def.Layer == layer))
						{
							pawn.health.RemoveHediff(mutation);
						}
						addedMutations.RemoveByPartAndLayer(partRecord, layer);
					}
				}
			}

			foreach (BodyPartRecord part in parts)
			{
				addedMutations.RemoveByPartAndLayer(part, layer);
				addedMutations.AddData(mutationDef, part, severityValue, halted, false);
				MutationResult result = MutationUtilities.AddMutation(pawn, mutationDef, part, ancillaryEffects: MutationUtilities.AncillaryMutationEffects.None); //don't give the green puffs
				if (result.Count > 0)
				{
					foreach (Hediff_AddedMutation mutation in result)
					{
						mutation.Severity = severityValue;
						mutation.UpdateStage();
					}
				}
			}
			recachePreview = true;
			RecachePawnMutations();
		}

		private void DrawSeverityBar(ref float curY, ref Rect partListViewRect, MutationLayer layer, MutationDef mutationDef, List<Hediff_AddedMutation> mutationsOfDef)
		{
			// Draw the various labels for the severity bar (need to refine this later).
			// Update current mutation stage.
			mutationsOfDef.FirstOrDefault().UpdateStage();
			string stageLabelText = $"Stage {mutationsOfDef.FirstOrDefault().CurStageIndex}: {mutationsOfDef.FirstOrDefault().LabelCap}";
			Rect severityLabelsRect = new Rect(partListViewRect.x, curY, partListViewRect.width, Text.CalcHeight(stageLabelText, partListViewRect.width));
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(severityLabelsRect, stageLabelText);
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(severityLabelsRect, mutationsOfDef.FirstOrDefault().Severity.ToString("n2"));
			Text.Anchor = TextAnchor.UpperLeft;
			curY += severityLabelsRect.height;

			// Draw the severity slider
			float curSeverity = mutationsOfDef.Select(n => n.Severity).Average();
			float minSeverity = 0; // mutationDef.minSeverity;
			float maxSeverity = Mathf.Min(mutationDef.maxSeverity, maxPawnSeverity);;

			if (debugMode)
			{
				minSeverity = 0;
				maxSeverity = mutationDef.maxSeverity;
			}


			float newSeverity = Widgets.HorizontalSlider(new Rect(partListViewRect.x, curY, partListViewRect.width, SLIDER_HEIGHT), curSeverity, minSeverity, maxSeverity);
			if (curSeverity != newSeverity)
			{
				curSeverity = newSeverity;
				foreach (Hediff_AddedMutation mutationOfDef in mutationsOfDef)
				{
					MutationData relevantEntry = addedMutations.MutationsByPartAndLayer(mutationOfDef.Part, layer);
					if (relevantEntry != null)
					{
						relevantEntry.severity = newSeverity;
					}
					else
					{
						addedMutations.AddData(mutationOfDef.Def, mutationOfDef.Part, newSeverity, mutationOfDef.ProgressionHalted, false);
					}
					mutationOfDef.Severity = newSeverity;
				}
				recachePreview = true;
			}
			curY += SLIDER_HEIGHT;
		}

		private const string REMOVING_MUTATION_DESC = "PPRemivingMutationDesc";
		private const string ADDING_MUTATION_DESC = "PPAddingMutationDesc";
		private const string HALTED_MUTATION_DESC = "PPNewMutationIsHalted";

		/// <summary>
		/// Draws the description boxes.
		/// </summary>
		/// <param name="inRect">The in rect.</param>
		public void DrawDescriptionBoxes(Rect inRect)
		{
			// Build the modification summary string.


			foreach (MutationData entry in addedMutations.mutationData)
			{
				if (entry.removing)
				{
					summaryBuilder.AppendLine(REMOVING_MUTATION_DESC.Translate(entry.mutation.Named("MUTATION"), pawn.Named("PAWN"), entry.part.Label.Named("PART")));
				}
				else
				{
					string isHaltedTxt = (entry.isHalted ? HALTED_MUTATION_DESC.Translate().RawText : "");

					var addStr = ADDING_MUTATION_DESC.Translate(entry.mutation.Named("MUTATION"), entry.part.Label.Named("PART"), pawn.Named("PAWN"),
																entry.severity.ToString("n2").Named("SEVERITY"))
							   + isHaltedTxt;

					summaryBuilder.AppendLine(addStr);
				}
				summaryBuilder.AppendLine();
			}

			// Draw modification summary description.
			Rect summaryMenuSectionRect = new Rect(inRect.x, inRect.y, inRect.width, (inRect.height - SPACER_SIZE) / 2);
			Widgets.DrawMenuSection(summaryMenuSectionRect);

			Rect summaryMenuTitleRect = summaryMenuSectionRect.ContractedBy(MENU_SECTION_CONSTRICTION_SIZE);
			float titleHeight = Text.CalcHeight(SUMMARY_TITLE_LOC_STRING.Translate(), summaryMenuTitleRect.width);
			Widgets.Label(new Rect(summaryMenuTitleRect.x, summaryMenuTitleRect.y, summaryMenuTitleRect.width, titleHeight), SUMMARY_TITLE_LOC_STRING.Translate());
			GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			Widgets.DrawLineHorizontal(summaryMenuTitleRect.x, titleHeight + summaryMenuTitleRect.y, summaryMenuTitleRect.width);
			titleHeight += 1f;
			GUI.color = Color.white;

			Rect summaryOutRect = new Rect(summaryMenuSectionRect.x, summaryMenuSectionRect.y + titleHeight, summaryMenuSectionRect.width, summaryMenuSectionRect.height - titleHeight).ContractedBy(MENU_SECTION_CONSTRICTION_SIZE);
			Rect summaryViewRect = new Rect(summaryOutRect.x, summaryOutRect.y, summaryScrollSize.x, summaryScrollSize.y);
			float summaryCurY = summaryOutRect.y;
			Widgets.BeginScrollView(summaryOutRect, ref summaryScrollPos, summaryViewRect);
			string summaryText = summaryBuilder.ToString();
			Widgets.Label(new Rect(summaryViewRect.x, summaryCurY, summaryViewRect.width, Text.CalcHeight(summaryText, summaryViewRect.width)), summaryText);
			summaryCurY += Text.CalcHeight(summaryText, summaryViewRect.width);
			if (Event.current.type == EventType.Layout)
			{
				summaryScrollSize.y = summaryCurY - summaryViewRect.y - titleHeight;
				summaryScrollSize.x = summaryOutRect.width - (summaryScrollSize.y > summaryOutRect.height ? 16f : 0f);
			}
			Widgets.EndScrollView();
			summaryBuilder.Clear();

			// Draw mutation description.
			Rect descMenuSectionRect = new Rect(inRect.x, (inRect.height + SPACER_SIZE) / 2 + inRect.y, inRect.width, (inRect.height - SPACER_SIZE) / 2);
			Rect descOutRect = descMenuSectionRect.ContractedBy(MENU_SECTION_CONSTRICTION_SIZE);
			Rect descViewRect = new Rect(descOutRect.x, descOutRect.y, descScrollSize.x, descScrollSize.y);
			float descCurY = descOutRect.y;
			Widgets.DrawMenuSection(descMenuSectionRect);
			Widgets.BeginScrollView(descOutRect, ref descScrollPos, descViewRect);
			Widgets.Label(new Rect(descViewRect.x, descCurY, descViewRect.width, Text.CalcHeight(PART_DESCRIPTION_TITLE_LOC_STRING.Translate(), descViewRect.width)), PART_DESCRIPTION_TITLE_LOC_STRING.Translate());
			descCurY += Text.CalcHeight(PART_DESCRIPTION_TITLE_LOC_STRING.Translate(), descViewRect.width);
			GUI.color = new Color(0.5f, 0.5f, 0.5f, 1f);
			Widgets.DrawLineHorizontal(descViewRect.x, descCurY, descOutRect.width);
			descCurY += 1f;
			GUI.color = Color.white;
			Widgets.Label(new Rect(descViewRect.x, descCurY, descViewRect.width, Text.CalcHeight(partDescBuilder.ToString(), descViewRect.width)), partDescBuilder.ToString());
			descCurY += Text.CalcHeight(partDescBuilder.ToString(), descViewRect.width);
			if (Event.current.type == EventType.Layout)
			{
				descScrollSize.y = descCurY - descViewRect.y;
				descScrollSize.x = descOutRect.width - (descScrollSize.y > descOutRect.height ? 16f : 0f);
			}
			Widgets.EndScrollView();
			partDescBuilder.Clear();
		}

		/// <summary>
		/// Sets the pawn preview.
		/// </summary>
		public void SetPawnPreview()
		{
			if (pawn != null)
			{
				recachePreview = false;

				Color? hybridColor = CalculatePawnSkin();

				if (hybridColor.HasValue)
					alienComp.ColorChannels["skin"].first = hybridColor.Value;
				else
				{
					alienComp.ColorChannels["skin"].first = initialPawnColors.Item1;
					alienComp.ColorChannels["skin"].second = initialPawnColors.Item2;
				}
				alienComp.CompTick();


				pawn.Drawer.renderer.SetAllGraphicsDirty();
				previewImage = PortraitsCache.Get(pawn, new Vector2(PREVIEW_SIZE.x, PREVIEW_SIZE.y), previewRot, new Vector3(0f, 0f, 0.15f), 1.1f, supersample: true, compensateForUIScale: true, toggleClothesEnabled, toggleClothesEnabled, healthStateOverride: PawnHealthState.Mobile);
			}
			else
			{
				Log.Error("Pawn was null when attempting to recache part picker preview.");
			}
		}


		private Color? CalculatePawnSkin()
		{
			// Get all current and new mutations.
			List<MutationDef> currentMutations = pawnCurrentMutations.Select(x => x.Def).ToList();
			currentMutations.AddRange(addedMutations.mutationData.Select(x => x.mutation));

			// Exclude all mutations being removed.
			IQueryable<MutationDef> removedMutations = addedMutations.mutationData.Where(x => x.removing).Select(x => x.mutation).AsQueryable();
			currentMutations = currentMutations.Where(x => removedMutations.Contains(x) == false).Distinct().ToList();



			List<Tuple<int, ThingDef>> animals = currentMutations.SelectMany(x => x.AssociatedAnimals.Distinct())
																 .GroupBy(x => x)
																 .Select(x => Tuple.Create<int, ThingDef>(x.Count(), x.First()))
																 .ToList();

			if (animals.Count == 0)
				return null;

			// Select the morph kind of which most added mutations are associated with.
			ThingDef highestInfluence = animals.OrderByDescending(x => x.Item1).First().Item2;
			MorphDef morph = highestInfluence.TryGetBestMorphOfAnimal();
			return morph?.GetSkinColorOverride(pawn);
		}

		private void ApplyTemplate(MutationTemplate template)
		{
			IReadOnlyList<MutationDef> taggedMutations = _database.StoredMutations;
			foreach (var templateMutation in template.MutationData)
			{
				// Only add mutations if tagged or in debug mode.
				if (taggedMutations.Contains(templateMutation.MutationDef) || debugMode)
				{
					IEnumerable<Hediff_AddedMutation> mutations = pawnCurrentMutations.Where(m => m.Part.LabelCap == templateMutation.PartLabelCap && m.Def.Layer == templateMutation.MutationDef.Layer);
					IEnumerable<BodyPartRecord> parts = (skinSync ? cachedMutableCoreParts : cachedMutableParts).Where(m => m.LabelCap == templateMutation.PartLabelCap);

					AddMutation(mutations, parts, templateMutation.MutationDef.Layer ?? MutationLayer.Core, templateMutation.MutationDef, templateMutation.Severity, templateMutation.Halted);
				}
			}
		}


		private void SaveTemplate(string name)
		{
			List<MutationTemplateData> mutationData = new List<MutationTemplateData>(pawnCurrentMutations.Count);
			foreach (Hediff_AddedMutation mutation in pawnCurrentMutations)
			{
				mutationData.Add(new MutationTemplateData(mutation.Def, mutation.Part.LabelCap, mutation.Severity, mutation.ProgressionHalted));
			}

			MutationTemplate template = new MutationTemplate(mutationData, name);
			_database.TryAddToDatabase(new TemplateGenebankEntry(template));
		}
	}


	/// <summary>
	/// stores information on the initial state of the hediff 
	/// </summary>
	public class HediffInitialState
	{
		/// <summary>
		/// The hediff
		/// </summary>
		public Hediff hediff;
		/// <summary>
		/// The severity
		/// </summary>
		public float severity;
		/// <summary>
		/// The is halted
		/// </summary>
		public bool isHalted;
		/// <summary>
		/// Initializes a new instance of the <see cref="HediffInitialState"/> class.
		/// </summary>
		/// <param name="hediff">The hediff.</param>
		/// <param name="severity">The severity.</param>
		/// <param name="isHalted">if set to <c>true</c> [is halted].</param>
		public HediffInitialState(Hediff hediff, float severity, bool isHalted)
		{
			this.hediff = hediff;
			this.severity = severity;
			this.isHalted = isHalted;
		}
	}
}
