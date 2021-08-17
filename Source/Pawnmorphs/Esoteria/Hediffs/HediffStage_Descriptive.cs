using JetBrains.Annotations;
using Verse;

namespace Pawnmorph.Hediffs
{
    /// <summary>
    /// Hediff stage with only an extra description field and nothing else
    /// </summary>
    /// <seealso cref="Verse.HediffStage" />
    public class HediffStage_Descriptive : HediffStage, IDescriptiveStage
    {
        [UsedImplicitly] private string description;
        [UsedImplicitly] private string labelOverride;

        public string DescriptionOverride => description;
        public string LabelOverride => labelOverride;
    }
}