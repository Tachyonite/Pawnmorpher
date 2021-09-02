
using JetBrains.Annotations;
using Pawnmorph.TfSys;
using RimWorld;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines misc settings regarding the transformation
    /// 
    /// NOTE This can be broken up into its components if extended logic seems useful
    /// </summary>
    public class TFMiscSettings
    {
        /// <summary>
        /// The settings that define the chance of going manhunter
        /// </summary>
        [UsedImplicitly] public ManhunterTfSettings manhunterSettings = ManhunterTfSettings.Default;

        /// <summary>
        /// The tale to use for the transformation
        /// </summary>
        [UsedImplicitly] public TaleDef tfTale;

        /// <summary>
        /// Forces the sapience to a specific value if present
        /// </summary>
        [UsedImplicitly] public float? forcedSapience;

        /// <summary>
        /// The settings that define the chance of going manhunter
        /// </summary>
        public virtual ManhunterTfSettings ManhunterSettings => manhunterSettings;

        /// <summary>
        /// The tale to use for the transformation
        /// </summary>
        public virtual TaleDef TfTale => tfTale;

        /// <summary>
        /// Forces the sapience to a specific value if present
        /// </summary>
        public virtual float? ForcedSapience => forcedSapience;

        /// <summary>
        /// A debug string printed out when inspecting the hediffs
        /// </summary>
        /// <param name="hediff">The parent hediff.</param>
        /// <returns>The string.</returns>
        public virtual string DebugString(Hediff_MutagenicBase hediff) => "";
    }
}
