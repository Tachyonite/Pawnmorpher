using JetBrains.Annotations;
using Pawnmorph.GraphicSys;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.Aspects
{
    public class Coloration : Aspect
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

        public void TryDirectRecolor(PawnGraphicSet graphics) 
        {
            if (this.Pawn != null && this.Pawn.RaceProps != null && !this.Pawn.RaceProps.Humanlike)
            {
                Color? colorFirst = this.TryGetColorationAspectColor(Aspects.Coloration.PawnColorSlot.SkinFirst);
                Color? colorSecond = this.TryGetColorationAspectColor(Aspects.Coloration.PawnColorSlot.SkinSecond);

                if (colorFirst.HasValue || colorSecond.HasValue)
                {
                    graphics.nakedGraphic = graphics.nakedGraphic.GetColoredVersion(graphics.nakedGraphic.Shader, colorFirst.HasValue ? colorFirst.Value : graphics.nakedGraphic.color, colorSecond.HasValue ? colorSecond.Value : graphics.nakedGraphic.colorTwo);
                }
            }
        }

        protected override void PostAdd()
        {
            base.PostAdd();
            if (this.Pawn != null)
            {
                
                if (this.Pawn.RaceProps.Humanlike)
                {
                    var graphicsUpdater = this.Pawn.GetComp<GraphicsUpdaterComp>();
                    if (graphicsUpdater != null)
                        graphicsUpdater.RefreshGraphics(this.Pawn.GetComp<MutationTracker>(), this.Pawn, true);
                }
                else
                {
                    /*handled in ResolveAllGraphicsPatch*/
                }
            }
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (this.Pawn != null)
            {
                if (this.Pawn.RaceProps.Humanlike)
                {
                    var graphicsUpdater = this.Pawn.GetComp<GraphicsUpdaterComp>();
                    if (graphicsUpdater != null)
                        graphicsUpdater.RefreshGraphics(this.Pawn.GetComp<MutationTracker>(), this.Pawn, true);
                }
                else
                {
                    this.Pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                    this.Pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
            }
        }


        public enum PawnColorSlot 
        {
            SkinFirst,
            SkinSecond,
            HairFirst, 
            HairSecond
        }

        public Color? TryGetColorationAspectColor(PawnColorSlot colorSlot)
        {
            var transformedPawn = Find.World.GetComponent<PawnmorphGameComp>()?.GetTransformedPawnContaining(this.Pawn);
            var seed = (transformedPawn != null && transformedPawn.Item2 == TfSys.TransformedStatus.Transformed) ? transformedPawn.Item1.OriginalPawns.Sum(p => p.thingIDNumber) : this.Pawn.thingIDNumber;
            Log.Message(transformedPawn.ToStringSafe());
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

        public ColorGenerator TryGetColorationAspectColorGenerator(PawnColorSlot colorSlot) 
        {
            if(this.def == ColorationAspectDefOfs.ColorationNatural)
            {
                //Log.Message("coloring ColorationNatural");
                switch (colorSlot)
                {
                    case PawnColorSlot.SkinFirst:
                        return NaturalColors;
                    case PawnColorSlot.SkinSecond:
                        return NaturalColors;
                    default:
                        return null;
                }
            }
            else if (this.def == ColorationAspectDefOfs.ColorationAlbinism)
            {
                //Log.Message("coloring ColorationAlbinism");
                switch (colorSlot)
                {
                    case PawnColorSlot.SkinFirst:
                        return albinismFirst;
                    case PawnColorSlot.SkinSecond:
                        return albinismSecond;
                    case PawnColorSlot.HairFirst:
                        return albinismFirst;
                    default:
                        return null;
                }
            }
            else if (this.def == ColorationAspectDefOfs.ColorationMelanism)
            {
                //Log.Message("coloring ColorationMelanism");
                switch (colorSlot)
                {
                    case PawnColorSlot.SkinFirst:
                        return melanismFirst;
                    case PawnColorSlot.SkinSecond:
                        return melanismSecond;
                    case PawnColorSlot.HairFirst:
                        return melanismFirst;
                    default:
                        return null;
                }
            }
            else if (this.def == ColorationAspectDefOfs.ColorationUnnatural)
            {
                //Log.Message("coloring ColorationUnnatural");
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
            else 
            {
                return null;
            }
        }
    }

    [DefOf]
    public static class ColorationAspectDefOfs
    {

        [UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
        public static AspectDef ColorationNatural;

        [UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
        public static AspectDef ColorationAlbinism;

        [UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
        public static AspectDef ColorationMelanism;

        [UsedImplicitly(ImplicitUseKindFlags.Assign), NotNull]
        public static AspectDef ColorationUnnatural;
    }
}
