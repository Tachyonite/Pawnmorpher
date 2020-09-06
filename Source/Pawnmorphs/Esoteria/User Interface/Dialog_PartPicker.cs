using AlienRace;
using Pawnmorph.Hediffs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Pawnmorph.User_Interface
{
    class Dialog_PartPicker : Window
    {
        /// <summary>The pawn that we want to modify.</summary>
        private Pawn pawn;
        private List<Hediff> cachedHediffList;

        // Reference variables
        private const string WINDOW_TITLE_LOC_STRING = "PartPickerMenuTitle";
        private const string DO_SYMMETRY_LOC_STRING = "DoSymmetry";
        private const string SKIN_SYNC_LOC_STRING = "SkinSync";
        private const string NO_MUTATIONS_LOC_STRING = "NoMutationsOnPart";
        private const string EDIT_PARAMS_LOC_STRING = "EditParams";
        private const string TOGGLE_CLOTHES_LOC_STRING = "ToggleClothes";
        private const string ROTATE_CW_LOC_STRING = "RotCW";
        private const string ROTATE_CCW_LOC_STRING = "RotCCW";
        private const string DIF_TITLE_LOC_STRING = "DifTitle";
        private const string PART_DESCRIPTION_TITLE_LOC_STRING = "PartDescTitle";
        private const string APPLY_BUTTON_LOC_STRING = "ApplyButtonText";
        private const string RESET_BUTTON_LOC_STRING = "ResetButtonText";
        private const string CANCEL_BUTTON_LOC_STRING = "CancelButtonText";
        private const float SPACER_SIZE = 17f;
        private const float BUTTON_HORIZONTAL_PADDING = 6f;
        private const float MENU_SECTION_CONSTRICTION_SIZE = 4f;
        private static Vector2 PREVIEW_SIZE = new Vector2(200, 280);
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

        // Description builders
        private StringBuilder difBuilder = new StringBuilder();
        private StringBuilder partDescBuilder = new StringBuilder();

        // Toggles
        private bool debugMode = false;
        private bool confirmed = false;
        private bool toggleClothesEnabled = true;
        private bool doSymmetry = true;
        private bool skinSync = true;
        private Rot4 previewRot = Rot4.South;

        // Preview related variables
        public bool recachePreview = false;
        private GameObject gameObject;
        private Camera camera;
        private RenderTexture previewImage;

        // Caching variables
        private static Dictionary<BodyPartDef, List<MutationDef>> cachedMutationDefsByPartDef;
        private static Dictionary<BodyPartDef, List<MutationLayer>> cachedMutationLayersByPartDef;
        private static List<BodyPartRecord> cachedMutableParts;
        private static List<BodyPartRecord> cachedMutableCoreParts;
        private static List<BodyPartRecord> cachedMutableSkinParts;
        private List<Hediff_AddedMutation> pawnCurrentMutations;
        private static string editButtonText = EDIT_PARAMS_LOC_STRING.Translate();
        private static float editButtonWidth = Text.CalcSize(editButtonText).x + BUTTON_HORIZONTAL_PADDING;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(750f, 840f);
            }
        }

        public Dialog_PartPicker(Pawn pawn, bool debugMode = false)
        {
            // Copying passed in data for use elsewhere in the class.
            this.pawn = pawn;
            this.debugMode = debugMode;

            // Settting various flags for this window. Will remove most of these when I am done testing things.
            forcePause = true;
            doCloseX = true;
            resizeable = true;
            draggable = true;

            // Storing these here to (probably) save a few cycles while caching.
            List<BodyPartRecord> allPawnParts = pawn.RaceProps.body.AllParts;
            List<MutationDef> allMutationDefs = DefDatabase<MutationDef>.AllDefs.ToList();

            // Cache various information so we don't have to constantly look it up elsewhere. This does present the chance for a data desync, but unless the def database gets updated mid-game, I can't think of a case where this could occur atm.
            cachedMutationDefsByPartDef = allMutationDefs.SelectMany(m => m.parts).Distinct().Select(k => new { k, v = allMutationDefs.Where(m => m.parts.Contains(k)).ToList()}).ToDictionary(x => x.k, x => x.v);
            cachedMutationLayersByPartDef = cachedMutationDefsByPartDef.Keys.Select(k => new { k, v = cachedMutationDefsByPartDef[k].Select(n => n.RemoveComp.layer).Distinct().ToList()}).ToDictionary(x => x.k, x => x.v);
            cachedMutableParts = allPawnParts.Where(m => allMutationDefs.SelectMany(n => n.parts).Distinct().Contains(m.def)).ToList();
            cachedMutableCoreParts = allPawnParts.Where(m => allMutationDefs.Where(n => n.RemoveComp.layer == MutationLayer.Core).SelectMany(o => o.parts).Distinct().Contains(m.def)).ToList();
            cachedMutableSkinParts = allPawnParts.Where(m => allMutationDefs.Where(n => n.RemoveComp.layer == MutationLayer.Skin).SelectMany(o => o.parts).Distinct().Contains(m.def)).ToList();

            // Initial caching of the mutations currently affecting the pawn and their initial hediff list.
            RecachePawnMutations();
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
            float col1, col2;

            // Step 2 - Draw the title of the window.
            Text.Font = GameFont.Medium;
            string title = $"{WINDOW_TITLE_LOC_STRING.Translate()} - {pawn.Name.ToStringShort} ({pawn.def.LabelCap})";
            float titleHeight = Text.CalcHeight(title, inRect.width);
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, Text.CalcHeight(title, inRect.width)), title);
            Text.Font = GameFont.Small;
            col1 = col2 = titleHeight;

            // Step 3 - Determine vewing areas for body part list and description.
            float drawableWidth = (inRect.width - PREVIEW_SIZE.x - 2 * SPACER_SIZE) / 2;
            float drawableHeight = inRect.height - titleHeight - Math.Max(APPLY_BUTTON_SIZE.y, Math.Max(RESET_BUTTON_SIZE.y, CANCEL_BUTTON_SIZE.y)) - 2 * SPACER_SIZE;
            Rect partListOutRect = new Rect(inRect.x, titleHeight, drawableWidth, drawableHeight);
            Rect partListViewRect = new Rect(partListOutRect.x, partListOutRect.y, partListScrollSize.x, partListScrollSize.y);
            Rect previewRect = new Rect(inRect.x + SPACER_SIZE + drawableWidth, titleHeight, PREVIEW_SIZE.x, PREVIEW_SIZE.y);

            // Step 4 - Draw the body part list, selection buttons and edit buttons.
            DrawPartsList(partListOutRect, partListViewRect, ref col1, titleHeight);

            // Step 5 - Draw the preview area...
            if (recachePreview || previewImage == null)
            {
                SetPawnPreview();
            }
            GUI.DrawTexture(previewRect, previewImage);
            col2 += previewRect.height;

            // Then the preview Buttons...
            float rotCWHorPos = previewRect.x + previewRect.width / 2 - TOGGLE_CLOTHES_BUTTON_SIZE.x / 2 - ROTATE_CW_BUTTON_SIZE.x - SPACER_SIZE;
            float toggleClothingHorPos = previewRect.x + previewRect.width / 2 - TOGGLE_CLOTHES_BUTTON_SIZE.x / 2;
            float rotCCWHorPos = previewRect.x + previewRect.width / 2 + TOGGLE_CLOTHES_BUTTON_SIZE.x / 2 + SPACER_SIZE;
            Rect rotCWRect = new Rect(rotCWHorPos, col2, ROTATE_CW_BUTTON_SIZE.x, ROTATE_CW_BUTTON_SIZE.y);
            Rect toggleClothesRect = new Rect(toggleClothingHorPos, col2, TOGGLE_CLOTHES_BUTTON_SIZE.x, TOGGLE_CLOTHES_BUTTON_SIZE.y);
            Rect rotCCWRect = new Rect(rotCCWHorPos, col2, ROTATE_CCW_BUTTON_SIZE.x, ROTATE_CCW_BUTTON_SIZE.y);
            col2 += SPACER_SIZE;
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
            col2 += Math.Max(TOGGLE_CLOTHES_BUTTON_SIZE.y, Math.Max(ROTATE_CW_BUTTON_SIZE.y, ROTATE_CCW_BUTTON_SIZE.y));

            // Then the crown and body type selectors...
            // Head [Type] <-- box that shows selection list.
            // Body [Type] (Need to include these in the reset function.)

            // Then the Aspect selection list...
            // Remember this needs scrolling, Brennen.

            // Then finally the parts list toggles.
            string skinSyncText = SKIN_SYNC_LOC_STRING.Translate();
            Rect skinSyncRect = new Rect(inRect.x + SPACER_SIZE + drawableWidth, col2, PREVIEW_SIZE.x, Text.CalcHeight(skinSyncText, PREVIEW_SIZE.x));
            Widgets.CheckboxLabeled(skinSyncRect, skinSyncText, ref skinSync);
            col2 += skinSyncRect.height;

            string symmetryToggleText = DO_SYMMETRY_LOC_STRING.Translate();
            Rect symmetryToggleRect = new Rect(inRect.x + SPACER_SIZE + drawableWidth, col2, PREVIEW_SIZE.x, Text.CalcHeight(symmetryToggleText, PREVIEW_SIZE.x));
            Widgets.CheckboxLabeled(symmetryToggleRect, symmetryToggleText, ref doSymmetry);

            // Step 6 - Draw description box.
            DrawDescriptionBoxes(new Rect(inRect.x + 2 * SPACER_SIZE + PREVIEW_SIZE.x + partListOutRect.width, titleHeight, drawableWidth, drawableHeight));

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
                recachePreview = true;
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

        private void RecachePawnMutations()
        {
            pawnCurrentMutations = pawn.health.hediffSet.hediffs.Where(m => m.def.GetType() == typeof(MutationDef)).Cast<Hediff_AddedMutation>().ToList();
        }

        private void DrawPartsList(Rect outRect, Rect viewRect, ref float curY, float initialPos)
        {
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
                foreach(BodyPartRecord partRecord in cachedMutableParts)
                {
                    DrawPartEntry(ref curY, viewRect, (skinSync ? cachedMutableCoreParts : cachedMutableParts).Where(m => m == partRecord).ToList(), partRecord.LabelCap);
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                partListScrollSize.x = outRect.width - 16f;
                partListScrollSize.y = curY - initialPos;
            }
            Widgets.EndScrollView();
        }

        private void DrawPartEntry(ref float curY, Rect partListViewRect, List<BodyPartRecord> parts, string label, bool skinEntry = false)
        {
            List<Hediff_AddedMutation> mutations;
            string buttonLabel;
            Widgets.ListSeparator(ref curY, partListViewRect.width, label);
            if (!skinSync || cachedMutationLayersByPartDef[parts.FirstOrDefault().def].Count > 2)
            {
                foreach (MutationLayer layer in cachedMutationLayersByPartDef[parts.FirstOrDefault().def])
                {
                    mutations = pawnCurrentMutations.Where(m => parts.Contains(m.Part) && m.Def.RemoveComp.layer == layer).ToList();
                    buttonLabel = $"{layer.ToString().Translate()}: {(mutations.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutations.Select(m => m.LabelCap).Distinct()))}";
                    DrawPartButtons(ref curY, partListViewRect, mutations, parts, layer, buttonLabel);
                }
            }
            else
            {
                MutationLayer layer;
                if (skinEntry)
                {
                    mutations = pawnCurrentMutations.Where(m => parts.Contains(m.Part) && m.Def.RemoveComp.layer == MutationLayer.Skin).ToList();
                    layer = MutationLayer.Skin;
                }
                else
                {
                    mutations = pawnCurrentMutations.Where(m => parts.Contains(m.Part) && m.Def.RemoveComp.layer == MutationLayer.Core).ToList();
                    layer = MutationLayer.Core;
                }
                buttonLabel = $"{(mutations.NullOrEmpty() ? NO_MUTATIONS_LOC_STRING.Translate().ToString() : string.Join(", ", mutations.Select(m => m.LabelCap).Distinct()))}";
                DrawPartButtons(ref curY, partListViewRect, mutations, parts, layer, buttonLabel);
            }
        }

        private void DrawPartButtons(ref float curY, Rect partListViewRect, List<Hediff_AddedMutation> mutations, List<BodyPartRecord> parts, MutationLayer layer, string label)
        {
            Rect partButtonRect = new Rect(partListViewRect.x, curY, partListViewRect.width - editButtonWidth, Text.CalcHeight(label, partListViewRect.width - editButtonWidth - BUTTON_HORIZONTAL_PADDING));
            Rect editButtonRect = new Rect(partButtonRect.width, curY, editButtonWidth, partButtonRect.height);
            Rect descriptionUpdateRect = new Rect(partListViewRect.x, curY, partListViewRect.width, partButtonRect.height);
            if (Widgets.ButtonText(partButtonRect, label))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                void removeMutations()
                {
                    foreach (Hediff_AddedMutation mutation in mutations)
                    {
                        pawn.health.RemoveHediff(mutation);
                    }
                    recachePreview = true;
                    RecachePawnMutations();
                }
                options.Add(new FloatMenuOption(NO_MUTATIONS_LOC_STRING.Translate(), removeMutations));
                foreach (MutationDef mutationDef in cachedMutationDefsByPartDef[parts.FirstOrDefault().def].Where(m => m.RemoveComp.layer == layer))
                {
                    void addMutation()
                    {
                        removeMutations();
                        foreach (BodyPartRecord part in parts)
                        {
                            MutationUtilities.AddMutation(pawn, mutationDef, part);
                        }
                        RecachePawnMutations();
                    }
                    options.Add(new FloatMenuOption(mutationDef.LabelCap, addMutation));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }
            if (Widgets.ButtonText(editButtonRect, editButtonText))
            {
                Find.WindowStack.Add(new Dialog_EditMutation(mutations));
            }
            if (Mouse.IsOver(descriptionUpdateRect))
            {
                foreach (MutationDef mutation in mutations.Select(m => m.Def).Distinct().ToList())
                {
                    partDescBuilder.AppendLine($"{mutation.LabelCap}");
                    partDescBuilder.AppendLine($"{mutation.description}");
                    partDescBuilder.AppendLine();
                }
            }
            curY += partButtonRect.height;
        }

        public void DrawDescriptionBoxes(Rect inRect)
        {
            float difCurY = 0f;
            Rect difMenuSectionRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height / 2 - SPACER_SIZE);
            Rect difOutRect = difMenuSectionRect.ContractedBy(MENU_SECTION_CONSTRICTION_SIZE);
            Rect difViewRect = new Rect(difOutRect.x, difOutRect.y, descriptionScrollSize.x, descriptionScrollSize.y);
            Rect partDescMenuSectionRect = new Rect(inRect.x, inRect.height / 2 + SPACER_SIZE, inRect.width, inRect.height / 2 + SPACER_SIZE);
            Rect partDescRect = partDescMenuSectionRect.ContractedBy(MENU_SECTION_CONSTRICTION_SIZE);

            Widgets.DrawMenuSection(difMenuSectionRect);
            Widgets.BeginScrollView(difOutRect, ref descriptionScrollPos, difViewRect);
            difCurY += Text.CalcHeight(partDescBuilder.ToString(), difOutRect.width);
            if (Event.current.type == EventType.Layout)
            {
                descriptionScrollSize.x = difOutRect.width - 16f;
                descriptionScrollSize.y = difCurY;
            }
            Widgets.EndScrollView();

            Widgets.DrawMenuSection(partDescMenuSectionRect);
            Widgets.Label(partDescRect, partDescBuilder.ToString());
            partDescBuilder.Clear();
        }

        public void SetPawnPreview()
        {
            if (pawn != null)
            {
                recachePreview = false;
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
                Material material = graphics.HeadMatAt_NewTemp(previewRot);
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
                    Material hairMat = graphics.HairMatAt_NewTemp(previewRot);
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
                previewImage = new RenderTexture((int)PREVIEW_SIZE.x, (int)PREVIEW_SIZE.y, 24);
                camera.targetTexture = previewImage;
            }
        }
    }
}
