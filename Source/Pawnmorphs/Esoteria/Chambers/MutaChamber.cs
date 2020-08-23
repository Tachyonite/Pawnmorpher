// MutaChamber.cs created by Iron Wolf for Pawnmorph on 07/26/2020 7:50 PM
// last updated 07/26/2020  7:50 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Pawnmorph.Building_MutagenChamber" />
    public class MutaChamber : Building_MutagenChamber
    {
        /// <summary>
        /// Draws this instance.
        /// </summary>
        public override void Draw()
        {
            
            //TODO draw pawn

            Comps_PostDraw();
        }
    }
}