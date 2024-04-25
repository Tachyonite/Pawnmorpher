using System.Collections.Generic;
using System.Linq;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs.Composable
{
	/// <summary>
	/// A class that determines the order in which mutations spread through a person
	/// </summary>
	public abstract class MutSpreadOrder : IInitializableStage
	{
		/// <summary>
		/// Gets the the spread manager that will be used to control the spread order
		/// </summary>
		/// <param name="hediff">The hediff doing the transformation.</param>
		public abstract IEnumerable<BodyPartRecord> GetSpreadList(Hediff_MutagenicBase hediff);

		/// <summary>
		/// Determines whether the given MutSpreadOrder creates spread orders equivalent
		/// to this one.  If two MutSpreadOrders are equivalent, the Hediff won't throw
		/// away the old spread manager since it's still valid.
		/// </summary>
		/// <param name="other">The other spread order.</param>
		public abstract bool EquivalentTo(MutSpreadOrder other);

		/// <summary>
		/// A debug string printed out when inspecting the hediffs
		/// </summary>
		/// <param name="hediff">The parent hediff.</param>
		/// <returns>The string.</returns>
		public virtual string DebugString(Hediff_MutagenicBase hediff) => "";

		/// <summary>
		/// gets all configuration errors in this stage .
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public virtual IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Resolves all references in this instance.
		/// </summary>
		/// <param name="parent">The parent.</param>
		public virtual void ResolveReferences(HediffDef parent)
		{
			//empty 
		}
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
		public override IEnumerable<BodyPartRecord> GetSpreadList(Hediff_MutagenicBase hediff)
		{
			return null;
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
	public class MutSpreadOrder_RandomSpread : MutSpreadOrder
	{
		/// <summary>
		/// Gets the the spread manager that will be used to control the spread order
		/// </summary>
		/// <param name="hediff">The hediff doing the transformation.</param>
		public override IEnumerable<BodyPartRecord> GetSpreadList(Hediff_MutagenicBase hediff)
		{
			var bodyParts = new List<BodyPartRecord>();
			hediff.pawn.RaceProps.body.RandomizedSpreadOrder(bodyParts);
			return bodyParts;
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
}
