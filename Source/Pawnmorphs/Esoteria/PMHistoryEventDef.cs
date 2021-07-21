using JetBrains.Annotations;
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

        /// <summary>
        /// Whenever a mutation is gained
        /// </summary>
        /// args:
        /// Subject(pawn)
        /// Mutation(Mutation) 
        [NotNull]
        public static HistoryEventDef MutationGained;

        /// <summary> Whenever a mutation is lost </summary>
        /// args
        /// Subject(Pawn)
        /// Mutation(Mutation)
        [NotNull]
        public static HistoryEventDef MutationLost;

        /// <summary> Whenever a pawn becomes a former human </summary>
        /// args:
        /// Subject(pawn)
        /// Animal(PawnkindDef)
        /// FactionResponsible(Faction) can be null
        [NotNull]
        public static HistoryEventDef TransformedIntoFormerHuman;

        /// <summary> Whenever a pawn is no longer a former human </summary>
        /// args:
        /// Subject(pawn)
        /// Animal(PawnKindDef)
        /// FactionResponsible(Faction) can be null
        [NotNull]
        public static HistoryEventDef TransformedFromFormerHuman;

        /// <summary>
        /// Whenever a pawn is transformed into a morph
        /// </summary>
        /// args:
        /// Subject(pawn)
        /// OldMorph(MorphDef) can be null
        /// NewMorph(MorphDef)
        [NotNull]
        public static HistoryEventDef Transformed;

        /// <summary>
        /// Whenever a pawn is reverted
        /// </summary>
        /// args:
        /// Subject(pawn)
        /// Morph(morphDef)
        /// FactionResponsible(Faction) can be null
        [NotNull]
        public static HistoryEventDef Reverted;

        /// <summary> Whenever sapience level changes </summary>
        /// args:
        /// Subject(pawn)
        /// OldSapienceLevel(SapienceLevel)
        /// NewSapienceLevel(SapienceLevel)
        [NotNull]
        public static HistoryEventDef SapienceLevelChanged;

    }
}
