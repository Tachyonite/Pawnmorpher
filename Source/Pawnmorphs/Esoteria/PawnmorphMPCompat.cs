using Verse;
using Multiplayer.API;

namespace Pawnmorph
{
    [StaticConstructorOnStartup]
    public static class PawnmorphMPCompat
    {
        static PawnmorphMPCompat()
        {
            if (!MP.enabled) return;

            MP.RegisterAll();
        }
    }
}
