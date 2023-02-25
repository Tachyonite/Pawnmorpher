// IRulePackContainer.cs created by Iron Wolf for Pawnmorph on 09/05/2021 8:44 AM
// last updated 09/05/2021  8:44 AM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse.Grammar;

namespace Pawnmorph
{
	/// <summary>
	/// interface for a def of def extension that contains additional rule packs to be used with mutation cause system 
	/// </summary>
	public interface ICauseRulePackContainer
	{
		/// <summary>
		/// Gets the rules using the given prefix 
		/// </summary>
		/// <param name="prefix">The prefix.</param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<Rule> GetRules(string prefix);
	}
}