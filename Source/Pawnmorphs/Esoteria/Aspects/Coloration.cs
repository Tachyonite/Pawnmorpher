using System;
using JetBrains.Annotations;
using Pawnmorph.GraphicSys;
using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.SimplePawnColorSet;

namespace Pawnmorph.Aspects
{


	/// <summary>
	/// Aspect that applies non-standard skin/hair coloration to the pawn.
	/// </summary>
	public class ColorationAspect : Aspect
	{
		private static ColorGenerator_HSV NaturalColors { get; } = new ColorGenerator_HSV()
		{
			HueRange = new FloatRange(0, 1f),
			SatuationRange = new FloatRange(0.1f, 0.3f),
			ValueRange = new FloatRange(0.4f, 0.8f)
		};

		private static ColorGenerator_Single albinismFirst = new ColorGenerator_Single() { color = Color.white };
		private static ColorGenerator_Single albinismSecond = new ColorGenerator_Single() { color = new Color(1.0f, 0.9f, 0.9f) }; //pink

		private static ColorGenerator_Single melanismFirst = new ColorGenerator_Single() { color = new Color(0.15f, 0.15f, 0.15f) };
		private static ColorGenerator_Single melanismSecond = new ColorGenerator_Single() { color = new Color(0.25f, 0.25f, 0.25f) };

		private static ColorGenerator_HSV UnnaturalColors { get; } = new ColorGenerator_HSV()
		{
			HueRange = new FloatRange(0, 1f),
			SatuationRange = new FloatRange(0.5f, 0.75f),
			ValueRange = new FloatRange(0.8f, 0.9f)
		};

		private SimplePawnColorSet _colorSet;

		/// <summary> Color set assigned to this instance </summary>
		public SimplePawnColorSet ColorSet
		{
			get
			{
				if (_colorSet == null)
				{
					GenerateColorSet();
				}
				return _colorSet;
			}
			set { _colorSet = value; }
		}

		/// <summary> True if color should apply at 100% regardless of current putation percentage, false otherwise </summary>
		public bool IsFullOverride { get { return this.def == ColorationAspectDefOfs.ColorationPlayerPicked; } }

		/// <inheritdoc />
		protected override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look<SimplePawnColorSet>(ref _colorSet, "colorSet");
		}

		/// <summary>
		/// Remove other ColorationAspects from parent pawn
		/// </summary>
		public void RemoveOthers()
		{

			var tracker = this.Pawn.GetAspectTracker();
			foreach (var aspect in tracker.Aspects)
			{
				if (!(aspect is ColorationAspect) || aspect == this)
					continue;

				Log.Message(String.Format("{0} is removing {1}", this.def.defName, aspect.def.defName));
				tracker.Remove(aspect); //this should be fine since the removal is delayed
			}
		}

		/// <inheritdoc />
		public override void PostTransfer(Aspect newAspect)
		{
			base.PostTransfer(newAspect);
			(newAspect as ColorationAspect).ColorSet = this.ColorSet;
			(newAspect as ColorationAspect).UpdatePawn();
		}

		/// <inheritdoc />
		protected override void PostAdd()
		{
			base.PostAdd();
			RemoveOthers();
			UpdatePawn();
		}

		/// <inheritdoc />
		public override void PostRemove()
		{
			base.PostRemove();
			UpdatePawn();
		}

		/// <summary>
		/// Update parent pawn's coloration via standard channels.
		/// </summary>
		public void UpdatePawn()
		{
			if (this.Pawn != null)
			{
				if (this.Pawn.RaceProps.Humanlike)
				{
					var graphicsUpdater = this.Pawn.GetComp<GraphicsUpdaterComp>();
					if (graphicsUpdater != null)
						graphicsUpdater.IsDirty = true;
				}
				else
				{
					this.Pawn.Drawer.renderer.SetAllGraphicsDirty();
					//this.Pawn.Drawer.renderer.ResolveAllGraphics();
				}
			}
			else
			{
				Log.Warning("ColorationAspect trying to update colors but pawn is null");
			}
		}

		/// <summary>
		/// Apply this coloration to a pawn directly.
		/// </summary>
		/// <param name="graphics">Pawn's graphics set</param>
		public Graphic TryDirectRecolorAnimal(Graphic graphics)
		{
			if (this.Pawn != null && this.Pawn.RaceProps != null && !this.Pawn.RaceProps.Humanlike)
			{
				if (ColorSet.skinColor.HasValue || ColorSet.skinColorTwo.HasValue)
				{
					return graphics.GetColoredVersion(graphics.Shader, ColorSet.skinColor.HasValue ? ColorSet.skinColor.Value : graphics.color, ColorSet.skinColorTwo.HasValue ? ColorSet.skinColorTwo.Value : graphics.colorTwo);
				}
				//if (this.Pawn.IsColonist || this.Pawn.IsColonistAnimal())
				//{
				//	PortraitsCache.SetDirty(this.Pawn);
				//	Find.ColonistBar.MarkColonistsDirty();
				//}
			}
			return graphics;
		}

		/// <summary>
		/// Generate color set using generators based on this instance's aspectDef
		/// </summary>
		private void GenerateColorSet()
		{
			ColorSet = new SimplePawnColorSet();
			ColorSet.skinColor = TryGenerateColorationAspectColor(PawnColorSlot.SkinFirst);
			ColorSet.skinColorTwo = TryGenerateColorationAspectColor(PawnColorSlot.SkinSecond);
			ColorSet.hairColor = TryGenerateColorationAspectColor(PawnColorSlot.HairFirst);
			ColorSet.hairColorTwo = TryGenerateColorationAspectColor(PawnColorSlot.HairSecond);
		}

		/// <summary>
		/// Generate a color using base Rand generator and the slot-appropriate color generator
		/// </summary>
		/// <param name="colorSlot">Color slot</param>
		/// <returns>Generated color, or null if base color should be used</returns>
		private Color? TryGenerateColorationAspectColor(PawnColorSlot colorSlot)
		{
			var seed = Find.TickManager.TicksAbs;
			ColorGenerator colorGen = TryGetColorationAspectColorGenerator(colorSlot);
			if (colorGen != null)
			{
				try
				{
					Rand.PushState((seed + (int)colorSlot) ^ 5224); //offet to generate different colors
					return colorGen.NewRandomizedColor();
				}
				finally
				{
					Rand.PopState();
				}
			}
			return null;
		}

		private ColorGenerator TryGetColorationAspectColorGenerator(PawnColorSlot colorSlot)
		{
			if (this.def == ColorationAspectDefOfs.ColorationNatural)
			{
				switch (colorSlot)
				{
					case PawnColorSlot.SkinFirst:
						return NaturalColors;
					case PawnColorSlot.SkinSecond:
						return NaturalColors;
					case PawnColorSlot.HairFirst:
					case PawnColorSlot.HairSecond:
					default:
						return null;
				}
			}
			else if (this.def == ColorationAspectDefOfs.ColorationAlbinism)
			{
				switch (colorSlot)
				{
					case PawnColorSlot.SkinFirst:
						return albinismFirst;
					case PawnColorSlot.SkinSecond:
						return albinismSecond;
					case PawnColorSlot.HairFirst:
						return albinismFirst;
					case PawnColorSlot.HairSecond:
					default:
						return null;
				}
			}
			else if (this.def == ColorationAspectDefOfs.ColorationMelanism)
			{
				switch (colorSlot)
				{
					case PawnColorSlot.SkinFirst:
						return melanismFirst;
					case PawnColorSlot.SkinSecond:
						return melanismSecond;
					case PawnColorSlot.HairFirst:
						return melanismFirst;
					case PawnColorSlot.HairSecond:
					default:
						return null;
				}
			}
			else if (this.def == ColorationAspectDefOfs.ColorationUnnatural)
			{
				switch (colorSlot)
				{
					case PawnColorSlot.SkinFirst:
						return UnnaturalColors;
					case PawnColorSlot.SkinSecond:
						return UnnaturalColors;
					case PawnColorSlot.HairFirst:
						return UnnaturalColors;
					case PawnColorSlot.HairSecond:
						return UnnaturalColors;
					default:
						return null;
				}
			}
			else if (this.def == ColorationAspectDefOfs.ColorationPlayerPicked)
			{
				switch (colorSlot)
				{
					case PawnColorSlot.SkinFirst:
						if (ColorSet != null && ColorSet.skinColor.HasValue)
							return new ColorGenerator_Single() { color = ColorSet.skinColor.Value };
						else
							return null;
					case PawnColorSlot.SkinSecond:
						if (ColorSet != null && ColorSet.skinColorTwo.HasValue)
							return new ColorGenerator_Single() { color = ColorSet.skinColorTwo.Value };
						else
							return null;
					case PawnColorSlot.HairFirst:
						if (ColorSet != null && ColorSet.hairColor.HasValue)
							return new ColorGenerator_Single() { color = ColorSet.hairColor.Value };
						else
							return null;
					case PawnColorSlot.HairSecond:
						if (ColorSet != null && ColorSet.hairColorTwo.HasValue)
							return new ColorGenerator_Single() { color = ColorSet.hairColorTwo.Value };
						else
							return null;
					default:
						return null;
				}
			}
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Helper clas for ColorationAspect defs
	/// </summary>
	[DefOf]
	public static class ColorationAspectDefOfs
	{
		/// <summary> Mild, natural colors </summary>
		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef ColorationNatural;

		/// <summary> Albinism </summary>
		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef ColorationAlbinism;

		/// <summary> Melanism </summary>
		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef ColorationMelanism;

		/// <summary> High-contrast, saturated colors </summary>
		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef ColorationUnnatural;

		/// <summary> Colors picked by player </summary>
		[UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
		public static AspectDef ColorationPlayerPicked;

		static ColorationAspectDefOfs()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(ColorationAspectDefOfs));
		}
	}
}
