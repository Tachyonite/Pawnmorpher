using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.Dialogs
{

    public class ColonistColorPicker : Window
    {
        private Color skinFirstColor;
        private Color hairFirstColor;

        private bool customSkinColor = true;
        private bool customHairColor = true;

        private Pawn targetPawn;

        public static void showDialogForPawn(Pawn pawn)
        {
            Find.WindowStack.Add(new ColonistColorPicker(pawn));
        }

        public ColonistColorPicker(Pawn pawn)
        {
            this.targetPawn = pawn;
            this.skinFirstColor = pawn.Drawer.renderer.graphics.nakedGraphic.Color;
            this.hairFirstColor = pawn.story.hairColor;

            this.forcePause = true;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(640f, 460f);
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            var tracker = targetPawn.GetAspectTracker();
            var preexistingAspect = tracker.GetAspect<Aspects.Coloration>();
            bool hasPreexisingAspect = preexistingAspect != null;

            if (!customSkinColor && !customHairColor)
            {
                if (hasPreexisingAspect)
                    tracker.Remove(preexistingAspect);
            }
            else
            {
                Aspects.Coloration aspect = preexistingAspect;
                if(!hasPreexisingAspect)
                    aspect = Aspects.ColorationAspectDefOfs.ColorationPlayerPicked.CreateInstance() as Aspects.Coloration;
                if (customSkinColor)
                {
                    aspect.ColorSet.skinColor = this.skinFirstColor;
                    aspect.ColorSet.skinColorTwo = this.skinFirstColor;
                }
                else
                {
                    aspect.ColorSet.skinColor = null;
                    aspect.ColorSet.skinColorTwo = null;
                }
                if (customHairColor)
                {
                    aspect.ColorSet.hairColor = this.hairFirstColor;
                }
                else
                {
                    aspect.ColorSet.hairColor = null;
                }
                if (!hasPreexisingAspect)
                    tracker.Add(aspect);
                else
                    aspect.UpdatePawn();
            }

            targetPawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
            targetPawn.Drawer.renderer.graphics.ResolveAllGraphics();

            base.Close(doCloseSound);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 42f).Rounded();
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "ColorPicker_Title".Translate(this.targetPawn.LabelCap));
            Text.Font = GameFont.Small;
            Rect contentRect = new Rect(inRect.x, 45f, inRect.width, inRect.height - 45f).ContractedBy(5f).Rounded();

            Rect skinRect = new Rect(contentRect.x, contentRect.y, contentRect.width / 2, 30f).Rounded();

            Widgets.DrawBoxSolid(skinRect.ContractedBy(5f), skinFirstColor);
            skinRect.y += 30f;
            Widgets.CheckboxLabeled(skinRect.Rounded(), "ColorPicker_CustomSkinColorCheckbox".Translate(), ref customSkinColor);
            if (customSkinColor) 
            {
                skinRect.width -= 60f;
                skinRect.x += 30f;

                skinRect.y += 30f;
                float r = Widgets.HorizontalSlider(skinRect, skinFirstColor.r, 0f, 1f, label: ColoredText.Colorize("ColorPicker_r".Translate(), Color.red));
                skinRect.y += 30f;
                float g = Widgets.HorizontalSlider(skinRect, skinFirstColor.g, 0f, 1f, label: ColoredText.Colorize("ColorPicker_g".Translate(), Color.green));
                skinRect.y += 30f;
                float b = Widgets.HorizontalSlider(skinRect, skinFirstColor.b, 0f, 1f, label: ColoredText.Colorize("ColorPicker_b".Translate(), Color.blue));
                skinFirstColor = new Color(r, g, b);
            }

            Rect hairRect = new Rect(contentRect.x + (contentRect.width / 2), contentRect.y, contentRect.width / 2, 30f).Rounded();

            Widgets.DrawBoxSolid(hairRect.ContractedBy(5f), hairFirstColor);
            hairRect.y += 30f;
            Widgets.CheckboxLabeled(hairRect.TopPartPixels(30f).Rounded(), "ColorPicker_CustomHairColorCheckbox".Translate(), ref customHairColor);
            if (customHairColor)
            {
                hairRect.width -= 60f;
                hairRect.x += 30f;

                hairRect.y += 30f;
                float r = Widgets.HorizontalSlider(hairRect, hairFirstColor.r, 0f, 1f, label: ColoredText.Colorize("ColorPicker_r".Translate(), Color.red));
                hairRect.y += 30f;
                float g = Widgets.HorizontalSlider(hairRect, hairFirstColor.g, 0f, 1f, label: ColoredText.Colorize("ColorPicker_g".Translate(), Color.green));
                hairRect.y += 30f;
                float b = Widgets.HorizontalSlider(hairRect, hairFirstColor.b, 0f, 1f, label: ColoredText.Colorize("ColorPicker_b".Translate(), Color.blue));
                hairFirstColor = new Color(r, g, b);
            }

            Rect confirmRect = new Rect(contentRect.x + ((contentRect.width / 6) * 2), contentRect.yMax - 40f, (contentRect.width / 6) * 2, 40f).Rounded();
            if (Widgets.ButtonText(confirmRect, "ColorPicker_Confirm".Translate()))
            {
                this.Close(true);
            }
        }
    }
}