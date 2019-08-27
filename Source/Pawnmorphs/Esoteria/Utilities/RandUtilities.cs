// RandUtilities.cs modified by Iron Wolf for Pawnmorph on 08/27/2019 7:03 PM
// last updated 08/27/2019  7:03 PM

using Multiplayer.API;
using Verse;

namespace Pawnmorph.Utilities
{
    public static class RandUtilities
    {
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

        public static int MPSafeSeed => Find.TickManager.TicksAbs; //is used as a seed in the compatibility examples
    }
}