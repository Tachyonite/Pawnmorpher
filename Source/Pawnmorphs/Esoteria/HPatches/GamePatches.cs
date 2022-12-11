using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Game))]
	internal static class GamePatches
	{


		[HarmonyPatch(nameof(Game.LoadGame)), HarmonyPostfix]
		private static void LoadGamePostFix()
		{
			FixMissingNarrowHeads();
		}
		private static void FixMissingNarrowHeads()
		{
			HeadTypeDef[] narrowHeads = DefDatabase<HeadTypeDef>.AllDefs.Where(x => x.defName.Contains("Narrow")).ToArray();
			Dictionary<HeadTypeDef, HeadTypeDef> headmap = narrowHeads.ToDictionary(x => x, x => DefDatabase<HeadTypeDef>.GetNamed(x.defName.Replace("Narrow", "Average")));

			foreach (Pawn pawn in PawnsFinder.All_AliveOrDead)
			{
				Pawn_StoryTracker story = pawn.story;
				if (story?.headType != null)
				{
					if (headmap.TryGetValue(story.headType, out HeadTypeDef headTypeDef))
						story.headType = headTypeDef;
				}
			}
		}
	}
}
