using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
    static class PawnGraphicSetPatch
    {

        [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
        static class ResolveAllGraphicsPatch
        {

            [HarmonyPostfix]
            static void Postfix(PawnGraphicSet __instance)
            {
                Pawn pawn = __instance.pawn;
                var colorationAspect = pawn?.GetAspectTracker()?.GetAspect<Aspects.ColorationAspect>();
                if (colorationAspect != null)
                {
                    if(!pawn.RaceProps.Humanlike)
                        colorationAspect.TryDirectRecolorAnimal(__instance);
                }
            }

        }

    }
}
