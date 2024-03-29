﻿using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Game))]
	[UsedImplicitly]
	internal static class GamePatches
	{
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
			// Can be null if narrow head types were entirely removed by another mod like Tweaks Galore
			Dictionary<HeadTypeDef, HeadTypeDef> headMap = new Dictionary<HeadTypeDef, HeadTypeDef>(HeadReplacements.Count);
			foreach (KeyValuePair<string, string> pair in HeadReplacements)
			{
				HeadTypeDef key = DefDatabase<HeadTypeDef>.GetNamed(pair.Key, false);
				HeadTypeDef value = DefDatabase<HeadTypeDef>.GetNamed(pair.Value, false);

				if (key == null || value == null)
					continue;

				headMap[key] = value;
			}

			if (headMap.Count == 0)
				return;

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
