// ChamberState.cs modified by Iron Wolf for Pawnmorph on 08/26/2019 10:21 AM
// last updated 08/26/2019  10:22 AM

namespace Pawnmorph.Chambers
{
    /// <summary>
    /// enum for the different states a mutagenic chamber can be in  
    /// </summary>
    public enum ChamberState
    {
        Idle, //the chamber isn't doing anything 
        Transforming, //turning a pawn into an animal
        MergeInto, //pawns are being merged in this chamber 
        MergeOutOf //the pawn is being merged into a different chamber 
    }
}