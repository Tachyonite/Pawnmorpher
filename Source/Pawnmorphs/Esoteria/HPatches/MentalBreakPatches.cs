// MentalBreakPatches.cs modified by Iron Wolf for Pawnmorph on 12/07/2019 2:40 PM
// last updated 12/07/2019  2:40 PM

using Harmony;
using JetBrains.Annotations;
using Pawnmorph.DefExtensions;
using Verse;
using Verse.AI;

namespace Pawnmorph.HPatches
{
    static class MentalBreakPatches
    {
        [HarmonyPatch(typeof(MentalBreakWorker), nameof(MentalBreakWorker.BreakCanOccur))]
        static class BreakCanOccurPatch
        {
            [HarmonyPostfix]
            static void CheckDefRestrictions([NotNull] Pawn pawn, [NotNull] MentalBreakWorker __instance, ref bool __result)
            {
                if (__result)
                {
                    __result = __instance.def.IsValidFor(pawn); 
                }
            }
        }
    }
}