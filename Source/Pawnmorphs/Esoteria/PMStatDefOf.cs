// PMStatDefOf.cs modified by Iron Wolf for Pawnmorph on 12/01/2019 9:01 AM
// last updated 12/01/2019  9:01 AM

using JetBrains.Annotations;
using RimWorld;

namespace Pawnmorph
{
    /// <summary>
    ///     static def of class for commonly used stats
    /// </summary>
    [DefOf]
    public static class PMStatDefOf
    {
        /// <summary>
        ///     stat that influences how fast a pawn adapts to new mutations
        /// </summary>
        /// has a range of [-1,2]
        /// values less then 0 means the pawn gets worse with mutations over time
        /// values greater then 0 mean the pawn gets better with mutations over time
        /// default value is 1
        [UsedImplicitly(ImplicitUseKindFlags.Assign)] [NotNull]
        public static StatDef MutationAdaptability;

        /// <summary>
        ///     stat that controls how large a change in control is caused by a change in instinct
        /// </summary>
        /// use the inverse of this value to get the multiplier
        [UsedImplicitly(ImplicitUseKindFlags.Assign)] [NotNull]
        public static StatDef SapientAnimalA;

        /// <summary>
        ///     stat that influences the amount of control a sapient animal has before going feral
        /// </summary>
        [UsedImplicitly(ImplicitUseKindFlags.Assign)] [NotNull]
        public static StatDef SapientAnimalResistance;


        /// <summary>
        ///     Multiplier on the impact of mutagenic buildup on this creature.
        /// </summary>
        public static StatDef MutagenSensitivity;

        /// <summary>
        /// this stat is the %chance a pawn will get sick from dangerous foods 
        /// </summary>
        /// note, the check is applied after the initial check on the food itself, so the actual chance a pawn will get sick from
        /// a specific, dangerous food is this multiplied by the food's FoodPoisonChanceFixedHuman stat
        public static StatDef DangerousFoodSensitivity;
        /// <summary>
        /// this stat is the %chance a pawn will get sick from rotten foods 
        /// </summary>
        /// note, the check is applied after the initial check on the food itself, so the actual chance a pawn will get sick from
        /// a specific, rotten food is this multiplied by the base chance to get sick from rotten food 
        public static StatDef RottenFoodSensitivity;

        /// <summary>
        /// the concentration of mutanite in a thing, this is used by refineries to determine how much of a thing is required to make mutanite 
        /// </summary>
        public static StatDef MutaniteConcentration; 


        /// <summary>
        /// stat that affects the likely hood that a pawn fully transforms
        /// this is a multiplier on the transformation chance 
        /// </summary>
        [NotNull] public static StatDef TransformationSensitivity; 

        /// <summary>
        /// stat that determines how much 'sapience' a pawn recovers over time 
        /// </summary>
        [NotNull]
        public static StatDef SapienceRecoverFactor;
        // ReSharper disable once NotNullMemberIsNotInitialized
        static PMStatDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDef));
        }
    }
}