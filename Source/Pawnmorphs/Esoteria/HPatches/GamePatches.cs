using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using AlienRace;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

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

			// Fix HAR bug for adding PM into existing save.
			// HAR breaks when adding additional color channels to existing save.
			foreach (Pawn pawn in Find.CurrentMap.mapPawns.AllPawns)
			{
				AlienComp comp = pawn.TryGetComp<AlienComp>();
				AlienPartGenerator apg = (pawn.def as ThingDef_AlienRace)?.alienRace.generalSettings.alienPartGenerator;
				if (apg == null)
					continue;

				// https://github.com/erdelf/AlienRaces/blob/d9104a6089953230a0cad7a7573c2e995e01d125/Source/AlienRace/AlienRace/AlienPartGenerator.cs#L560C1-L565C26
				foreach (ColorChannelGenerator channel in apg.colorChannels)
				{
					if (!comp.ColorChannels.ContainsKey(channel.name))
					{
						comp.ColorChannels.Add(channel.name, new ExposableValueTuple<Color, Color>(Color.white, Color.white));
						comp.ColorChannels[channel.name] = comp.GenerateChannel(channel, comp.ColorChannels[channel.name]);
					}
				}
			}
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
