using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
	static class PawnRenderNode_AnimalPartPatch
	{

		[HarmonyPatch(typeof(PawnRenderNode_AnimalPart), nameof(PawnRenderNode_AnimalPart.GraphicFor))]
		static class GraphicForPatch
		{

			[HarmonyPostfix]
			static void Postfix(Graphic __result, PawnRenderNode_AnimalPart __instance)
			{
				Pawn pawn = __instance.tree.pawn;
				var colorationAspect = pawn?.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
				if (colorationAspect != null)
				{
					if (!pawn.RaceProps.Humanlike)
						colorationAspect.TryDirectRecolorAnimal(__result);
				}
			}

		}

	}
}
