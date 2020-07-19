using Pawnmorph.Hediffs;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Pawnmorph.User_Interface
{
    class Dialog_PartPicker : Window
    {
        /// <summary>The pawn that we want to modify.</summary>
        private Pawn pawn;
        private Pawn_HealthTracker cachedHealth;

        // Reference variables
        private const string WINDOW_TITLE_LOC_STRING = "PartPickerMenuTitle";
        private const string DESCRIPTION_TITLE_LOC_STRING = "DescriptionPaneTitle";
        private const string DO_SYMMETRY_LOC_STRING = "DoSymmetry";
        private const string NO_MUTATIONS_LOC_STRING = "NoMutationsOnPart";
        private const string EDIT_PARAMS_LOC_STRING = "EditParams";
        private const string TOGGLE_CLOTHES_LOC_STRING = "ToggleClothes";
        private const string ROTATE_LEFT_LOC_STRING = "RotLeft";
        private const string ROTATE_RIGHT_LOC_STRING = "RotRight";
        private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
        private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
        private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
        private const float SPACER_SIZE = 5f;
        private const float BUTTON_HORIZONTAL_PADDING = 6f;
        private static Vector2 PART_BUTTON_SIZE = new Vector2(15, 15);
        private static Vector2 PREVIEW_SIZE = new Vector2(120, 200);
        private static Vector2 TOGGLE_CLOTHES_BUTTON_SIZE = new Vector2(30, 30);
        private static Vector2 ROTATE_LEFT_BUTTON_SIZE = new Vector2(30, 30);
        private static Vector2 ROTATE_RIGHT_BUTTON_SIZE = new Vector2(30, 30);
        private static Vector2 APPLY_BUTTON_SIZE = new Vector2(120f, 40f);
        private static Vector2 RESET_BUTTON_SIZE = new Vector2(120f, 40f);
        private static Vector2 CANCEL_BUTTON_SIZE = new Vector2(120f, 40f);

        // Scrolling variables
        private Vector2 partListScrollPos;
        private Vector2 partListScrollSize;
        private Vector2 descriptionScrollPos;
        private Vector2 descriptionScrollSize;

        // Toggles
        private bool confirmed = false;
        private bool toggleClothesEnabled = true;
        private bool doSymmetry = true;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(1000f, 500f);
            }
        }

        public Dialog_PartPicker(Pawn pawn)
        {
            this.pawn = pawn;
            forcePause = true;
            doCloseX = true;
            resizeable = true;
            draggable = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            cachedHealth = pawn.health;
        }

        public override void Close(bool doCloseSound = false)
        {
            if (!confirmed)
            {
                resetPawnHealth();
                SoundDefOf.Click.PlayOneShotOnCamera(null);
            }
            else
            {
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera(null);
            }
            base.Close(doCloseSound);
        }

        public override void OnAcceptKeyPressed()
        {
            confirmed = true;
            base.OnAcceptKeyPressed();
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Step 1 - Gather and set relevent information.
            float col1, col2, col3;
            List<MutationDef> allMutations = DefDatabase<MutationDef>.AllDefs.ToList();
            List<BodyPartRecord> mutableParts = pawn.RaceProps.body.AllParts.Where(m => DefDatabase<MutationDef>.AllDefs.SelectMany(n => n.parts).Distinct().Contains(m.def)).ToList();
            List<Hediff_AddedMutation> pawnMutations = pawn.health.hediffSet.hediffs.Where(m => m.def.GetType() == typeof(MutationDef)).Cast<Hediff_AddedMutation>().ToList();

            // Step 2 - Draw the title of the window.
            Text.Font = GameFont.Medium;
            string title = $"{WINDOW_TITLE_LOC_STRING.Translate()} - {pawn.Name.ToStringShort} ({pawn.def.LabelCap})";
            float titleHeight = Text.CalcHeight(title, inRect.width);
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, Text.CalcHeight(title, inRect.width)), title);
            Text.Font = GameFont.Small;
            col1 = col2 = col3 = titleHeight;

            // Step 3 - Determine vewing areas for body part list and description.
            float drawableWidth = (inRect.width - PREVIEW_SIZE.x - 2 * SPACER_SIZE) / 2;
            float drawableHeight = inRect.height - titleHeight - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - 2 * SPACER_SIZE;
            Rect partListOutRect = new Rect(inRect.x, titleHeight, drawableWidth, drawableHeight);
            Rect partListViewRect = new Rect(partListOutRect.x, partListOutRect.y, partListScrollSize.x, partListScrollSize.y - titleHeight);
            Rect previewRect = new Rect(inRect.x + SPACER_SIZE + drawableWidth, titleHeight, PREVIEW_SIZE.x, PREVIEW_SIZE.y);
            Rect descriptionOutRect = new Rect(inRect.x + 2 * SPACER_SIZE + PREVIEW_SIZE.x, titleHeight, drawableWidth, drawableHeight);
            Rect descriptiontViewRect = new Rect(descriptionOutRect.x, descriptionOutRect.y, descriptionOutRect.width - 16f, descriptionOutRect.height);

            // Step 4 - Draw the body part list, selection buttons and edit buttons.
            string editButtonText = EDIT_PARAMS_LOC_STRING.Translate();
            float editButtonWidth = Text.CalcSize(editButtonText).x + BUTTON_HORIZONTAL_PADDING;
            Widgets.BeginScrollView(partListOutRect, ref partListScrollPos, partListViewRect);
            if (doSymmetry)
            {
                List<BodyPartDef> uniqueMutablePartDefs = mutableParts.Select(m => m.def).Distinct().ToList();
                foreach (BodyPartDef part in uniqueMutablePartDefs)
                {
                    List<Hediff_AddedMutation> mutationsOnPart = pawnMutations.Where(m => m.Part.def == part).ToList();
                    string text = part.LabelCap;
                    float textHeight = Text.CalcHeight(text, partListViewRect.width);
                    Widgets.Label(new Rect(0f, col1, partListViewRect.width, textHeight), text);
                    col1 += textHeight;
                    foreach (MutationLayer layer in Enum.GetValues(typeof(MutationLayer)))
                    {
                        List<Hediff_AddedMutation> mutationsOnLayer = mutationsOnPart.Where(m => m.TryGetComp<RemoveFromPartComp>().Layer == layer).ToList();
                        string buttonText = $"{layer}: {(mutationsOnLayer.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutationsOnLayer.Select(m => m.LabelCap).Distinct()))}";
                        float buttonHeight = Text.CalcHeight(buttonText, partListViewRect.width);
                        if (Widgets.ButtonText(new Rect(0f, col1, partListViewRect.width - editButtonWidth, buttonHeight), buttonText))
                        {
                            Action action = delegate ()
                            {
                                Log.Error("You Pressed me!");
                            };
                            List<FloatMenuOption> options = new List<FloatMenuOption>();
                            options.Add(new FloatMenuOption(NO_MUTATIONS_LOC_STRING.Translate(), action));
                            foreach (MutationDef mutationForPartAndLayer in allMutations.Where(m => m.parts.Contains(part) && m.comps.Find(n => n.GetType() == typeof(RemoveFromPartCompProperties)).ChangeType<RemoveFromPartCompProperties>().layer == layer))
                            {
                                options.Add(new FloatMenuOption(mutationForPartAndLayer.LabelCap, action));
                            }
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                        if (Widgets.ButtonText(new Rect(partListViewRect.width - editButtonWidth, col1, editButtonWidth, buttonHeight), editButtonText))
                        {

                        }
                        col1 += buttonHeight;
                    }
                }
            }
            else
            {
                foreach (BodyPartRecord part in mutableParts)
                {
                    string text = part.LabelCap;
                    float textHeight = Text.CalcHeight(text, partListViewRect.width);
                    Widgets.Label(new Rect(0f, col1, partListViewRect.width, textHeight), text);
                    col1 += textHeight;
                    foreach (MutationLayer layer in Enum.GetValues(typeof(MutationLayer)))
                    {
                        Hediff_AddedMutation mutationOnPartAndLayer = pawnMutations.Find(m => m.TryGetComp<Hediffs.RemoveFromPartComp>().Layer == layer && m.Part == part);
                        string buttonText = $"{layer}: {(mutationOnPartAndLayer == null ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : mutationOnPartAndLayer.LabelCap)}";
                        float buttonHeight = Text.CalcHeight(buttonText, partListViewRect.width);
                        if (Widgets.ButtonText(new Rect(0f, col1, partListViewRect.width - editButtonWidth, buttonHeight), buttonText))
                        {

                        }
                        if (Widgets.ButtonText(new Rect(partListViewRect.width - editButtonWidth, col1, editButtonWidth, buttonHeight), editButtonText))
                        {

                        }
                        col1 += buttonHeight;
                    }
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                partListScrollSize.x = partListOutRect.width - 16f;
                partListScrollSize.y = col1;
            }
            Widgets.EndScrollView();

            // Step 5 - Draw the preview area then rotation and clothes buttons then symmetry toggle.
            // Step 6 - Draw description box.

            // Step 7 - Draw the apply, reset and cancel buttons.
            float buttonVertPos = titleHeight + drawableHeight + SPACER_SIZE;
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
                resetPawnHealth();
            }
            if (Widgets.ButtonText(new Rect(cancelHorPos, buttonVertPos, CANCEL_BUTTON_SIZE.x, CANCEL_BUTTON_SIZE.y), CANCEL_BUTTON_LOC_STRING.Translate()))
            {
                OnCancelKeyPressed();
            }
        }

        public void resetPawnHealth()
        {
            pawn.health = cachedHealth;
        }
    }
}
