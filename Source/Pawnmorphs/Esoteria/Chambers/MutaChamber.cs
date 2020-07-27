// MutaChamber.cs created by Iron Wolf for Pawnmorph on 07/26/2020 7:50 PM
// last updated 07/26/2020  7:50 PM

using RimWorld;
using Verse;

namespace Pawnmorph.Chambers
{
    public class MutaChamber : Building_MutagenChamber
    {
        public override void Draw()
        {
            
            //TODO draw pawn

            Comps_PostDraw();
        }
    }
}