// EtherState.cs modified by Iron Wolf for Pawnmorph on 07/28/2019 2:05 PM
// last updated 07/28/2019  2:05 PM

namespace Pawnmorph
{
    /// <summary>
    /// enum for the 3 possible states a pawn can be in (in relation to 'ether' hediffs)  
    /// </summary>
    public enum EtherState 
    {
        None=0, //has neither EtherBroken or EtherBonded
        Broken,
        Bond
    }
}