using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(Caravan))]
	static class CaravanPatches
	{
		[HarmonyPatch(nameof(Caravan.Notify_DestinationOrPauseStatusChanged)), HarmonyPostfix]
		static void Caravan_Postfix(Caravan __instance)
		{
			foreach (Pawn pawn in __instance.pawns)
			{
				FormerHumanUtilities.InvalidateIntelligence(pawn);
			}
		}
	}
}
