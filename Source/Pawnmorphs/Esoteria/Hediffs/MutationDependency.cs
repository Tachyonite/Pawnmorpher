// MutationDependency.cs modified by Iron Wolf for Pawnmorph on 08/09/2019 9:09 AM
// last updated 08/09/2019  9:09 AM

using System.Collections.Generic;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.Hediffs
{
	/// <summary>
	/// component representing a mutation dependency, some mutation that will be added to the pawn if not already there 
	/// </summary>
	public class Comp_MutationDependency : HediffCompBase<CompProperties_MutationDependency>
	{

		/// <summary>
		/// called just after this instance's parent is added to a pawn 
		/// </summary>
		/// <param name="dinfo">The damage info.</param>
		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			base.CompPostPostAdd(dinfo);

			Props.mutationDependency.TryApply(Pawn);
		}
	}


	/// <summary>
	/// a hediff component property for a mutation dependency, ie some hediff giver that fires when the mutation is added 
	/// </summary>
	public class CompProperties_MutationDependency : HediffCompPropertiesBase<Comp_MutationDependency>
	{
		/// <summary>
		/// The mutation dependency
		/// </summary>
		public HediffGiver_Mutation mutationDependency;

		/// <summary>
		/// Get all Configuration Errors with this instance
		/// </summary>
		/// <param name="parentDef">The parent definition.</param>
		/// <returns></returns>
		public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
		{
			foreach (string configError in base.ConfigErrors(parentDef))
			{
				yield return configError;
			}

			if (mutationDependency == null)
			{
				yield return nameof(mutationDependency) + " is not set!";

			}

		}
	}
}