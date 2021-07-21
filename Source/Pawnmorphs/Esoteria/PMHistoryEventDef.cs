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
        //Notes: 
        //Doer: should refer to the pawn who the even is about
        //Subject should refer to a thing the Doer sees or does something about 


        /// <summary>
        /// Whenever a mutation is gained
        /// </summary>
        /// args:
        /// Doer(pawn)
        /// Mutation(Mutation) 
        [NotNull]
        public static HistoryEventDef MutationGained;

        /// <summary> Whenever a mutation is lost </summary>
        /// args
        /// Doer(Pawn)
        /// Mutation(Mutation)
        [NotNull]
        public static HistoryEventDef MutationLost;

        /// <summary> Whenever a pawn becomes a former human </summary>
        /// args:
        /// Doer(pawn)
        /// Animal(PawnkindDef)
        /// FactionResponsible(Faction) can be null
        [NotNull]
        public static HistoryEventDef TransformedIntoFormerHuman;

        /// <summary> Whenever a pawn is no longer a former human </summary>
        /// args:
        /// Doer(pawn)
        /// Animal(PawnKindDef)
        /// FactionResponsible(Faction) can be null
        [NotNull]
        public static HistoryEventDef TransformedFromFormerHuman;

        /// <summary>
        /// Whenever a pawn is transformed into a morph
        /// </summary>
        /// args:
        /// Doer(pawn)
        /// OldMorph(MorphDef) can be null
        /// NewMorph(MorphDef)
        [NotNull]
        public static HistoryEventDef Transformed;

        /// <summary>
        /// Whenever a pawn is reverted
        /// </summary>
        /// args:
        /// Doer(pawn)
        /// Morph(morphDef)
        /// FactionResponsible(Faction) can be null
        [NotNull]
        public static HistoryEventDef Reverted;

        /// <summary> Whenever sapience level changes </summary>
        /// args:
        /// Doer(pawn)
        /// OldSapienceLevel(SapienceLevel)
        /// NewSapienceLevel(SapienceLevel)
        [NotNull]
        public static HistoryEventDef SapienceLevelChanged;

    }
}
