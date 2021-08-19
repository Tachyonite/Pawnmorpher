using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
    /// <summary>
    /// A class that determines the order in which mutations spread through a person
    /// </summary>
    public abstract class MutSpreadOrder
    {
        /// <summary>
        /// Gets the the spread manager that will be used to control the spread order
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public abstract SpreadManager GetSpreadManager(Hediff_MutagenicBase hediff);

        /// <summary>
        /// Determines whether the given MutSpreadOrder creates spread orders equivalent
        /// to this one.  If two MutSpreadOrders are equivalent, the Hediff won't throw
        /// away the old spread manager since it's still valid.
        /// </summary>
        /// <param name="other">The other spread order.</param>
        public abstract bool EquivalentTo(MutSpreadOrder other);
    }

    /// <summary>
    /// A simple spread order that traverses the body in 100% random order
    /// Suitable for chaotic mutations like buildup
    /// </summary>
    public class MutSpreadOrder_FullRandom : MutSpreadOrder
    {
        /// <summary>
        /// Gets the the spread manager that will be used to control the spread order
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public override SpreadManager GetSpreadManager(Hediff_MutagenicBase hediff)
        {
            var bodyParts = hediff.pawn.RaceProps.body.AllParts.InRandomOrder().ToList();
            return new SpreadManager(bodyParts);
        }

        /// <summary>
        /// Determines whether the given MutSpreadOrder creates spread orders equivalent
        /// to this one.  If two MutSpreadOrders are equivalent, the Hediff won't throw
        /// away the old spread manager since it's still valid.
        /// </summary>
        /// <param name="other">The other spread order.</param>
        public override bool EquivalentTo(MutSpreadOrder other)
        {
            return other is MutSpreadOrder_FullRandom;
        }
    }

    /// <summary>
    /// A simple spread order that uses a "spreading" order from a random part
    /// Suitable for more directed sources of mutation, like injectors
    /// </summary>
    /// <param name="hediff">The hediff doing the transformation.</param>
    public class MutSpreadOrder_RandomSpread : MutSpreadOrder
    {
        /// <summary>
        /// Gets the the spread manager that will be used to control the spread order
        /// </summary>
        /// <param name="hediff">The hediff doing the transformation.</param>
        public override SpreadManager GetSpreadManager(Hediff_MutagenicBase hediff)
        {
            var bodyParts = new List<BodyPartRecord>();
            hediff.pawn.RaceProps.body.RandomizedSpreadOrder(bodyParts);
            return new SpreadManager(bodyParts);
        }

        /// <summary>
        /// Determines whether the given MutSpreadOrder creates spread orders equivalent
        /// to this one.  If two MutSpreadOrders are equivalent, the Hediff won't throw
        /// away the old spread manager since it's still valid.
        /// </summary>
        /// <param name="other">The other spread order.</param>
        public override bool EquivalentTo(MutSpreadOrder other)
        {
            return other is MutSpreadOrder_RandomSpread;
        }
    }

    //TODO SpreadFromPart
    //TODO SpreadFromInjury

    /// <summary>
    /// A class to handle the stateful part of managing spread order.
    /// Gets saved and loaded with the Hediff class.
    /// 
    /// Don't add any logic to this class, since instances regularly gets thrown
    /// away and recreated. Instead, add the logic to the MutSpreadOrder that
    /// creates it.
    /// </summary>
    public sealed class SpreadManager : Checklist<BodyPartRecord>
    {
        public SpreadManager() { }

        public SpreadManager(List<BodyPartRecord> parts) : base(parts) { }

        public override LookMode LookMode => LookMode.BodyPart;
    }
}
