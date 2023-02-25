// MathUtilities.cs created by Iron Wolf for Pawnmorph on 07/10/2021 10:42 AM
// last updated 07/10/2021  10:42 AM

using UnityEngine;

namespace Pawnmorph.Utilities
{
	/// <summary>
	///     class for various math related utility functions
	/// </summary>
	public static class MathUtilities
	{
		/// <summary>
		///     natural log of 2
		/// </summary>
		public const float LN2 = 0.69314718056f;

		/// <summary>
		///     smoothstep interpolation
		/// </summary>
		/// <param name="edge0">The edge0.</param>
		/// <param name="edge1">The edge1.</param>
		/// <param name="x">The x.</param>
		/// <returns></returns>
		public static float SmoothStep(float edge0, float edge1, float x)
		{
			// Scale, bias and saturate x to 0..1 range
			x = Mathf.Clamp((x - edge0) / (edge1 - edge0), 0, 1);
			// Evaluate polynomial
			return x * x * (3 - 2 * x);
		}


		/// <summary>
		/// Fits a value from the initial range into the new range 
		/// </summary>
		/// note: it is up to the caller to ensure the ranges are valid 
		/// <param name="val">The value.</param>
		/// <param name="oldMin">The old minimum.</param>
		/// <param name="oldMax">The old maximum.</param>
		/// <param name="newMin">The new minimum.</param>
		/// <param name="newMax">The new maximum.</param>
		/// <param name="clamp">if set to <c>true</c> clamp value between the newMin and newMax values .</param>
		/// <returns></returns>
		public static float FitToRange(float val, float oldMin, float oldMax, float newMin, float newMax, bool clamp = true)
		{
			val = (val - oldMin) / (oldMax - oldMin);
			val = val * (newMax - newMin) + newMin;
			if (clamp) val = Mathf.Clamp(val, newMin, newMax);

			return val;
		}
	}
}