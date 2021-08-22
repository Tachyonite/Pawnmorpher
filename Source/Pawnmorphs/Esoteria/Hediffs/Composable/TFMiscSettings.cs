
namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines misc settings regarding the transformation
    /// </summary>
    public class TFMiscSettings
    {
        //TODO

        /// <summary>
        /// A debug string printed out when inspecting the hediffs
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public virtual string DebugString(Hediff_MutagenicBase hediff) => "";
    }
}
