using Pawnmorph.Hediffs;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface
{
    class Dialog_PartPicker : Window
    {
        /// <summary>The pawn that we want to modify.</summary>
        private Pawn pawn;
        private Pawn_HealthTracker startingHealth;

        // Reference variables
        private const string WINDOW_TITLE_LOC_STRING = "PartPickerMenuTitle";
        private const string DESCRIPTION_TITLE_LOC_STRING = "DescriptionPaneTitle";
        private const string DO_SYMMETRY_LOC_STRING = "DoSymmetry";
        private const string NO_MUTATIONS_LOC_STRING = "NoMutationsOnPart";
        private const string TOGGLE_CLOTHES_LOC_STRING = "ToggleClothes";
        private const string ROTATE_LEFT_LOC_STRING = "RotLeft";
        private const string ROTATE_RIGHT_LOC_STRING = "RotRight";
        private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
        private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
        private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
        private const float SPACER_SIZE = 5f;
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
                return new Vector2(500f, 500f);
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
            startingHealth = new Pawn_HealthTracker(pawn);
        }

        public override void Close(bool doCloseSound = true)
        {
            if (!confirmed)
                resetPawnHealth();
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
            List<BodyPartDef> allValidParts = DefDatabase<MutationDef>.AllDefs.SelectMany(m => m.parts).Distinct().ToList();
            List<BodyPartRecord> mutableParts = pawn.RaceProps.body.AllParts.Where(m => allValidParts.Contains(m.def)).ToList();
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
            float drawableHight = inRect.height - col1 - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - 2 * SPACER_SIZE;
            Rect partListOutRect = new Rect(inRect.x, col1, drawableWidth, drawableHight);
            Rect partListViewRect = new Rect(partListOutRect.x, partListOutRect.y, partListScrollSize.x, partListScrollSize.y - col1);
            Rect previewRect = new Rect(inRect.x + SPACER_SIZE + drawableWidth, col1, PREVIEW_SIZE.x, PREVIEW_SIZE.y);
            Rect descriptionOutRect = new Rect(inRect.x + 2 * SPACER_SIZE + PREVIEW_SIZE.x, col1, drawableWidth, drawableHight);
            Rect descriptiontViewRect = new Rect(descriptionOutRect.x, descriptionOutRect.y, descriptionOutRect.width - 16f, descriptionOutRect.height);

            // Step 4 - Draw the body part list.
            Widgets.BeginScrollView(partListOutRect, ref partListScrollPos, partListViewRect);
            if (doSymmetry)
            {
                List<BodyPartDef> uniqueMutablePartDefs = mutableParts.Select(m => m.def).Distinct().ToList();
                foreach (BodyPartDef part in uniqueMutablePartDefs)
                {
                    List<Hediff_AddedMutation> mutationsOnPart = pawnMutations.Where(m => m.Part.def == part).ToList();
                    string text = part.label.CapitalizeFirst();
                    float textHeight = Text.CalcHeight(text, partListViewRect.width);
                    Widgets.Label(new Rect(0f, col1, partListViewRect.width, textHeight), text);
                    col1 += textHeight;
                    foreach (MutationLayer layer in Enum.GetValues(typeof(MutationLayer)))
                    {
                        List<Hediff_AddedMutation> mutationsOnLayer = mutationsOnPart.Where(m => m.TryGetComp<Hediffs.RemoveFromPartComp>().Layer == layer).ToList();
                        string buttonText = $"{layer.ToString()}: {(mutationsOnLayer.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutationsOnLayer.Select(m => m.LabelCap).Distinct()))}";
                        float buttonHeight = Text.CalcHeight(buttonText, partListViewRect.width);
                        Widgets.ButtonText(new Rect(0f, col1, partListViewRect.width, buttonHeight), buttonText);
                        col1 += buttonHeight;
                    }
                    // Add spacer
                }
            }
            else
            {
                foreach (BodyPartRecord item in mutableParts)
                {
                    // Show all parts
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
        }

        public void resetPawnHealth()
        {
            pawn.health = startingHealth;
            startingHealth = new Pawn_HealthTracker(pawn);
        }
    }
}
