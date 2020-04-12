using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.HPatches
{
    public static class PawnGraphicSetPatch
    {

        [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
        static class ResolveAllGraphicsPatch
        {

            [HarmonyPostfix]
            static void Postfix(PawnGraphicSet __instance)
            {
                var colorationAspect = __instance.pawn?.GetAspectTracker()?.GetAspect(typeof(Aspects.Coloration)) as Aspects.Coloration;
                if (colorationAspect != null)
                    colorationAspect.TryDirectRecolor(__instance);
            }

        }

    }
}
