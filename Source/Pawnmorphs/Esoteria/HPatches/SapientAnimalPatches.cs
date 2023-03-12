// SapientAnimalPatches.cs modified by Iron Wolf for Pawnmorph on 12/17/2019 7:21 PM
// last updated 12/17/2019  7:21 PM

using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
	class SapientAnimalPatches
	{
		[HarmonyPatch(typeof(JobGiver_Mate)), HarmonyPatch("TryGiveJob")]
		static class DisableMattingPatch
		{
			[HarmonyPostfix]
			static void DisableMattingForSA([NotNull] Pawn pawn, [CanBeNull] ref Job __result)
			{
				if (__result != null)
				{
					if (!CanMate(pawn))
					{
						__result = null;
						return;
					}

					if (__result.targetA.Thing is Pawn oPawn)
					{
						if (!CanMate(oPawn))
						{
							__result = null;
							return;
						}
					}
				}

				bool CanMate(Pawn p) //only pure animals and permanently ferals can mate
				{
					if (p?.IsFormerHuman() != true) return true;
					var sapienceLevel = p.GetQuantizedSapienceLevel() ?? SapienceLevel.PermanentlyFeral;
					return sapienceLevel == SapienceLevel.PermanentlyFeral;
				}
			}
		}
	}
}