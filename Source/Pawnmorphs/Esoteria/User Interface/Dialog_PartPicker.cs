using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;
using AlienRace;
using Pawnmorph.Hediffs;

namespace Pawnmorph.User_Interface
{
    class Dialog_PartPicker : Window
    {
        /// <summary>The pawn that we want to modify.</summary>
        private Pawn pawn;
        private List<Hediff> cachedHediffList;

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
        private static Vector2 ROTATE_CW_BUTTON_SIZE = new Vector2(30, 30);
        private static Vector2 ROTATE_CCW_BUTTON_SIZE = new Vector2(30, 30);
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
        private Rot4 previewRot = Rot4.South;

        // Preview related variables
        public bool forceRecachePreview = false;
        private GameObject gameObject;
        private Camera camera;
        private RenderTexture previewImage;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(750f, 840f);
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
            cachedHediffList = new List<Hediff>(pawn.health.hediffSet.hediffs);
        }

        public override void Close(bool doCloseSound = false)
        {
            if (!confirmed)
            {
                ResetPawnHealth();
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            else
            {
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
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
                        List<MutationDef> applicableMutations = allMutations.Where(m => m.parts.Contains(part) && m.comps.Find(n => n.GetType() == typeof(RemoveFromPartCompProperties)).ChangeType<RemoveFromPartCompProperties>().layer == layer).ToList();
                        if (!applicableMutations.NullOrEmpty())
                        {
                            List<Hediff_AddedMutation> mutationsOnLayer = mutationsOnPart.Where(m => m.TryGetComp<RemoveFromPartComp>().Layer == layer).ToList();
                            string buttonText = $"{layer}: {(mutationsOnLayer.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutationsOnLayer.Select(m => m.LabelCap).Distinct()))}";
                            float buttonHeight = Text.CalcHeight(buttonText, partListViewRect.width);
                            if (Widgets.ButtonText(new Rect(0f, col1, partListViewRect.width - editButtonWidth, buttonHeight), buttonText))
                            {
                                List<FloatMenuOption> options = new List<FloatMenuOption>();
                                Action removeAction = delegate ()
                                {
                                    if (!mutationsOnLayer.NullOrEmpty())
                                    {
                                        foreach (Hediff_AddedMutation hediff in mutationsOnLayer)
                                        {
                                            pawn.health.RemoveHediff(hediff);
                                        }
                                    }
                                    forceRecachePreview = true;
                                };
                                options.Add(new FloatMenuOption(NO_MUTATIONS_LOC_STRING.Translate(), removeAction));
                                foreach (MutationDef mutationDef in applicableMutations)
                                {
                                    Action action = delegate ()
                                    {
                                        if (!mutationsOnLayer.NullOrEmpty())
                                        {
                                            foreach (Hediff_AddedMutation hediff in mutationsOnLayer)
                                            {
                                                pawn.health.RemoveHediff(hediff);
                                            }
                                        }
                                        foreach (BodyPartRecord bpr in pawn.RaceProps.body.AllParts.Where(m => m.def == part))
                                        {
                                            MutationUtilities.AddMutation(pawn, mutationDef, bpr);
                                        }
                                        forceRecachePreview = true;
                                    };
                                    options.Add(new FloatMenuOption(mutationDef.LabelCap, action));
                                }
                                Find.WindowStack.Add(new FloatMenu(options));
                            }
                            if (Widgets.ButtonText(new Rect(partListViewRect.width - editButtonWidth, col1, editButtonWidth, buttonHeight), editButtonText))
                            {
                                // Edit the paramaters of the relevant mutations, such as current stage, if it's halted, etc. (Check for full list of what can be modified later)
                            }
                            col1 += buttonHeight;
                        }
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
                        List<MutationDef> applicableMutations = allMutations.Where(m => m.parts.Contains(part.def) && m.comps.Find(n => n.GetType() == typeof(RemoveFromPartCompProperties)).ChangeType<RemoveFromPartCompProperties>().layer == layer).ToList();
                        if (!applicableMutations.NullOrEmpty())
                        {
                            Hediff_AddedMutation mutationOnPartAndLayer = pawnMutations.Find(m => m.TryGetComp<Hediffs.RemoveFromPartComp>().Layer == layer && m.Part == part);
                            string buttonText = $"{layer}: {(mutationOnPartAndLayer == null ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : mutationOnPartAndLayer.LabelCap)}";
                            float buttonHeight = Text.CalcHeight(buttonText, partListViewRect.width);
                            if (Widgets.ButtonText(new Rect(0f, col1, partListViewRect.width - editButtonWidth, buttonHeight), buttonText))
                            {
                                List<FloatMenuOption> options = new List<FloatMenuOption>();
                                Action removeAction = delegate ()
                                {
                                    if (mutationOnPartAndLayer != null) pawn.health.RemoveHediff(mutationOnPartAndLayer);
                                    forceRecachePreview = true;
                                };
                                options.Add(new FloatMenuOption(NO_MUTATIONS_LOC_STRING.Translate(), removeAction));
                                foreach (MutationDef mutationDef in applicableMutations)
                                {
                                    Action action = delegate ()
                                    {
                                        if (mutationOnPartAndLayer != null) pawn.health.RemoveHediff(mutationOnPartAndLayer);
                                        MutationUtilities.AddMutation(pawn, mutationDef, part);
                                        forceRecachePreview = true;
                                    };
                                    options.Add(new FloatMenuOption(mutationDef.LabelCap, action));
                                }
                                Find.WindowStack.Add(new FloatMenu(options));
                            }
                            if (Widgets.ButtonText(new Rect(partListViewRect.width - editButtonWidth, col1, editButtonWidth, buttonHeight), editButtonText))
                            {
                                // Edit the paramaters of the mutation on the current part and layer, such as its current stage, if it's halted, etc. (Check for full list of what can be modified later)
                            }
                            col1 += buttonHeight;
                        }
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
            if (forceRecachePreview || previewImage == null)
            {
                setPawnPreview();
            }
            GUI.DrawTexture(previewRect, previewImage);
            col2 += previewRect.height;
            float rotCWHorPos = previewRect.x + previewRect.width / 2 - TOGGLE_CLOTHES_BUTTON_SIZE.x / 2 - ROTATE_CW_BUTTON_SIZE.x - SPACER_SIZE;
            float toggleClothingHorPos = previewRect.x + previewRect.width / 2 - TOGGLE_CLOTHES_BUTTON_SIZE.x / 2;
            float rotCCWHorPos = previewRect.x + previewRect.width / 2 + TOGGLE_CLOTHES_BUTTON_SIZE.x / 2 + SPACER_SIZE;
            col2 += SPACER_SIZE;
            if (Widgets.ButtonImageFitted(new Rect(rotCWHorPos, col2, ROTATE_CW_BUTTON_SIZE.x, ROTATE_CW_BUTTON_SIZE.y), ButtonTexturesPM.rotCW, Color.white, Color.blue))
            {
                previewRot.Rotate(RotationDirection.Clockwise);
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                forceRecachePreview = true;
            }
            if (Widgets.ButtonImageFitted(new Rect(toggleClothingHorPos, col2, TOGGLE_CLOTHES_BUTTON_SIZE.x, TOGGLE_CLOTHES_BUTTON_SIZE.y), ButtonTexturesPM.toggleClothes, Color.white, Color.blue))
            {
                toggleClothesEnabled = !toggleClothesEnabled;
                (toggleClothesEnabled ? SoundDefOf.Checkbox_TurnedOn : SoundDefOf.Checkbox_TurnedOff).PlayOneShotOnCamera();
                forceRecachePreview = true;
            }
            if (Widgets.ButtonImageFitted(new Rect(rotCCWHorPos, col2, ROTATE_CCW_BUTTON_SIZE.x, ROTATE_CCW_BUTTON_SIZE.y), ButtonTexturesPM.rotCCW, Color.white, Color.blue))
            {
                previewRot.Rotate(RotationDirection.Counterclockwise);
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                forceRecachePreview = true;
            }
            col2 += Math.Max(TOGGLE_CLOTHES_BUTTON_SIZE.y, Math.Max(ROTATE_CW_BUTTON_SIZE.y, ROTATE_CCW_BUTTON_SIZE.y));
            string toggleText = DO_SYMMETRY_LOC_STRING.Translate();
            float toggleTextHeight = Text.CalcHeight(toggleText, PREVIEW_SIZE.x);
            Widgets.CheckboxLabeled(new Rect(inRect.x + SPACER_SIZE + drawableWidth, col2, PREVIEW_SIZE.x, toggleTextHeight), DO_SYMMETRY_LOC_STRING.Translate(), ref doSymmetry);
            col2 += toggleTextHeight;

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
                ResetPawnHealth();
                forceRecachePreview = true;
            }
            if (Widgets.ButtonText(new Rect(cancelHorPos, buttonVertPos, CANCEL_BUTTON_SIZE.x, CANCEL_BUTTON_SIZE.y), CANCEL_BUTTON_LOC_STRING.Translate()))
            {
                OnCancelKeyPressed();
            }
        }

        public void ResetPawnHealth()
        {
            pawn.health.hediffSet.hediffs = new List<Hediff>(cachedHediffList);
        }

        public void setPawnPreview()
        {
            if (pawn != null)
            {
                forceRecachePreview = false;
                pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(pawn);
                RenderPawn();
                InitCamera();
                camera.gameObject.SetActive(true);
                camera.transform.position = new Vector3(0f, pawn.Position.y + 1f, 0f);
                camera.forceIntoRenderTexture = true;
                camera.targetTexture = previewImage;
                camera.Render();
                camera.targetTexture = null;
                camera.forceIntoRenderTexture = false;
                camera.gameObject.SetActive(false);
            }
            else
            {
                Log.Error("Pawn was null when attempting to recache part picker preview.");
            }
        }

        // Taken from RenderingTool.RenderPawnInternal in CharacterEditor
        private void RenderPawn()
        {
            PawnGraphicSet graphics = pawn.Drawer.renderer.graphics;
            Quaternion quaternion = Quaternion.AngleAxis(0f, Vector3.up);

            Mesh bodyMesh = pawn.RaceProps.Humanlike ? MeshPool.humanlikeBodySet.MeshAt(previewRot) : graphics.nakedGraphic.MeshAt(previewRot);
            Vector3 bodyOffset = new Vector3 (0f, pawn.Position.y + 0.007575758f, 0f);
            foreach (Material mat in graphics.MatsBodyBaseAt(previewRot))
            {
                Material damagedMat = graphics.flasher.GetDamagedMat(mat);
                GenDraw.DrawMeshNowOrLater(bodyMesh, bodyOffset, quaternion, damagedMat, false);
                bodyOffset.y += 0.00390625f;
                if (!toggleClothesEnabled)
                {
                    break;
                }
            }
            Vector3 vector3 = new Vector3(0f, pawn.Position.y + (previewRot == Rot4.North ? 0.026515152f : 0.022727273f), 0f);
            Vector3 vector4 = new Vector3(0f, pawn.Position.y + (previewRot == Rot4.North ? 0.022727273f : 0.026515152f), 0f);
            if (graphics.headGraphic != null)
            {
                Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(previewRot);
                Vector3 headOffset = quaternion * pawn.Drawer.renderer.BaseHeadOffsetAt(previewRot);
                Material material = graphics.HeadMatAt(previewRot);
                GenDraw.DrawMeshNowOrLater(mesh2, vector4 + headOffset, quaternion, material, false);

                Mesh hairMesh = graphics.HairMeshSet.MeshAt(previewRot);
                Vector3 hairOffset = new Vector3(headOffset.x, pawn.Position.y + 0.030303031f, headOffset.z);
                bool isWearingHat = false;
                if (toggleClothesEnabled)
                {
                    foreach (ApparelGraphicRecord apparel in graphics.apparelGraphics)
                    {
                        if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                        {
                            Material hatMat = graphics.flasher.GetDamagedMat(apparel.graphic.MatAt(previewRot));
                            if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                            {
                                isWearingHat = true;
                                hairOffset.y += 0.03f;
                                GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, quaternion, hatMat, false);
                            }
                            else
                            {
                                Vector3 hatOffset = new Vector3(headOffset.x, pawn.Position.y + (previewRot == Rot4.North ? 0.003787879f : 0.03409091f), headOffset.z);
                                GenDraw.DrawMeshNowOrLater(hairMesh, hatOffset, quaternion, hatMat, false);
                            }
                        }
                    }
                }
                if (!isWearingHat)
                {
                    Material hairMat = graphics.HairMatAt(previewRot);
                    GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, quaternion, hairMat, false);
                }
            }
            if (toggleClothesEnabled)
            {
                foreach (ApparelGraphicRecord graphicsSet in graphics.apparelGraphics)
                {
                    if (graphicsSet.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
                    {
                        Material clothingMat = graphics.flasher.GetDamagedMat(graphicsSet.graphic.MatAt(previewRot));
                        GenDraw.DrawMeshNowOrLater(bodyMesh, vector3, quaternion, clothingMat, false);
                    }
                }
            }
            HarmonyPatches.DrawAddons(false, vector3, pawn, quaternion, previewRot, false);
            if (toggleClothesEnabled)
            {
                if (pawn.apparel != null)
                {
                    foreach (Apparel apparel in pawn.apparel.WornApparel)
                    {
                        apparel.DrawWornExtras();
                    }
                }
            }
        }


        internal void InitCamera()
        {
            if (gameObject == null)
            {
                gameObject = new GameObject("PreviewCamera", new Type[]
                {
                    typeof(Camera)
                });
                gameObject.SetActive(false);
            }
            if (camera == null)
            {
                camera = gameObject.GetComponent<Camera>();
                camera.transform.position = new Vector3(0f, 1f, 0f);
                camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                camera.orthographic = true;
                camera.orthographicSize = 1f;
                camera.clearFlags = CameraClearFlags.Color;
                camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
                camera.renderingPath = RenderingPath.Forward;
                camera.nearClipPlane = Current.Camera.nearClipPlane;
                camera.farClipPlane = Current.Camera.farClipPlane;
                camera.targetTexture = null;
                camera.forceIntoRenderTexture = false;
                previewImage = new RenderTexture(200, 280, 24);
                camera.targetTexture = previewImage;
            }
        }
    }
}
