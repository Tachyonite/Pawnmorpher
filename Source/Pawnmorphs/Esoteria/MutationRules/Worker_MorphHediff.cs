// Worker_MorphHediff.cs created by Iron Wolf for Pawnmorph on 03/15/2020 1:25 PM
// last updated 03/15/2020  1:25 PM

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Pawnmorph.Hediffs;
using Pawnmorph.Utilities;
using Verse;

namespace Pawnmorph.MutationRules
{
	/// <summary>
	/// rule worker combining the various 'morph hediffs' into a single morph hediff 
	/// </summary>
	/// <seealso cref="Pawnmorph.DefaultMutationRuleWorker" />
	public class Worker_MorphHediff : DefaultMutationRuleWorker
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MutationRuleWorker" /> class.
		/// </summary>
		/// <param name="ruleDef">The rule definition.</param>
		public Worker_MorphHediff([NotNull] MutationRuleDef ruleDef) : base(ruleDef)
		{
		}


		private List<HediffDef> _condList;

		[NotNull]
		IReadOnlyList<HediffDef> ConditionList
		{
			get
			{
				return _condList ?? (_condList = RuleDef.conditions.MakeSafe().SelectMany(r => r.hediffs.MakeSafe()).ToList());
			}
		}



		/// <summary>
		/// Called when the rule is successfully applied 
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		protected override void OnRuleApplied(Pawn pawn)
		{
			base.OnRuleApplied(pawn);

			foreach (var hediff in pawn.health.hediffSet.hediffs.MakeSafe())
			{
				if (ConditionList.ContainsHediff(hediff) && hediff is IMutagenicHediff mutHediff)
				{
					mutHediff.MarkForRemoval(); //don't directly remove them, but mark them for removal so they can be removed
				}
			}

		}
	}
}