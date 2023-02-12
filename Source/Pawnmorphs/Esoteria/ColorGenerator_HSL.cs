using UnityEngine;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// Color generator that generates colors in a given Hue-Saturation-Value range
	/// </summary>
	public class ColorGenerator_HSV : ColorGenerator
	{
		/// <summary> Hue range </summary>
		public FloatRange HueRange;
		/// <summary> Saturation range </summary>
		public FloatRange SatuationRange;
		/// <summary> Value range </summary>
		public FloatRange ValueRange;

		/// <summary> Constructor </summary>
		public ColorGenerator_HSV() { }

		/// <summary> Constructor </summary>
		public ColorGenerator_HSV(FloatRange hueRange, FloatRange satuationRange, FloatRange valueRange)
		{
			HueRange = hueRange;
			SatuationRange = satuationRange;
			ValueRange = valueRange;
		}

		/// <inheritdoc/>
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
