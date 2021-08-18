using System;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines how quickly mutations are gained
    /// </summary>
    public abstract class MutRate
    {
        //TODO mut rate should "queue up" mutation counts, the base hediff class will then add them
    }

    /// <summary>
    /// A simple mutation rate that uses vanilla's MTB class to determine when to add mutations
    /// </summary>
    public class MutRate_MutationsPerDay
    {
        //TODO mut rate should "queue up" mutation counts, the base hediff class will then add them
    }
}
