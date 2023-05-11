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
	[HarmonyPatch(typeof(Pawn_MindState))]
	internal static class PawnMindStatePatches
	{
		[HarmonyPatch("CanStartFleeingBecauseOfPawnAction"), HarmonyPostfix]
		static bool CanStartFleeingBecauseOfPawnActionPatch(bool __result, Pawn p)
		{
			// Make conflicted act like normal animals except when drafted.
			//TODO do a thorough check through behaviour for conflicted.
			switch (p.GetQuantizedSapienceLevel())
			{
				case (SapienceLevel.Sapient):
				case (SapienceLevel.MostlySapient):
					return false;

				case (SapienceLevel.Conflicted):
					if (p.Drafted)
						return false;
					else
						return __result;

				case (SapienceLevel.Feral):
				case (SapienceLevel.MostlyFeral):
				case (SapienceLevel.PermanentlyFeral):
				default:
					return __result;
			}

		}

	}
}
