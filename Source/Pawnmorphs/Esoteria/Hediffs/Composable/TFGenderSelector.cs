using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines the gender of the pawn post-transformation
    /// </summary>
    public abstract class TFGenderSelector
    {
        /// <summary>
        /// Gets the gender of the pawn post-transformation
        /// </summary>
        /// <param name="pawn">The pawn to transform.</param>
        /// <param name="hediff">The TFing hediff.</param>
        public abstract void GetGender(Pawn pawn, Hediff_MutagenicBase hediff);

        /// <summary>
        /// A debug string printed out when inspecting the hediffs
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public virtual string DebugString(Hediff_MutagenicBase hediff) => "";
    }

    /// <summary>
    /// A gender selector that simply always uses the same gender as the pawn
    /// </summary>
    public class TFGenderSelector_Same : TFGenderSelector
    {
        /// <summary>
        /// Gets the gender of the pawn post-transformation
        /// </summary>
        /// <param name="pawn">The pawn to transform.</param>
        /// <param name="hediff">The TFing hediff.</param>
        public override void GetGender(Pawn pawn, Hediff_MutagenicBase hediff)
        {
            //TODO
        }
    }
}
