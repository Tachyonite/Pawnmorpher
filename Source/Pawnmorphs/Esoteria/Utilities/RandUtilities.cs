using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.Utilities
{
	/// <summary>
	///     A collection of utilities around random functions
	/// </summary>
	public static class RandUtilities
	{
		/// <summary>
		///     Generate a random number according to a Gaussian distribution.
		/// </summary>
		/// <param name="mu">The mean.</param>
		/// <param name="sigma">The standard deviation.</param>
		public static float generateNormalRandom(float mu = 0, float sigma = 1)
		{
			if (sigma <= 0)
				throw new ArgumentException("Standard deviation cannot be negative");

			float rand1 = Rand.Range(0.0f, 1.0f);
			float rand2 = Rand.Range(0.0f, 1.0f);

			float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos(2.0f * Mathf.PI * rand2);

			return mu + sigma * n;
		}

		/// <summary>
		///     Generate a random number according to an exponential distribution. 
		/// </summary>
		/// <param name="rate">The rate.</param>
		public static float generateNormalRandom(float rate)
		{
			if (rate <= 0)
				throw new ArgumentException("Rate cannot be negative");

			float rand = Rand.Range(0.0f, 1.0f);
			return Mathf.Log(1 - rand) / (-rate);
		}

		/// <summary>
		///     Generate a random number according to a Skew Normal distribution (non symmetric).
		/// </summary>
		/// <param name="loc"> The location (not actually the mean).</param>
		/// <param name="scale">The scale (not actually the standard deviation).</param>
		/// <param name="shape">The shape, determining the skewness. Negative if you want only a few values after the mode (value that appears the most), positive before.</param>
		public static float generateSkewNormalRandom(float loc, float scale, float shape)
		{
			if (scale <= 0)
				throw new ArgumentException("Scale cannot be negative");

			float corr = shape / Mathf.Sqrt(1 + Mathf.Pow(shape, 2));
			float u0 = generateNormalRandom(0, 1);
			float v = generateNormalRandom(0, 1);
			float u1 = corr * u0 + Mathf.Sqrt(1 - Mathf.Pow(corr, 2)) * v;

			float y = u0 > 0 ? u1 : -u1;
			return loc + scale * y;
		}

		/// <summary>
		///     Gets the uniform probability of some event checked every so often with a set mean time to happen
		/// </summary>
		/// <param name="meanTimeToHappen">The mean time to happen.</param>
		/// <param name="checkPeriod">how often the event is checked</param>
		/// <returns></returns>
		public static float GetUniformProbability(float meanTimeToHappen, float checkPeriod)
		{
			if (checkPeriod <= 0 || meanTimeToHappen <= 0) return 0; //don't divide by zero 

			return Mathf.Min(checkPeriod / meanTimeToHappen, 1);
		}

		/// <summary>
		///     checks if an event, with the given mtb in days, has occured
		/// </summary>
		/// <param name="days">The days.</param>
		/// <param name="checkDuration">how often this check occurs in ticks</param>
		/// <returns></returns>
		public static bool MtbDaysEventOccured(float days, float checkDuration = 60)
		{
			return Rand.MTBEventOccurs(days, 60000f, checkDuration);
		}

		/// <summary>
		/// Rounds a float up or down at random, based on how close it is to either side.
		/// 
		/// 1.5 has a 50% chance of rounding up or down
		/// 1.1 has a 90% chance of rounding down and 10% of rounding up
		/// </summary>
		/// <returns>The rounded float.</returns>
		/// <param name="f">The float to round.</param>
		public static int RandRound(float f)
		{
			int baseVal = Mathf.FloorToInt(f);

			float remainder = f - baseVal;
			if (Rand.Value < remainder)
				baseVal++;

			return baseVal;
		}

		private static float Marsaglia(float d, float c)
		{
			while (true)
			{
				float x, v;

				do
				{
					x = generateNormalRandom();
					v = Mathf.Pow(1.0f + c * x, 3);
				}
				while (v <= 0);

				float u = Rand.Range(0.0f, 1.0f);

				//Shortcut to make the procedure faster according to the paper.
				if (u < 1 - 0.0331 * Mathf.Pow(x, 4))
					return d * v;
				//Normal step.
				if (Math.Log(u) < 0.5 * Mathf.Pow(x, 2) + d * (1.0 - v + Math.Log(v)))
					return d * v;
			}
		}

		private static float generateGammaRandom(float shape, float scale)
		{
			if (shape < 1)
			{
				float d = shape + 1.0f - 1.0f / 3.0f;
				float c = (1.0f / 3.0f) / Mathf.Sqrt(d);

				float u = Rand.Range(0.0f, 1.0f);
				return scale * Marsaglia(d, c) * Mathf.Pow(u, 1.0f / shape);
			}
			else
			{
				float d = shape - 1.0f / 3.0f;
				float c = (1.0f / 3.0f) / Mathf.Sqrt(d);

				return scale * Marsaglia(d, c);
			}
		}

		/// <summary>
		/// Generates a random number between 0 and 1 using a beta distribution.
		/// </summary>
		/// <param name="alpha">The alpha component.</param>
		/// <param name="beta">The beta component.</param>
		/// <returns></returns>
		public static float generateBetaRandom(float alpha, float beta)
		{
			float x = generateGammaRandom(alpha, 1);
			float y = generateGammaRandom(beta, 1);
			return x / (x + y);
		}
	}
}