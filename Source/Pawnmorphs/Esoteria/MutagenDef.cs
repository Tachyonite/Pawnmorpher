// MutagenDef.cs created by Nick M(Iron Wolf) for Blue Moon (Pawnmorph) on 08/13/2019 4:02 PM
// last updated 08/13/2019  4:02 PM

using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// def for a mutagen strain
    /// </summary>
    /// A mutagen is a collection of transformation related hediff's ingestionOutcomeDoers, ect. that all share
    /// a common IFF system 
    /// <seealso cref="Verse.Def" />
    public class MutagenDef : Def
    {
        public bool canInfectAnimals;
        public bool canInfectMechanoids; 

        public bool CanInfect(Pawn pawn)
        {
            if (!canInfectAnimals && pawn.RaceProps.Animal) return false;
            if (!canInfectMechanoids && pawn.RaceProps.IsMechanoid) return false;
            var ext = pawn.def.GetModExtension<RaceMutagenExtension>();
            if (ext != null)
            {
                return !ext.immuneToAll && !ext.blackList.Contains(this); 
            }

            return true; 
        }
    }
}