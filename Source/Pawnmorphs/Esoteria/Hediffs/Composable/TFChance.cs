using System;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines what the chance of a full transformation is
    /// </summary>
    public abstract class TFChance
    {
        /// <summary>
        /// Whether or not to transform the pawn.  Checked only upon entering a stage.
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public abstract bool ShouldTransform(Hediff_MutagenicBase hediff);
    }

    /// <summary>
    /// A simple TFChance class that always transforms the pawn
    /// </summary>
    public class TFChance_Always : TFChance
    {
        /// <summary>
        /// Whether or not to transform the pawn.  Checked only upon entering a stage.
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public override bool ShouldTransform(Hediff_MutagenicBase hediff)
        {
            return true;
        }
    }
}
