using Verse;
//using Multiplayer.API;

namespace Pawnmorph
{
    /// <summary>
    /// static class for initializing the mp compatibility 
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PawnmorphMPCompat
    {
        static PawnmorphMPCompat()
        {
            //if (!MP.enabled) return;

            //MP.RegisterAll();
        }
    }
}
