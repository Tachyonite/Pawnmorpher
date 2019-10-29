// PMUtilities.cs created by Iron Wolf for Pawnmorph on 09/15/2019 7:38 PM
// last updated 09/15/2019  7:38 PM

using Verse;

namespace Pawnmorph
{
    /// <summary>
    /// a collection of general Pawnmorpher related utilities 
    /// </summary>
    public static class PMUtilities
    {

        /// <summary>Gets the mod settings.</summary>
        /// <returns></returns>
        public static PawnmorpherSettings GetSettings()
        {
            return LoadedModManager.GetMod<PawnmorpherMod>().GetSettings<PawnmorpherSettings>();
        }
    }
}