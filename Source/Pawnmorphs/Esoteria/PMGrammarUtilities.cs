// PMGrammarUtilities.cs created by Iron Wolf for Pawnmorph on 09/03/2021 8:01 PM
// last updated 09/03/2021  8:02 PM

using System.Collections.Generic;
using JetBrains.Annotations;
using Verse.Grammar;

namespace Pawnmorph
{
	/// <summary>
	/// static class containing various grammar related utilities 
	/// </summary>
	public static class PMGrammarUtilities
	{
		[NotNull]
		private static readonly Dictionary<string, Rule> _cachedNullRules = new Dictionary<string, Rule>();

		/// <summary>
		/// Gets the null rule. ie the rule that evaluates to an empty string 
		/// </summary>
		/// <param name="keyWord">The key word.</param>
		/// <returns></returns>
		[NotNull]
		public static Rule GetNullRule(string keyWord)
		{
			if (_cachedNullRules.TryGetValue(keyWord, out var rule))
			{
				return rule;
			}

			rule = new Rule_String(keyWord, "");
			_cachedNullRules[keyWord] = rule;
			return rule;
		}
	}
}