// RulePackExtension.cs created by Iron Wolf for Pawnmorph on 09/05/2021 8:56 AM
// last updated 09/05/2021  8:56 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Pawnmorph.Utilities;
using Verse;
using Verse.Grammar;

namespace Pawnmorph.DefExtensions
{


	/// <summary>
	/// def extension put onto other things to give them additional rule packs describing what cause mutations or transformations 
	/// </summary>
	/// <seealso cref="Verse.DefModExtension" />
	/// <seealso cref="Pawnmorph.ICauseRulePackContainer" />
	public class MutationCauseExtension : DefModExtension, ICauseRulePackContainer
	{
		/// <summary>
		/// The rule pack 
		/// </summary>
		[CanBeNull]
		public RulePack rulePack;
		/// <summary>
		/// The rule pack definition
		/// </summary>
		[CanBeNull]
		public RulePackDef rulePackDef;

		/// <summary>
		/// Gets the rules using the given prefix 
		/// </summary>
		/// <param name="prefix">The prefix.</param>
		/// <returns></returns>
		public IEnumerable<Rule> GetRules(string prefix)
		{
			if (rulePack != null)
				foreach (Rule rule in rulePack.Rules.MakeSafe())
				{
					yield return rule;
				}

			if (rulePackDef != null)
			{
				foreach (Rule rule in rulePackDef.RulesPlusIncludes.MakeSafe())
				{
					yield return rule;
				}
			}
		}
	}
}