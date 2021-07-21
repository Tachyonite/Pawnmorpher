using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary> Static container for HistoryEventDef (event system for percepts). </summary>
    [DefOf]
    public static class PMHistoryEventDefOf
    {
        static PMHistoryEventDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PMHistoryEventDefOf));
        }

        /// <summary> Whenever a mutation is gained </summary>
        public static HistoryEventDef MutationGained;

        /// <summary> Whenever a mutation is lost </summary>
        public static HistoryEventDef MutationLost;

        /// <summary> Whenever a pawn becomes a former human </summary>
        public static HistoryEventDef TransformedIntoFormerHuman;

        /// <summary> Whenever a pawn is no longer a former human </summary>
        public static HistoryEventDef TranfromedFromFormerHuman;

        /// <summary> Whenever a pawn is transformed </summary>
        public static HistoryEventDef Transformed;

        /// <summary> Whenever a pawn is reverted </summary>
        public static HistoryEventDef Reverted;

        /// <summary> Whenever sapience level changes </summary>
        public static HistoryEventDef SapienceLevelChanged;

    }
}
