using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph
{
    public class ColorGenerator_HSV : ColorGenerator
    {
        public FloatRange HueRange;
        public FloatRange SatuationRange;
        public FloatRange ValueRange;

        public ColorGenerator_HSV() { }

        public ColorGenerator_HSV(FloatRange hueRange, FloatRange satuationRange, FloatRange valueRange)
        {
            HueRange = hueRange;
            SatuationRange = satuationRange;
            ValueRange = valueRange;
        }

        public override Color NewRandomizedColor()
        {
            float hue = HueRange.min + Rand.Value * HueRange.Span;
            if (hue > 1f) //hue wraparound
                hue -= 1f;
            return Color.HSVToRGB(
                hue,
                SatuationRange.min + Rand.Value * SatuationRange.Span,
                ValueRange.min + Rand.Value * ValueRange.Span
                );
        }
    }
}
