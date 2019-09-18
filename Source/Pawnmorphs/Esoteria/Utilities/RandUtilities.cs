// RandUtilities.cs modified by Iron Wolf for Pawnmorph on 08/27/2019 7:03 PM
// last updated 08/27/2019  7:03 PM

using Multiplayer.API;
using Verse;

namespace Pawnmorph.Utilities
{
    public static class RandUtilities
    {
        /// <summary>
        /// if the game is in multiplayer pushes a deterministic seed to Rand
        /// if not in multiplayer this call does nothing 
        /// </summary>
        public static void PushState()
        {
            if (MP.IsInMultiplayer)
            {
                Rand.PushState(MPSafeSeed);

            }
        }

        /// <summary>
        /// Pops the Rand state if the game is in multiplayer.
        /// this does nothing if the game is not in multiplayer
        /// </summary>
        public static void PopState()
        {
            if (MP.IsInMultiplayer)
            {
                Rand.PopState();
            }
        }


        /// <summary>
        /// Multiplayer save version of Rand.MTBEventOccurs 
        /// </summary>
        /// <param name="mtb">The MTB.</param>
        /// <param name="mtbUnit">The MTB unit.</param>
        /// <param name="checkDuration">Duration of the check.</param>
        /// <returns></returns>
        public static bool MPSafeMTBEventOccurs(float mtb, float mtbUnit, float checkDuration)
        {
            if (MP.IsInMultiplayer)
            {
                Rand.PushState(MPSafeSeed); 
            }

            var res = Rand.MTBEventOccurs(mtb, mtbUnit, checkDuration);

            if (MP.IsInMultiplayer)
            {
                Rand.PopState();
            }

            return res; 

        }


        private static int _lastTick;
        private static int _lastSeed; 
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
        /// preform a zorShift on the given int value 
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        static int ZorShift(int val)
        {
            uint uVal = unchecked((uint) val); //just copy the bit pattern 
            uVal ^= uVal << 13;
            uVal ^= uVal >> 17;
            uVal ^= uVal << 5;
            return unchecked((int) uVal); //return the shuffled bit pattern 
        }
    }
}