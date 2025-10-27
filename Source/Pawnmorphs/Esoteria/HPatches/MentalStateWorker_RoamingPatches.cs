using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	[HarmonyPatch(typeof(MentalStateWorker_Roaming))]
	static class MentalStateWorker_RoamingPatches
	{

		[HarmonyPatch(nameof(MentalStateWorker_Roaming.CanRoamNow))]
		[HarmonyPostfix]
		static void CanRoamNowPostfix(Pawn pawn, ref bool __result)
		{
			if (__result)
			{
				if (pawn.IsHumanlike())
					__result = false;
			}
		}
	}
}
