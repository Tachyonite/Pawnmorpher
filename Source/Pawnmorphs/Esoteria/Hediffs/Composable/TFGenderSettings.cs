using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines the gender of the pawn post-transformation
    /// </summary>
    public abstract class TFGenderSettings
    {
        /// <summary>
        /// Gets the gender of the pawn post-transformation
        /// </summary>
        /// <param name="pawn">The pawn to transform.</param>
        /// <param name="hediff">The TFing hediff.</param>
        public abstract void GetGender(Pawn pawn, Hediff_MutagenicBase hediff);
    }
}
