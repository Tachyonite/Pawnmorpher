using UnityEngine;
using Verse;
using System;

namespace Pawnmorph.Utilities
{
    /// <summary>
    ///     a collection of utilities around random functions
    /// </summary>
    public static class RandUtilities
    {
        /// <summary>
        ///     Generate a random number according to a Gaussian distribution.
        /// </summary>
        /// <param name="mu"> The mean.</param>
        /// <param name="sigma">The Standard deviation.</param>
        public static float generateNormalRandom(float mu = 0, float sigma = 1)
        {
            if (sigma <= 0)
                throw new ArgumentException("Standard deviation cannot be null");

            float rand1 = Rand.Range(0.0f, 1.0f);
            float rand2 = Rand.Range(0.0f, 1.0f);

            float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos(2.0f * Mathf.PI * rand2);

            return mu + sigma * n;
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
                throw new ArgumentException("Scale cannot be null");

            float corr = shape / Mathf.Sqrt(1 + Mathf.Pow(shape, 2));
            float u0 = generateNormalRandom(0,1);
            float v = generateNormalRandom(0,1);
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
    }
}