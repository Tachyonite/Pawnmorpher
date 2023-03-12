// PMInteractionDefOf.cs created by Iron Wolf for Pawnmorph on 03/15/2020 3:54 PM
// last updated 03/15/2020  3:54 PM

using JetBrains.Annotations;
using RimWorld;

// ReSharper disable NotNullMemberIsNotInitialized

#pragma warning disable 1591
namespace Pawnmorph
{
	[DefOf]
	public static class PMInteractionDefOf
	{
		[NotNull] public static InteractionDef FormerHumanAnimalChat;

		static PMInteractionDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(PMInteractionDefOf));
		}
	}
}