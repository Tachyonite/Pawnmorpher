// InstinctUtlities.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 1:35 PM
// last updated 12/07/2019  1:35 PM

using System;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph
{
    /// <summary>
    ///     static class containing useful instinct related utilities
    /// </summary>
    public static class InstinctUtilities
    {
        private const float BETA = 50f / 3f; //converts 'resistance stat' to sapience
        private const float ALPHA = 50f / 20f; //converts intelligence to sapience

        private const int AVERAGE_INT = 3;
        private const float AVERAGE_RESISTANCE_STAT = 0.6f;
        /// <summary>
        ///     The average resistance of pawns
        /// </summary>
        /// use this value to scale control values to a better range
        public const float AVERAGE_MAX_SAPIENCE = AVERAGE_INT * ALPHA + AVERAGE_RESISTANCE_STAT * BETA;

        /// <summary>
        ///     Calculates the total control has left
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="instinct">The instinct.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static float CalculateControl([NotNull] Pawn pawn, float instinct)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));
            float res = CalculateNetResistance(pawn);
            float lambda = 1 / pawn.GetStatValue(PMStatDefOf.SapientAnimalA);

            return res - instinct * lambda;
        }

        /// <summary>
        ///     Calculates the change in control caused by the given instinct change.
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <param name="instinctChange">The instinct change.</param>
        /// <returns></returns>
        public static float CalculateControlChange([NotNull] Pawn pawn, float instinctChange)
        {
            return -instinctChange / pawn.GetStatValue(PMStatDefOf.SapientAnimalA);
        }

        /// <summary>
        ///     Calculates the net resistance of this pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">pawn</exception>
        public static float CalculateNetResistance([NotNull] Pawn pawn)
        {
            if (pawn == null) throw new ArgumentNullException(nameof(pawn));

            int i = pawn.skills?.GetSkill(SkillDefOf.Intellectual)?.Level ?? 0;
            float rs = pawn.GetStatValue(PMStatDefOf.SapientAnimalResistance);
            return i * ALPHA + rs * BETA;
        }


        /// <summary>
        /// Gets the control need of the given pawn
        /// </summary>
        /// <param name="pawn">The pawn.</param>
        /// <returns></returns>
        [CanBeNull]
        public static Need_Control GetControlNeed([NotNull] Pawn pawn)
        {
            return pawn.needs?.TryGetNeed<Need_Control>();
        }
    }
}