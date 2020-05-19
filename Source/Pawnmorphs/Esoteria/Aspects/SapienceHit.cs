// SapienceHit.cs created by Iron Wolf for Pawnmorph on 04/27/2020 8:16 AM
// last updated 04/27/2020  8:16 AM

namespace Pawnmorph.Aspects
{
    /// <summary>
    /// aspect that affects sapience in a negative way 
    /// </summary>
    /// <seealso cref="Pawnmorph.Aspect" />
    public class SapienceHit : Aspect 
    {
        /// <summary> Called after this instance is added to the pawn. </summary>
        protected override void PostAdd()
        {
            base.PostAdd();
            var sTracker = Pawn.GetSapienceTracker();
            if (sTracker != null && sTracker.CurrentState == null)
            {
                sTracker.EnterState(SapienceStateDefOf.Animalistic, 1); 
            }

        }
    }
}