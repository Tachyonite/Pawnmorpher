﻿using HarmonyLib;
using Pawnmorph.GraphicSys;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Pawn_GeneTracker))]
	static class GeneTrackerPatches
	{
		[HarmonyPatch("EnsureCorrectSkinColorOverride"), HarmonyPostfix]
		static void EnsureCorrectSkinColorOverridePostfix(Pawn ___pawn)
		{
			GraphicsUpdaterComp graphicsComp = ___pawn.TryGetComp<GraphicsUpdaterComp>();
			if (graphicsComp != null)
			{
				graphicsComp.GeneOverrideColor = ___pawn.story.skinColorOverride;
				graphicsComp.RefreshGraphics();
			}
		}

	}
}
