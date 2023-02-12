// Worker_Hellhound.cs created by Iron Wolf for Pawnmorph on 07/29/2020 8:54 PM
// last updated 07/29/2020  8:54 PM

using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.MutationRules
{
	/// <summary>
	/// worker that removes all the conditional hediffs when applied 
	/// </summary>
	/// <seealso cref="Pawnmorph.MutationRuleWorker" />
	public class Worker_RemoveConditionalHediffs : DefaultMutationRuleWorker
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MutationRuleWorker" /> class.
		/// </summary>
		/// <param name="ruleDef">The rule definition.</param>
		public Worker_RemoveConditionalHediffs([NotNull] MutationRuleDef ruleDef) : base(ruleDef)
		{
		}


		/// <summary>
		/// Called when the rule is successfully applied 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		protected override void OnRuleApplied(Pawn pawn)
		{
			base.OnRuleApplied(pawn);
			var health = pawn.health?.hediffSet;
			if (health == null) return;

			foreach (MutationRuleDef.HediffEntry hediffEntry in RuleDef.conditions.MakeSafe())
			{
				foreach (HediffDef hediffDef in hediffEntry.hediffs.MakeSafe())
				{
					var hDiff = health.GetFirstHediffOfDef(hediffDef);
					if (hDiff != null)
					{
						pawn.health.RemoveHediff(hDiff);
					}
				}
			}

		}
	}
}