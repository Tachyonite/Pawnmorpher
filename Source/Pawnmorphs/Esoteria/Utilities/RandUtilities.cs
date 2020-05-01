// RandUtilities.cs modified by Iron Wolf for Pawnmorph on 08/27/2019 7:03 PM
// last updated 08/27/2019  7:03 PM

//using Multiplayer.API;
using UnityEngine;
using Verse;

namespace Pawnmorph.Utilities
{
    /// <summary>
    /// a collection of utilities around random functions 
    /// </summary>
    public static class RandUtilities
    {
        private static int _lastTick;
        private static int _lastSeed;

        /// <summary>
        /// Gets the uniform probability of some event checked every so often with a set mean time to happen 
        /// </summary>
        /// <param name="meanTimeToHappen">The mean time to happen.</param>
        /// <param name="checkPeriod">how often the event is checked</param>
        /// <returns></returns>
        public static float GetUniformProbability(float meanTimeToHappen, float checkPeriod)
        {
            if (checkPeriod <= 0 || meanTimeToHappen <= 0) return 0; //don't divide by zero 

            return Mathf.Min(checkPeriod / (meanTimeToHappen), 1); 

        }



        /// <summary>Gets the multiplayer safe seed.</summary>
        /// <value>The mp safe seed.</value>
        public static int MPSafeSeed
        {
            get
            {
                var ticks = Find.TickManager.TicksAbs;
                if (ticks != _lastTick)
                {
                    _lastTick = ticks;
                    _lastSeed = ticks;
                    return _lastTick;
                }

                _lastSeed = ZorShift(_lastSeed);
                return _lastSeed;
            }
        }

        /// <summary>
        /// If the game is in multiplayer pushes a deterministic seed to Rand. <br />
        /// If not in multiplayer this call does nothing .
        /// </summary>
        public static void PushState()
        {
            //if (MP.IsInMultiplayer)
            //{
            //    Rand.PushState(MPSafeSeed);

            //}
        }

        /// <summary>
        /// Pops the Rand state if the game is in multiplayer. <br />
        /// This does nothing if the game is not in multiplayer.
        /// </summary>
        public static void PopState()
        {
            //if (MP.IsInMultiplayer)
            //{
            //    Rand.PopState();
            //}
        }

        /// <summary>
        /// Generate a color using the specified seed
        /// </summary>
        /// <param name="colorGen">Color generator</param>
        /// <param name="seed">Seed</param>
        /// <returns>Generated color</returns>
        public static Color NewRandomizedColorUsingSeed(this ColorGenerator colorGen, int seed) 
        {
            try
            {
                Rand.PushState(seed);
                return colorGen.NewRandomizedColor();
            }
            finally
            {
                Rand.PopState();
            }
        }


        /// <summary> Multiplayer save version of Rand.MTBEventOccurs  </summary>
        /// <param name="mtb"> The MTB. </param>
        /// <param name="mtbUnit"> The MTB unit. </param>
        /// <param name="checkDuration"> Duration of the check. </param>
        public static bool MPSafeMTBEventOccurs(float mtb, float mtbUnit, float checkDuration)
        {
            PushState();

            var res = Rand.MTBEventOccurs(mtb, mtbUnit, checkDuration);

            PopState();

            return res;
        }

        /// <summary> Preform a zorShift on the given int value. </summary>
        /// <param name="val"> The value. </param>
        static int ZorShift(int val)
        {
            uint uVal = unchecked((uint) val); // Just copy the bit pattern. 
            uVal ^= uVal << 13;
            uVal ^= uVal >> 17;
            uVal ^= uVal << 5;
            return unchecked((int) uVal); // Return the shuffled bit pattern.
        }
    }
}