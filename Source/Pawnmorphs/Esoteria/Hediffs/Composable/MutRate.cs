using System;
using JetBrains.Annotations;
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
        /// How many mutations to queue up for this tick.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public abstract int GetMutationsToAdd(Hediff_MutagenicBase hediff);
    }

    /// <summary>
    /// A simple mutation rate that uses vanilla's MTB class to add roughly a given
    /// number of mutations per day.
    /// </summary>
    public class MutRate_MutationsPerDay : MutRate
    {
        [UsedImplicitly] private float meanMutationsPerDay;

        /// <summary>
        /// How many mutations to queue up for this tick.
        /// 
        /// Called once a second by Hediff_MutagenicBase.  Queued up mutations will
        /// be spread out by that class, so no rate limiting needs to happen here.
        /// </summary>
        /// <returns>The number of mutations to add.</returns>
        /// <param name="hediff">Hediff.</param>
        public override int GetMutationsToAdd(Hediff_MutagenicBase hediff)
        {
            //Don't worry about division by zero, MTBEventOccurs handles positive infinity
            if (Rand.MTBEventOccurs(1f / meanMutationsPerDay, 60000, 60))
                return 1;

            return 0;
        }
    }
}
