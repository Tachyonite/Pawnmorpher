using System;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines the order in which mutations spread through a person
    /// </summary>
    public abstract class MutSpreadOrder
    {
        /// <summary>
        /// Gets the next body part to mutate.
        /// </summary>
        public abstract void GetNextBodyPart();
    }
}
