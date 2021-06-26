using UnityEngine;

namespace Pawnmorph.Utilities
{
    /// <summary> Generation of a random Gaussian with a Box–Muller transform. </summary>
    public static class GaussianRandom
    {
        /// <summary>
        ///     Generate a random number according to a Gaussian distribution.
        /// </summary>
        /// <param name="mu"> The mean.</param>
        /// <param name="sigma">The Standard deviation.</param>
        public static float generateNormalRandom(float mu = 0, float sigma = 1)
        {
            float rand1 = Verse.Rand.Range(0.0f, 1.0f);
            float rand2 = Verse.Rand.Range(0.0f, 1.0f);

            float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

            return (mu + sigma * n);
        }
    }

}
