using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches.Optional
{
	[OptionalPatch("PMNarrowHeadsCaption", "PMNarrowHeadsDescription", nameof(_enabled), true)]
	[HarmonyPatch(typeof(Game))]
	[UsedImplicitly]
	internal static class NarrowHeads
	{
		static bool _enabled = true;

		static bool Prepare(MethodBase original)
		{
			if (original == null && _enabled)
				Log.Message("[PM] Optional narrow head patch enabled.");

			return _enabled;
		}

		private static readonly Dictionary<string, string> HeadReplacements = new()
		{
			{ "Male_NarrowNormal", "Male_AverageNormal" },
			{ "Male_NarrowPointy", "Male_AveragePointy" },
			{ "Male_NarrowWide", "Male_AverageWide" },
			{ "Female_NarrowNormal", "Female_AverageNormal" },
			{ "Female_NarrowPointy", "Female_AveragePointy" },
			{ "Female_NarrowWide", "Female_AverageWide" }
		};

		[HarmonyPatch(nameof(Game.LoadGame)), HarmonyPostfix]
		[UsedImplicitly]
		private static void LoadGamePostFix()
		{
			FixMissingNarrowHeads();
		}

		private static void FixMissingNarrowHeads()
		{
			Dictionary<HeadTypeDef, HeadTypeDef> headMap =
				HeadReplacements.ToDictionary(pair => DefDatabase<HeadTypeDef>.GetNamed(pair.Key),
											  pair => DefDatabase<HeadTypeDef>.GetNamed(pair.Value));

			foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
			{
				Pawn_StoryTracker story = pawn.story;
				if (story?.headType != null)
				{
					if (headMap.TryGetValue(story.headType, out HeadTypeDef headTypeDef))
						story.headType = headTypeDef;
				}
			}
		}
	}
}
