// AnimalPenUtilityPatches.cs created by Iron Wolf for Pawnmorph on 07/06/2021 8:36 PM
// last updated 07/06/2021  8:36 PM

using HarmonyLib;
using Verse;

namespace Pawnmorph.HPatches
{
    [HarmonyPatch]
    static class AnimalPenUtilityPatches
    {
        [HarmonyPatch(typeof(AnimalPenUtility), nameof(AnimalPenUtility.NeedsToBeManagedByRope)), HarmonyPostfix]
        static void NeedsToBeManagedByRopePatch(Pawn pawn, ref bool __result)
        {
            if (__result)
            {
                __result = !pawn.IsSapientFormerHuman();
            }
            else
            {
                __result = (pawn.IsWildMan()); 
            }
        }
    }
}