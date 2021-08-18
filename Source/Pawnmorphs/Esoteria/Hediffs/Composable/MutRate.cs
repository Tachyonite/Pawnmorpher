using System;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using UnityEngine;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines how quickly mutations are gained
    /// </summary>
    public abstract class MutRate
    {
        /// <summary>
        /// How many mutations to queue up for the next second.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public virtual int GetMutationsPerSecond(Hediff_MutagenicBase hediff)
        {
            return 0;
        }

        /// <summary>
        /// How many mutations to queue up for a given severity change.  Note that severity
        /// changes can be negative, and negative mutations are allowed.
        /// (negative mutations can cancel queued mutations but won't remove existing ones)
        /// 
        /// Called any time severity changes in Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        /// <param name="sevChange">How much severity changed by.</param>
        public virtual int GetMutationsPerSeverity(Hediff_MutagenicBase hediff, float sevChange)
        {
            return 0;
        }
    }

    /// <summary>
    /// A simple mutation rate that uses vanilla's MTB class to add roughly a given
    /// number of mutations per day.
    /// </summary>
    public class MutRate_MutationsPerDay : MutRate
    {
        [UsedImplicitly] private float meanMutationsPerDay;

        /// <summary>
        /// How many mutations to queue up for the next second.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public override int GetMutationsPerSecond(Hediff_MutagenicBase hediff)
        {
            //Don't worry about division by zero, MTBEventOccurs handles positive infinity
            if (Rand.MTBEventOccurs(1f / meanMutationsPerDay, 60000, 60))
                return 1;

            return 0;
        }
    }

    /// <summary>
    /// A mutation rate that gives a normally-distributed amount of mutations based on severity changes
    /// </summary>
    public class MutRate_MutationsPerSevChange : MutRate
    {
        [UsedImplicitly] private float meanMutationsPerSeverity;
        [UsedImplicitly] private float standardDeviation;

        /// <summary>
        /// How many mutations to queue up for a given severity change.  Note that severity
        /// changes can be negative, and negative mutations are allowed.
        /// (negative mutations can cancel queued mutations but won't remove existing ones)
        /// 
        /// Called any time severity changes in Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        /// <param name="sevChange">How much severity changed by.</param>
        public override int GetMutationsPerSeverity(Hediff_MutagenicBase hediff, float sevChange)
        {
            float expectedMutations = meanMutationsPerSeverity * sevChange;
            float actualMutations = RandUtilities.generateNormalRandom(expectedMutations, standardDeviation);

            return RandUtilities.RandRound(actualMutations);
        }
    }
}
