// PMRulePackDefOf.cs created by Iron Wolf for Pawnmorph on 10/09/2019 2:02 PM
// last updated 10/09/2019  2:02 PM

using RimWorld;
using Verse;

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class PMRulePackDefOf
	{
		/// <summary> Default rule pack used for generating mutation log entries. </summary>
		public static RulePackDef MutationTaleRulePack;

		/// <summary>
		///     Rule pack used when there is no mutation tale
		/// </summary>
		public static RulePackDef MutationRulePackTaleless;

		public static RulePackDef DefaultMutationLogPack;

		public static RulePackDef DefaultTailMutationLogPack;

		public static RulePackDef DefaultHornMutationLogPack;

		public static RulePackDef InjectorCauseLogPack;

		public static RulePackDef GetDefaultPackForMutation(HediffDef mutation)
		{
			if (mutation.defName.Contains("Tail")) return DefaultTailMutationLogPack;
			if (mutation.defName.Contains("Horn") || mutation.defName.Contains("Antlers"))
				return DefaultHornMutationLogPack;
			return DefaultMutationLogPack;


		}

		static PMRulePackDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMRulePackDefOf));
		}
	}
}