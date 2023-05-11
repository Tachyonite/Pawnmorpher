// MutationRuleUtilities.cs created by Iron Wolf for Pawnmorph on 03/06/2020 9:58 PM
// last updated 03/06/2020  9:58 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// various mutation rule related utilities 
	/// </summary>
	[StaticConstructorOnStartup]
	public static class MutationRuleUtilities
	{
		[NotNull]
		private static readonly LinkedList<MutationRuleDef> _allRules;

		/// <summary>
		/// Gets a sorted collection of all MutationRuleDefs 
		/// </summary>
		/// <value>
		/// All rules.
		/// </value>
		[NotNull]
		public static IEnumerable<MutationRuleDef> AllRules => _allRules;

		static MutationRuleUtilities()
		{
			_allRules = new LinkedList<MutationRuleDef>();


			foreach (MutationRuleDef def in DefDatabase<MutationRuleDef>.AllDefs)
			{
				//create and sort the all rules list 
				var node = _allRules.First;
				while (node != null && node.Value.priority < def.priority)
				{
					node = node.Next;
				}

				if (node == null)
				{
					_allRules.AddLast(def);
				}
				else
				{
					_allRules.AddBefore(node, def);
				}
			}



		}

		/// <summary>
		/// Tries to execute the rules on the given pawn.
		/// </summary>
		/// <param name="pawn">The pawn.</param>
		/// <returns></returns>
		public static bool TryExecuteRulesOn([NotNull] Pawn pawn)
		{
			foreach (MutationRuleDef rule in AllRules)
			{
				if (rule.TryRule(pawn)) return true;
			}

			return false;
		}

	}
}