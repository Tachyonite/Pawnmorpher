// PMDesignationDefOf.cs created by Iron Wolf for Pawnmorph on 03/15/2020 2:56 PM
// last updated 03/15/2020  2:56 PM

using JetBrains.Annotations;
using RimWorld;
using Verse;

// ReSharper disable NotNullMemberIsNotInitialized

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public class PMDesignationDefOf
	{
		[NotNull]
		public static DesignationDef RecruitSapientFormerHuman;

		static PMDesignationDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMDesignationDefOf));
		}
	}
}