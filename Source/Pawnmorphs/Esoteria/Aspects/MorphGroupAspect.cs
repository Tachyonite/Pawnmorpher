// MorphGroupAspect.cs modified by Iron Wolf for Pawnmorph on 09/29/2019 1:24 PM
// last updated 09/29/2019  1:24 PM

using Verse;

namespace Pawnmorph.Aspects
{
    public class MorphGroupAspect : Aspect, IRaceChangeEventReceiver
    {
        void IRaceChangeEventReceiver.OnRaceChange(ThingDef oldRace)
        {
            throw new System.NotImplementedException();
        }
    }
}