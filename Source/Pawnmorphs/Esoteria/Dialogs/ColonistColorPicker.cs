using Pawnmorph.Aspects;
using Pawnmorph.GraphicSys;
using UnityEngine;
using Verse;
using static Pawnmorph.SimplePawnColorSet;

namespace Pawnmorph.Dialogs
{

	/// <summary>
	/// A simple color picker dialog
	/// </summary>
	public class ColonistColorPicker : Window
	{
		private Color skinFirstColor;
		private Color hairFirstColor;

		private bool customSkinColor = true;
		private bool customHairColor = true;

		private Pawn targetPawn;

		/// <summary> Show color picker dialog for given pawn </summary>
		/// <param name="pawn">Pawn</param>
		public static void showDialogForPawn(Pawn pawn)
		{
			Find.WindowStack.Add(new ColonistColorPicker(pawn));
		}

		/// <summary> Constructor </summary>
		/// <param name="pawn">Pawn</param>
		public ColonistColorPicker(Pawn pawn)
		{
			this.targetPawn = pawn;

			this.skinFirstColor = getOriginalColor(PawnColorSlot.SkinFirst);
			this.hairFirstColor = getOriginalColor(PawnColorSlot.HairFirst);

			var pawnColorSet = pawn.GetAspectTracker()?.GetAspect<ColorationAspect>()?.ColorSet;
			if (pawnColorSet != null)
			{
				if (pawnColorSet.skinColor.HasValue)
					this.skinFirstColor = pawnColorSet.skinColor.Value;
				if (pawnColorSet.hairColor.HasValue)
					this.hairFirstColor = pawnColorSet.hairColor.Value;

				this.customSkinColor = pawnColorSet.skinColor.HasValue;
				this.customHairColor = pawnColorSet.hairColor.HasValue;
			}

			this.forcePause = true;
		}

		/// <inheritdoc />
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(640f, 460f);
			}
		}

		/// <inheritdoc />
		public override void Close(bool doCloseSound = true)
		{
			var tracker = targetPawn.GetAspectTracker();
			var preexistingAspect = tracker.GetAspect(ColorationAspectDefOfs.ColorationPlayerPicked);
			bool hasPreexisingAspect = preexistingAspect != null;

			if (!customSkinColor && !customHairColor)
			{
				if (hasPreexisingAspect)
					tracker.Remove(preexistingAspect);
			}
			else
			{
				ColorationAspect aspect = preexistingAspect as ColorationAspect;
				if (!hasPreexisingAspect)
					aspect = ColorationAspectDefOfs.ColorationPlayerPicked.CreateInstance() as ColorationAspect;
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

			//targetPawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
			//targetPawn.Drawer.renderer.graphics.ResolveAllGraphics();

			base.Close(doCloseSound);
		}

		/// <inheritdoc />
		public override void DoWindowContents(Rect inRect)
		{
			Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width, 42f).Rounded();
			Text.Font = GameFont.Medium;
			Widgets.Label(titleRect, "ColorPicker_Title".Translate(this.targetPawn.LabelCap));
			Text.Font = GameFont.Small;
			Rect contentRect = new Rect(inRect.x, 45f, inRect.width, inRect.height - 45f).ContractedBy(5f).Rounded();

			var hintText = "<i>" + "ColorPicker_RemovalHint".Translate() + "</i>";
			var hintTextSize = Text.CalcSize(hintText);
			Rect hintRect = new Rect(contentRect.center.x - (hintTextSize.x / 2), contentRect.y, hintTextSize.x, hintTextSize.y).Rounded();
			Widgets.Label(hintRect, hintText);
			contentRect.y += hintRect.height;
			contentRect.height -= hintRect.height;

			Rect skinRect = new Rect(contentRect.x, contentRect.y, contentRect.width / 2, 30f).Rounded();

			Widgets.DrawBoxSolid(skinRect.ContractedBy(5f), customSkinColor ? skinFirstColor : getOriginalColor(PawnColorSlot.SkinFirst));
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

			Widgets.DrawBoxSolid(hairRect.ContractedBy(5f), customHairColor ? hairFirstColor : getOriginalColor(PawnColorSlot.HairFirst));
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

		private Color getOriginalColor(PawnColorSlot slot)
		{
			InitialGraphicsComp initialGraphicsComp = targetPawn.GetComp<InitialGraphicsComp>();
			switch (slot)
			{
				case PawnColorSlot.SkinFirst:
					return initialGraphicsComp != null ? initialGraphicsComp.SkinColor : targetPawn.Drawer.renderer.BodyGraphic.color;
				case PawnColorSlot.SkinSecond:
					return initialGraphicsComp != null ? initialGraphicsComp.SkinColorSecond : targetPawn.Drawer.renderer.BodyGraphic.ColorTwo;
				case PawnColorSlot.HairFirst:
					return initialGraphicsComp != null ? initialGraphicsComp.HairColor : targetPawn.story.HairColor;
				case PawnColorSlot.HairSecond:
					return initialGraphicsComp != null ? initialGraphicsComp.HairColorSecond : Color.white;
				default: return Color.white;
			}
		}
	}
}
